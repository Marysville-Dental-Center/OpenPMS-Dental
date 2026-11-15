# Port note: TC-4 (Helix Toolkit 3.1.2)

Package TC-4 of the WPF tooth chart track ([docs/specs/wpf-tooth-chart-plan.md](../specs/wpf-tooth-chart-plan.md), the TC-4 row of
section 4, sections 3, 9 and 10): branch `chart/tc4-1` from `main` (`5a7e16d8f`). **Status: done.** Step 1 stopped on 2026-11-14 for a
batch H2 (section 3); the owner approved it, the lead added it to the offline folder (`nuget-packages/BATCH-H2.txt`), and the resumed
package re-ran step 1's exe probe (it restores) and did step 2. No chart code uses Helix: Helix changes no pixel. Nothing was downloaded, no
package source was added and no program was started.

Commits: `7c124ea36` (step 1, this note), `2c229759b` (packages, lock files, `Directory.Build.targets`, redirects), `0ff303872` (release
lists, notices, `ReleaseIdentityTests`, the binaries audit), `4b9ff3fca` (tests), then the docs commit with this note.

## 1. Step 1: how it was run

A scratch folder outside the repository (the session's scratch folder: `tc4\step1` for the first run, `tc4r\step1` for the re-run) held
copies of the repository's `nuget.config`, `global.json`, `Directory.Build.props`, `Directory.Build.targets`, `Directory.Packages.props`
(plus one line, `<PackageVersion Include="HelixToolkit.Wpf" Version="3.1.2" />`) and `tools\Initialize-BuildEnvironment.ps1`, and two
probe projects:

- `SparksToothChart\SparksToothChart.csproj`: the chart project's own settings (net48, `LangVersion` 7.3, `Platforms` AnyCPU;x86, its ten
  framework references), `<PackageReference Include="HelixToolkit.Wpf" />`, no project references and no SharpDX or Tao files; one
  source file without Helix code.
- `ProbeHost\ProbeHost.csproj`: a net48 `WinExe` that references the probe chart, as `OpenPmsDental` references `SparksToothChart`.

Both were restored and built through the build lane (`odbuild.ps1 -Root <scratch> -Solution ProbeHost\ProbeHost.csproj -WaitMinutes 20`:
Build Tools 2026 MSBuild, SDK 10.0.114, locked restore with no lock file, so the restore writes one; `OPMS_NUGET_OFFLINE` = the repository's
offline folder; the default global packages folder). The repository's settings that matter here came along unchanged:
`CentralPackageTransitivePinningEnabled` true (on in every SDK-style project since S2-1), `DisableMicrosoftExtensionsLoggingSourceGenerator`
and `DisableSystemTextJsonSourceGenerator` true, `PlatformTarget` x86.

## 2. Step 1: the results

### 2.1 The stop, and the re-run with batch H2

The first run's `ProbeHost` (an x86 .NET Framework exe, so the SDK gives it a `win-x86` runtime target, as `OpenPmsDental` and every exe and
test host with a `.NETFramework,Version=v4.8/win-x86` section in its lock file have) failed its restore:

```
error NU1101: Unable to find package runtime.any.System.IO. No packages exist with this id in source(s): opms-offline
error NU1101: Unable to find package runtime.any.System.Runtime. No packages exist with this id in source(s): opms-offline
```

`System.IO` and `System.Runtime` 4.3.0 (dependencies of `System.Security.Cryptography.Algorithms` 4.3.0, which `System.Net.Http` 4.3.4
brings through `System.Security.Cryptography.X509Certificates` 4.3.0) depend, for any runtime identifier, on those two packages (the runtime
graph in `runtime.json` of `Microsoft.NETCore.Targets` 1.1.3). A library has no runtime target, so the lead's batch H probe, a library, did
not show them.

