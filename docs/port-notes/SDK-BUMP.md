# Port note: SDK-BUMP (the .NET SDK pin 10.0.114 -> 10.0.115)

The first run of the bump procedure of [docs/specs/net10-plan.md](../specs/net10-plan.md), section 2.4 (band (a), steps 2-4), on branch
`net10/sdk-bump-1` from `main` (`d22ff0de9`, the stage 3a gate). The S3b-0 port note ([S3b-0.md](S3b-0.md), "Consequences", S3b-1 (1))
asks for it before stage 3b starts: .NET 10.0.15 (2026-11-10) is a security release, and SDK 10.0.115 is the 10.0.1xx SDK that bundles it.
No program code, no schema change, no package version changed; nothing downloaded by this package; no program started. Date: 2026-11-15.

## Step 1 (done by the lead before this package)

SDK 10.0.115 (runtime 10.0.15) is installed per user in `%LOCALAPPDATA%\Microsoft\dotnet` beside 10.0.114 (`dotnet --list-sdks` lists
both), under the owner's standing approval of the monthly SDK patch (section 2.4). The download, its SHA-256 and its source are in the common
git folder's `od-tools\downloads\sdk-10.0.1xx-20261115\` (`SOURCES.txt`, `SHA256SUMS.txt`, `MANIFEST.txt`: SHA-512 as Microsoft's
`releases.json` gives it, `dotnet.exe` and `hostfxr.dll` Authenticode-valid). The runtime packs of batch D at 10.0.15 have been in the
offline folder since S3b-0. The per-user folder now has `host\fxr`, `shared\Microsoft.NETCore.App` and the packs (the x64 apphost, the
reference packs) at both 10.0.14 and 10.0.15.

## Step 2: the pin

