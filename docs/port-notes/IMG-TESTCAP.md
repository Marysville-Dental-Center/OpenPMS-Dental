# Port note: IMG-TESTCAP (Test capture leads to the verification record)

Why: at the OP1 clinic run (2026-11-22) the tester took a good test image but never clicked "Write verification record". Without that
record, the capture session took 8-bit X-rays. The owner asked on 2026-11-24 that the window lead the user to that step.

## What changes

`ImagingCapture/UI/TestCaptureModel.cs` (new: IsRecordTheNextStep, GetVerificationText, GetNextStepText) and `FrmTestCapture.xaml(.cs)`:

1. **The step, in words** (a bold line next to the results), worked out from a trial record of the current image with both checks assumed
   ticked, so it never promises more than the record would give:
   - before an image: "Step 1 of 2: take a test image ..., asking for N bits" (N = the depth the session asks for);
   - a record would unlock N bits: "Step 2 of 2, required: tick the ... check(s) on the left, then click Write verification record (Setup
     permission). It unlocks N-bit X-rays for this device; without it they stay 8-bit.";
   - it would unlock nothing (an 8-bit image, asked for 14 with Bits=16, byte order looks swapped, and so on): the reason, and "Take the
     test image again, asking for N bits";
   - this computer or device takes 8 bits anyway (Allow16Bit=0, Bits=8): the record notes the test and unlocks nothing;
   - settings not verified on this computer: close Test capture, save the device in Capture settings, open Test capture again (shown
     before the first image too, so no image is taken on safe values);
   - no record possible (for example, no driver version reported): why.
2. **The device's state, in words** (a bold line above the step):
   - "Not verified in this window ...";
   - "Verified: record written <date>. The session can now take N-bit X-rays from this device.";
   - "Record written <date>: it unlocks nothing ...".
3. **Write verification record is the primary action** (IsPrimary, the accent look) while it is the next step. The focus moves to it once,
   when it becomes the next step, but not while the operator is in one of the window's lists. A record that would unlock nothing never
   gets the primary look.
4. After a record that unlocks nothing, a new test image makes the record the next step again. After one that unlocks, a new image does
   not prompt, since a later failing record would become the latest and take 16 bits back.
5. Layout:
   - the results box is shorter (96 px);
   - the two new labels sit under it in the right column;
   - the X-ray category warning keeps to the left column.

   No new colours or fonts.
6. Unchanged:
   - CanWriteRecord and BuildRecord;
   - the Setup permission check;
   - the "Write it anyway?" confirmation;
   - the securitylog row (Setup, FKey = ImagingDeviceNum).

## Tests

`TestCaptureModelTests` (+5):
- the default 16-bit path, from step 1 to verified, with the checks left as they were;
- a retake after a record that unlocks nothing, which prompts again;
- what a record would really unlock: 14 with Bits=14, 14 when the driver gave 14, nothing when asked for 14 with Bits=16, nothing for an
  8-bit image;
- 8 bits on this computer or device;
- unverified settings, and a source without a driver version.

One review was done; its findings (the texts promised 16 bits too broadly, no prompt after a retake, the unverified-settings advice, label
sizes, focus while in a list) are fixed here.
