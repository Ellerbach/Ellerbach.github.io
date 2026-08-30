# 2026-08-29 Building a USB man-in-the-middle for a Lego Dimensions portal

This project started with a simple question: what does a Lego Dimensions
portal actually say to an Xbox over USB? Answering it meant building a
real-time bridge that sits *between* the console and the portal, so every
byte in both directions could be captured and understood.

The hardware side settled on two boards working together:

- A **Raspberry Pi Zero** acts as the USB *host* for the real portal.
- An **RP2350** (Pico 2) acts as the USB *device*, presenting itself to the
  Xbox as if it were the portal.
- The two are linked by a simple 3-wire UART, relaying every report between
  them.

```text
Real portal --USB--> Pi Zero (host) --UART--> RP2350 (device) --USB--> Xbox
```

That plumbing is genuinely useful — but it turned out the Xbox One and the
Xbox 360 portals needed very different amounts of it.

## Inside the RP2350 gadget

The RP2350 side runs on [TinyUSB](https://github.com/hathach/tinyusb), and
regardless of which console it's pretending to be, the firmware's core loop
is the same simple idea: pump USB events, then shuttle bytes between the
Xbox-facing USB endpoint and the UART link back to the Pi.

```c
while (true) {
    tud_task(); // let TinyUSB process USB events

    // Real portal report (arrived over UART from the Pi) -> Xbox,
    // over this gadget's own vendor IN endpoint.
    while (uart_is_readable(RELAY_UART)) {
        uint8_t byte = uart_getc(RELAY_UART);
        if (frame_parser_feed(&parser, byte) &&
            parser.direction == FRAME_PORTAL_TO_XBOX) {
            tud_vendor_write(parser.payload, parser.length);
            tud_vendor_flush();
        }
    }

    // Xbox OUT transfer -> relay to the Pi, which forwards it to the
    // real portal.
    if (tud_vendor_available()) {
        uint8_t buf[FRAME_MAX_PAYLOAD];
        uint32_t n = tud_vendor_read(buf, sizeof(buf));
        uart_send_frame(RELAY_UART, FRAME_XBOX_TO_PORTAL, buf, (uint8_t)n);
    }
}
```

That's genuinely the whole idea for the endpoints both consoles share:
the gadget never needs to understand LEGO frames or GIP to relay them, it
just needs to know which direction each byte is going. The Xbox 360 needs
one more piece of plumbing on top of this — its security handshake doesn't
use endpoints at all, only control transfers, covered further down — but
even that follows the same "just relay it" principle. The interesting work
otherwise all happens in how the gadget presents *itself* to USB in the
first place: the **descriptors**. And this is where the Xbox One and Xbox
360 gadgets stop looking anything alike.

The Xbox One portal is refreshingly simple: one vendor-class interface,
two interrupt endpoints, done.

```c
enum { ITF_NUM_VENDOR, ITF_NUM_TOTAL };

uint8_t const desc_vendor_configuration[] = {
    TUD_CONFIG_DESCRIPTOR(1, ITF_NUM_TOTAL, 0, CONFIG_TOTAL_LEN, 0x80, 500),

    9, TUSB_DESC_INTERFACE, ITF_NUM_VENDOR, 0, 2, TUSB_CLASS_VENDOR_SPECIFIC,
    PORTAL_IF_SUBCLASS, PORTAL_IF_PROTOCOL, 0,

    7, TUSB_DESC_ENDPOINT, EPNUM_VENDOR_OUT, TUSB_XFER_INTERRUPT,
    U16_TO_U8S_LE(PORTAL_EP_SIZE), PORTAL_EP_INTERVAL,

    7, TUSB_DESC_ENDPOINT, EPNUM_VENDOR_IN, TUSB_XFER_INTERRUPT,
    U16_TO_U8S_LE(PORTAL_EP_SIZE), PORTAL_EP_INTERVAL,
};
```

The Xbox 360 portal, captured straight from real hardware, turned out to
be a **four-interface composite device** — because it's built on the same
descriptor shape as a genuine wired Xbox 360 controller, headset channels
and all:

```c
uint8_t const desc_configuration[] = {
    TUD_CONFIG_DESCRIPTOR(1, 4, 0, CONFIG_TOTAL_LEN, 0x80, 500),

    // Interface 0: the only one that actually carries data.
    9, TUSB_DESC_INTERFACE, ITF_NUM_MAIN, 0, 2, TUSB_CLASS_VENDOR_SPECIFIC,
    MAIN_IF_SUBCLASS, MAIN_IF_PROTOCOL, 0,
    7, TUSB_DESC_ENDPOINT, EPNUM_MAIN_OUT, TUSB_XFER_INTERRUPT, U16_TO_U8S_LE(EP_SIZE), 4,
    7, TUSB_DESC_ENDPOINT, EPNUM_MAIN_IN,  TUSB_XFER_INTERRUPT, U16_TO_U8S_LE(EP_SIZE), 4,

    // Interfaces 1 and 2: present only so the composite shape matches a
    // real controller (headset audio out/in) -- 0 endpoints, unused.
    9, TUSB_DESC_INTERFACE, ITF_NUM_HEADSET_OUT, 0, 0, TUSB_CLASS_VENDOR_SPECIFIC,
    MAIN_IF_SUBCLASS, HEADSET_OUT_IF_PROTOCOL, 0,

    9, TUSB_DESC_INTERFACE, ITF_NUM_HEADSET_IN, 0, 0, TUSB_CLASS_VENDOR_SPECIFIC,
    MAIN_IF_SUBCLASS, HEADSET_IN_IF_PROTOCOL, 0,

    // Interface 3: security -- control transfers only, no endpoints at all.
    9, TUSB_DESC_INTERFACE, ITF_NUM_SECURITY, 0, 0, TUSB_CLASS_VENDOR_SPECIFIC,
    SECURITY_IF_SUBCLASS, SECURITY_IF_PROTOCOL, 0,
};
```

Interfaces 1 and 2 are declared with **zero endpoints** on purpose: nothing
here relays headset audio, but the shape still has to be present, or the
composite device doesn't match what a real controller looks like. Interface
3 is the interesting one — no endpoints either, but very much *not*
decorative, since it's how the Xbox 360's security handshake gets carried,
covered below.

## Inside the Pi Zero bridge

The Pi Zero's side is just as simple as the RP2350's, mirrored: it's the
USB *host* for the real portal, so instead of `tud_*` callbacks it uses
`pyusb` to read reports straight off the real hardware, and forwards each
one over the same UART link, framed the same way:

```python
def portal_to_rp2350(dev, ep_in, ser, stop):
    while not stop.is_set():
        try:
            data = dev.read(ep_in.bEndpointAddress, ep_in.wMaxPacketSize, timeout=100)
        except usb.core.USBTimeoutError:
            continue
        ser.write(encode_frame(FRAME_PORTAL_TO_XBOX, bytes(data)))
```

A second thread does the mirror image — reading frames back off the UART
and writing them to the real portal — and, for the Xbox 360, also performs
whatever raw `ctrl_transfer()` the security interface asked for and relays
the real portal's answer back. Same principle throughout the whole
project, on both boards: nobody in the middle needs to understand the
protocol, they just need to move bytes in the right direction.

This Python code is throwaway tooling, not code I plan to maintain.

## Why build a MITM at all?

There's already excellent prior art here:
[Ellerbach/LegoDimensions](https://github.com/Ellerbach/LegoDimensions) is a
.NET library and set of tools that already talks to the PS4, Wii and PC portals
directly, including a documented protocol. And now adding the Xbox One project's
[`XboxPortalProtocol.md`](https://github.com/Ellerbach/LegoDimensions/blob/main/XboxPortalProtocol.md) and a diagnostic probe ([`XboxPortalProbe`](https://github.com/Ellerbach/LegoDimensions/tree/main/XboxPortalProbe)) that
were both used throughout this project as the reference for what a correct
implementation should look like.

So why not just stop there? Because a library talking to real hardware
answers "does this work," not "why does this work, in every case." Building
a real-time, transparent MITM — something that sits between a *genuine*
console and a *genuine* portal and simply observes — gives a few things a
library alone can't:

- **Ground truth, not just a working case.** Capturing an unmodified
  console talking to an unmodified portal shows the actual protocol as
  implemented, including edge cases, timing, and quirks that documentation
  written against one accessory might not generalize from.
- **A way to validate a *new* fake implementation against the real thing.**
  Once I write my own gadget firmware, the question becomes "does the
  real console accept it," and the fastest way to check that without
  risking a real console is to test the fake gadget against the same probe
  tooling used to understand the real portal in the first place.
- **A path for the parts that aren't documented yet.** The Xbox 360
  variant of the portal isn't covered by the existing library at all, and
  turned out to need its own protocol investigation from scratch (more on
  that below).

## Xbox One: the bridge we didn't need

The Xbox One portal (`0E6F:0141`) enumerates as a plain vendor-class USB
device: two interrupt endpoints, no HID, nothing exotic. Getting a fake
gadget to *look* right was the easy part — the interesting part was
understanding what it actually *says*.

### GIP: one protocol for every Xbox One accessory

Every Xbox One accessory — controllers, headsets, chatpads, and this
portal — speaks the same underlying framing protocol: **GIP**, the Xbox
Game Input Protocol. GIP doesn't know or care what kind of accessory it's
carrying; it's a generic envelope, and each accessory type just uses
different GIP *commands* inside it. A non-chunked GIP packet looks like
this:

| Field | Size | Description |
| --- | ---: | --- |
| Command | 1 byte | Which GIP command this is |
| Options | 1 byte | Client ID (low nibble) + flags (high nibble) |
| Sequence | 1 byte | Rolling sequence number, independent per direction |
| Payload length | LEB128 | How many payload bytes follow |
| Payload | variable | Command-specific data |

A handful of GIP commands matter for a portal: `0x02` ANNOUNCE (device
identifies itself when freshly connected), `0x04` IDENTIFY (extended,
chunked identification data), `0x06` AUTHENTICATE (activates the device),
`0x01` ACKNOWLEDGE (acks chunked transfers), and — the one that actually
matters for LEGO Dimensions — `0x21`, a vendor-defined command this portal
uses to carry one complete, ordinary 32-byte LEGO Dimensions frame as its
payload:

```text
21 00 SS 20 [32-byte LEGO frame]
     |  |
     |  +-- payload length (0x20 = 32 bytes)
     +----- GIP sequence number
```

In other words: GIP is just the outer envelope. Once you unwrap it, the
actual "place a tag on the portal" / "light up green" commands are the same
32-byte LEGO frames (sync byte, length, command, payload, checksum) that
every version of the portal uses underneath.

### Getting the gateway open

A freshly connected (or freshly authenticated-with-a-new-host) portal
won't just accept LEGO commands wrapped in GIP `0x21` right away — the
gateway has to be activated first. The sequence that works for both a warm
and a cold portal:

1. Send the standard LEGO "wake" command, wrapped in GIP `0x21`, and start
   listening for a reply immediately (don't wait to send this — a warm
   portal can reply right away).
2. If a wrapped reply comes back quickly, the gateway was already
   authenticated from a previous session — nothing more to do.
3. If nothing comes back, send GIP `0x06` (AUTHENTICATE) with payload
   `01 00` ("authentication complete"), then wait again for the wrapped
   wake reply.
4. From here on, ordinary LEGO commands and events flow through GIP `0x21`
   in both directions.

None of this — GIP framing, the wake/authenticate dance, or the LEGO
frames it carries — needed a real portal attached once we understood it,
which is what let us skip the bridge entirely for this console.

### The actual shortcut: Windows doesn't check any of that

Here's the part that made the Xbox One side simpler than expected: none of
the GIP detail above is what makes Windows (and, since its accessory stack
is Windows-derived, likely the Xbox itself) trust a device in the first
place. That decision happens *during USB enumeration*, before a single GIP
packet is ever exchanged, using a much older, unrelated mechanism: a
**Microsoft OS 1.0 Descriptor**. The device answers a special string
descriptor request with the signature `"MSFT100"`, and Windows follows up
with a vendor-specific control request that returns a **compatible ID**.
The real portal answers with `XGIP10` ("Xbox Gaming Input Protocol 1.0"),
which is exactly what tells Windows to bind its own inbox driver
(`dc1-controller.inf`) instead of leaving the device unrecognized.

That mechanism doesn't require the real portal to be present at all. It's
answered entirely by the gadget's own firmware:

```c
// "MSFT100" signature string, fetched at descriptor index 0xEE
static uint8_t const ms_os_string_desc[] = {
    0x12, TUSB_DESC_STRING,
    'M', 0, 'S', 0, 'F', 0, 'T', 0, '1', 0, '0', 0, '0', 0,
    MS_OS_VENDOR_CODE, 0x00,
};

// Extended Compat ID descriptor: compatible ID "XGIP10"
static uint8_t const ms_os_compat_id_desc[] = {
    0x28, 0x00, 0x00, 0x00,  0x00, 0x01,  0x04, 0x00,  0x01,
    0x00,0x00,0x00,0x00,0x00,0x00,0x00,  0x00,  0x01,
    'X','G','I','P','1','0', 0x00, 0x00,
    /* ...padding... */
};
```

The one sharp edge: Windows *caches* whether a given VID/PID/revision
supports this mechanism, and which vendor request code to use, in the
registry. Get that vendor code wrong and Windows silently ignores your
descriptor rather than telling you why.

Once that piece was right, our RP2350 gadget enumerated on Windows
*identically* to the real portal — same driver, same class, same
`Status=OK` — without the Pi Zero or the real portal anywhere in the
picture. The full MITM bridge we built is still there and still works, but
for the Xbox One specifically, it wasn't the thing that made the fake
portal acceptable. A firmware-only trick was.

*(A fully detailed writeup of the GIP protocol as implemented lives in the
[ellerbach/legodimensions](https://github.com/Ellerbach/LegoDimensions/blob/main/XboxPortalProtocol.md)
project — link to the specific doc to follow.)*

## Xbox 360: the one that actually needs the bridge

The older Xbox 360 portal (`24C6:FA01`) is a different animal entirely, and
this is where the full relay earns its keep.

A few high-level differences from the Xbox One:

- LEGO's normal 32-byte command frames are wrapped differently — a 2-byte
  header (`0B 14`) in front of the first 30 bytes of the frame — sent over
  plain interrupt endpoints, no GIP-style envelope.
- The USB descriptor is a **4-interface composite device**: the main data
  interface, two unused headset-shaped interfaces (present only because
  genuine Xbox 360 controllers have them, and the console's driver stack
  seems to expect the shape to match), and a fourth interface dedicated
  entirely to **security**.
- That security interface doesn't use interrupt endpoints at all. It's
  driven purely by USB **control transfers**, implementing something called
  **XSM3** (Xbox Security Method 3) — a real challenge-response protocol
  built on TripleDES encryption and MACs, gating whether the console will
  talk to the accessory at all.

```c
// Xbox 360 LEGO frames: a 2-byte prefix, then as much of the standard
// 32-byte LEGO frame as still fits in a 32-byte report. lego_frame is the
// full, standard 32-byte frame; only its first 30 bytes are copied in --
// forced by the math (32 - 2 prefix bytes = 30), not a bug or a shortcut.
// The dropped trailing 2 bytes are normally just zero padding.
uint8_t report[32] = { 0x0B, 0x14 };
memcpy(report + 2, lego_frame, 30);
```

This is the part that ruled out the "fake it in firmware" shortcut that
worked for the Xbox One: XSM3's cryptographic key material for this
specific licensed accessory is proprietary and was never published. There's
no descriptor trick that substitutes for having the actual key. So for the
Xbox 360, the RP2350 doesn't try to *answer* the security handshake at
all — it forwards every single control transfer, verbatim, to the Pi Zero,
which performs the identical request against the real portal and relays
the real answer back. The gadget stays a dumb pipe; the real hardware does
the actual cryptography.

Since the security interface has no endpoints at all, none of that goes
through `tud_vendor_read()`/`tud_vendor_write()` like the main loop shown
earlier — control transfers are a separate path, with its own SETUP/DATA/
STATUS stages to handle correctly (an earlier, oversimplified version of
this snippet stalled non-setup stages and used a stack buffer past its
lifetime — fixed here to match what's actually running):

```c
bool tud_vendor_control_xfer_cb(uint8_t rhport, uint8_t stage,
                                 tusb_control_request_t const *request) {
    if (!targets_security_interface(request)) {
        return false;
    }
    bool const device_to_host = request->bmRequestType_bit.direction == TUSB_DIR_IN;

    if (stage == CONTROL_STAGE_SETUP) {
        if (device_to_host) {
            // No way to answer XSM3 ourselves -- ask the Pi to run this
            // exact control-IN against the real portal, and wait for it.
            static uint8_t response[64]; // file-scope: outlives this call
            uint8_t len = relay_control_in(request, response, sizeof(response));
            return tud_control_xfer(rhport, request, response, len);
        }
        // Control-OUT: just accept the data for now, relay once it's here.
        return tud_control_xfer(rhport, request, out_buf, request->wLength);
    }

    if (stage == CONTROL_STAGE_DATA && !device_to_host) {
        relay_control_out(request, out_buf, request->wLength); // data has arrived now
    }

    return true; // accept later stages of a request we already claimed
}
```

That relay is now confirmed working end-to-end against real hardware: the
full XSM3 sequence (identity → challenge → status poll → response) passes
cleanly between a test tool, the RP2350, the Pi, and the real portal, with
no corruption and sub-2ms round trips — control-IN (identity, status,
response) and control-OUT (challenge, with a non-trivial payload) both
exercised, not just the empty/trivial case. The test tool's own reference
implementation can't finish validating the response — it only knows the
*published* Microsoft keys, and the real LEGO portal doesn't use them — but
that's exactly the expected, documented outcome, not a bug in the relay.

## What's next

The Xbox 360 side is validated as far as a PC can take it. What we haven't
seen yet is a real Xbox 360, running a real copy of the game, talking to
our gadget. That's the next step: get it in front of the actual console,
and capture what a genuine play session looks like — tag placements,
removals, LED commands, all of it — through the same JSONL logging the
bridge already writes. Once we can see real traffic flowing end to end,
there may be more to learn (and maybe even more shortcuts to find) in the
protocol itself.

So, rendez-vous later once the game I've ordered will arrive. In the mean time,
the old Xbox 360 is out of the drawer, one controller connected, Xbox account
connected, everything up to date!
