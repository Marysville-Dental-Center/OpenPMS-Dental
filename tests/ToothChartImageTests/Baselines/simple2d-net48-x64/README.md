# The Simple 2D chart's x64 pictures (.NET Framework 4.8, 64-bit)

Package TC-10 of the WPF tooth chart track (`docs/specs/wpf-tooth-chart-plan.md`, the TC-10 row; `docs/port-notes/TC-10.md`). In a 64-bit
test process `Simple2DImageTests` compares a picture with its copy here when there is one (`WpfCharts.ForThisBitness`), otherwise with
`../simple2d-net48`, with the same tolerances (section 7 of the plan). A recording in a 64-bit process (`OPMS_RECORD_2D_CHART=1`) writes
every picture here, never into the x86 set; keep only the pictures that differ beyond tolerance.

Here: the 18 of the 35 pictures that the 64-bit process draws beyond tolerance, recorded in a full x64 run of the assembly. In each, the
tooth numbers that GDI+ draws in their white boxes (`ToothChart2D`) have other pixels: 2916 to 4175 pixels differ by more than 8 of 255
(the limit is 0.1%, 185 pixels at 500x370), up to 146 in one 16x16 block; the glyphs keep their places and their size, and their
anti-aliasing (sub-pixel colour fringes) differs. The other 17 pictures, the 410x307 ones among them, are within tolerance of the x86 set.

Re-record a picture here whenever its x86 picture is re-recorded. At stage 3b S3b-5 records the .NET 10 x64 set and this one retires.
