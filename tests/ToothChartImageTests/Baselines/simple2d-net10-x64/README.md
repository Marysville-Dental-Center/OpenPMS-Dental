# The Simple 2D chart's pictures on .NET 10 x64

Package S3b-5W (`docs/port-notes/S3b-5W.md`). Every picture of `../simple2d-net48` (35), recorded in a 64-bit .NET 10 process by
`Simple2DImageTests` (`OPMS_RECORD_2D_CHART=1`; on .NET x64 it records and compares here), with the same `.txt` facts. The test assembly
sets the program's default font first (`TestAssemblySetup`: Microsoft Sans Serif 8.25, as every exe's `Main` does since S3b-4), because
the Simple 2D chart draws its tooth numbers and texts with the control's `Font`, which .NET would make Segoe UI 9.

**Against .NET Framework 4.8 x64** (gallery G5): 34 identical (the 18 of `../simple2d-net48-x64` and 16 of the 32-bit set); `926x858/
text-all-kinds` is within tolerance of its 32-bit picture (140,733 pixels a little apart, no channel by more than 8 of 255: the 64-bit
GDI+ scaling of the background, TC-10). None is beyond tolerance.
