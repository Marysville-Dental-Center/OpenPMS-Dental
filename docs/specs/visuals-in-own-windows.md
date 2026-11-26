# Visuals in their own windows

**What this is.** Each screen that shows a visual (an image, a mount, the camera, an X-ray as it arrives, the tooth chart,
the perio chart) can be opened in its own ordinary window. The window has the usual minimise, maximise and close buttons,
so it can be dragged to any monitor and maximised there. In an operatory with two monitors in extended mode, that's how a
visual gets onto the monitor that faces the patient. Nothing else is involved: no monitor setting, no special "patient"
mode, and no separate button for it.

**Date.** 2026-11-26. **Status.** Built, not yet checked on a two-monitor computer (section 3).

**Owner's decisions, 2026-11-26.**
- No "Show to patient" button: an ordinary window with its own maximize button is enough, as long as it is a standalone window.
- No special design or flow for a patient monitor: use what Windows already provides.

This replaces the earlier proposal (patient monitor setting, screen manager, idle screen, blank key, a patient view that
copies the provider's screen), which the owner turned down.

## 1. The rule

A visual that a patient might want to see opens in a **standalone window**:
- **Not modal.** The rest of the program stays usable while it is open.
- **Minimise, maximise and close** in the title bar, and the window can be resized.
- **Shown in the taskbar**, or owned by the window it came from, so it can't get lost behind it.
- **The visual fills the window**, so maximising it on a large monitor makes the visual larger.

Windows already does the rest: dragging, Win+Shift+Left/Right to move a window to the other monitor, Win+Up to maximise.

## 2. Surfaces

| Visual | How it gets its own window | State before | Change |
|---|---|---|---|
| Imaging module: image or mount | The existing pop-out (`FormImageFloat`) | Already a standalone window with minimise and maximise | None |
| Imaging module: one image | Double-click opens the image viewer (`FormImageViewer`) | Already standalone; opens at normal size (owner, 2026-11-22), maximise available | None |
| Intraoral camera session | The session's full-screen toggle (F11, button) | A borderless window covering the monitor, with no title bar; minimising or maximising put the session back into the module | An ordinary window (`ControlImages.controlCameraSession_EventFullScreenToggle`). It has the Windows title bar, is resizable and is in the taskbar, and it opens maximised on the module's monitor. Its X or Alt+F4 puts the session back into the module; End ends the session, as before. |
| X-ray capture session | Double-click on the session's preview | The preview (260 x 230) existed only inside the session window, which is a fixed-size dialog | New `FrmCaptureImageWindow`: the preview's image, with the same rotation and flip, filling an ordinary window. It follows each new image and closes with the session. The session window keeps the keyboard, so the session keys still work. |
| Tooth chart (Chart module) | The big tooth chart (`FormToothChartingBig`) | Already standalone (`Show`), opens maximised | None |
| Graphical perio chart | Perio chart > Graphical (`FormPerioGraphical`) | Modal, and the chart had a fixed size | Opened with `Show(this)` (owned by the perio window, closes with it), and the chart is anchored on all four sides, so it grows with the window |

Not changed:
- **The perio chart's grid (`FormPerio`).** It stays a modal editor with a fixed layout; the graphical chart is its visual.
- **The treatment plan.** It is a grid of the plan, not a visual.

## 3. Verification

- **Source checks:** `tests/ImagingTests/CameraSessionWiringTests.cs` (the camera window is sizable, keeps minimise, maximise
  and close, and is in the taskbar).
- **Manual, on a two-monitor computer:**
  - for each row of section 2, open the window, drag it to the other monitor, maximise it, restore it and close it;
  - camera: F11 opens the window maximised; dragging it by the title bar to the other monitor while the preview runs; its X
    puts the session back into the module;
  - X-ray: double-click the preview, take two images, and check that the window follows the newest image, rotation and flip;
    closing the session closes it;
  - graphical perio: maximise it, and check that the chart grows and that Print and "Save to Images" still give a whole chart;
  - a monitor with another display scale.
