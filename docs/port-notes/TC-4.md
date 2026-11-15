# Port note: TC-4 (Helix Toolkit 3.1.2), stopped at step 1 for a batch H2

Package TC-4 of the WPF tooth chart track ([docs/specs/wpf-tooth-chart-plan.md](../specs/wpf-tooth-chart-plan.md), the TC-4 row of
section 4, sections 3, 9 and 10): branch `chart/tc4-1` from `main` (`5a7e16d8f`). **Status: stopped after step 1, as the row says.** The
offline check finds that the program's restore needs two packages that batch H does not have, so TC-4 waits for a batch H2 (O-17). No
project file, package version, lock file, release list, notice or test was changed; this note is the only file of the branch. Nothing was
downloaded and no program was started.

## 1. Step 1: how it was run

A scratch folder outside the repository (the session's scratch folder, `tc4\step1`) held copies of the repository's `nuget.config`,
`global.json`, `Directory.Build.props`, `Directory.Build.targets`, `Directory.Packages.props` (plus one line,
`<PackageVersion Include="HelixToolkit.Wpf" Version="3.1.2" />`) and `tools\Initialize-BuildEnvironment.ps1`, and two probe projects:

- `SparksToothChart\SparksToothChart.csproj`: the chart project's own settings (net48, `LangVersion` 7.3, `Platforms` AnyCPU;x86, its ten
  framework references), `<PackageReference Include="HelixToolkit.Wpf" />`, no project references and no SharpDX or Tao files; one
  source file without Helix code.
- `ProbeHost\ProbeHost.csproj`: a net48 `WinExe` that references the probe chart, as `OpenPmsDental` references `SparksToothChart`.

Both were restored and built through the build lane (`odbuild.ps1 -Root <scratch> -Solution <project> -WaitMinutes 20`: Build Tools 2026
MSBuild, SDK 10.0.114, locked restore, `OPMS_NUGET_OFFLINE` = the repository's offline folder, the default global packages folder). The
repository's settings that matter here came along unchanged: `CentralPackageTransitivePinningEnabled` true (on in every SDK-style project
since S2-1), `DisableMicrosoftExtensionsLoggingSourceGenerator` and `DisableSystemTextJsonSourceGenerator` true, `PlatformTarget` x86.

## 2. Step 1: the results

### 2.1 The stop: the program's `win-x86` target needs two packages outside batch H

`ProbeHost` (an x86 .NET Framework exe, so the SDK gives it a `win-x86` runtime target, as `OpenPmsDental` and the 14 other exes and test
hosts with a `.NETFramework,Version=v4.8/win-x86` section in their lock files have) fails its restore:

```
error NU1101: Unable to find package runtime.any.System.IO. No packages exist with this id in source(s): opms-offline
error NU1101: Unable to find package runtime.any.System.Runtime. No packages exist with this id in source(s): opms-offline
```

Why: `System.IO` 4.3.0 and `System.Runtime` 4.3.0 (dependencies of `System.Security.Cryptography.Algorithms` 4.3.0, which
`System.Net.Http` 4.3.4 brings through `System.Security.Cryptography.X509Certificates` 4.3.0) depend, for any runtime identifier, on
`runtime.any.System.IO` 4.3.0 and `runtime.any.System.Runtime` 4.3.0. The mapping is the runtime graph in `runtime.json` of
`Microsoft.NETCore.Targets` 1.1.3 (batch H): for `win-x86` (imports `win`, `any`, `base`, from `Microsoft.NETCore.Platforms` 1.1.1) it names
24 `runtime.*` packages, and these two are the only ones whose base package is in the graph. A library has no runtime target, so the chart
project itself restores (2.2); the lead's batch H probe (`BATCH-H.txt`, a net48 probe project) was such a library, which is why the list
lacks them. Every exe and test host above `SparksToothChart` would fail the same way, so step 2 cannot start.

