# The WPF chart's x64 pictures (.NET Framework 4.8, 64-bit)

Package TC-10 of the WPF tooth chart track (`docs/specs/wpf-tooth-chart-plan.md`, the TC-10 row; `docs/port-notes/TC-10.md`). The image
tests run in the lane's x64 test build too. In a 64-bit test process a picture is compared with its copy here when there is one
(`WpfCharts.ForThisBitness`), otherwise with `../wpf-net48`. The tolerances are the same (section 7 of the plan). Only the pictures that the
64-bit process draws beyond those tolerances are here: 163 of the 326, recorded by `WpfImageTests` in a full x64 run of the assembly
(`OPMS_RECORD_WPF_CHART=1`, `OPMS_RECORD_WPF_CHART_OUT`), with the same `.txt` facts as the x86 set (`Framework: .NET Framework 4.8, x64`).

Why each group is here. Both groups have one cause: a 2D text item's cell is placed one pixel apart. The scene builder places it at
`Math.Ceiling` and `Math.Floor` of a single-precision projection (`ToothSceneBuilder.AddText`, `ToothScene.ProjectDirectX`); the 32-bit JIT
computes that on the x87 unit at double precision (control word 0x0009001F), the 64-bit JIT with SSE in single precision (MXCSR 0x0008001F,
the same on every thread), so a value that is a whole pixel in one process can be a hair beside it in the other. Measured in both bitnesses
on the same scene data (TC-10.md section 3):

- `410x307/` (142 of the 149 module-size scenes; the 7 others draw no Universal "5" there): the tooth number "5" starts at x 121 instead of 120. 31 pixels of its glyph
  differ; the worst 16x16 block has 31 of them, over the limit of 24. The other sizes put the "5" on the same pixel in both bitnesses.
- `649x696/` (15 perio scenes), `perio-print/` and `perio-save/` (3 each): the perio legend's second row ("Suppuration", "Clinical
  Attachment Level", "Furcation 2") starts at y 648 instead of 649. 1380 pixels differ in the chart (2862 in the print and the saved image).

`restored-adult` and `treatment-plan` at 410x307 are also the exact bytes that the determinism and gallery tests compare in a 64-bit
process. Re-record a picture here whenever its x86 picture is re-recorded for a drawing change; a picture that later draws within tolerance
of the x86 one in a 64-bit process can be deleted. At stage 3b S3b-5 records `wpf-net10-x64` and this set retires with .NET Framework.
