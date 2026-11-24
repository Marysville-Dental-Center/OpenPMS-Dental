# Port note: IMG-TW13 (twain-contract 1.3 in the capture path)

Why: the private TWAIN source project published twain-contract 1.3 (2026-11-23). It adds continuous mode (W8), device events and a device-online
check (W9), five more installation adjustments and a third enhancement. All of them are opt-in, and with them off 1.3 behaves as 1.2. The
owner set adoption as a priority on 2026-11-24. The clinic keeps the 1.2 build until the owner has 1.3 packaged, so every change works with 1.2:
the session detects each feature and never assumes it.

## What changes

1. **Metadata parser safety** (`ImagingCapture/Metadata/CaptureMetadataParser.cs`, `CaptureMetadata.cs`). `adjustments` now reads:
   - invert (true/false);
   - levels ([low, high], whole numbers);
   - relief, sharpen (whole numbers);
   - gamma (a number).

   A key that is missing or null, or that holds its neutral value (false, [0, 255], 0, 0, 1.0), stays neutral. These make the image count as
   adjusted:
   - any other value;
   - a value that cannot be read (with a warning);
   - a member this version does not know (with a warning; names compared as written).

   Out-of-range values are kept and warned about. Orientation neutrality still looks at rotate and flip only.
2. **Device events** (twain-contract 1.3 section 10.6), opt-in with `[Device.N] DeviceEvents=1`:
   - The key is not gated, because it changes text only. It appears in Capture settings, the sidecars, and the options sent to the 32-bit
     helper.
   - Before each enable, in state 4, the session sets `CAP_DEVICEEVENT` (a TW_ARRAY of DEVICEREADY, IMAGECAPTURED, DEVICEOFFLINE and
     DEVICEREMOVED). No MSG_QUERYSUPPORT is sent first: a 1.2 source answers TWCC_CAPUNSUPPORTED, which is logged, and nothing changes.
   - The message loop, and its one-shot poll, read each `MSG_DEVICEEVENT` with `DAT_DEVICEEVENT` / MSG_GET, using only the Event field.
     They pass it on as a `TwainCaptureStatus` with `DeviceEvent` set and the state unchanged. The 32-bit helper passes the code on
     with the status, over its wire (the round trip is tested; the helper end to end with events is not).
   - A device event that comes before a notification never hides it: the one-shot poll asks again after each event, and a program message
     that a source answers with an event is dispatched all the same.
   - The capture session shows it:
     - "Sensor ready, waiting for exposure: <target>", until the next exposure;
     - "Exposure captured, receiving the image";
     - an error for offline or unplugged.
   - Nothing is stopped or disabled. The texts are skipped while the session is stopping, paused after a failed save, or after an
     unreadable layout.
   - Without events, the texts are unchanged and never claim the sensor is ready (design 7.5).
3. **Continuous mode** was already implemented behind `[Device.N] SessionMode=Continuous` and `ContinuousCap` (design 7.4), and it matches
   1.3:
   - The transfer loop goes on while MSG_ENDXFER's count is above 0 and waits in state 5 at 0. 1.3 counts the images saved during a
     transfer.
   - Repeated MSG_XFERREADY with nothing pending is ignored.
   - MSG_CLOSEDSREQ ends the enable.

   The documented value for the 1.3 source is now `ContinuousCap=0x8001`. A 1.2 source refuses it, and the enable runs per slot. No code
   change was needed.
4. **Device online**: `CAP_DEVICEONLINE` was already read before each enable, and FALSE already gives "Sensor not connected" without
   enabling. With 1.3 it becomes a real USB check once the source's installation setting names the sensor. No code change was needed.
5. **The source's own 1.3 settings** (`HdMode=caries`, the five new adjustments) are set in the source's INI file on the computer, not by
   Open PMS - Dental. The parser change (item 1) makes any of them count as an adjustment. The setup guide says to leave them at their
   defaults for diagnostic captures.
6. Docs:
   - `docs/imaging-capture-design.md`: status, sections 7.4, 7.5 and 7.9, and the [Device.N] block.
   - `docs/imaging/intraoral-capture-setup.md`: new section 15.

## Tests

- `CaptureMetadataParserTests` (+4):
  - the neutral 1.3 object;
  - each non-neutral value;
  - unreadable and out-of-range values;
  - unknown keys.
- `TwainCaptureSessionTests` (+3), against the simulated sensor source, which now has device events:
  - events come in order as statuses of the running enable, and only the selected ones;
  - a 1.2 source refuses the capability, and nothing else changes;
  - with the key off, nothing is asked for.
- `CaptureSessionControllerTests` (+1): ready, captured, offline, back to ready, removed, and the plain waiting text after an exposure.