Not resolved, as the record expected: `System.Numerics.Vectors` 4.5.0 and `System.Runtime.CompilerServices.Unsafe` 4.5.3, the two older
packages the lead left out. The repository's transitive pinning gives the feed's 4.6.1 and 6.1.2 (2.2), so neither is needed.

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
| CommunityToolkit.Mvvm | 8.3.2 | `lib/netstandard2.0/CommunityToolkit.Mvvm.dll` | build asset `buildTransitive/CommunityToolkit.Mvvm.targets` (2.5) |
| System.ComponentModel.Annotations | 5.0.0 | `lib/net461/System.ComponentModel.Annotations.dll` | |
| System.Net.Http | 4.3.4 | `lib/net46/System.Net.Http.dll` (`runtimes/win/lib/net46/...` for `win-x86`) | not copied (2.3) |
| System.Private.Uri | 4.3.2 | none | |
| System.Text.RegularExpressions | 4.3.1 | `lib/net463/System.Text.RegularExpressions.dll` | not copied |
| System.IO.Compression | 4.3.0 | `lib/net46/System.IO.Compression.dll` (`runtimes/win/...` for `win-x86`) | not copied |
| System.Security.Cryptography.Algorithms, .Encoding, .Primitives, .X509Certificates | 4.3.0 | `lib/net463`, `lib/net46`, `lib/net46`, `lib/net461` (`runtimes/win/...` for three of them on `win-x86`) | not copied |
| System.IO, System.Runtime | 4.3.0 | `lib/net462/System.IO.dll`, `lib/net462/System.Runtime.dll` | not copied; their `runtime.any.*` packages are 2.1's stop |
| Microsoft.NETCore.Platforms 1.1.1, Microsoft.NETCore.Targets 1.1.3 | | `_._` | |
| Microsoft.Bcl.AsyncInterfaces, Microsoft.Extensions.DependencyInjection.Abstractions, Microsoft.Extensions.Logging.Abstractions, System.Diagnostics.DiagnosticSource | 10.0.15 | `lib/net462` | the central versions (the packages ask for 8.0.0 and 8.0.2) |
| System.Buffers 4.6.1, System.Memory 4.6.3, System.Numerics.Vectors 4.6.1, System.Runtime.CompilerServices.Unsafe 6.1.2, System.Threading.Tasks.Extensions 4.6.3 | | `lib/net462` | the central versions (the packages ask for 4.5.5 and 6.0.0) |

The restore and the build gave no warning at all (no NU1603, NU1605, NU1608 or NU1107).

### 2.3 The DLLs in `bin\Debug` of the chart probe

The build's conflict resolution drops every 4.3.x framework-facade package file (the .NET Framework 4.8 reference assemblies are newer),
so no `System.Net.Http.dll`, `System.IO.Compression.dll`, `System.Text.RegularExpressions.dll`, `System.IO.dll`, `System.Runtime.dll` or
`System.Security.Cryptography.*.dll` is in `bin`. What is there (all IL-only, none 32-bit-required):

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

Plus the nine shared files, each the central version's file the program already ships (`Microsoft.Bcl.AsyncInterfaces`,
`Microsoft.Extensions.DependencyInjection.Abstractions`, `Microsoft.Extensions.Logging.Abstractions`, `System.Diagnostics.DiagnosticSource`
10.0.0.15; `System.Buffers` 4.0.5.0, `System.Memory` 4.0.5.0, `System.Numerics.Vectors` 4.1.6.0, `System.Runtime.CompilerServices.Unsafe`
6.0.3.0, `System.Threading.Tasks.Extensions` 4.2.4.0). So the staged list would gain eight files: the four Helix DLLs, the three
CommunityToolkit DLLs and `System.ComponentModel.Annotations.dll`.

### 2.4 The assembly references of each new DLL

