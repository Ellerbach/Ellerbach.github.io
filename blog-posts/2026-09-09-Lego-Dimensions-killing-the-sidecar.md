# 2026-09-09 Killing the sidecar: getting the Xbox 360 portal to authenticate on its own

Last time, I left things in a decent place: an RP2350 pretending to be a Lego Dimensions portal's data interface, and a second board (a Pi Zero) doing the actual security handshake against a real, physical portal plugged into it. It worked. The console was happy, the game ran, tags placed fine. But it bugged me. Two boards and a genuine licensed accessory, just to get one console to say yes? That felt like cheating in the wrong direction. So the question became: can interface 3's security handshake be answered locally, with nothing real in the loop at all?

Short answer: yes, mostly, and the part that finally worked is not the part I expected.

![The finished simulator: browser-controlled toy pad, tags placed on the center/left/right positions](https://raw.githubusercontent.com/Ellerbach/LegoDimensions/main/docs/portal-simulator-main.png)

## There's already a public implementation of this thing

XSM3 (Xbox Security Method 3) sounds like it should be locked away somewhere, and in a sense the actual secret material is. But the protocol itself, and a working from-scratch implementation of it, is public. [InvoxiPlayGames/libxsm3](https://github.com/InvoxiPlayGames/libxsm3) has the DES/3DES, the SHA-1, a "Parve" cipher used only for the final authenticity code, and the XSM3 state machine sitting on top of all of it.

Swapping the old UART relay for direct calls into this library was mostly mechanical. Where the firmware used to forward bytes to the Pi Zero and wait, it now just calls `xsm3_do_challenge_init()` or `xsm3_do_challenge_verify()` directly. The protocol shape was already understood from the previous round of captures. Getting a real console to actually accept what came out the other end, though, took a lot longer than I expected.

## Proving the crypto right without burning another reflash cycle

Every single fix meant rebuild, reflash, plug into the console, wait, check. That loop is slow, and after the third or fourth time waiting a minute just to learn "still nothing," I wanted a faster way to check my own work.

The trick that ended up saving the most time: decrypt the device's own response, offline, on a laptop, using the same library functions, and see if it's internally consistent. The response is basically `encrypt(random_controller || random_console)` under a key derived from the console's ID — and that derivation doesn't depend on any randomness at all, it's just `SHA1(console_id)` run through 3DES with two public root keys. So you can derive that key yourself, decrypt the real ciphertext the Pico already sent over the wire, and check whether the tail end of the plaintext matches the console's own random value (which is independently decryptable too):

```c
UsbdSecXSM3AuthenticationCrypt(kv_key1, random_console_data, 0x10, random_console_data_enc, 1);
UsbdSecXSM3AuthenticationCrypt(random_console_data_enc, challenge_response + 5, 0x20, decrypted_body, 0);
// decrypted_body[0x10..0x20) should equal random_console_data if the whole
// pipeline -- key derivation, encryption, MAC, and the final authenticity
// code -- is bit-for-bit correct
```

It matched. Exactly, down to the final 8-byte authenticity code. That was genuinely useful to know before spending another reflash cycle chasing a crypto bug that, it turned out, didn't exist. Whatever was wrong, it wasn't the math.

## The fix that had nothing to do with math

The XSM3 identification packet includes a single "category" byte. A real Lego Dimensions portal reports `0x82`. I set that exact value — matched a real captured portal byte for byte — and it still didn't work. The console would accept a cryptographically perfect challenge response and then just go quiet. No error, no retry, nothing. Just silence, which is somehow worse than an obvious failure.

The actual fix was to report `0x02` instead. That's the generic "1st-party controller" category, not the portal one. Everything else stayed the same — same crypto, same VID, same identity in every other respect. One byte, and it was the entire difference between the console going quiet forever and the full handshake completing, game running, tags recognized.

My best guess at why: `libxsm3`'s two public root keys are literally commented in the source as being for "1st party controllers." A real portal probably authenticates against a different keyvault slot, one that nobody's ever needed to reverse-engineer, because nobody building a homebrew XSM3 accessory has ever needed a portal's keys before. Every public project like this one only ever needed to pass as a controller. So reporting a controller identity lands you in the keyvault slot these public keys actually belong to.

## A Guitar Hero controller muddies the theory (in a useful way)

While writing this up I went looking for older prior work and found [brandonw.net/360bridge](https://brandonw.net/360bridge/) from Brandon Wilson's "Salamax" project from around 2011, built for basically the same reason as mine: getting non-Microsoft input onto an Xbox 360 by bridging through the real handshake. Buried in [a raw USB analyzer log of a real accessory](https://brandonw.net/360bridge/Xbox360WiredController.xlsx) is a full, checksum-valid XSM3 identification packet:

```text
49 4B 00 00 17 84 3D 35 33 16 D6 33 28 23 03 20 00 00 80 82 AD 1B 01 FA 03 00 01 01 28
                                                          ^^ category = 0x82
                                                                ^^^^^ VID = 0x1BAD (Harmonix)
```

VID `0x1BAD` belongs to Harmonix — this is a Guitar Hero or Rock Band peripheral, not a Microsoft controller — and it reports category `0x82`. The same value that failed for me. I fed that capture's actual challenge bytes through the same library and the same public root keys, and it validated cleanly, checksum and MAC both correct. This shows that the published keys also validate this capture.

So, honestly, reporting `0x02` is a fix that works, not one I can fully explain. Something about the combination of category `0x82` with my specific synthetic identity produces a different outcome than a real, licensed Harmonix accessory reporting that same category, and I don't know what that something is yet. What I do know is that a controller identity authenticates fine with only the publicly known keys, and a portal identity, at least in my hands, did not. Maybe someone with a real portal to capture against will eventually explain the gap. For now the workaround is good enough that the game just works.

## Small bonus: it plays on Wii U too

Once the Xbox 360 side was solid, I pointed the same simulator's standard (non-Xbox) USB personality at Cemu, the Wii U emulator, running the actual game. No changes needed on my end. Toys dropped onto the browser UI's center/left/right positions showed up correctly in-game, and the game was fully playable — characters, vehicles, all of it. That one was almost anticlimactic after everything else.

## What's still missing: Xbox One

The Xbox One portal doesn't use XSM3 at all. It runs on GIP (the Xbox Game Input Protocol) with a certificate-based authentication flow instead, and it's the one console my simulator still can't get past. There's a detailed decode of a real capture sitting in the [`legodimensions`' `XboxOnePortalCaptureDecoded.md`](https://github.com/Ellerbach/LegoDimensions/blob/main/XboxOnePortalCaptureDecoded.md), and it shows real progress, just not enough. Enumeration succeeds. The portal's GIP `IDENTIFY` reassembles cleanly. The real portal even sends its full 825-byte accessory certificate over fifteen GIP chunks without issue. What doesn't happen is a final wrapped Lego response after all that — the trace just shows repeated authentication challenge records that the document itself says "remain opaque unless their algorithms and private key material are available," followed by the console's wake requests going unanswered.

Unlike the category byte on the Xbox 360, this one looks like real cryptography doing its job, not a metadata quirk waiting to be found. The certificate might be replayable as public identity data, but the challenge responses that come after it are session-bound, so a single capture can't just be replayed into a working answer. That one stays open for now. And I think, will be open for a very long time!
