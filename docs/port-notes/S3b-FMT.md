# Port note: S3b-FMT (numbers and fractional times as .NET Framework produced them)

Package S3b-FMT of stage 3b of the .NET 10 move (`docs/specs/net10-plan.md`, decision 24: "Keep Open Dental's 1.01", owner 2026-11-16),
on `net10/s3bfmt-1` from `net10/switch` at `72d612a60`, with the review fixes on `net10/s3bfmt-2` (the x86 record at 15 digits: sections
2, 3 and 3a). Every place of the shipped code that writes a `double` or `float` as text, and every
`DateTime`/`DateTimeOffset.AddX` with an argument that is not a whole number, goes through one helper that gives the .NET Framework's text
and result; a scan with the compiler's types finds the places, a mechanical rewrite routed them, and a guard test fails on a new one.

## 1. Summary

- **Truth table:** `tests/FormatParity/net48-truth.txt`, recorded on the x86 .NET Framework 4.8 (the runtime Open Dental ran on), with the
  x64 runtime's differing text kept: 99,424 number rows and 2,655 time rows (section 2). The two runtimes differ at 15 digits too, for
  values about half way at the 16th digit (736331.0849999995 `F2`: 736331.09 on x86, 736331.08 on x64); the helper gives the x86 text
  except for 5 values of the record (section 3a).
- **Helper:** `CodeBase/Utilities/ODNumberTextNetFx.cs` (`ToStringNetFx`, `AsNetFx`, `ToStringNetFxIfNumber`, `AsNetFxIfNumber`) and
  `CodeBase/Utilities/ODDateTimeAdd.cs` (`AddDaysNetFx` .. `AddMillisecondsNetFx`); S3a-3's `ToStringNoNegativeZero`, `ToStringG15` and
  `ToStringG7` now call it (section 3).
- **Scan and sweep:** `tools/FormatSweep` (Roslyn 5.12.0) over 22 shipped projects; 1,610 places routed in 341 files (section 5); the
  framework places measured and fixed at the narrowest point (section 6).
- **Guard:** `tests/FormatParityTests` (20 tests): every truth-table row, the decision-24 pins, the framework places, the scan and rewrite on a
  fixture, and the scan of the shipped projects with 0 unrouted places (section 8).
- **The two waiting failures pass:** GoldenOutputTests 28/28 (the six SQL, statement PDF and CSV texts) and BINBTests
  `Fixture_G15AndG7Texts` (a value exactly half way between two 15-digit numbers, `-650217936677028.5`: .NET's "G15" rounds it to the even
  digit, the .NET Framework away from zero).
- Full filter x64: 0 errors. Targeted x64 runs of 120 test assemblies, all passing (section 9).

## 2. The truth table and how it was made

`tests/FormatParity/Net48Probe/Net48Probe.cs` is a C# 5 console program, outside every project. `tests/FormatParity/Make-Net48Truth.ps1`
compiles it with `csc.exe` of `%WINDIR%\Microsoft.NET\Framework\v4.0.30319` twice, as x86 (the .NET Framework program was x86: the record)
and as x64, runs both on .NET Framework 4.8 (4.0.30319.42000) with `tests/FormatParity/formats.txt`, compares them row by row and writes
the x86 output, plus the x64 rows that differ as `X` rows, to `net48-truth.txt`. The compiled probes stay in `%TEMP%`.

- **Values:** 99 fixed doubles (the half steps 0.005, 1.005, 2.675, 1.115, 10.075, 1234567.125, 99.995; -0.0, -0.001, -0.004, -0.005;
  0.1+0.2, 1/3, 2/3; 1e15..1e29, 1e-4..1e-20, Epsilon, MaxValue, NaN and the infinities; the exact 16-digit half 650217936677028.5; computed
  balances such as 42.11-646.06) and 283 seeded random ones (cents, half cents, sums, differences and shares of amounts, every size); 30
  fixed floats and 65 random ones. Fixed values in the invariant culture and en-US, random ones in the invariant culture.
- **Near-half bands (`AddNearHalfBands`, s3bfmt-2):** 330 more seeded doubles where the x86 and x64 runtimes can differ at 15 digits: half
  cents and half tenths 1 to 8 ulps off the half (`bits±k`), 15 significant digits followed by a 5, one ulp off, from 1E-4 to 1E15, shares
  (`a*0.15`) and sevenths (`a/7`) of amounts; and of each kind 20 to 40 candidates chosen because their exact digits 16 to 19 lie in
  4940..5009 (BigInteger arithmetic, the same on both runtimes; the script checks that both probes made the same rows). Before them every
  half cent was the nearest double to the half, `(cents*10+5)/1000.0`, and no row lay where the runtimes differ at 15 digits.
