# Port note: BUILD-HYGIENE (three build findings left from stage 2)

Branch `build/hygiene-1` from `main` (`20c24b056`). Three small findings of stage 2 of the .NET 10 move
([docs/specs/net10-plan.md](../specs/net10-plan.md)): open point O-17 (a resource that comes out in two forms), the MSB3245 warning of
UnitTests in a cold Release build (`docs/port-notes/S2-4.md`, section 8), and the out-of-date comment above the binding redirects of the
configs (`docs/port-notes/S2-4.md`, section 5). No program code, no package, no version and no schema change; nothing was downloaded; no
program was started. Nothing an office would notice, so `docs/release-notes/next.md` is unchanged.

Commits: `7b5530a8b` (part 1, the `.resx` files), `a01e8b3de` (part 2, UnitTests), `ca487331a` (part 3, the configs), `1ea201cfb` (part 2,
DocumentationBuilder's lock file), `a6063d6a4` and `b80be692f` (part 1, the test), and the docs commit with this note.

All builds below are lane builds (Build Tools 2026, MSBuild 18, SDK 10.0.114, 6 nodes, `/nodeReuse:false`) of the whole
`OpenDental.Local.slnf` in this worktree, each from a clean state (`git clean -fdXq` first):

| Build | State | Configuration | Warnings | MSB3245 |
|---|---|---|---|---|
| B0 | `main` (`20c24b056`) | Release | 1,114 | 1 (UnitTests, OpenPmsDentalStorage) |
| B1 | `main` (`20c24b056`) | Debug | 353 | 0 |
| A1 | this branch | Release | 1,113 | 0 |
| A2 | this branch | Release | 1,113 | 0 |
| A3 | this branch | Debug | 353 | 0 |
| A4 | this branch, final (with the docs and the test's last commit) | Debug | 353 | 0 |

The resources were read from every built assembly of every `bin\<configuration>` folder with Mono.Cecil (the `.resources` streams of
each `EmbeddedResource`, by name, with their size and SHA-256; copies with the same name and bytes counted once): 1,085 `.resources` in
201 assemblies (199 in Debug, where two Release-only XmlSerializers assemblies are not built).

## 1. O-17: the resources that came out in two forms

**Cause.** `OpenPmsDental/Tables Old Grids/ContrTable.resx`, `ODR/ComboBoxMulti.resx` and 41 forms of RdlDesign are the 43 `.resx` files
that the Windows Forms designer of Visual Studio .NET 2003 wrote last. That designer kept its own design-time properties as `<data>`
entries: `$this.Language` (a `System.Globalization.CultureInfo`), `$this.Localizable`, and per component `Locked`, `Modifiers`,
`DefaultModifiers`, the grid settings `SnapToGrid`, `GridSize`, `DrawGrid` and the tray settings `TrayHeight`, `TrayLargeIcon`.
GenerateResource compiles every `<data>` entry into the `.resources`; `<metadata>` it does not compile. For `$this.Language` (value
`(Default)`) the resx reader asks `CultureInfoConverter`, which returns `CultureInfo.InvariantCulture` itself, a singleton of the process,
and the resource writer stores it with the binary formatter. The binary form of a CultureInfo holds its lazily created members
(DateTimeFormatInfo, TextInfo, Calendar) when something in the process has created them before, and leaves them out when not. A small
.NET Framework 4.8 console probe (outside the repository) shows it: the converted value is the singleton (`ReferenceEquals` true), and its
serialized size is 1,547 bytes untouched, 1,727 after `TextInfo`, 1,862 after `Calendar`, 3,163 after `DateTimeFormat`, 3,343 after
`TextInfo` and `DateTimeFormat`, 3,389 after formatting a few dates. GenerateResource runs in the build node's process
(`ResGenExecuteAsTool` is false), and a node builds one project after the other, so what the node had done before, which depends on the
scheduling of the parallel build, decided the form: 5,606 or 7,424 bytes for `OpenPmsDental.ContrTable.resources`, 5,299 or 7,117 for
`ODR.ComboBoxMulti.resources` (S2-4), and once the 41 forms of `RdlDesigner.exe` (S2-2). So it was not the Icon that S2-4 suspected
(`ComboBoxMulti.resx` holds a Bitmap, which serializes the same every time); `$this.Language` is the entry the files share.

**Who reads these entries at run time: nobody.** No source calls `ComponentResourceManager.ApplyResources` (which would apply a
resource set's entries to a form's properties by name), and the one `GetResourceSet` (`ClaimForms`) enumerates the business layer's
`Properties.Resources`, which holds no such entry. No `GetObject` or `GetString` names one of these keys: `ContrTable` creates no
resource manager at all, `ComboBoxMulti` reads `dropButton.Image`, the RdlDesign forms read their images, icons and one label text. A
form has no `Language` or `Localizable` property at run time (the designer's localization provider adds them while designing), and
`Locked`, `Modifiers` and the rest are the designer's too.

**Fix.** The 2,159 entries of the 43 files are `<metadata>` now (DefaultModifiers 592, Locked 582, Modifiers 549, DrawGrid, GridSize and
SnapToGrid 88 each, Language, Localizable, TrayHeight and TrayLargeIcon 43 each), as the designer has written them since Visual Studio
2005 and as the other forms of the repository already hold them (289 `TrayLocation`, 55 `TrayHeight`, 21 `TrayLargeIcon`, 3 `Locked`,
3 `GenerateMember`, all `<metadata>`). The designer still reads them; the build no longer compiles them. Only the element names changed:
each file, with `<metadata name=` and `</metadata>` turned back into `<data name=` and `</data>`, is byte for byte its `main` version. A
build setting was the other way and is not taken: `GenerateResourceUsePreserializedResources` needs System.Resources.Extensions at run
time (a package, and another resource reader in the program), and `ResGenExecuteAsTool` keeps the same serializer and only makes the
state of the process a fresh one; both leave design-time data in the program.

No other `.resx` of the repository holds one of these properties as `<data>`. One upstream file does: PDFsharp's
`src/foundation/src/PDFsharp/src/PdfSharp-gdi/Forms/PagePreview.resx` in the `third-party/PDFsharp` submodule (its `$this.Language`
too), which `third-party/build/PdfSharp-gdi` embeds as `PdfSharp.Forms.PagePreview.resources` of `PdfSharp-gdi.dll`. The submodule
keeps upstream's bytes, and nothing reads that resource (`PagePreview.cs` creates no resource manager). It came out the same in all five
builds (3,019 bytes, `6acb1474e1a4010290e509946f57519d6f0bda9d7cb14f89659fba22c6290e54`; the project builds early, before the node has
done much). It stays as it is, and the test lists it as its one exception (section 4).

**Proof.**

| Resource | B0 (Release) | B1 (Debug) | A1 = A2 (Release) = A3 (Debug) |
|---|---|---|---|
| `OpenPmsDental.ContrTable.resources` in `OpenPmsDental.exe` | 5,606 `7cc13d17a2eef699c1f079dbc491b81adf4acea18eb7c4c9879557d416e5aaec` | 7,424 `6c0d8ff74ff3df80c2a2303ecedd6e0285fa998eefd61f8d2f2a7de24bdc0559` | 225 `541f0e18222490d28c88d3b2209f886798fb2ed27c1203fa899868927f976e47` |
| `ODR.ComboBoxMulti.resources` in `ODR.dll` | 5,299 `165516ef101996b8e8edb5e9fd502a750e34de67e85ce6cec3b4e6364d756e69` | the same as B0 | 1,138 `5a8b30dd33c5b79b1a036d861a45f19c35403fce3a5ce321726769c529843497` |
| the 56 `.resources` of `RdlDesigner.exe` (41 of them changed) | 434,713 bytes in all | the same as B0 | 48,826 bytes in all |
| `PdfSharp.Forms.PagePreview.resources` in `PdfSharp-gdi.dll` | 3,019 `6acb1474...` | the same | the same |

- B0 and B1 show both forms of ContrTable's resource on `main`, one build after the other.
- A1 and A2, two cold Release builds: every one of the 1,085 `.resources` of the 201 assemblies has the same bytes (the two lists are
  identical), every `.resources` inside `OpenPmsDental.exe` and `ODR.dll` included.
- A3 (Debug) has the same bytes as A2 (Release) for every `.resources` but the two WPF `.g.resources` (BAML, which is not the same in
  Debug and Release builds by design): with other configurations and another scheduling of the nodes, the same resources.
- A3 and A4, two cold Debug builds: all 1,085 `.resources` of the 199 assemblies have the same bytes too.
- Before against after, in each configuration: exactly the 43 resources of the 43 files differ (ContrTable, ComboBoxMulti, 41 of
  RdlDesigner); none of the other 1,042.
- `ODR.dll` of A2: 38,912 bytes, `2ee9adf8bb9dc4464f060028b85b23a5c94691232b6d18674ff62d9cbc6a1008`.

The exe still differs between builds by its build stamp (`ForkBuildUtc`), as before; the plan's section 3 no longer lists the ContrTable
resource with the files that change on every build.

## 2. MSB3245: UnitTests could not locate OpenPmsDentalStorage

**Cause.** Not a race: `UnitTests/UnitTests.csproj` referenced the assembly as a file, `Reference Include="OpenPmsDentalStorage, ..."`
with `HintPath` `..\OpenPmsDentalStorage\bin\Debug\OpenPmsDentalStorage.dll`, a fixed configuration of another project's output folder.
In a Debug build the file is there by the time UnitTests resolves its references (UnitTests' project references, OpenPmsDental and
OpenPmsDentalBusiness, build OpenPmsDentalStorage first), so B1 has no warning. In a Release build it is there only when a Debug build ran
before, so the first Release build of a fresh worktree warned (B0, `main`'s state):

```
Microsoft.Common.CurrentVersion.targets(2468,5): warning MSB3245: Could not resolve this reference. Could not locate the assembly
"OpenPmsDentalStorage". Check to make sure the assembly exists on disk. If this reference is required by your code, you may get compilation
errors. [UnitTests\UnitTests.csproj]
    For SearchPath "{HintPathFromItem}".
    Considered "UnitTests\..\OpenPmsDentalStorage\bin\Debug\OpenPmsDentalStorage.dll", but it didn't exist.
```

(paths here relative to the worktree). The build went on because the assembly also reaches UnitTests as a transitive project reference.

**Fix.** A `ProjectReference` to `..\OpenPmsDentalStorage\OpenPmsDentalStorage.csproj` instead of the file reference; it resolves the
project's own output in every configuration. The locked restore then stopped with NU1004 in DocumentationBuilder, whose lock file records
UnitTests' project dependencies: its "Unit Tests" entry names `OpenPmsDentalStorage` now (no package, no version). It is the only
reference of the repository with a HintPath into another project's output folder. UnitTests' two other file references to projects of the
solution, `OpenPmsDentalGraph` and `ServiceManager`, carry no HintPath; they resolve to those projects' outputs through the transitive
project references and warn in no build.

**After.** A1 and A2 (cold Release) and A3 (cold Debug) have no MSB3245; A1 and A2 have 1,113 warnings, B0's 1,114 less this one.
`UnitTests\bin\Release` holds `OpenPmsDentalStorage.dll` and `.pdb` (S2-4 found them missing after such a build), and
`UnitTests\bin\Debug` the same files as in B1.

**ProjectFileTests and the project baseline.** ProjectFileTests passes with the changed project file (section 4). The baseline record of
`tools/project-baseline` records no Reference or ProjectReference items, and UnitTests' Compile, EmbeddedResource, None and Content items
and its output files are unchanged, so part 2 needs no new record. Part 3 does change the record: its `configs.txt` and `config-texts.txt`
hold the configs' texts and hashes, and the comment is in them. The lead re-records the baseline at the next gate; this package does not.

## 3. The comment above the binding redirects

Since S2-4 every assembly that the binding redirects name (Google.Apis, Google.Apis.Core, Microsoft.Bcl.AsyncInterfaces, the
Microsoft.Extensions abstractions, Microsoft.IdentityModel.Abstractions, the System.* support assemblies, and in WpfControlsOD's config
Newtonsoft.Json and Microsoft.Web.WebView2.Core) comes from a NuGet package, and LibraryBindingTests checks the redirects against the
package files. `Required dlls` holds none of them. The comment said:

```
    <!-- The support assemblies in "Required dlls" are newer than the versions the libraries were built against: every older version goes to the
         one that ships. tests/LibraryBindingTests checks these redirects against the files there. -->
```

and says now:

```
    <!-- The assemblies of the NuGet packages (the libraries and the .NET Framework support packages they depend on, such as System.Memory) are
         newer than the versions the libraries were built against: every older version goes to the one that ships, the package's file at the
         version the lock files name. tests/LibraryBindingTests checks these redirects against the package files. -->
```

in 19 files: the `app.config`/`App.config` of CentralManager, DatabaseIntegrityCheck, DocumentationBuilder, MySQLManager, OpenPmsDental,
OpenPmsDentalBusiness, OpenPmsDentalDbSettings, OpenPmsDentalGraph, OpenPmsDentalHL7, ServiceManager, SparksToothChart, UnitTests,
WpfControlsOD, xCrudGenerator, tools/AdoptionValidation, tools/DevDbTool and tools/SyntheticData, `OpenPmsDentalServer/Web.config`, and
`tests/BindingRedirects.config`. The check: for each file, the `main` text and the new text with every XML comment removed (and line ends
normalised) are equal, so nothing but the comment changed; the line ends of each file are kept.

The stage 2 gate left the comment alone so that the shipped `.config` files stayed byte for byte the same; that gate has passed, so they
change now. Seven of them ship (`OpenPmsDental.exe.config`, `OpenPmsDentalBusiness.dll.config`, `WpfControlsOD.dll.config`,
`OpenPmsDentalGraph.dll.config`, `SparksToothChart.dll.config`, `ServiceManager.exe.config`, `OpenPmsDentalDbSettings.exe.config`). No
release check compares their bytes: `tools/Build-Release.ps1` lists them without a hash, and `docs/develop/release-process.md` describes no such
comparison, so it is unchanged. The next gate's hash comparison and the project baseline see them changed in the comment only (the plan's
section 3 says so).

## 4. Tests

- `tests/ProjectFileTests/RepositoryProjectsTests.cs`, new test
  `NoResxFileOfTheSolutionFilter_CompilesTheDesignersOwnProperties`: it reads every `.resx` that a project of `OpenDental.Local.slnf`
  embeds (the EmbeddedResource items the project-file reader evaluates, the submodule files the third-party builds link included; more
  than 1,000) and fails on a `<data>` entry whose name ends in `.Language`, `.Localizable`, `.Locked`, `.Modifiers`, `.DefaultModifiers`,
  `.GenerateMember`, `.TrayLocation`, `.TrayHeight`, `.TrayLargeIcon`, `.SnapToGrid`, `.GridSize` or `.DrawGrid`. It checks that it reads
  the right files (ContrTable's and ComboBoxMulti's `$this.Language` are `<metadata>`), and that the list of upstream exceptions is exactly
  PDFsharp's `PagePreview.resx`: a submodule update that drops the entries makes the test name the exception to remove, one that adds a
  file with such entries makes it fail. It is in the existing test file, so ProjectFileTests' Compile items (and the baseline) do not change.
- Targeted runs (Debug, after A3): ProjectFileTests, LibraryBindingTests and NoHqTests; results in section 6.

## 5. Every file changed

| File | Part |
|---|---|
| `OpenPmsDental/Tables Old Grids/ContrTable.resx`, `ODR/ComboBoxMulti.resx`, 41 `RdlDesign/*.resx` | 1 |
| `tests/ProjectFileTests/RepositoryProjectsTests.cs` | 1 (the test) |
| `UnitTests/UnitTests.csproj`, `DocumentationBuilder/packages.lock.json` | 2 |
| the 18 `app.config`/`App.config`/`Web.config` files and `tests/BindingRedirects.config` listed in section 3 | 3 |
| `docs/specs/net10-plan.md` (O-17 closed, section 3's hash comparisons, the open points' summary at the top), this note | docs |

## 6. Test results

Targeted runs (`run-tests.ps1 -Mode Targeted`, Debug), after the final full lane build of this branch (A4):

| Assembly | Total | Passed | Failed |
|---|---|---|---|
| ProjectFileTests | 133 | 132 | 0 (1 skipped: the opt-in baseline check, Inconclusive without `OPMS_CHECK_PROJECT_BASELINE`) |
| LibraryBindingTests | 17 | 17 | 0 |
| NoHqTests | 57 | 57 | 0 |

The deliberate break: with ContrTable's `$this.Language` turned back into `<data>` (the working file only, restored afterwards), the new
test fails with `1 entries: OpenPmsDental\Tables Old Grids\ContrTable.resx: <data name="$this.Language"> (the designer's own property:
<metadata>, as the designer writes it)`.

InitSqlTests and ForkMigrationTests were not run (no schema change), nor the test assemblies of the parallel streams.

## A copy of this note as an artifact

While it edited this note, the implementing agent published it by mistake as a private artifact on claude.ai
(https://claude.ai/artifact/2M79x7TKJwx1HKGh5y1r2V), visible only to the owner. The owner chose on 2026-11-14 to keep it. It is a
snapshot from before the merge: this file is the record, and the artifact is not updated with it.
