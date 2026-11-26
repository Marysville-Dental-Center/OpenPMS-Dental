# Port note: FB-1, user feedback fixes (2026-11-25)

Changes tagged `FB-1` in comments. Not committed. Nothing here names a device: device values live in the device's own
documentation and in each computer's capture.ini.

## Hover flicker and the tooltip crash

- `CodeBase/UI/UiInputFrame.cs`: the frame over stock inputs is painted into a bitmap and blitted once, instead of filling the parent color and then drawing the border on the window DC (Windows 11 hover animation repainted it 4-5 times, each a visible flash). Face-drawn drop-down lists validate their client area so Windows' own hover face never shows.
- `CodeBase/UI/UiScrollLook.cs`: scroll bars drawn the same way (`PaintBarsBuffered`).
- `WpfControlsOD/UI/Controls/ToolTip.xaml.cs`: shown without activation (it called `Activate()` on every mouse move); guarded against "This Visual is not connected to a PresentationSource" in `control_MouseLeave` and `SetVisible`. That exception ended the program when a window closed or a panel was rebuilt under the mouse: `MouseLeave` fires as the control leaves its window, and `PointToScreen` then throws.

## Crash reports

Before, an exception in a WPF control's event on the UI thread (the tooltip crash) went to WPF's dispatcher, which nothing handled: it left the message loop and ended the process with no message and only Windows' event log entry. The handled paths (Application.ThreadException, ODThread) logged one line of exception text.

- `CodeBase/Utilities/CrashReport.cs`: one text file per unhandled exception in `%LOCALAPPDATA%\OpenPMS-Dental\CrashReports\crash-yyyyMMdd-HHmmss-fff-pid.txt` (the newest 50 are kept; a line naming it goes to the Logger's `Crash` folder). It holds the handler and whether the process ends; the thread; the program (path, file and product version, build, bitness, argument names only); the computer (Windows, .NET, user, processors, remote session, screens); the process (uptime, working set, private and managed memory, GC counts, handles, threads, GDI and USER objects); the program's state (module, PatNum, UserNum, server and database, active and open window types); the last 60 actions (module changes, windows opened and closed, patient selected, by PatNum); and every exception of the chain (type, message, HResult, source, method, Data, stack; AggregateException's inner ones too). No patient name, no password, no argument value. Never throws; the same exception from two handlers gives one report.
- `ProgramEntry`: `AppDomain.CurrentDomain.UnhandledException` (exceptions that end the process), `TaskScheduler.UnobservedTaskException` (report only), and the UI thread's `Dispatcher.UnhandledException`, which now takes the WinForms path: crash report, the error window (which names the report's file), then the program closes. `CrashReport.FuncAppState` gives the state lines.
- Notes: `FormODBase` and `FrmODBase` (window opened and closed), `FormOpenDental` (module, patient).
- Tests: `tests/S0FTests/CrashReportTests.cs`.

## Retake

- Capture session: any filled slot can be tapped to retake it (blue border, "Retake slot N"); a second tap clears the choice. `CaptureSessionSetup.RetakeMountItemNum` opens a session with a slot already chosen; the old image is moved to an unmounted item only at Start, so closing first changes nothing. A full mount may be opened for a retake.
- Imaging module: the unmounted bar's Retake and a new "Retake X-ray..." on a slot's right-click menu open the capture session on that slot (`ControlImages.RetakeInMount`). Photo mounts are refused (they are retaken in the camera session).
- View (Pan) mode: an image can be dragged to another slot (move, or swap with the image there), with the same permission and signed-image checks as Adj mode.

## Mount type

A mount template holds X-rays or photos, never both. "Mount holds" (X-rays / Photos) is chosen in Generate (upfront for a new mount) and in the mount editor; every slot follows it (`MountItemDefGuides.SetMountType`), a new slot takes it, and the slot editor offers only intraoral/extraoral for a photo mount and no kind for an X-ray mount. A template that already mixes them opens as Photos with a notice and is made uniform on Save. Patient mounts already made keep their guides.

## Camera buttons: light on/off, brighter, dimmer, next and previous slot

Learn found only the Capture key. A handpiece may have more keys; Learn now has an optional step 4 of 6, **Other buttons**: pick what a key does (Light on/off, Light brighter, Light dimmer, Next slot, Previous slot), click Record and press that key three times; again for each key. The key-capture rules apply, on the bits not used by Capture, another button or Baseline B, so a key in use is never learned twice (pressing one says so). The light actions also need the VideoProcAmp property of the light, picked from those the camera reports ("Light setting"), and optionally its highest value ("Highest level"); Learn never writes to the camera to find it. With a light button and a level input, the level step offers "Turn the light off while resting". Skip forgets them; a new baseline or key recording does too. The level step never offers a button's bits. The test page counts each button's presses, and a press must not raise the photo count.

Each learned key is a `[Level.N]` after the level input, with a press action: `ToggleLight`, `LightUp`, `LightDown` (each + `LightProperty`), `NextSlot`, `PreviousSlot` (`LevelActions`). A press action acts once on the active edge; `TriggerEngine` ignores a key held when polling starts and a second edge within DebounceMs. The camera session (`ControlCameraSession`) moves to the next or previous slot as the arrow keys do; the Video window ignores them. `ToggleLight` (in `VideoCaptureSession`): on a press the property goes to its minimum, or back to the value it had (its default the first time), never above `LightMax`. `LightUp`/`LightDown`: one step brighter or dimmer, a fifth of the way from the minimum to the highest value (`VideoCaptureSession.LightStepCount`); brighter turns an off light on at the lowest step, dimmer stops there, and on/off and the holder bring back the brightness last chosen. `LightMax` (on the light levels): when the triggers start with the property above it, it is set to `LightMax`. `LightOffWhileActive=1` (on a held level, such as the handpiece's rest sensor): the light is off while the level is active and comes back on when it ends.

Design rule 8.7 ("never write camera controls") is narrowed to this one write path: `DirectShowFrameSource.WriteLightControl` is the only `IAMVideoProcAmp.Set` call and `VideoCaptureSession.WriteLight` its only caller (`CameraWriteGuardTests`). It is a review flag that needs the owner's sign-off (decision 13), and the new key mechanism falls under decision 10's provenance sign-off. `docs/imaging-capture-design.md` 8.5 to 8.7 are updated.

## Capture settings: capability names

Capability rows show `NAME (0x8005)` instead of the bare number, and the value box accepts the name (stored as the hex number, so capture.ini, its readers and the gated settings hash are unchanged). The names are not in the code (VendorNeutralityTests): each computer's capture.ini lists the names its TWAIN source's contract gives, `NAME=0xNNNN` under `[CapabilityNames]`. `ImagingCapture/Settings/CaptureCapabilityNames.cs` reads them; without them the numbers show as before, with a hint.

## Installer

The connection page's default database is `openpms_dental` (was `opendental`); `/DBNAME=` still overrides it, and an upgrade never passes the default. `NoHqTests/Data/brand-kept.tsv` loses the three rows of the old default.

## Not verified

No run against hardware or in the running program: the flicker, the camera buttons and light, the session retake and drag, and a real crash report (the tests write one from a test exception) need a check on a real installation.