- **Formats (104):** none (`ToString(provider)`), `null`, `""`, every numeric format the scan found in the code before the sweep (`c`, `C`,
  `c0`, `c2`, `C2`, `n`, `N`, `n0`..`n2`, `N2`, `f`, `F`, `f0`..`f3`, `F0`..`F2`, `G7`, `G9`, `G15`, `G17`, `R`, `P`, `p`, `0`, `0.0`,
  `0.00`, `0.000`, `0.#`, `0.##`, `0.###`, `0.######`, `0.##########`, `#.##`, `+0.000;-0.000;0.000`, the RDL reports' `#,##0.00`, `0.00`,
  `c`, the composite `{0:0.00}`, `{0:#,##0.##}` ...), and the standard ones around them (`E`, `E2`..`E16`, `G2`..`G17`, `P0`..`P2`, `F4`..`F10`,
  sections, percent, exponents, escapes, `D` and `X`).
- **Times:** `DateTime(2026,11,16,8,0,0).AddDays .. AddMilliseconds`, the same on a `DateTimeOffset`, and `TimeSpan.FromDays .. FromMilliseconds`,
  for 175 arguments (thirds, tenths, half milliseconds of each unit, values just below and above a half, NaN, the infinities, values out of
  range): the ticks, or the exception's type.

Regenerate with `powershell -NoProfile -ExecutionPolicy Bypass -File tests\FormatParity\Make-Net48Truth.ps1` (add a format to `formats.txt`
first; `//` starts a comment there, since a format may start with `#`).