| DLL | References |
|---|---|
| `HelixToolkit.Wpf` | CommunityToolkit.Diagnostics 8.3.0.0, HelixToolkit 3.1.2.0, HelixToolkit.Geometry 3.1.2.0, HelixToolkit.Maths 3.1.2.0, mscorlib, PresentationCore, PresentationFramework, **SRTM 0.0.8.0** (not in any package; terrain code only), System, System.Core, System.Memory 4.0.1.2, System.Numerics, System.Xaml, System.Xml, WindowsBase |
| `HelixToolkit` | CommunityToolkit.Diagnostics 8.3.0.0, Microsoft.Extensions.Logging.Abstractions 8.0.0.2, mscorlib, System, System.Core, System.Memory 4.0.1.2, System.Numerics |
| `HelixToolkit.Geometry` | CommunityToolkit.Diagnostics 8.3.0.0, HelixToolkit 3.1.2.0, HelixToolkit.Maths 3.1.2.0, mscorlib, System, System.Core, System.Numerics |
| `HelixToolkit.Maths` | HelixToolkit 3.1.2.0, Microsoft.Extensions.Logging.Abstractions 8.0.0.2, mscorlib, System, System.Core, System.Memory 4.0.1.2, System.Numerics |
| `CommunityToolkit.Common` | netstandard 2.0.0.0 |
| `CommunityToolkit.Diagnostics` | netstandard 2.0.0.0, System.Memory 4.0.1.2, System.Runtime.CompilerServices.Unsafe 4.0.4.1 |
| `CommunityToolkit.Mvvm` | Microsoft.Bcl.AsyncInterfaces 8.0.0.0, netstandard 2.0.0.0, System.Buffers 4.0.2.0, System.ComponentModel.Annotations 4.2.0.0, System.Memory 4.0.1.2, System.Runtime.CompilerServices.Unsafe 6.0.0.0, System.Threading.Tasks.Extensions 4.2.0.1 |
| `System.ComponentModel.Annotations` | mscorlib, System.ComponentModel.DataAnnotations |

The references below the shipped versions bind through the programs' existing redirects, except `System.ComponentModel.Annotations`
4.2.0.0 to 4.2.1.0, which is new (`OpenPmsDental/app.config` and `tests/BindingRedirects.config` would need it, or the build's generated
redirect; step 2 checks it with `BindingRedirectTests`).

### 2.5 Analyzer and build assets that reach the project

