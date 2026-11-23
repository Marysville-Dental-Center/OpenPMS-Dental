# Port note: IMG-P4b-2 (camera session tools: freeze, retake, teeth, the shot menu, flash and click)

Why: the owner's imaging design, camera and video (2026-11-22), section 2. This package adds the tools of the session from IMG-P4b-1.
IMG-P4b-3 follows with F11 (whole monitor), the idle timeout and a quarantine for photos that cannot be saved.

## What changes

1. **Freeze (F, or the Freeze button).**
   - The picture on screen stops and gets a yellow border, and the guide starts with "FROZEN".
   - Capture saves the frozen picture, then the session goes back to live. F or Esc goes back to live without saving. Esc ends the session
     only when the picture is live.
   - A frozen picture is saved as the camera gave it. A mirror slot's flip is set on the document, as for a live shot.
   - The camera side is `VideoCaptureSession.BeginCaptureFrozen`. It uses the same capture gate, without needing the stream.
   - Design decision C2 makes "freeze first" a per-computer setting. That setting comes with the camera settings page (IMG-P4e); until
     then, Freeze is pressed by hand.
2. **Teeth (T, or the Teeth button)** opens the tooth picker of IMG-P2 for the next photo, offering the slot's teeth.
   - The teeth set apply to that slot only; moving to another slot drops them.
   - Quick photos keep the teeth until they are set again.
3. **Retake (Ctrl+Z for the last photo, or right-click a photo of this session).**
   - The X-ray capture session's Retake (design 7.11) is followed: the photo moves to a new unmounted item of the mount, described
     "Retake: Upper occlusal", with an ImageEdit entry. Its slot is empty again and is the next one.
   - Nothing is deleted.
   - It needs Image Delete, as the design says for Retake and Delete.
   - Open PMS has no image trash. Design decision C7 ("the old one goes to the image trash") is met by this unmounted copy, which the user
     can delete like any image. A real trash would be a separate feature.
4. **Right-click a photo of this session** (in the mount, or in the quick photos):
   - Retake (mount slots only).
   - Teeth...: ImageEdit, plus Signed Image Edit for a signed image. Logged as the module's Teeth.
   - Rotate right: a 90 degree display rotation, logged.
   - Delete...: asks first; Image Delete; `ImageStore.DeleteDocuments`. The slot is empty and next again.

   Photos already in the mount before the session are changed in the module as usual.
5. **Every shot is confirmed** by a 120 ms white flash over the live picture and a soft 30 ms click.
   - The click is made in code (`PhotoSounds.MakeClickWav`), so no sound file ships.
   - The click can be turned off with the camera settings later (IMG-P4e).

6. **Review fixes:**
   - A camera problem, a reconnect, or a change of the next slot ends a freeze, so a picture that is no longer on screen is never saved, and
     a mirror slot's flip always matches what was shown.
   - Ctrl+Z retakes only the photo just taken, once.
   - After deleting the last quick photo, the session ends on a photo that still exists.
   - A mount the session made is removed when all its photos were deleted again. Retakes stay in it.
   - The click plays from a background thread, so its sound data stays alive while it plays.
   - A missing log entry no longer reports a completed retake as failed.

## Files

- `ImagingCapture/Photo/PhotoSessionModel.cs`: the shots of the session, teeth for the next shot, and RemoveShot for a retake or delete.
- `ImagingCapture/Photo/PhotoStore.cs`: GetDocument, MoveToRetake (rolls back on failure), DeleteDocument, LogEdit.
- `ImagingCapture/Photo/PhotoSounds.cs`.
- `ImagingCapture/Video/VideoCaptureSession.cs`: BeginCaptureFrozen.
- `OpenPmsDental/User Controls/ControlCameraSession.cs` (built in code).

No designer file, no schema and no new permission.

## Tests

- `PhotoSessionModelTests` (+3):
  - the teeth of the next shot (a slot's and quick photos');
  - a retake empties the slot and makes it next;
  - the click is a valid short WAV.
- `PhotoSessionDbTests`: a retake moves the photo to an unmounted item of the mount, described and logged, and empties the slot; a
  delete removes the row.
- `CameraSessionWiringTests` (+1): the keys F, T and Ctrl+Z; Esc unfreezes before it ends; the permissions of Retake, Delete, Teeth and
  Rotate; a frozen capture and the flash.
- Gallery G-IMG6 (8 pictures; "before" is G-IMG5):
  - the new buttons, and a frozen picture with its yellow border.
  - The audit shortened the frozen note, which was cut off, and made room for the longer hint.

Targeted x64 run:

| Suite | Passed | Total |
|---|---|---|
| ImagingCaptureTests | 827 | 827 |
| ImagingTests | 168 | 168 |
| WindowChecksTests | 30 (44 skipped) | 74 |
| NoHqTests | 127 | 127 |

UiRefreshTests fails the base set only.
