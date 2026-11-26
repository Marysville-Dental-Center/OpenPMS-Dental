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

## Camera buttons: settings and program actions

Learn found only the Capture key. A handpiece may have more keys; Learn now has an optional step 4 of 6, **Other buttons**. A button either changes a camera setting or runs a program action (owner, 2026-11-26: "buttons are ways to toggle a control, or to trigger a software action or workflow; leave it flexible"). Pick what the next key does ("Setting: turn on/off", "Setting: raise", "Setting: lower", "Setting: set to a value", or "Program: take a photo", "next slot", "previous slot", "retake the last photo", and the other program actions below), click Record and press that key three times; again for each key, any number per action ("Forget last" drops the latest). The key-capture rules apply, on the bits not used by Capture, another button or Baseline B, so a key in use is never learned twice (pressing one says so). A setting action also needs the setting, picked by name from those the camera reports, and optionally its highest value, or for "set to" the value (`Auto`, `60 Hz`, `1/64 s`); Learn never writes to the camera to find it. With a setting button and a level input, the level step offers "Also turn [setting] off while resting". Skip forgets them; a new baseline or key recording does too. The level step never offers a button's bits. The test page counts each button's presses; a press must not raise the photo count, except a "take photo" button, which takes one through the Capture key's lockout.

Each learned key is a `[Level.N]` after the level input (`Control=Roll`, `Bits=5`, `ActiveLevel=Low`) with a press action (`LevelActions`):

- **Setting actions** on `Setting=<name>`: `Toggle` (to the lowest value, or back to the last on value; the first "on" is the highest value), `Raise`/`Lower` (one step; `Steps`, five by default, from the lowest value to `Highest`, else the camera's maximum; raising an off setting turns it on at the lowest step) and `SetValue` (`Value=`). `Highest` also caps the setting once as the triggers start. `CameraSettingActions` does this on top of the one write path; a change lasts until the camera closes and is written to the program log.
- **Held actions**: `PausePreview`, and `TurnOff` with `Setting=` (off while the level is active, back on when it ends), combined as `Action=PausePreview|TurnOff` for a rest sensor.
- **Program actions**: `TakePhoto` goes through `TriggerEngine`'s debounce gate as a press (`TriggerSource.Button`); the others go to the camera session (`CameraButtonAction`, `ControlCameraSession.RunCommand`), which does exactly what the key does (Esc's order kept: end session first unfreezes a frozen picture). The Video window ignores them.

A press action acts once on the active edge; `TriggerEngine` ignores a key held when polling starts and a second edge within DebounceMs.

The light keys of 2026-11-25 (`ToggleLight`, `LightUp`, `LightDown`, `LightProperty`, `LightMax`, `LightOffWhileActive`) were replaced by these before any computer used them, without a compatibility layer.

## Camera settings

Owner decision 2026-11-26 (design decision 13, section 8.7): Open PMS can change every picture setting the camera's driver offers, shown readable. "Picture settings..." in the camera settings (Setup) opens **Camera Picture Settings**: each setting the open camera offers (`CameraSettingCatalog`: Brightness, Contrast, Hue, Saturation, Sharpness, Gamma, White balance, Backlight compensation, Gain, Color, Power line frequency, Exposure, Low light compensation, Iris, Focus, Zoom, Pan, Tilt, Roll) with its value, the camera's default, its range and the saved value. Values read as people write them: `1/64 s`, `f/2.8`, `4600 K`, `60 Hz`, `On`, `Auto`. A change goes to the camera at once; Save writes `[Camera.Settings]` (`Gain=20`, `Exposure=1/64 s`) and a Setup securitylog entry with old and new values; Close without saving puts the found values back. A setting the buttons are read from is read-only. Power line frequency and low light compensation go through `IKsControl` (`KsControlProperty`); the others through `IAMVideoProcAmp`/`IAMCameraControl`. `[Camera.Settings]` is applied at each open, before the triggers start, because cameras may forget their settings when unplugged; each write and each refusal is logged.

`CameraSettingWriter.Write` is the only `IFrameSource.WriteControl` caller (`CameraWriteGuardTests`). Camera picture settings are a review flag for counsel (decision 18).

## No hex on screen

Bindings read "Roll, bit 3, pressed reads 0" instead of a hex mask; capture.ini writes `Control=` and `Bits=` (older `Interface=`/`Property=`/`Mask=` files are still read and rewritten when the binding changes). Learn's raw-values grid shows "Bits on" (for example `0 1 2 4 5`) instead of hex and binary columns.

## Capture settings: capability names

Capability rows show the capability's name (since 2026-11-26 the name alone, no number) instead of the bare number, and the value box accepts the name (stored as the number, so capture.ini, its readers and the gated settings hash are unchanged). A capability without a name keeps its number. The names are not in the code (VendorNeutralityTests): each computer's capture.ini lists the names its TWAIN source's contract gives, `NAME=0xNNNN` under `[CapabilityNames]`. `ImagingCapture/Settings/CaptureCapabilityNames.cs` reads them; without them the numbers show as before, with a hint.

## Installer

The connection page's default database is `openpms_dental` (was `opendental`); `/DBNAME=` still overrides it, and an upgrade never passes the default. `NoHqTests/Data/brand-kept.tsv` loses the three rows of the old default.

## Not verified

No run against hardware or in the running program: the flicker, the camera buttons and settings (including `IKsControl` against a real driver, and whether a camera keeps its settings after unplugging), the session retake and drag, and a real crash report (the tests write one from a test exception) need a check on a real installation.