- `global.json`: `sdk.version` and the `errorMessage` name 10.0.115; `rollForward` stays `disable`, `allowPrerelease` `false`.
- `README.md`, Building: the SDK line names 10.0.115 (with `dotnet-install.ps1 -Version 10.0.115`) and says each monthly patch moves it.
- `tools\Initialize-BuildEnvironment.ps1` reads the version from `global.json` and looks for `sdk\<version>\dotnet.dll`: unchanged. The lane
  script (the lead's) takes its environment from it: unchanged.

## What follows the pin in the tests

- **The project-file evaluator** (`tests/ProjectFileTests/ProjectFile.cs`) models the pinned SDK's props and targets. A diff of the two
  SDK folders (`sdk\10.0.114` and `sdk\10.0.115`, every `.props`, `.targets` and `.tasks` file) finds four changed files, all with version
  values only: `Microsoft.NETCoreSdk.BundledVersions.props` (`NETCoreSdkVersion`, `BundledNETCoreAppPackageVersion` 10.0.15, the known
  frameworks' and packs' versions 10.0.14 -> 10.0.15 and 9.0.22 -> 9.0.23, `_NetFrameworkHostedCompilersVersion`, and the duplicated
  entries of `_KnownRuntimeIdentiferPlatforms` and `_ExcludedKnownRuntimeIdentiferPlatforms`, a few more copies of the same names),
  `Microsoft.NETCoreSdk.BundledMSBuildInformation.props` (`BundledMSBuildVersion` 18.0.36 -> 18.0.37),
  `Microsoft.CodeAnalysis.NetAnalyzers.props` (`_NETAnalyzersSDKAssemblyVersion`) and the F# `Microsoft.FSharp.Core.NetSdk.props`
  (`FSCorePackageVersion`). No `.targets` file changed. Of these the evaluator models only `NETCoreSdkVersion`, which now reads 10.0.115;
  the line numbers its notes quote are the same. The "SDK 10.0.114" notes of `ProjectFile.cs`, `ImportsTests.cs`, `SdkComputedTests.cs`
  and the versioned `Sdk="Microsoft.NET.Sdk/<version>"` case of `ReaderLimitsTests.cs` name 10.0.115.
- **New test** `SdkComputedTests.Sdk_NETCoreSdkVersion_IsTheVersionGlobalJsonPins`: the evaluator's `NETCoreSdkVersion` is the version
  `global.json` pins (with `rollForward` `disable`), and a classic project's throws (not modelled). The next bump cannot leave the
  evaluator on the old SDK.
- **The compiler** stays Roslyn 5.0.0 (`Roslyn\bincore\csc.dll` 5.0.0-2.26522.126 in 10.0.114, 5.0.0-2.26529.108 in 10.0.115), so the
  notes in `docs/develop/third-party-source-builds.md` and `third-party/build/WeCantSpell.Hunspell/WeCantSpell.Hunspell.csproj` ("Roslyn 5.0 in
  the pinned SDK") only name the new pin.
- **Left as they are, on purpose:**
  - `tests/ReleaseFlowTests` (`RuntimeManifestTests`, `ReleaseGuardDbTests`, `Fixtures/Net10Manifest.example.txt`) and the S3a-10 note,
    `docs/develop/release-process.md`, `ForkReleases.cs` and `ReleaseManifest.cs` use `Microsoft.NETCore.App 10.0.14 win-x64` as a synthetic
    example of a manifest's `Runtime` line (a release dated 2027-02-01, synthetic hashes and runtime key). Nothing ties it to the SDK the
    build uses; the release-flow check that compares a build's runtime with the offline folder's newest patch is S3b-7's.
  - `Directory.Packages.props`, `THIRD-PARTY-NOTICES.txt`, `tests/LibraryBindingTests` and `tests/LibraryUpdateTests`: `System.Text.Json`,
    `System.Text.Encodings.Web` and `System.IO.Pipelines` 10.0.14 are NuGet package versions the net48 build ships (section 5, batch A),
    not the runtime an SDK bundles; they change only with their own package decision. `tests/VendorDirectTests` names no SDK or runtime.
  - The port notes and the plan's sections that record what was true on their date (S1-0's choice of 10.0.114, S3b-0's measurements on
    10.0.114/10.0.14, BUILD-HYGIENE, READER-ENV, TC-4 and the others) keep 10.0.114: they are history.

## Step 3: the locked restore

The lane restores every build first, in locked mode (`/restore /p:RestoreLockedMode=true`), from the offline folder only, so the full slnf
lane build of step 4 is also step 3's locked restore of the whole slnf. It passed: no NU1004 (lock-file mismatch), and `git status` after the
build shows no changed `packages.lock.json` (164 lock files). No unlocked restore was needed. Expected: every slnf project is net48, and
the SDK adds no implicit package to a net48 project (no `Microsoft.NETFramework.ReferenceAssemblies`, no compiler package: the resolver sets
`DOTNET_HOST_PATH`, section 2.4); the lock files hold only `Direct`, `Transitive`, `CentralTransitive` and `Project` entries of
`Directory.Packages.props`' packages.

## Step 4: the full build and the Core run

- **Full slnf lane build** (Debug|x86, MSBuild 18.11.1, the lane's own environment; the lane log names `sdk 10.0.115`): **0 errors, 353
  warnings**. The SDK-style projects compiled with `sdk\10.0.115\Roslyn\bincore\csc.exe` (the build log's compiler calls name only that
  SDK). The warnings are the same 353 with the same 17 classes (CS0618, CS0168, CS0414, CS0169, CS0612, C4244, CS0162, CS0649, CS0219,
  CS0105, CS8632, CS0109, CS0108, CS0067, MSB3270, CS1690, CS0114) as an earlier full lane build of a branch from the same `main` on
  10.0.114: no new warning class.
- **`tools\Test-ProjectFileEvaluator.ps1`** (re-run, as S4-2 did, since `NETCoreSdkVersion` is one of the names it compares): exit 0, "The
  evaluator agrees with MSBuild": 165 projects, 330 evaluations, 54,975 comparisons (398 names), 0 mismatch, 0 silent-empty, 0
  core-not-modelled, 0 evaluator-error, 0 msbuild-error; `NETCoreSdkVersion` equal (10.0.115) in all 164 SDK-style projects.
- **Targeted runs** (`.git\od-tools\run-tests.ps1 -Mode Targeted`, record `od-tools\test-results\sdkbump-targeted.txt` in the common git
  folder): all pass. ProjectFileTests 151 (150 passed, 1 skipped, as before: the solution-filter case that needs a recorded item list),
  ReleaseFlowTests 216 (211 passed, 5 skipped, as before: Release and x64 outputs not built), LibraryBindingTests 20, LibraryUpdateTests
  138, VendorDirectTests 941, NoHqTests 112.
- **Core run** (`-Mode Core -RequireBuildOutput`, record `od-tools\test-results\sdkbump-core.txt`, 23:07-23:26): **not
  green, for a reason outside the bump.** 121 assemblies, 10,462 tests: 10,381 passed, 53 skipped, 28 failed; 119 assemblies pass. The 28
  failures are all in the two assemblies that draw WPF pictures: ToothChartHostTests (13 of 20) and ToothChartImageTests (15 of 263). Every
  one is a picture that came out blank or wrong ("sheet: blank", "The tooth was drawn", "The same bytes as the committed baseline"). The
  desktop session went from connected to disconnected during the run (`query user`: state `Disc`, idle 13 minutes at 23:26; the lock screen's
  LogonUI running), and in that state WPF's RenderTargetBitmap draws nothing, as WINCHK-HEADLESS found ("Found by the runs: a locked
  session"): a probe that renders a red 40x40 square with RenderTargetBitmap in an STA PowerShell right after the run got 0 red pixels of
  1,600. The same two assemblies passed in the stage 3a gate's Full run on 10.0.114 earlier the same evening (`gate3a-full.txt`: 19 of 20
  and 257 of 263, the same skips), the compiler is the same Roslyn 5.0.0 and no rendering code changed. **To finish step 4:** with the
  session unlocked, a Targeted run of `ToothChartHostTests,ToothChartImageTests` (or the Core run again) on this branch's build.
- `tools\Test-NoEolFlips.ps1`: no changed file changed its line ends.

## Step 5 (the lead's)

10.0.114 stays installed. Open worktrees rebase onto the new pin; with `rollForward` `disable`, a worktree still on 10.0.114 builds as long
as 10.0.114 is installed, and one on 10.0.115 needs 10.0.115. Remove 10.0.114 (per user: its `sdk\10.0.114` folder and the 10.0.14
`host\fxr`, `shared` and `packs` folders it brought, unless another SDK needs them) after every open worktree has moved. The runtime packs
at 10.0.14 in the offline folder can then go as well, once no branch restores them.

## Merged with one rerun owed (lead, 2026-11-15)

Merged on `main` without the WPF picture rerun: the owner's desktop session stays disconnected, in which WPF draws nothing. The SDK change touches only version values in four `.props` files and the compiler is still Roslyn 5.0.0, and both picture assemblies passed on 10.0.114 the same evening, so the risk is low. **Owed:** a Targeted run of ToothChartImageTests and ToothChartHostTests on `main` in a connected, unlocked session (check `query user` first), before the next stage gate at the latest.
