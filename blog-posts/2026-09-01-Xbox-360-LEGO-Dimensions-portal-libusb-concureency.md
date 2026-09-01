# 2026-09-01 Waking up the Xbox 360 LEGO Dimensions portal: a libusb concurrency bug, not a security gate

A few days ago I wrote about
[building a USB man-in-the-middle for a LEGO Dimensions portal](2026-08-29-USB-man-in-the-middle.html),
where the Xbox 360 variant needed a full hardware relay because its security
interface, XSM3, is a real challenge-response protocol whose key material for
this specific accessory was never published. That project answers "what does
a genuine console say to a genuine portal." This post is about a different,
narrower question: can the [`LEGODimensions`](https://github.com/Ellerbach/LEGODimensions)
.NET library talk to that same Xbox 360 portal (`24C6:FA01`) directly, the
same way it already talks to the standard and Xbox One portals? The answer
turned out to hinge on something that had nothing to do with security at all.

## The starting point: everything sent, nothing received

The library and its diagnostic companion, [`XboxPortalProbe`](https://github.com/Ellerbach/LEGODimensions/tree/main/XboxPortalProbe),
already knew how to wrap a normal 32-byte LEGO Dimensions frame for this
portal: a 2-byte prefix in front of the first 30 bytes of the frame, sent over
a plain interrupt endpoint, no GIP-style envelope like the Xbox One needs.
Sending a command and watching its effect worked immediately - set a pad to
red, and the pad turned red. But anything that depended on a *reply* -
the wake acknowledgement, a color readback, the tag list, an unsolicited tag
placement event - never arrived. Not a garbled reply, not a delayed one:
nothing, ever, on the read endpoint.

That's exactly what a one-directional security gate would look like: outbound
commands accepted and acted on, inbound data withheld until the console
proves itself via XSM3. So that's what we spent a long time chasing.

## Down the XSM3 rabbit hole

The Xbox 360's security interface (interface 3, no data endpoints, control
transfers only) implements XSM3 - identity, challenge, status polling, a
phase-one response, a verify packet, and a final acknowledgement, all built on
TripleDES and MACs. We implemented the full exchange in the probe
(`Xbox360Xsm3Host.cs`) and validated it against a genuine Microsoft Xbox 360
wired controller: full authentication succeeded, response and MAC both
verified. That confirmed the implementation itself was correct - and also
confirmed the portal's own phase-one response couldn't be decrypted with the
same key material, because a licensed accessory like this one uses its own
category-specific root key, which has never been publicly recovered. Physical
chip extraction aside, there was no software path through XSM3 for this
device.

Which made the "reads are gated until auth completes" theory feel settled -
until we found [Bart Dopheide's `LEGODimensions`](https://github.com/dopheideb/LEGODimensions)
project, whose Python client talks to this same portal and never performs
XSM3 at all. Either that client had never actually been run against real
hardware for this code path, or our gate theory was wrong.

## Breaking the deadlock with raw libusb calls

To settle it, we stopped going through LibUsbDotNet entirely and wrote a
handful of direct P/Invoke calls straight to `libusb-1.0.dll` - open, claim
interface 0, write, read, nothing else in between. Sent the wake command,
waited, read the endpoint.

It replied. First try, fully decoded, matching the field layout from
Bart's own client byte for byte.

So the portal *does* reply without authentication. The gate, whatever it
was, lived somewhere in our own host-side code, not in the device. A second
raw test - this time going through LibUsbDotNet's normal device context, but
still doing the actual transfer with the same raw calls - also got a real
reply, which ruled out LibUsbDotNet's endpoint wrapper and interface-3
claiming as causes. The difference that finally mattered was structural: our
probe's normal code path ran a background thread continuously polling the
read endpoint, while commands were written from a different thread. Rebuild
that exact shape with the raw calls, and the reply vanishes again, reliably.

## The real bug: reads and writes racing on the same handle

`libusb_interrupt_transfer` is a synchronous convenience function - it submits
a transfer and waits for its own completion internally. Call it concurrently
from two threads on the same device handle - one endlessly polling reads, the
other issuing an occasional write - and, at least on this libusb Windows
backend, the read side can silently lose its completion notification. Writes
barely show the symptom because they complete fast and rarely overlap; a
background read loop is maximally exposed because it's *always* mid-call,
waiting on a slow, unpredictable device. That asymmetry - writes always
working, reads never working - is exactly what had looked like a
one-directional hardware gate for so long.

## Fixing it for good: full-duration mutual exclusion

For the probe, the fix was to delete the background thread and make every
command strictly sequential: write, then read, on one thread, nothing else
touching the handle in between. `LEGOPortal` couldn't take that shortcut - its
public API depends on a background thread that's always listening, so
`LEGOTagEvent` fires the moment a tag is placed, independent of whatever the
caller happens to be doing. So instead of removing the concurrency, we made it
safe: a semaphore held for the *entire* duration of every read and every
write, not just around the libusb call itself.

```csharp
if (Volatile.Read(ref _xbox360PendingWriters) > 0)
{
    Thread.Sleep(5);
}

_xbox360IoGate.Wait(_cancelThread.Token);
try
{
    error = _endpointReader.Read(readBuffer, Xbox360ReadTimeout, out bytesRead);
}
finally
{
    _xbox360IoGate.Release();
}
```

## A second gotcha: SemaphoreSlim doesn't queue fairly

That alone produced a working but occasionally infuriating portal: colors
sometimes took up to thirty seconds to change. `SemaphoreSlim` doesn't
guarantee FIFO ordering, and the read loop's own release-then-immediately-
reacquire cycle could keep winning that race against a write's pending
`Wait()` call for many consecutive iterations. The fix was a plain interlocked
counter the write side increments before waiting on the gate, which the read
loop checks first - if a write is pending, back off for a few milliseconds
instead of racing straight back in:

```csharp
Interlocked.Increment(ref _xbox360PendingWriters);
try
{
    _xbox360IoGate.Wait();
    try
    {
        error = _endpointWriter.Write(bytes, ReadWriteTimeout, out bytesWritten);
    }
    finally
    {
        _xbox360IoGate.Release();
    }
}
finally
{
    Interlocked.Decrement(ref _xbox360PendingWriters);
}
```

## A third gotcha: one tracked command at a time

Even with reads and writes safely serialized, `ReadTag()` would work the first
time and then silently fail on the next page. The remaining culprit: this
portal apparently can't reliably multiplex two in-flight "Read" commands, even
with distinct message IDs, if a second one is sent before the first's reply
arrives. `LEGOPortal` already has an internal auto-read that fires whenever a
tag is placed; if that raced against an explicit `ReadTag()` call from the
caller's own thread, one of the two replies would get corrupted or dropped.
The fix serializes every tracked request/reply cycle behind its own lock, and
the internal auto-read simply skips itself for that one event if the lock
isn't immediately free, rather than blocking and risking a deadlock against
the very call it exists to support.

## Thanks, Bart

None of this would have converged nearly as fast without
[Bart Dopheide](https://www.linkedin.com/in/bart-d-5510b255/)'s
[`LEGODimensions`](https://github.com/dopheideb/LEGODimensions) project. His
reverse engineering corrected a wire-format detail
we'd gotten wrong from an earlier capture (the frame prefix is `0B 16`, not
`0B 14`), and his Python client was the reference that let us confirm details
like the wake command's
hardcoded message ID and the Windows-specific skip of the USB configuration
step. Real, working, previously-published client code for hardware this
under-documented is worth a lot - thank you, Bart.

## Where it landed

`LEGOPortal` now detects vendor/product ID `24C6:FA01` and exposes the exact
same public API for it as for a standard or Xbox One portal - wake, colors,
tag events, tag reads, all of it, confirmed against real hardware. The full
protocol writeup, including the frame format and every concurrency
requirement above, lives in
[`Xbox360PortalProtocol.md`](https://github.com/Ellerbach/LEGODimensions/blob/main/Xbox360PortalProtocol.md).

The XSM3 side of this investigation isn't wasted, either: it lives on as a
diagnostic path in `XboxPortalProbe`, and it's the same protocol the
[MITM bridge project](2026-08-29-USB-man-in-the-middle.html) has to relay
verbatim to a real portal, since - unlike this library - it can't just decide
authentication isn't required. And wait, I just received the Xbox 360 game.
So, game on for some MITM play to see if we can discover this key! XSM3 is
fascinating!