**What the measurements showed** (each of the plan's four differences confirmed, two of them narrower than assumed):

| | .NET 10 (UseNls) | .NET Framework 4.8 |
|---|---|---|
| (a) standard formats `F`, `N`, `C`, `P`, `E` | the exact binary value: 1.005 `F2` is 1.00, 99.995 `F2` 99.99 | 15 significant digits first: 1.01, 100.00 |
| (a) custom formats (`0.00`, `#,##0.00`) and `G15` | already 15 digits (1.005 `0.00` is 1.01 on .NET too), but an exact 16-digit half rounds to even: 650217936677028.5 `G15` is ...028 | away from zero: ...029 |
| (b) a negative value that rounds to zero | `-0.00`, `($0.00)`, `-0` | `0.00`, `$0.00`, `0` |
| (c) no format | shortest round-trip text: 0.30000000000000004 | `G15`: 0.3 (a float `G7`) |
| (d) `AddHours`, `AddMinutes`, `AddSeconds`, `AddMilliseconds`, `AddDays` | every tick: `AddMilliseconds(0.5)` adds 5,000 ticks | rounded to the millisecond, half away from zero: 10,000 |

Also measured: NaN and the infinities are the culture's symbols for any format on the .NET Framework (even `D`, which .NET refuses for
`-Infinity`); `G16`, `G17` and `E15`+ use 17 digits there (9 for a float's `G8`+ and `E7`+), whose 17th digit the x86 runtime does not
always get correctly rounded (0.0001 `G17` is 0.00010000000000000001 on x86, 0.0001 on x64); `R` is 15 digits when they round-trip, else
17, where .NET writes the shortest text. **x86 and x64 also differ at 15 digits** (every format but the 17-digit ones, no format included):
x64 rounds the exact value to 15 digits, x86 rounds digits it computed with about 17 digits of accuracy, so for a value whose digits after
the 15th read about 0.5 the two differ: 0x4123E3EB2B851EB4 with `F2` is 651765.59 on x86 and 651765.58 on x64, 0x412678962B851EB4
(736331.0849999995, measured in scratch) 736331.09 and 736331.08; without a format 0x4123E3EB2B851EB4 is 651765.585 on x86 and
651765.584999999 on x64, 0x412D2C7291EB8523 955961.285000001 and 955961.285. The record has 1,168 `X` rows:
1,126 of them 15-digit ones (74 values; 10 of them in each money format, `F2`, `N2`, `C`, `0.00`), 42 the 17-digit ones. Floats: x86 and
x64 agree (600,000 seeded floats near 7-digit halves and half cents, 4.2M rows, no difference). `(decimal)double` gives the same decimal on both runtimes (20,000 seeded values compared, none differed), so
the code's `(decimal)` conversions needed nothing. 8,147 of the double and float rows differ between .NET 10's own formatter and the record;
1,546 of the time rows.

## 3. The helper

`ODNumberText` (partial: `ODNumberText.cs` holds S3a-3's parse rules, `ODNumberTextNetFx.cs` the formatter):

- `value.ToStringNetFx()`, `(format)`, `(provider)`, `(format,provider)` for `double`, `float`, and `ToStringNetFx()` for `double?`/`float?`
  ("" for null).
- `value.AsNetFx()`: a struct `NetFxNumber : IFormattable` for a composite format's argument or an interpolated string's hole (the hole's
  alignment and format and the composite's culture reach the helper); `AsNetFxIfNumber(object)` and `ToStringNetFxIfNumber(object[,provider])`
  for a value of unknown type (a data table's cell): a boxed double or float through the helper, anything else as `ToString()` /
  `Convert.ToString` did.
- **How:** the .NET Framework's `FormatDouble`: `ParseFormatSpecifier` (a letter and at most two digits is standard), 15 significant digits
  (17 for `G16`+/`E15`+; a float 7, or 9) rounded half away from zero. A double's 15 digits are rounded from the 17 correctly rounded
  digits .NET's `E16` gives, as the x86 .NET Framework did (17 digits ending in 50 round up: section 3a); the other precisions from the
  exact value (only when the digits after the kept ones read 5 and zeros is the exact value read in full, `E800`; for a float, dropping
  that read differs from both runtimes in 4,813 of 4.2M rows, so it stays), then the format applied to a `decimal`
  that holds exactly those digits (.NET formats a decimal with the same `NumberToString`/`NumberToStringFormat` and no sign on zero; `G` with
  no digits becomes `G15`/`G7`). Beyond a decimal (above 7.9E+28, or digits past the 28th place): own `G` and `E` code, `BigInteger` for the
  other formats of large values, 28 places for small ones; a custom format with an exponent beyond a decimal falls back to .NET's formatter
  without the sign of zero. `R` stays .NET's (exact; for -0 "0"). NaN and infinities: the culture's symbols.
- **Not CodeBase:** RdlEngine, ReportDesigner, ImagingDeviceManager, UpdateFileCopier and OpenPmsDentalHelper32 compile
  `ODNumberTextNetFx.cs` in as an internal class (`OD_NUMBERTEXT_LINKED`; none of them has InternalsVisibleTo to a project that also references
  CodeBase and uses the helper).

`ODDateTimeAdd`: `AddDaysNetFx` .. `AddMillisecondsNetFx` for `DateTime` and `DateTimeOffset`, the .NET Framework's `DateTime.Add(double,int)`
(milliseconds rounded half away from zero; NaN and values beyond the span of `DateTime` throw `ArgumentOutOfRangeException`, as there,
where .NET's saturating cast would make NaN 0). `ODTimeSpan` (S3b-5) is unchanged and passes every `TimeSpan.FromX` row.

### 3a. x86 digits: the record is x86, and what still differs

The first helper (s3bfmt-1) rounded the exact value to 15 digits, which is what the **x64** .NET Framework does; the record, and the
program Open Dental shipped, are **x86**. A review measured it, and s3bfmt-2 measured it again: seeded stress probes compiled with the
Framework's `csc` (as `Make-Net48Truth.ps1` does) and run on net48 x86 and x64, 600,000 doubles (half cents and half tenths 1 to 8 ulps
off, 15 digits and a 5 one ulp off from 1E-4 to 1E16, `a*0.15`, `a/7`, random sizes) in `E16`, `G15`, `F2`, `E14`, `R`, `N2`, `0.00`,
`0.##`, `G`, `F1`, against the helper on .NET 10 (UseNls), compiled from `ODNumberTextNetFx.cs` with the SDK's `csc.dll` in a scratch folder:

| helper's 15 digits from | rows differing from x86 (of 4.8M) | from x64 |
|---|---|---|
| the exact value (s3bfmt-1) | 10,887 (3,159 values; `F2` 227) | 0 |
| 17 correctly rounded digits, half away from zero (s3bfmt-2) | 2,744 (788 values; `F2` 88) | 13,565 |
| 16, 18, 19 or 20 correctly rounded digits | 121,915 / 9,473 / 10,776 / 10,866 | |
| x86's own 17 digits (its `E16` text) | 2,995 | |

So x86 rounds from about 17 accurate digits, and the helper now rounds the 17 correctly rounded digits for a double's 15 (no format, `G`,
`G15`, `F`, `N`, `C`, `P`, `E14` and shorter, every custom format). Every half-cent, half-tenth, share and seventh value of the stress set
now matches x86 (400,000 values, 0 differences). **What still differs:** the 788 values, all of them 15-digits-and-a-5 or random ones,
all with exact digits 16 to 19 between 4949 and 4994: there x86's own digits (computed with an error of up to about half a unit of the
17th digit) fall on either side of half way, while 17 correctly rounded digits read 50 and round up. Example: 0x3F8E9FB839D6D918
(0.01495307852279075, exact digits 16 to 19 4978) is 0.0149530785227907 on x86 and x64 and 0.0149530785227908 from the helper; in a money
format, 0x427E701EB65EE15C `F2` is 2091681277422.08 on x86, 2091681277422.09 from the helper. About 0.13% of values chosen to lie half way
at the 16th digit, none of the money amounts tried; a value has to sit within a few parts in 10^18 of a 15-digit half for it to show.

**Not ported:** the x86 runtime's digit routine (its x87 code is not public, and its error is not regular: in 4950..4994 x86 rounds up
about two times in three and down otherwise, with no threshold). The record holds 5 such values (`_arrayX86BandValues` in
`TruthTableTests`, 107 rows); every other row must give the x86 text, an `X` row (x64) is accepted only for the 17-digit formats, and
`NumberRows_X86AndX64DifferAt15Digits_TheHelperGivesTheX86Text` checks the helper against the 1,126 15-digit rows where x86 and x64 differ
(the s3bfmt-1 helper gave the x64 text in all of them).

## 4. The scan tool: tools\FormatSweep

`Directory.Build.targets` records each SDK-style C# project's compiler inputs at build time (`ODRecordCompileInputs`, before `CoreCompile`,
written only when they change): `obj\...\<project>.compile-inputs.txt` with the language version, the constants, unsafe, every `Compile`
item and every reference the compiler gets. `FormatSweep` builds the same `CSharpCompilation` from it (no analyzers or generators; 0 compiler
errors in every scanned project), and walks every syntax tree of the repository's own source (not `obj\`, `bin\` or `third-party\`); files
that mention `DEBUG` are parsed a second time without it, so `#if DEBUG`'s other branch is scanned too. A file linked into two projects
counts once.

