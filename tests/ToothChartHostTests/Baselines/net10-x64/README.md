# The host pictures on .NET 10 x64

Package S3b-5W (`docs/port-notes/S3b-5W.md`). The host tests ran in 32-bit on .NET Framework 4.8 until S3b-5, so their pictures one
folder up are 32-bit pictures, and they stay there. On .NET 10 x64 the tests compare with this set (`HostPaths.Baselines`): every host
picture (9), recorded by `HostPictures.Check` with `OPMS_RECORD_HOST_BASELINES=1` on a scratch database with synthetic patients. The test
assembly sets the program's default font first, as every exe's `Main` does since S3b-4.

**Against the 32-bit .NET Framework pictures** (gallery G5; no .NET Framework x64 picture of a host exists, because the program was never
built for x64 on .NET Framework): five within tolerance (`big-chart`, `referral-letter`, `sheet`, `treatplan-sheet`,
`sheet-editor-preview`: at most 21 pixels), four beyond it, each one of the 64-bit differences TC-10 measured on .NET Framework x64
(`docs/port-notes/TC-10.md` section 3), which the image tests' x64 pictures show as identical between .NET Framework x64 and .NET 10 x64:

- `perio-chart` (1380 pixels) and `perio-save` (2585 beyond tolerance; 108,379 a little apart in the scaled page): the perio legend's
  second row one pixel higher.
- `dashboard` (31 pixels): the tooth number "5" at 410x307 one pixel to the right.
- `sheet-simple2d` (2941 beyond tolerance, 130 in one block): the Simple 2D chart's 64-bit GDI+ drawing (anti-aliasing of the tooth
  numbers and the scaled teeth).
