# Port note: UI-6 (toolbars, menus and the module bar)

Package UI-6 of the UI refresh ([docs/specs/ui-refresh-plan.md](../specs/ui-refresh-plan.md), section 7's UI-6 row, sections 3.2, 3.3 and
3.6), on `ui/ui6-1` from `main` at `2ae1982f3`. Status: in progress.

## 1. Scope (from the plan)

- Owns: `ToolBarOD.cs`, `ODToolBarButton*.cs`, `WpfControlsOD/UI/Controls/ToolBar*.xaml(.cs)`, `MenuOD.cs` (`MenuRenderer`), the WPF `Menu`,
  `MenuItem` and `ContextMenu`, `ModuleBar.cs`, `ODButtonPanel.cs`, `ODPanelItem.cs`, `LightSignalGrid`, the body of `UiTheme.InitWinForms()`
  (the token renderer) and its call in every exe's `Main` (not `UpdateFileCopier` or `OpenPmsDentalHelper32`) and at every extra UI thread's
  start, and RdlDesign's linked files.
- Work: section 3.6's toolbars, menus and module bar. Item widths keep today's formula (3.3, UQ-9). No control moves; no bounds change.
- Tests: C20; every module toolbar and the main menu at 1024, 1366 and 1920 px; the renderer in every exe and on a second UI thread; picture
  tests of each state; the scan rerun.
- Gallery: G-UI2's toolbar part, each module toolbar and the menu bar as before and after strips.

The rest of this note is written as the package goes (stages: tests and before pictures, the renderer, the WinForms controls, the WPF
controls, the gallery).