- **Shipped projects** (`tools/FormatSweep/shipped-projects.txt`, 22): CodeBase, DataConnectionBase, ODCrypt, PasswordVaultWrapper,
  OpenPmsDentalStorage, OpenPmsDentalBusiness, OpenPmsDentalGraph, SparksToothChart, WpfControlsOD, ImagingDeviceManager, ImagingCapture, ODR,
  RdlEngine, RdlViewer, ReportDesigner, ServiceManager, OpenPmsDentalDbSettings, OpenPmsDentalHelper32, UpdateFileCopier, OpenPmsDentalHL7,
  OpenPmsDentalServer, OpenPmsDental. Not scanned: tests, `tools\`, DocumentationBuilder, xCrudGenerator (its output is in the business
  library), PluginExample, CentralManager, DatabaseIntegrityCheck and MySQLManager (not in the release), `third-party\` (section 7).
- **Run it** after a full lane build: `tools\FormatSweep\bin\Debug\FormatSweep.exe --root . [--report r.txt] [--formats f.txt] [--rewrite]
  [--projects a.csproj,b.csproj]`. Exit 0 when nothing guarded is unrouted. `--rewrite` applies the edits (insertions and renames only; a
  file keeps its bytes, encoding, BOM and line ends; a Windows-1252 file is rewritten byte for byte as Latin-1) and adds `using CodeBase;`
  where needed. Review the diff, build, run the guard.
- **Allow list:** `tools/FormatSweep/allowed-sites.txt` (path | category | code | reason): only the helper's own calls of .NET's formatter.
  A line that matches nothing fails the guard too.

## 5. The sweep: every category, with counts

From the scan before the sweep (22 projects; 1,610 places in 341 files, plus the helper's own 8):

| Category | What | Rewrite | Places |
|---|---|---|---|
| ToString | `double`/`float`/`double?`/`float?` `.ToString(...)`, any overload, also `?.ToString()` | renamed `ToStringNetFx` | 1,370 |
| Composite | a double handed as `object` to `string.Format` (62) or `StringBuilder.AppendFormat` (49) | `value.AsNetFx()` | 111 |
| Concat | a double in a string concatenation (`+`, `+=`), SQL text included; a constant only when its text differs (`Math.PI`) | `value.ToStringNetFx()` | 88 |
| Interpolation | a double in an interpolated string's hole (`{amount:C}`, `{payPlan.APR}`) | `value.AsNetFx()` (alignment and format stay) | 24 |
| Convert.ToString | `Convert.ToString(double[,provider])` (RDL's aggregate functions, MarkupEdit) | `value.ToStringNetFx([provider])` | 11 |
| DateTime.AddX | `AddDays` .. `AddMilliseconds` whose argument is not integral by its type and not a constant of whole milliseconds | `AddXNetFx` | 14 |
| Append | `StringBuilder.Append/Insert`, `TextWriter`/`Console.Write(Line)` of a double | `value.ToStringNetFx()` | 0 |
| TimeSpan.FromX | `FromDays` .. `FromMilliseconds(double)` not whole | `ODTimeSpan.FromX` (S3b-5 did its 86) | 0 |
| Join, MethodGroup | `string.Join/Concat` of a sequence of doubles; `double.ToString` as a delegate | by hand | 0 |
| Boxed (reported) | a double stored as `object` (a data row's cell, a DTO argument, a return value) | section 6 | 114, 79 after section 6 |

By project: OpenPmsDental 926, OpenPmsDentalBusiness 457, RdlEngine 79, WpfControlsOD 45, ReportDesigner 40, SparksToothChart 30,
CodeBase 15, ImagingCapture 13, ODR 5, ImagingDeviceManager 5, OpenPmsDentalGraph 3 (and the helper's 8, allowed). The DateTime ones:
FormXChargeReconcile (hours, minutes, seconds parsed as doubles), AgingData (bill-in-advance days), TerminalConnector (six millisecond
timeouts), the HL7 parser (appointment length in minutes), WindowsTime (three NTP offsets). The 33 changed RDL files (LGPL) carry the change
notice (35 with section 6's). Integral and decimal values are left alone.

## 6. The framework places, measured and fixed

Each measured on .NET 10 against the .NET Framework's text (`tests/FormatParityTests/FrameworkPlacesTests.cs` pins the measurements):

- **A data table's double cell as `object.ToString()`** (a user query's `SUM`: MariaDB 10.11 sends `0.30000000000000004`, the cell is that
  double, .NET writes it so, the .NET Framework wrote 0.3). Fixed where such cells become text generically: `SIn.ByteArray` (MakeReadable,
  letter merges), the user query grid (`FormQuery.BindGrid`: a `CellFormatting` handler; its export, cell text and column widths), the eCW
  query grids, the RDL report exports (`FormReportCustom`, `FormReportForRdl`), ReportComplex's raw field text (`FormReportComplex`,
  `QueryObject`), the Database Maintenance results table and the treatment finder export: `ODNumberText.ToStringNetFxIfNumber(cell)`.
- **A double stored in a string column** (`DataColumn` converts with .NET's text): RpInsAging (19 cells), RpProdInc, RpUnearnedIncome,
  FormRpProcNotBilledIns, FormRpProdInc's clinic totals, AccountModules' `adjAmt_` and `value` now store `ToStringNetFx()`. The other 15 data
  row assignments are `typeof(double)` columns (RpPresentedTreatmentProduction, RpTreatPlanPresentationStatistics) or a database column
  (AccountModules' `principal_`), shown through the routed report formats.
- **The Middle Tier:** `XmlConverter.TableToXml` sends a table cell as text and the client's table holds that string; a double cell is now
  sent as the .NET Framework's text (the client shows what it showed with a .NET Framework server); `WebSerializer` likewise. The DTOs
  (`Meth.GetObject/GetVoid/GetBool` arguments, 11 boxed places) go through `XmlSerializer`, which writes `XmlConvert`'s round-trip text:
  every double and float of the test (NaN, the infinities, -0, Epsilon, MaxValue, 0.1+0.2, the 16-digit half) reads back bit for bit.
- **Charts** (WinForms.DataVisualization in OpenPmsDentalGraph, `C0`/`C` labels): the library formats with .NET's formatter; its
  `FormatNumber` event writes each number label again through the helper (`ChartNumberText.Attach`, both graphs); date labels untouched.
  Measured: a point of 1.005 with `C` is labelled $1.01.
- **RDL:** the formatted path (`Style.GetFormatedString`, the S3b-2b finding) and the unformatted one (`ToStringNoFormat`, now also the
  exact 16-digit half) go through the helper; the chart's default label (`DrawStringDefaults`), the VB `CStr` and the `Format` function too.
  The RDL functions' `object` results (10 return statements) reach text only through those.
- **Grid columns, data binding, WPF:** GridOD's cells are strings the code makes (routed); no WinForms `Binding`/`FormatString`, no XAML
  `StringFormat` binds a double; the only `DataGridView` bound to raw data is the user query grid (above).
- **The others reported as boxed** are not text: `object.Equals` (6), a WPF font size, combo box tags, a progress delegate, a balance sheet
  row of a double column, ODR's parameter values (formatted with `"n"` by a cast, routed).

**Not matched** (listed, with the reason and an example):

- `R` is .NET's shortest round-trip text (1/3 is 0.3333333333333333; the .NET Framework wrote 0.33333333333333331): both read back to the
  same double, and R is the code's round-trip format (8 places). Also the property grid of the report designer (`DoubleConverter`, the same
  `R`).
- A database double cell shown with `ToString()` at a place that is not one of the generic ones above (a form that puts `row["x"].ToString()`
  of a raw query's computed double straight into a grid): .NET's text for a computed value with a binary tail (0.30000000000000004), stored
  values and money sums that come back short (12.34) read the same. The 12,000 `row[...].ToString()` calls of the code are mostly string
  columns or parsed with `PIn.Double`/`PIn.Decimal`, which reads both texts to the same value within the 15 digits every later format uses.
- PDFsharp 6.5's own numbers in a PDF's content stream (coordinates, written with custom formats such as `0.###` inside the submodule): .NET
  writes `-0` where the .NET Framework wrote `0`, and an exact 16-digit half's last digit can differ. No visible text changes (the text a
  statement or claim shows is the code's, routed); the submodule is not changed.
