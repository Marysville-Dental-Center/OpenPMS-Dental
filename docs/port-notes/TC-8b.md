# Port note: TC-8b (File > Graphics as Open Dental 26.2 has it)

Package TC-8b of the WPF tooth chart track ([docs/specs/wpf-tooth-chart-plan.md](../specs/wpf-tooth-chart-plan.md), section 10, "Open Dental
parity (2026-11-14)", the TC-8 table: rows ENG-1, ENG-2, ENG-3, ENG-5, NEW-1, and ENG-6 from ENG-2's caveat): branch `chart/tc8b-1` from `main`
(`3d910a435`, TC-8 and TC-9a merged). **Status: in progress (design first).**

## The design (written before the code)

- **The owner's decision of 2026-11-15:** "Match Open Dental 26.2" for File > Graphics (`docs/owner-questions.md`, the TC-8 entry; the plan's
  O-30 note). It revises O-30, O-31 and O-34 (b1); their earlier answers stay in the plan as history.
- **ENG-2, the choice:** Open Dental's own rows, no schema change. The preference `DirectX11ToothChartUseIfAvail` (global, default 1, read as
  Open Dental 24.3 read it at start-up: `PrefC.GetBoolSilent(...,true)`) and `computerpref.GraphicsUseDirectX11` (Unknown = the global, Yes,
  No). The computer uses the 3D tooth chart (the WPF chart, in DirectX 11's place) when its own setting is Yes, or Unknown and the global setting
  is on. Otherwise Open Dental's older technology applies (`GraphicsSimple`); of DirectX 9, Simple 2D and OpenGL only Simple 2D is left after
  TC-9a, so such a computer draws Simple 2D whatever is stored.
- **The save rule:** only what a choice changes. The global setting through `Prefs.UpdateBool` and a preference signal; `GraphicsUseDirectX11`
  when the per-computer choice changed; `GraphicsSimple` by TC-8's rule only when the choices change the chart the computer draws (1 when it
  turns to Simple 2D, 0 over a stored 1 when it turns to the 3D chart, a stored 0 or 2 kept), so a return to Open Dental reads the same.
- **ENG-3:** the smoothed picture is the on-screen default (an empty `DirectXFormat`); "Draw in software" unticked stores `WpfLiveView`, the
  quick live view.
- **ENG-5:** graphical perio opens only when the computer uses the 3D chart; otherwise Open Dental's 24.3 sentence with the 3D tooth chart in
  DirectX's place and File > Graphics named.
- **NEW-1:** the status on this computer, "Available" or "Fell back: <reason>" (the WPF chart's start check).
- **ENG-6:** Setup > Computers' "Use Simple Graphics" also writes "Do not use" (od-unknown; the manual's purpose).
- **ENG-1:** File > Graphics on Open Dental's layout (its screenshot `https://www.opendental.com/manual/images/graphics.png`): the global group,
  the per-computer group, the status group, "Older Tooth Chart Technology" with Simple 2D, the options group with "Draw in software", the live
  preview, the medical wording of Open Dental's own Load.