The plan expected Helix's `exclude="Build,Analyzers"` to keep CommunityToolkit.Mvvm's generator out. **It does not.** The restore gives
`CommunityToolkit.Mvvm` the build asset `buildTransitive/CommunityToolkit.Mvvm.targets` (imported by the project's `nuget.g.targets`), and
the compiler gets `/analyzer:` for `communitytoolkit.mvvm\8.3.2\analyzers\dotnet\roslyn4.3\cs\CommunityToolkit.Mvvm.SourceGenerators.dll`
and `...CommunityToolkit.Mvvm.CodeFixers.dll` (the package's own targets choose the roslyn4.3 pair). No other analyzer reaches the
probe: Microsoft.Extensions.Logging.Abstractions' generator is removed by `DisableMicrosoftExtensionsLoggingSourceGenerator`, as in every
project today. Step 2 has to take the Mvvm generator out again in every project that gets Helix. The package's targets have no off switch
like the two `Disable...SourceGenerator` properties (they only remove the analyzers for a compiler older than Roslyn 4.0), so it would be an
`Analyzer Remove` by `%(NuGetPackageId)` == `CommunityToolkit.Mvvm` in a shared file; a setting only in `SparksToothChart.csproj` may not
reach the projects above it (the plan's review finding 15).

### 2.6 System.Net.Http (risk W2)

No Helix DLL references `System.Net.Http` at all (2.4): HelixToolkit's nuspec names the package, but `HelixToolkit.dll` 3.1.2 never uses it.
In the chart probe the package's file is not copied (2.3), so no binding redirect is needed for Helix. Still open, because the stop of 2.1
comes first: whether the program's `win-x86` target, which takes `runtimes/win/lib/net46/System.Net.Http.dll` and three
`runtimes/win/.../System.Security.Cryptography.*.dll` files instead of the `lib` files, also drops them; step 2 checks that in the
program's output once H2 is in the feed.

## 3. Owner question for the lead: batch H2

**H2. Two more packages for TC-4.** `runtime.any.System.IO` 4.3.0 and `runtime.any.System.Runtime` 4.3.0 from nuget.org, Microsoft's,
from the same 4.3.x family and under the same "Microsoft .NET Library" licence as the twelve 4.3.x packages of batch H (O-18). Every exe
and test host with a `win-x86` target needs them to restore once any project it references has Helix. The other 22 `runtime.*` packages
of the runtime graph are not needed (2.1). Their dependencies, and whether they bring any .NET Framework file (expected: none, only
placeholders; not checked, since they are not in the feed), are checked when they are fetched, as for batch H; a further package stops
TC-4 again. **Recommended:** approve both. No way around them was found inside batch H: the dependency comes from `System.Net.Http` 4.3.4,
which HelixToolkit 3.1.2 names for net48, and the program's `win-x86` target is where its restore takes WebView2's native loader
(`runtimes/win-x86/native`, which `tools/Build-Release.ps1` checks, S2-4), so it cannot simply go.

## 4. For the resumed TC-4 (after H2)

- **Pinning.** The plan's premise that stage 2 leaves `CentralPackageTransitivePinningEnabled` off no longer holds: S2-1 turned it on in
  `Directory.Packages.props` for every SDK-style project. So the nine shared packages already resolve to their central versions in every
  project that gets Helix (2.2), with no setting in `SparksToothChart.csproj`. Step 2's proof stays as the row asks: an unlocked restore of
  the whole slnf (every lock file deleted and written again by the lane's restore, as S2-1 to S2-3 did) with no NU1603, NU1605, NU1608 or
  NU1107, and the nine at their central versions in every changed lock file.
- **The Mvvm generator** (2.5) and the **`System.ComponentModel.Annotations` redirect** (2.4) are step 2 work that the row did not list.
- **The staged diff** should be the eight files of 2.3 and nothing else; `System.Net.Http.dll` and the other 4.3.x files must stay out of the
  program's output (2.6).
- The scratch probe is a quick way to repeat step 1 with H2 in the feed: the same two projects, through the lane.

## 5. Rules for the chart's Helix use (from the TC-4 row, for the later briefs)

These hold once TC-4 has merged; no chart code uses Helix in TC-4 itself.

- Use `MeshBuilder` (namespace `HelixToolkit.Geometry`, float, `System.Numerics`) with `ToMesh().ToWndMeshGeometry3D(true)`.
- Never use `ConverterExtensions.ToWndMatrix3D`: row 3 reads "M31, M32, M32, M34" in 3.1.2 and on develop3.
- Never use `AddExtrudedGeometry` (#2459, fix unreleased).
- No `BillboardTextVisual3D`, `TextCreator` or screen-space visuals.
- 3.x API only: 2.x samples do not compile against 3.x.
- The `SRTM.dll` reference inside `HelixToolkit.Wpf.dll` (2.4) has no file anywhere; the chart never runs terrain code, and the reference
  test of step 2 allows exactly that one missing reference.

## 6. Not done here (the row's step 2)

`Directory.Packages.props`, `SparksToothChart.csproj`, the lock files, `tools/Build-Release.ps1`, `THIRD-PARTY-NOTICES.txt`,
`docs/third-party-binaries-audit.md`, `ReleaseIdentityTests`, the new ReleaseFlowTests and LibraryBindingTests files, the redirect files, the
release dry run and the Targeted runs. The plan's TC-4 row is not marked done, and `docs/release-notes/next.md` is unchanged.