- VB code in a report's `<Code>` block formats with .NET's formatter; no report of the repository has such a block.

## 7. The DateTime sweep

`AddDays`, `AddHours`, `AddMinutes`, `AddSeconds` and `AddMilliseconds` of `DateTime` and `DateTimeOffset` with a `double` argument that is
not integral by its type (an `int`, `long` ... converted to double adds whole units on both runtimes) and not a constant whose milliseconds
are whole (`AddMinutes(0.5)`, `AddHours(1.5)`): 14 places, now `AddXNetFx` (section 5). `TimeSpan.FromX` (S3b-5's `ODTimeSpan`, 86 calls)
is guarded by the same scan and finds none left. The truth table's 2,655 time rows pass, `ODTimeSpan` included.

## 8. The guard

`tests/FormatParityTests` (new; MSTest, x64, in `OpenDental.sln` and `OpenDental.Local.slnf`): `TruthTableTests` (every number row through
`ToStringNetFx`, a sample through `AsNetFx` in composite formats, every time row; the x86 text required but for the 17-digit formats' `X`
rows and the 5 listed x86 band values, which must still lie in the band and differ; the 15-digit rows where x86 and x64 differ), `PinsTests` (decision 24's cases), `FrameworkPlacesTests`
(section 6), `ScanGuardTests` (the finder and the rewrite on a fixture, which compiles and is found no more, and
`ShippedProjects_HaveNoUnroutedNumberTextOrFractionalTime`: the scan of the 22 projects, 0 unrouted, no unused allow-list line; it reads the
last lane build's compile inputs, inconclusive without them, a failure with `OPMS_REQUIRE_BUILD_OUTPUT=1`). The assembly takes about 65 s,
55 of them the scan; it runs in every Core and Full run. **For the lead:** making it part of every Targeted run, as NoHqTests is, is a change
to `od-tools/run-tests.ps1` (outside the repository); not done here.

## 9. Runs (x64, `-RequireBuildOutput`, Targeted; results in `od-tools/test-results/s3bfmt-1-*.txt`)

All pass: FormatParityTests 18, GoldenOutputTests 28, BINBTests 105 (7 net48-only inconclusive), UnitTests 1,731 (3 skipped as before),
ProjectFileTests, LibraryBindingTests, AttributionTests, RdlCodeTests, ReportTests, StatementTests, PayPlanTests, AccountingTests,
MiddleTierMockTests, MiddleTierHostTests, ReleaseFlowTests, PdfLibraryTests, ChartLetterTests, SheetTests, InsuranceTests, FeeTests,
BPAYTests, the ten P262 suites, BACCT, BCLAIM, BDBM, BIMG, BERA, BEF, BINSH, BMED, BPERIO, BSERVER, BSHELL, BTASK, BWIKI, BAUDIT, BTIME,
BTP, BPROC, BPRINT, BAPPT, ApptTests, every M2 suite (M201, M202, ADJ, APPT, AUDIT, CAN, CARRIER, CLAIMFORM, CLAIMRCV, DUN, EF, EMP, ERA,
EST, HIST, IMG, INSPLAN, INSV, MED, MISC, NOTES, PAYW, RECUR, SDV, SHEETS, SHELL, TERM, UDS), ODOPTSTests, MaintenanceTests,
QueryMonitorTests, SignatureTests, CryptoTests, Helper32Tests, ImagingTests, ImagingCaptureTests, ImagingDeviceManagerTests,
VendorDirectTests, DbConnectionSecurityTests, EFormTests, EFormPrintTests, GroupNoteTests, InventoryTests, TaskCategoryTests, CLAIMDCMTests,
DbGrantsTests, DicomReaderTests, INSVERIFYTests, LibSpellTests, LibraryUpdateTests, LibWebTests, MedicationTests, N64ATests,
NativeFirstTests, RegistryTests, RowHashTests, S0FTests, S3aM1Tests, SshHostKeyTests, SWMAPTests, VBRIDGE1/2, VDXC2, VEDS, VGP, VPEARL1/2,
EhrTests, and NoHqTests in every run. Not run: InitSqlTests and ForkMigrationTests (coordinator), the WPF picture projects
(ToothChartImageTests, ToothChartHostTests, WindowChecksTests) and BINATests (its `WpfChart_StartsOnAHiddenForm`), AdoptionValidation.Tests
and SyntheticData.Tests (tools, unchanged by this package).

**s3bfmt-2 (the review fixes; `od-tools/test-results/s3bfmt-2-*.txt`), after a full lane build of the filter, x64, 0 errors:** all pass:
FormatParityTests 20, GoldenOutputTests 28, BINBTests 105 (7 net48-only inconclusive, as before), ProjectFileTests 168 (1 skipped, as
before), UnitTests 1,731 (3 skipped), and a Core-sized sample of the money and report suites: ReportTests, StatementTests, PayPlanTests,
AccountingTests, RdlCodeTests, Helper32Tests, MiddleTierMockTests, P262ReportsTests, LibraryBindingTests, PdfLibraryTests, BIMGTests,
BPAYTests, FeeTests, InsuranceTests; NoHqTests in every run.

Test changes that follow the sweep: LibraryBindingTests maps the new package (not shipped) and counts 38 package files; P262ReportsTests
and BIMGTests read `ToStringNetFx` in the source they check; Helper32Tests' QuickBooks reference (`QuickBooksToday.cs`) writes the deposit
amount with `ToStringNetFx`, the text the program sends (99999.995 with `F`: 100000.00, as the .NET Framework).

## 10. Download

Microsoft.CodeAnalysis.CSharp 5.12.0 (the newest stable; it pins Microsoft.CodeAnalysis.Common 5.12.0 exactly, which the offline folder
had) fetched under the standing rule into `od-tools/downloads/s3bfmt-roslyn-csharp-20261116` (SOURCES, SHA256SUMS, MANIFEST; author and
repository signatures verified) and added to the offline folder; `Directory.Packages.props` names it. Used by FormatSweep and
FormatParityTests only, never shipped.

## 11. Checks

- Full lane build of `OpenDental.Local.slnf`, Debug x64: 0 errors (after the sweep and again after the framework places).
- `tools\Test-NoEolFlips.ps1 -Base net10/switch`: 0 changed text files changed their line ends.
- No scratch database of these runs is left on 127.0.0.1:3306.
- `db/init/new-database.sql` not regenerated; no schema change.