**The re-run (2026-11-14, after H2):** the same two projects restore and build with exit 0, no warning at all. `ProbeHost`'s lock file:
the 29 packages of 2.2 plus the project, and a `win-x86` section of 9 packages (System.IO, System.IO.Compression, System.Net.Http,
System.Runtime, System.Security.Cryptography.Algorithms, .Encoding, .X509Certificates 4.3.x, `runtime.any.System.IO` and
`runtime.any.System.Runtime` 4.3.0). Nothing else new resolved; `System.Numerics.Vectors` 4.5.0 and `System.Runtime.CompilerServices.Unsafe`
4.5.3, the two older packages the lead left out of batch H, are not needed (the pinning gives the feed's 4.6.1 and 6.1.2). Both H2 packages
hold only placeholders for .NET Framework (`lib/net45/_._`). The exe's output folder holds exactly the eight new DLLs of 2.3 and the nine
shared ones, and no `System.Net.Http.dll` or other 4.3.x file (2.6).

### 2.2 The resolved packages (the chart probe's restore, `.NETFramework,Version=v4.8`)

29 packages: the 20 of batch H and 9 the feed already has, each of the nine at its central version.

| Package | Version | Runtime asset (net48) | Note |
|---|---|---|---|
| HelixToolkit.Wpf | 3.1.2 | `lib/net48/HelixToolkit.Wpf.dll` | direct |
| HelixToolkit | 3.1.2 | `lib/net48/HelixToolkit.dll` | |
| HelixToolkit.Geometry | 3.1.2 | `lib/net48/HelixToolkit.Geometry.dll` | |
| HelixToolkit.Maths | 3.1.2 | `lib/net48/HelixToolkit.Maths.dll` | |
| CommunityToolkit.Common | 8.3.2 | `lib/netstandard2.0/CommunityToolkit.Common.dll` | |
| CommunityToolkit.Diagnostics | 8.3.2 | `lib/netstandard2.0/CommunityToolkit.Diagnostics.dll` | |
| CommunityToolkit.Mvvm | 8.3.2 | `lib/netstandard2.0/CommunityToolkit.Mvvm.dll` | analyzers and `buildTransitive` targets (2.5) |
| System.ComponentModel.Annotations | 5.0.0 | `lib/net461/System.ComponentModel.Annotations.dll` | |
| System.Net.Http | 4.3.4 | `lib/net46/System.Net.Http.dll` (`runtimes/win/lib/net46/...` for `win-x86`) | not copied (2.6) |
| System.Private.Uri | 4.3.2 | none | |
| System.Text.RegularExpressions | 4.3.1 | `lib/net463/System.Text.RegularExpressions.dll` | not copied |
| System.IO.Compression | 4.3.0 | `lib/net46/System.IO.Compression.dll` (`runtimes/win/...` for `win-x86`) | not copied |
| System.Security.Cryptography.Algorithms, .Encoding, .Primitives, .X509Certificates | 4.3.0 | `lib/net463`, `lib/net46`, `lib/net46`, `lib/net461` (`runtimes/win/...` for three of them on `win-x86`) | not copied |
| System.IO, System.Runtime | 4.3.0 | `lib/net462/System.IO.dll`, `lib/net462/System.Runtime.dll` | not copied |
| Microsoft.NETCore.Platforms 1.1.1, Microsoft.NETCore.Targets 1.1.3 | | `_._` | |
| Microsoft.Bcl.AsyncInterfaces, Microsoft.Extensions.DependencyInjection.Abstractions, Microsoft.Extensions.Logging.Abstractions, System.Diagnostics.DiagnosticSource | 10.0.15 | `lib/net462` | the central versions (the packages ask for 8.0.0 and 8.0.2) |
| System.Buffers 4.6.1, System.Memory 4.6.3, System.Numerics.Vectors 4.6.1, System.Runtime.CompilerServices.Unsafe 6.1.2, System.Threading.Tasks.Extensions 4.6.3 | | `lib/net462` | the central versions (the packages ask for 4.5.5 and 6.0.0) |

### 2.3 The DLLs in `bin\Debug`

The build's conflict resolution drops every 4.3.x file: the .NET Framework 4.8 reference assemblies of those names are newer (System.Net.Http
4.2.0.0 against the package's 4.1.1.3; System.Security.Cryptography.Algorithms 4.3.0.0 against 4.2.1.0; the facades likewise), so no
`System.Net.Http.dll`, `System.IO.Compression.dll`, `System.Text.RegularExpressions.dll`, `System.IO.dll`, `System.Runtime.dll` or
`System.Security.Cryptography.*.dll` is in any output. What is there (all IL-only, none 32-bit-required; the same bytes in the chart probe,
the exe probe and the release staging folder of section 9):

| File | Assembly version | File version | Bytes | SHA-256 |
|---|---|---|---|---|
| `HelixToolkit.Wpf.dll` | 3.1.2.0 | 3.1.2.0 | 581,632 | `424b0d930bd90727442fbbef95da0f0a8196f45a0591f88f64446a12097e1003` |
| `HelixToolkit.dll` | 3.1.2.0 | 3.1.2.0 | 41,472 | `44dbd25cc27f73ba674313079793feacb34a6037adade1d9b0bb51b6a3a5213d` |
| `HelixToolkit.Geometry.dll` | 3.1.2.0 | 3.1.2.0 | 101,888 | `dd63000af7d76050838457b377690c9e909ba2d9e3c6e350f3fbd7c7522f2393` |
| `HelixToolkit.Maths.dll` | 3.1.2.0 | 3.1.2.0 | 194,048 | `967c46716b938c7fdcb6098438439592f2287348601dcca64fddcbef725835c1` |
| `CommunityToolkit.Common.dll` | 8.3.0.0 | 8.3.2.1 | 36,136 | `45790beffe3a6eb46ca8c01ff70bf4adb08aa6bcebeda3a9423a79813b7aef8d` |
| `CommunityToolkit.Diagnostics.dll` | 8.3.0.0 | 8.3.2.1 | 99,624 | `3324d1a8ac5d20223f68e07c8b4f498b2997d8a0b1bc26fab9d4e197c9add08b` |
| `CommunityToolkit.Mvvm.dll` | 8.3.0.0 | 8.3.2.1 | 115,496 | `e899bb3f1c4c484e4e901bbb889ae9a4a51c0727499f576fe33ff40ffd9b862a` |
| `System.ComponentModel.Annotations.dll` | 4.2.1.0 | 4.6.26515.06 | 43,152 | `04113c8549185519f3202790ceb23df609644872b9c249a56d2bcf59566102c4` |

Plus the nine shared files, each the central version's file the program already ships.

### 2.4 The assembly references of each new DLL

| DLL | References |
|---|---|
| `HelixToolkit.Wpf` | CommunityToolkit.Diagnostics 8.3.0.0, HelixToolkit 3.1.2.0, HelixToolkit.Geometry 3.1.2.0, HelixToolkit.Maths 3.1.2.0, mscorlib, PresentationCore, PresentationFramework, **SRTM 0.0.8.0** (no package holds it; terrain types only), System, System.Core, System.Memory 4.0.1.2, System.Numerics, System.Xaml, System.Xml, WindowsBase |
| `HelixToolkit` | CommunityToolkit.Diagnostics 8.3.0.0, Microsoft.Extensions.Logging.Abstractions 8.0.0.2, mscorlib, System, System.Core, System.Memory 4.0.1.2, System.Numerics |
| `HelixToolkit.Geometry` | CommunityToolkit.Diagnostics 8.3.0.0, HelixToolkit 3.1.2.0, HelixToolkit.Maths 3.1.2.0, mscorlib, System, System.Core, System.Numerics |
| `HelixToolkit.Maths` | HelixToolkit 3.1.2.0, Microsoft.Extensions.Logging.Abstractions 8.0.0.2, mscorlib, System, System.Core, System.Memory 4.0.1.2, System.Numerics |
| `CommunityToolkit.Common` | netstandard 2.0.0.0 |
| `CommunityToolkit.Diagnostics` | netstandard 2.0.0.0, System.Memory 4.0.1.2, System.Runtime.CompilerServices.Unsafe 4.0.4.1 |
| `CommunityToolkit.Mvvm` | Microsoft.Bcl.AsyncInterfaces 8.0.0.0, netstandard 2.0.0.0, System.Buffers 4.0.2.0, System.ComponentModel.Annotations 4.2.0.0, System.Memory 4.0.1.2, System.Runtime.CompilerServices.Unsafe 6.0.0.0, System.Threading.Tasks.Extensions 4.2.0.1 |
| `System.ComponentModel.Annotations` | mscorlib, System.ComponentModel.DataAnnotations |

The references below the shipped versions bind through the programs' existing redirects, except `System.ComponentModel.Annotations`
4.2.0.0 to 4.2.1.0, which is new (section 7). No Helix file references `CommunityToolkit.Mvvm` or `CommunityToolkit.Common`, so nothing
loads those two or `System.ComponentModel.Annotations` (owner question O2, section 12).

### 2.5 Analyzer and build assets that reach the project

The plan expected Helix's `exclude="Build,Analyzers"` to keep CommunityToolkit.Mvvm's generator out. **It does not**, in the chart and in
every project above it: the compiler gets `/analyzer:` for `communitytoolkit.mvvm\8.3.2\analyzers\dotnet\roslyn4.3\cs\`
`CommunityToolkit.Mvvm.SourceGenerators.dll` and `...CommunityToolkit.Mvvm.CodeFixers.dll`, and the restore imports
`buildTransitive/CommunityToolkit.Mvvm.targets`. The same with transitive pinning off (a probe with the property false), so it is not the
pinning. No other analyzer reaches the probes: Microsoft.Extensions.Logging.Abstractions' generator is removed by
`DisableMicrosoftExtensionsLoggingSourceGenerator`, as in every project. Section 5 says what was done about both.

### 2.6 System.Net.Http (risk W2)

No Helix DLL references `System.Net.Http` (2.4): HelixToolkit's nuspec names the package for net48, but `HelixToolkit.dll` 3.1.2 never uses
it, so no binding redirect is needed. The program's `win-x86` target takes `runtimes/win/lib/net46/System.Net.Http.dll` and three
`runtimes/win/.../System.Security.Cryptography.*.dll` (and `System.IO.Compression.dll`) instead of the `lib` files, and the build's conflict
resolution drops those too: the exe probe's output, every built folder of the repository (section 8) and the release staging folder hold none.

## 3. Batch H2 (answered)

The owner approved H2 on 2026-11-14 ("Yes, download"); the lead fetched `runtime.any.System.IO` 4.3.0 and `runtime.any.System.Runtime`
4.3.0, checked their signatures and added them (`nuget-packages/BATCH-H2.txt`). The re-run of 2.1 restores with them; nothing else is
needed.

## 4. Step 2: what changed

| File | Change |
|---|---|
| `Directory.Packages.props` | `PackageVersion HelixToolkit.Wpf 3.1.2`, with why no other batch H or H2 package needs one (section 4.1) |
| `SparksToothChart/SparksToothChart.csproj` | `<PackageReference Include="HelixToolkit.Wpf" ExcludeAssets="buildTransitive" PrivateAssets="compile;contentfiles;analyzers;build" />` (sections 5 and 6) |
| `Directory.Build.targets` | `ODRemoveCommunityToolkitMvvmAnalyzers` and `ODRecordCompilerAnalyzers` (section 5) |
| `OpenPmsDental/app.config`, `tests/BindingRedirects.config`, and `CentralManager`, `DocumentationBuilder`, `SparksToothChart`, `UnitTests`, `WpfControlsOD` `app.config` | the `System.ComponentModel.Annotations` 0.0.0.0-4.2.1.0 redirect, only the element (section 7) |
| 42 `packages.lock.json` | written again by the lane's restore (section 4.1) |
| `tools/Build-Release.ps1` | the eight files in `ThirdPartyFiles` as `package:` files; `Get-ProgramPackageFiles` reads the program's `win-x86` target only (section 9) |
| `THIRD-PARTY-NOTICES.txt` | the components "Helix Toolkit" and ".NET Community Toolkit"; System.ComponentModel.Annotations in ".NET libraries" (section 10) |
| `docs/third-party-binaries-audit.md` | "Packages added after the audit, never in `Required dlls`": the eight files, and why the others ship nothing |
| tests | section 11 |
| docs | this note; the plan's TC-4 row, sections 2, 3 and 9; `docs/release-notes/next.md` (one line: the two new licence entries) |

`FormLicense.cs`, `Properties/Resources.resx`, `Resources.Designer.cs` and `Resources/*.txt` are unchanged: the Helix entry needs no vendor
text (section 10).

### 4.1 Pinning and the restore proof

The plan's premise that stage 2 leaves `CentralPackageTransitivePinningEnabled` off was out of date: S2-1 turned it on centrally. So, by the
lead's decision, no chart-scoped pinning setting. The proof the row asks for: every one of the 155 lock files was deleted and the lane's
locked restore of the whole `OpenDental.Local.slnf` wrote them again (a project without a lock file restores and writes one, as S2-1 to
S2-3 did), with the build: 155 projects restored, **no NU warning of any number** (no NU1603, NU1605, NU1608 or NU1107), 0 errors. 42 lock
files changed, 113 came out the same. The 42 are `SparksToothChart` and every project that references it, directly or not: CentralManager,
DocumentationBuilder, ImagingCapture, OpenPmsDental, UnitTests, WpfControlsOD and 35 test projects. Each gains exactly the 20 batch H
packages (`HelixToolkit.Wpf` `Direct` in SparksToothChart, `CentralTransitive` elsewhere; the rest `Transitive`), and the four with a
`win-x86` target (OpenPmsDental, CentralManager, DocumentationBuilder, UnitTests) gain the 9 `win-x86` entries of 2.1 with the two H2
packages. No entry changed version and none went away (`git diff --stat`: 6,807 insertions, no deletion). **The nine shared packages are at
their central versions in every changed lock file** (Microsoft.Bcl.AsyncInterfaces, Microsoft.Extensions.DependencyInjection.Abstractions,
Microsoft.Extensions.Logging.Abstractions, System.Diagnostics.DiagnosticSource 10.0.15; System.Buffers 4.6.1, System.Memory 4.6.3,
System.Numerics.Vectors 4.6.1, System.Runtime.CompilerServices.Unsafe 6.1.2, System.Threading.Tasks.Extensions 4.6.3), checked by a script
over the 42 files. Only `HelixToolkit.Wpf` needs a `PackageVersion`: every other package of the closure has one version in the whole graph.

## 5. CommunityToolkit.Mvvm's analyzers and buildTransitive targets

**The analyzers** (the lead's decision): `ODRemoveCommunityToolkitMvvmAnalyzers` in `Directory.Build.targets` removes every `Analyzer` item
whose `NuGetPackageId` is `CommunityToolkit.Mvvm`, `AfterTargets="ResolvePackageDependenciesForBuild;ResolveNuGetPackageAssets"`, as the
Logging and Text.Json packages remove theirs. It is in `Directory.Build.targets` because every project that gets the package needs it:
`PrivateAssets="analyzers"` on the chart's reference does not stop them reaching the projects above (a probe: they still got both), and the
package has no switch of its own. `ODRecordCompilerAnalyzers` (`BeforeTargets="CoreCompile"`, SDK-style projects) writes each compilation's
analyzers to `obj\<configuration>\<project>.analyzers.txt` (package id and file name per line). Probed twice with the repository's new
`Directory.Build.targets`: with the removal target neither compiler line nor record names a Mvvm analyzer; without it both name the two
Mvvm analyzers, so the record says what csc got. (Both probes also show Logging.Abstractions' generator, because there the package comes
only through Helix, whose buildTransitive files are excluded, so its switch does not arrive; in the repository every project that gets Helix
gets the package through the other projects too, see below.) In the repository build all 155 records are empty (no project gets any
analyzer), and `HelixPackageTests.Compilations_GetNoCommunityToolkitMvvmAnalyzer` checks that no record names Mvvm, Logging.Abstractions or
System.Text.Json.

**The buildTransitive targets** (`CommunityToolkit.Mvvm.targets`, which imports three files):
- `FeatureSwitches.targets`: a default for `MvvmToolkitEnableINotifyPropertyChangingSupport`, a `CompilerVisibleProperty` for it (read
  only by the generators) and, for net6.0 and later only, a `RuntimeHostConfigurationOption`.
- `SourceGenerators.targets`: targets that remove the package's analyzers when no Roslyn or a Roslyn older than 4.0 is found (they run a
  `GetAssemblyIdentity` on the compiler's task assembly on every build), choose the roslyn4.3 or roslyn4.0 pair for a classic project, and
  warn for a packages.config project.
- `WindowsSdk.targets`: a Windows SDK projection check for net8.0-windows10.0.17763 and later only.

None does anything a net48 build here needs. **They are kept out too**, with `ExcludeAssets="buildTransitive"` on the chart's reference,
which applies to Helix's whole closure in the chart and in every project above it (a probe: the import is gone from both probe projects).
Besides being useless, they would break the tests' project-file reader (`tests/ProjectFileTests/ProjectFile.cs`), which reads the package
build files the restore imports and does not model `[MSBuild]::IsTargetFrameworkCompatible`, so every project above the chart would throw in
ProjectFileTests, LibraryBindingTests and ReleaseFlowTests. A direct `CommunityToolkit.Mvvm` reference with `ExcludeAssets` in the chart
does not do it (a probe: the exe above still imported the targets). The other packages of Helix's closure with buildTransitive files are
the nine shared ones, which every project that gets Helix also gets through the other projects with their files: the 408 package-import
lines of all `obj\*.nuget.g.props/.targets` are the same before and after (compared file by file), so the Logging and Text.Json switches
still arrive everywhere (and the records hold no generator of theirs).

## 6. Compile references stay in the chart (found in step 2)

The first build of the repository with Helix failed in OpenPmsDental: `FormFamilyBalancer.cs(102,42): error CS1739: The best overload for
'AppendFormatted' does not have a parameter named 'format'` (four lines, `$"{x:N0}"`). `HelixToolkit.dll` declares a **public**
`System.Runtime.CompilerServices.DefaultInterpolatedStringHandler` (a polyfill; Mono.Cecil listing of the public System types of the eight
files: it is the only one), and the compiler of every project that references the DLL binds interpolated strings to it, whose
`AppendFormatted` has no `format` parameter. Reproduced in a probe (an exe with `LangVersion` 8.0 and one `$"{count:N0}"` above the chart
probe: CS1739) and fixed there by `PrivateAssets="compile;contentfiles;analyzers;build"` on the chart's reference: the projects above get
the Helix files at run time (copied to their output, as before) but no compile reference to them. Only `SparksToothChart` compiles against
Helix; it has no interpolated string today (section 13 has the rule).

## 7. The System.ComponentModel.Annotations redirect

`CommunityToolkit.Mvvm` references `System.ComponentModel.Annotations` 4.2.0.0, and the package's file is 4.2.1.0. The redirect element
(`0.0.0.0-4.2.1.0` to `4.2.1.0`, token `b03f5f7f11d50a3a`) went into `OpenPmsDental/app.config` and `tests/BindingRedirects.config` as the
lead decided, after the `System.Buffers` element, without touching the comments BUILD-HYGIENE wrote. The binding check
(`BindingRedirectTests.Libraries_EveryReferenceBetweenThemResolves_InEveryConfigThatUsesThem`) also asks for it in the config of every other
project that gets the package and has a config: CentralManager, DocumentationBuilder, SparksToothChart (its `.dll.config` ships), UnitTests
and WpfControlsOD (`.dll.config` ships). Before TC-4 the check bound every library file in every program's config, which would have asked
for the redirect in all 19 configs, `OpenPmsDentalDbSettings/app.config` (another stream's file) among them; it now binds the library files
each project gets (its closure, as `SolutionProjects.LibrariesInClosure` reads it, packages of its lock file included), and the shared test
config every library file. The same for the built configs of the other programs in the program folder (`ServiceManager.exe.config`,
`OpenPmsDentalDbSettings.exe.config`), which neither gets CommunityToolkit.Mvvm. The built `OpenPmsDental.exe.config` has the redirect.

## 8. Risk W2

No `System.Net.Http.dll` or `System.Security.Cryptography.*.dll` (or any other 4.3.x file) is in any built folder: the Debug folders of all
42 projects that get Helix, compared before and after (a SHA-256 list of every `bin\Debug` file: 8 new files in each of the 42 folders,
nothing gone, none of those names anywhere), the Release output and the staged release (section 9), and `HelixPackageTests.
BuiltFolders_HoldNoFileOfTheSystemNetHttpPackages` checks every `bin\Debug` and `bin\Release` of the projects whose lock file names
System.Net.Http, and no `runtimes` folder. The cause is the SDK's conflict resolution, not a setting: the .NET Framework 4.8 assemblies of
those names are newer. `NuGetFiles` models exactly that (section 11). `HelixTypes_TheChartWillUse_LoadFromTheProgramFolder` loads the types
the chart will use in the program's setting (an AppDomain on `OpenPmsDental\bin\Debug` with `OpenPmsDental.exe.config`): a `MeshBuilder`
box with `ToMesh().ToWndMeshGeometry3D(true)` gives a WPF `MeshGeometry3D` of 24 positions, every Helix assembly loads from the program
folder, and none of the 4.3.x names loads from there.

## 9. The staged-file diff (release dry run)

Run in this worktree, through the lane, once on the branch's start (`7c124ea36`, = `main` `5a7e16d8f` plus this note) and once after the
tests commit (`4b9ff3fca`):

```
powershell -NoProfile -ExecutionPolicy Bypass -File tools\Build-Release.ps1 -BuildLane <odbuild.ps1> -LaneWaitMinutes 20 -OutDir <scratch>\rel-before -SkipInstaller
powershell -NoProfile -ExecutionPolicy Bypass -File tools\Build-Release.ps1 -BuildLane <odbuild.ps1> -LaneWaitMinutes 20 -OutDir <scratch>\rel-after -SkipInstaller
```

Both exit 0 (Release build, the staging checks with every package file compared with its package, the source archive); then the SHA-256 of
every staged file. **81 staged files before, 89 after:** the eight files of 2.3 added, byte for byte the hashes there; none removed. Changed:
`THIRD-PARTY-NOTICES.txt` (29,738 to 33,448 bytes), `OpenPmsDental.exe` (13,440,512 to 13,444,096: the embedded notices and the build stamp),
`OpenPmsDental.exe.config`, `SparksToothChart.dll.config` and `WpfControlsOD.dll.config` (each 257 bytes longer: the Annotations redirect),
and the two `XmlSerializers` DLLs, which SGen numbers anew with every build (S2-1). `SparksToothChart.dll` is unchanged. The build id
changes with the exe (`8cabb075...` before, `26960ba2...` after).

`Get-ProgramPackageFiles` had to change: it read the runtime assets of every target of the program's restore, and System.Net.Http 4.3.4 has
`lib/net46/System.Net.Http.dll` in the framework's target and `runtimes/win/lib/net46/System.Net.Http.dll` in the `win-x86` one, which it
reported as "Two package files named System.Net.Http.dll" and failed the release. It now reads the `win-x86` target only, the one the SDK
copies from for the program (every package is in it). None of those files is on the allow-list.

## 10. Notices and the licence list

- **Helix Toolkit** (new component; files HelixToolkit, .Geometry, .Maths, .Wpf; 3.1.2; Helix Toolkit contributors; MIT; source
  helix-toolkit at tag v3.1.2, commit `0d10a9fc2`). Its `Notices:` field holds, inside the entry, the BSD notice of `LoopSubdivision`
  (copied from the package's `HelixToolkit.Wpf.xml`, which quotes it: "Copyright (c) 2009, Jesus Mena", the two conditions and the
  disclaimer), the MIT-licensed code derived from SharpDX (Alexandre Mutel) and SlimMath (SlimDX Group) in HelixToolkit.Maths, the MIT origin
  of `MeshSimplification` (Fast-Quadric-Mesh-Simplification by Sven Forstmann, the URL the package's XML names), and `CanonicalSplineHelper`'s
  2009 copyright line without a licence. The MIT text follows from the `License: MIT License` field.
- **.NET Community Toolkit** (new component; the three CommunityToolkit files; 8.3.2; .NET Foundation and Contributors; MIT; source
  CommunityToolkit/dotnet at the packages' commit), with the notice of PedroLamas/DeferredEvents 1.0.4 (MIT, Copyright (C) 2017 Pedro Lamas)
  that the packages' `ThirdPartyNotices.txt` gives for code in CommunityToolkit.Common.
- **System.ComponentModel.Annotations** 5.0.0 joins ".NET libraries" (MIT, dotnet/runtime).
- **The licence-list rule.** No new entry or resource is named "BSD" or "Microsoft"; the BSD notice is inside the Helix entry, so the
  component needs no vendor text (one would replace its MIT text, `LicenseList.GetLicenseText`), and `FormLicense.cs` and the resources are
  unchanged. The removed-entry and removed-resource lists of `ReleaseIdentityTests` are not edited. The licence window shows the two new
  entries (one line in `docs/release-notes/next.md`).
- Not shipped, so not noticed: the System.* 4.3.x packages, `Microsoft.NETCore.*`, `runtime.any.*` (the binaries audit says why).

## 11. Tests

**New:**
- `tests/ReleaseFlowTests/HelixPackageTests.cs` (5): the eight files are on the allow-list as `package:` files and no 4.3.x file or
  `SRTM.dll` is; the program folder holds each, byte for byte its package's file, IL-only without 32-bit-required or -preferred (CLI header
  flags read from the bytes); risk W2 over every built folder; the analyzer records; the Helix load in the program's AppDomain setting.
- `tests/LibraryBindingTests/HelixReferenceTests.cs` (3): the eight are their packages' library files at their versions; every reference is
  a library file (at or below its version), part of the .NET Framework, or exactly `HelixToolkit.Wpf -> SRTM 0.0.8.0` (the one allowed
  missing reference), and none is System.Net.Http; every type of HelixToolkit.Wpf that names a type or member of SRTM (base types, fields,
  properties, signatures, locals, method bodies) is a terrain or hgt type.
- `tests/ProjectFileTests/NuGetFilesTests.cs`: `ReadCopiedFiles_TheRuntimeTarget_WithoutWhatTheFrameworkReplaces` (synthetic assemblies).

**Changed** (the plan said "new files only" for these folders; that holds for TC-9b, but a new package changes the lists that pin the
package files):
- `tests/ProjectFileTests/NuGetFiles.cs`: `ReadCopiedAssets` reads the runtime identifier's target when the restore has one, as the SDK
  copies; `ReadCopiedFiles` leaves out a managed file whose name the .NET Framework 4.8 reference assemblies (or their facades) hold at a
  higher assembly version (`IsReplacedByTheFramework`; equal versions throw as not modelled). Without this, `ReadCopiedFiles` of the program
  threw ("two package files are named System.Net.Http.dll") in ReleaseFlowTests and LibraryBindingTests, and in `tests/LibraryUpdateTests`
  (`LibraryFilesTests.PackageFiles_AreTheApprovedVersions`), which links the same file; that project belongs to the parallel stream and was
  neither edited nor run here.
- `tests/LibraryBindingTests/TestSupport.cs` (`PackageFiles`): the eight packages mapped; the fourteen that bring no file the build keeps
  listed in `_dictNoLibraries` with their reasons (System.Net.Http, System.IO, System.IO.Compression, System.Runtime, the four Cryptography
  packages, System.Text.RegularExpressions, System.Private.Uri, Microsoft.NETCore.Platforms and .Targets, runtime.any.System.IO and
  .Runtime). Its comment asks for the lead's agreement for such entries (owner question O3).
- `RequiredDllsTests.cs` and `ReleaseFlowTests/PackageFilesTests.cs`: the package-file counts 30 to 38; the components and not-shipped
  packages mapped; the `ReadCopiedAssets` fixture names System.ValueTuple in its `win-x86` target too, as a real restore does.
- `ReleaseListTests.cs`: `SRTM` joins the known gaps, with why.
- `BindingRedirectTests.cs`: each config binds the library files its project gets (section 7), and the Annotations redirect is checked in
  `OpenPmsDental/app.config` and `tests/BindingRedirects.config`.
- `tests/AttributionTests/ReleaseIdentityTests.cs`: the two components and eight files; the Helix entry shows the MIT text, the BSD notice
  and the origins, and has no vendor text; the toolkit entry shows the DeferredEvents notice.

**Targeted runs** (`run-tests.ps1 -Mode Targeted`, results in `.git/od-tools/test-results/tc4-1-*.txt`): see section 14.

## 12. Owner and lead questions

- **O1. Notice lines read from the package, not the source.** No Helix source is in the offline folder. The `LoopSubdivision` notice is the
  text the package's `HelixToolkit.Wpf.xml` quotes; the SharpDX, SlimMath and Fast-Quadric-Mesh-Simplification lines name their authors
  without years, and `CanonicalSplineHelper`'s author is not named, because their source headers could not be read here. **Recommended:**
  the lead compares the Helix entry with the files at tag v3.1.2 (`0d10a9fc2`) and adds the exact copyright lines.
- **O2. Three shipped files nothing loads.** No Helix DLL references `CommunityToolkit.Mvvm.dll` or `CommunityToolkit.Common.dll`, and only
  CommunityToolkit.Mvvm references `System.ComponentModel.Annotations.dll` (2.4): about 195 KB the program never loads, and the reason for
  the redirect of section 7. They ship because the build copies every runtime file of the closure, as for every package here.
  **Recommended:** keep them (the owner's "ship the DLLs the staged list needs" read as what the build stages; dropping them needs a target
  that removes copy-local files, and TC-5 or later may use the toolkit through Helix's API).
- **O3. The no-library list.** `PackageFiles._dictNoLibraries` says a new entry needs the lead's agreement; TC-4 adds fourteen, each the
  framework's own or placeholders, checked by `HelixPackageTests` in every built folder.

## 13. Rules for the chart's Helix use (for the later briefs)

These hold from TC-4's merge on; no chart code uses Helix in TC-4 itself.

- Use `MeshBuilder` (namespace `HelixToolkit.Geometry`, float, `System.Numerics`) with `ToMesh().ToWndMeshGeometry3D(true)` (the load test
  calls exactly that).
- Never use `ConverterExtensions.ToWndMatrix3D`: row 3 reads "M31, M32, M32, M34" in 3.1.2 and on develop3.
- Never use `AddExtrudedGeometry` (#2459, fix unreleased).
- No `BillboardTextVisual3D`, `TextCreator` or screen-space visuals.
- 3.x API only: 2.x samples do not compile against 3.x.
- No terrain types (`HgtTerrainModel`, `TerrainModel`, `TerrainVisual3D`, `TerrainTexture`): they need `SRTM.dll`, which ships nowhere; the
  reference tests allow exactly that one missing reference.
- Only `SparksToothChart` compiles against Helix (`PrivateAssets` with `compile`). A project that needs Helix types (a test project of the
  chart, say) adds its own `PackageReference Include="HelixToolkit.Wpf"` with the same `ExcludeAssets`, and then has the next rule too.
- **No interpolated strings in `SparksToothChart`** (or any project that compiles against Helix): they bind to HelixToolkit.dll's public
  `DefaultInterpolatedStringHandler`, and one with a format (`{x:N0}`) fails with CS1739. Use `string.Format` or `ToString(format)`.
- CommunityToolkit.Mvvm's generators never run (`Directory.Build.targets`): no `[ObservableProperty]` or `[RelayCommand]` code.

## 14. Builds and Targeted runs

- **Builds** (the lane, `OpenDental.Local.slnf`, Debug x86, Build Tools 2026 MSBuild, SDK 10.0.114): before the change (the branch's start)
  0 errors, 353 warnings; the restore that wrote the lock files again, 0 errors after section 6's fix (the build before it failed with the
  four CS1739 errors); the final `-Rebuild` on `6fb7e677d`, 0 errors, 353 warnings, the same 353 distinct warning lines as before (only the
  random names of WpfControlsOD's temporary markup project differ). Its log (normal verbosity, every compiler command line) holds **no
  `/analyzer:` at all**, so no compilation, the markup project's included, got a CommunityToolkit.Mvvm analyzer.
- **Release dry runs** (section 9): both exit 0.
- **Targeted runs** (`run-tests.ps1 -Root . -Mode Targeted -Include ReleaseFlowTests,LibraryBindingTests,AttributionTests,
  ToothChartImageTests,ProjectFileTests,NoHqTests`, on the final rebuild; `.git/od-tools/test-results/tc4-1-final.txt`): all pass.
  AttributionTests 50/50, LibraryBindingTests 20/20, ProjectFileTests 133 passed and 1 skipped (the opt-in baseline check), NoHqTests
  57/57, ReleaseFlowTests 128/128 (its database tests on scratch databases of the local MariaDB), ToothChartImageTests 174 passed and 4
  skipped (the opt-in recorder, comparison, gallery and on-screen timing tests), unchanged: Helix changes no pixel. Not run, by the
  coordinator's rules: Core and Full, InitSqlTests, ForkMigrationTests, and the parallel stream's DbConnectionSecurityTests and
  LibraryUpdateTests (the latter reads `NuGetFiles.ReadCopiedFiles`, section 11).
