# Port note: IMG-P4c (video recording in the camera session)

Why: the owner's imaging design, camera and video (2026-11-22), section 4, with decisions C5 (a 2-minute limit) and C6 (sound off). IMG-P4d
follows with playback, trim and "save frame as photo" in the module; IMG-P4e with the camera settings.

## What changes

1. **R (or Record) starts and stops a video; P pauses it.**
   - A red line over the live picture reads "REC 0:42 / 2:00". 15 seconds before the limit it adds "stops at 2:00", and recording stops
     cleanly at the limit.
   - Space and the camera button still take photos while it records.
   - There is no sound (C6: off; the office setting comes with IMG-P4e).
2. **The file** is MP4 with H.264, written by Windows' Media Foundation sink writer, so no codec is installed.
   - The bit rate is about 8 Mbit/s at 1080p and 30 frames a second, scaled to the picture, between 1 and 16 Mbit/s.
   - It is fragmented MP4, so what was written stays playable if the program or Windows stops while recording.
3. **Interop written in the repository:** `ImagingCapture/Uvc/Interop/MediaFoundation.cs`.
   - IMFAttributes, IMFSample, IMFMediaBuffer and IMFSinkWriter, and the MF functions, taken from the Windows SDK headers (10.0.26100).
   - No NuGet package and no vendor DLL (design 146).
   - `InteropLayoutTests` pins every called vtable slot (14). Unused slots are `Unused_` placeholders.
4. **Every frame, not just the newest.** `IFrameSource.FrameTap` is called on the streaming thread with each preview frame before it is
   published. The preview's FrameExchange keeps only the newest frame, which is right for the screen but would drop frames from a video.
   - `VideoRecorder` copies the frame into a pooled buffer and returns at once. Its own MTA thread writes it.
   - When the encoder falls behind, frames are left out and counted; the stream is never held up.
   - Timestamps come from the frames' stream times, with pauses taken out.
5. **Where it goes:**
   - The recording streams to %ProgramData%\OpenPMS-Dental\Imaging\Video\recording on this computer (the protected Imaging folder), with a
     sidecar written at the start.
   - On stop it is imported into the session patient's images (ImageStore.Import of the file), unmounted, in the session's category,
     described "Video 0:42", with the teeth for the next shot and the time it started. The local files then go.
   - When the import fails, the files move to the unsaved photos folder and **Camera, Unsaved photos** files them later.
   - A recording left by a program that stopped is moved there when the next session starts.
   - Recording needs 2 GB free on this computer (design section 6, "Disk nearly full": "Video is off until there is at least 2 GB free.
     Photos still work.").
6. **It always stops and saves first** on End or Esc, on a patient change (to the session's patient), when the camera fails or is opened
   again, and when the module is left. The idle release never fires while recording.
7. In quick-photo sessions a video shows among the photos with a play mark. Right-click offers Teeth and Delete; Rotate is for photos
   only.

## Files

New:
- `ImagingCapture/Uvc/Interop/MediaFoundation.cs`
- `ImagingCapture/Video/Mp4Writer.cs`
- `ImagingCapture/Video/VideoRecorder.cs`
- `ImagingCapture/Photo/VideoFiles.cs`

Changed:
- `IUvcBackend.cs` and `DirectShowFrameSource.cs` (FrameTap)
- `PhotoStore.cs` (ImportVideo)
- `PhotoQuarantine.cs` (videos are filed too)
- `PhotoSessionModel.cs` (AddVideo)
- `ControlCameraSession.cs`

No designer file, no schema and no new permission (ImageCreate).

## Tests

- `VideoRecordingTests` (6):
  - the timeline from stream times with a pause taken out;
  - the limit and its warning;
  - a slow encoder drops frames and never blocks;
  - a writer failure is reported;
  - bit rate and frame rate;
  - **a real MP4 written by Windows' encoder** from synthetic frames in a temporary folder: ftyp, avc1, moof; odd sizes made even. It is
    inconclusive where Windows has no H.264 encoder (an N edition).
- `PhotoQuarantineTests` (+2):
  - a recording's sidecar is written at the start;
  - a stale recording is recovered, though not while it is still being written;
  - a video is filed with its teeth and time;
  - the 2 GB rule.
- `CameraSessionWiringTests` (+1): R and P; the tap set and cleared; stopped before the camera goes, on refresh and on a camera error; the
  limit; no idle release while recording.
- `InteropLayoutTests`: 20 interfaces, and the 14 Media Foundation slots pinned.
- Gallery G-IMG8 adds the recording state. The audit renamed the button "Stop recording (R)", because "Stop" next to "End" was unclear.

Targeted x64 run:

| Suite | Passed | Total |
|---|---|---|
| ImagingCaptureTests | 839 | 839 |
| ImagingTests | 170 | 170 |
| WindowChecksTests | 30 (44 skipped) | 74 |
| NoHqTests | 127 | 127 |

UiRefreshTests fails the base set only. Not tried yet: recording from a real camera, which needs the clinic.
