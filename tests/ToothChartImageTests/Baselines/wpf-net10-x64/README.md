# The WPF chart's pictures on .NET 10 x64

Package S3b-5W of stage 3b (`docs/port-notes/S3b-5W.md`; the S3b-5 item of `docs/specs/wpf-tooth-chart-plan.md`'s 3b section). From
stage 3b the program and every test run on .NET 10, 64-bit, and .NET draws WPF with another render DLL (wpfgfx_cor3, not
wpfgfx_v0400), so the image tests compare with this set: every picture of `../wpf-net48` (326), recorded in a 64-bit .NET 10 process by
`WpfImageTests` (`OPMS_RECORD_WPF_CHART=1`; on .NET x64 it writes here), with the same `.txt` facts (`Framework: .NET 10.0, x64`) and a
`manifest.txt`. In a 64-bit .NET process `WpfCharts.ForThisBitness` always gives this set's picture (a missing one fails as "no
baseline"); on .NET Framework nothing changed (`../wpf-net48`, and `../wpf-net48-x64` in a 64-bit process). The tolerances are the same
(plan section 7). The .NET Framework sets stay as they are.

Every picture was repeatable when it was recorded (a second `GetBitmap` and a fresh render of both layers gave the same pixels), and two
recordings in two processes gave the same PNG files byte for byte.

**Against .NET Framework 4.8 x64** (gallery G5; `RuntimeBaselineTests.WriteG5Gallery`, with `ChartImageCompare`):

- The 163 pictures that have a .NET Framework x64 copy (`../wpf-net48-x64`): 157 identical. `410x307/shiftm-plus` (3 pixels),
  `410x307/shifto-unerupted` and `410x307/missing-many` (1 pixel each) differ in single 3D edge pixels (the new render DLL). The three
  `perio-save` pictures differ in 789 pixels, 36 of them beyond the channel tolerance: the scaled chart's outermost pixel column (x 0)
  and its top row (y 118, 255 to 254) in the 750x1000 page are blended differently when GDI+ draws the chart picture into the page; the
  print (`perio-print`) is identical.
- The 163 pictures that have only the 32-bit picture: 2 identical, 161 within tolerance (at most 24 pixels changed, 6 beyond the channel
  tolerance, 6 in one block): the 64-bit differences TC-10 measured on .NET Framework (`../wpf-net48-x64/README.md`).

None is beyond tolerance. The one-pixel text cells of TC-10 (the tooth number "5" at 410x307, the perio legend's second row) are the
64-bit state here, as on .NET Framework x64.
