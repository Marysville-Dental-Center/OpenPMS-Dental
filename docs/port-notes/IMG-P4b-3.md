# Port note: IMG-P4b-3 (camera session: F11 for the whole monitor, idle release, unsaved photos)

Why: the owner's imaging design, camera and video (2026-11-22), sections 2 and 6. This package finishes the photo session; video
recording (IMG-P4c) comes next.

## What changes

1. **F11 puts the session on the whole monitor** the module is on, and F11 again puts it back into the module.
   - This is decision C1 of the approved camera design ("Inside the Imaging module, with F11 for the whole monitor").
   - It is a borderless window owned by the main window, sized to the monitor's bounds. It is not maximised, not topmost, and not in the
     taskbar.
   - Alt+F4 returns the session to the module. End or Esc ends the session from either place, and the window closes with it.
   - The image viewer's decisions D1/D2 (never full screen, never maximised) are unchanged; they cover the image viewer only.
   - A second monitor (C3) remains a later option.
2. **Idle release** (design section 2, "Session timeout"):
   - After 10 minutes with no photo, key or click, the camera is released and its light goes off. The live picture says so, with a
     **Wake** button.
   - Capture, Space or Wake opens the camera again; that first press takes no photo.
   - The session and its patient stay.
   - The camera's own button cannot wake it, because the camera is closed.
   - The minutes become a setting with IMG-P4e.
3. **A photo that cannot be saved is kept on this computer** (design section 6, "Photo could not be saved"):
   - Where: `ImagingCapture/Photo/PhotoQuarantine.cs` writes it as a JPEG with a text sidecar to
     %ProgramData%\OpenPMS-Dental\Imaging\Photos\unsaved, through `LocalCaptureConfig.EnsureImagingFolder`, which sets up the same
     protected Imaging folder that holds capture.ini and the X-ray capture quarantine.
   - The sidecar records the patient (PatNum and name), the mount, slot, flip, rotation, teeth, category, time, computer and reason.
   - A red notice over the live picture says so and stays until the next photo is saved. The slot stays empty and next.
   - **Camera, Unsaved photos (N)...** appears when any are waiting. It lists the patients and, after OK, files each photo to the patient it
     was taken for: into its slot when the slot is still empty, else unmounted, flipped, rotated and with its teeth. The files are then
     removed. A photo that cannot be filed stays, and the reason is reported. It needs ImageCreate.
   - The X-ray capture quarantine and its recovery window are not changed: they read radiographs only. Photos have their own folder.

4. **Review fixes:**
   - A mount the session made stays when photos were kept unsaved, so they file into its slots.
   - Filing puts a photo into a slot only when the mount belongs to the same patient, because a deleted mount's number can be given out
     again.
   - A filed photo keeps the time it was taken.
   - A photo filed but not placed is reported.
   - The full-screen window can't be minimised or maximised; if Windows does it anyway, the session goes back into the module.
   - The idle release counts the toolbar buttons as activity, and never fires while a window (the tooth picker, a question) is over the
     session.
   - Not changed: the unsaved-photo folder is readable by every Windows user of the computer, like the X-ray capture quarantine; and a
     photo whose patient was deleted cannot be filed and stays listed.

## Files

- `ImagingCapture/Photo/PhotoQuarantine.cs` (new).
- `OpenPmsDental/User Controls/ControlCameraSession.cs`: F11 event, idle timer and Sleep, KeepUnsaved, the red notice.
- `OpenPmsDental/Main Modules/ControlImages.cs`: the full-screen window, and the Unsaved photos menu item.

No designer file, no schema and no new permission.

## Tests

- `tests/ImagingCaptureTests/PhotoQuarantineTests.cs` (3, in a temporary folder):
  - the photo and its sidecar written and listed back; stray files ignored;
  - filing into the empty slot, then the files go;
  - a slot taken meanwhile means unmounted; a failed save keeps the files; an unreadable file is reported.
- `CameraSessionWiringTests` (+1):
  - F11; the idle timer and Sleep; Capture wakes;
  - a failed save is kept;
  - the full-screen window is borderless at the monitor's bounds and never maximised, and it is closed when the session ends;
  - the menu item and its permission.
- Gallery G-IMG7 adds the released camera and the unsaved-photo notice. The audit moved that notice out of the one-line note, where it
  was cut off, into a red box over the live picture.

Targeted x64 run:

| Suite | Passed | Total |
|---|---|---|
| ImagingCaptureTests | 830 | 830 |
| ImagingTests | 169 | 169 |
| WindowChecksTests | 30 (44 skipped) | 74 |
| NoHqTests | 127 | 127 |

UiRefreshTests fails the base set only.
