# Port note: S3a-8b (the .NET Framework-only test APIs outside S3a-8's row)

Follow-up of S3a-8 ([S3a-8.md](S3a-8.md), section 6) in stage 3a of the .NET 10 move ([docs/specs/net10-plan.md](../specs/net10-plan.md), the
S3a-8 row): branch `net10/s3a8b-1` from `main` (`fa5640fa9`). The sites S3a-8 found outside its row no longer use what .NET does not have
(https://learn.microsoft.com/en-us/dotnet/core/porting/net-framework-tech-unavailable): new application domains, reflection-only loads,
`AppDomain.DefineDynamicAssembly` (and `AssemblyBuilder.Save`) and the run-time C# compiler (`CSharpCodeProvider.CompileAssemblyFromSource`).
The reference readers that named `OpenPmsDental.exe` or `ServiceManager.exe` read the program's managed assembly through `ProgramAssembly`,
and the host copies a dead test process leaves in a folder are cleaned up. Test code only: no program code, no schema change, no new package
(Mono.Cecil 0.10.3 of `Directory.Packages.props`, which S3a-8 already used), nothing downloaded, no program started, no window shown. S3a-8 left
none of these sites on purpose (its note lists them as outside its row), so none is kept.

Commits: `11efe84d8` (wiring: the new project `tests/TopazStandIn` in `OpenDental.sln` and `OpenDental.Local.slnf`, for the lead to review as
TestProbeHost was; the TestProbeHost, TopazStandIn and Mono.Cecil references and their lock files; `ProgramAssembly.cs` linked into BINATests,
BINBTests and VendorDirectTests), `38e27281e` (TestProbeHost: run once, stale copies), `28ba8976b` (the sites), `ca0616e88` (the new tests),
and this note with the plan's status line.

## 1. Each site, old and new

| Site | Old mechanism | New mechanism |
|---|---|---|
| `tests/MiddleTierMockTests/MiddleTierHostTests.cs` `RunInNewDomain` (5 tests through `HostSeamProbe`) | a new AppDomain of the test's folder and `.dll.config` per test; `HostSeamProbe` a MarshalByRefObject returning dictionaries | `RunInNewProcess(methodName,args)`: a TestProbeHost child per test from the test's folder with the test's `.dll.config`, `HostSeamProbe.Handle` dispatching to the same five methods; a dictionary crosses as one ProbeText list of keys and values. A fresh process is the Middle Tier worker that has just started, as the domain was. |
| `tests/ReleaseFlowTests/HelixPackageTests.cs` `HelixTypes_TheChartWillUse_LoadFromTheProgramFolder` | a new AppDomain whose base was `OpenPmsDental\bin\Debug` and whose config was `OpenPmsDental.exe.config`; `HelixLoadProbe` a MarshalByRefObject | a TestProbeHost child from a scratch folder in the temp folder holding the four Helix assemblies of the program folder and every program-folder file they reference, directly or through each other (`CopyWithReferences`, Mono.Cecil, as S3a-8's SSH.NET test does), with `OpenPmsDental.exe.config` as its config. Nothing is written into the program folder. The assertions are the same, the location being the copy of the program folder's file; a System.* 4.3.x file in the program folder that Helix references would be copied and caught as before. |
| `tests/WindowChecksTests/Harness/InProcHost.cs` (every in-process session) and `InProc/ProgramDriver.cs` | a new AppDomain based on the program folder (Debug or Release) with `OpenPmsDental.exe.config`; `ProgramDriver` a MarshalByRefObject, `CheckResult` serialized across | a TestProbeHost child per session (`InProcHost.StartSessionProcess`): its copy of the host is written into the program folder (the session's application base must be that folder, as the domain's was) and deleted when the session ends, with `OpenPmsDental.exe.config` as its config. `ProgramDriverHost.Handle` holds the process's one `ProgramDriver` and answers Patch, Start, SetKeys, RunCheck, GetStuckState and Stop; `CheckResult.ToText`/`FromText` carry a result as one line. `PatchProcessIdentity` is unchanged (StartupPath, ExecutablePath and the entry assembly now differ from the copy of TestProbeHost instead of the test host). New: the program's own exit (`Environment.Exit`) or an unhandled exception ends the session's process, not the test host; `InProcHost.Run` then fails that check and marks the session stuck, so the caller starts a new one. Each reply is waited for the driver's own timeout plus 180 s. |
| `tests/WindowChecksTests/Harness/AssemblyProbe.cs` (A3's build stamp, A8's serializer pairing) | `Assembly.ReflectionOnlyLoadFrom` and `ReflectionOnlyAssemblyResolve` in a throwaway AppDomain | static `GetMetadata`, `GetMvid`, `GetSerializerVersion(folder,fileName)` with Mono.Cecil from a copy of the bytes: the same texts (a string argument in double quotes as `CustomAttributeTypedArgument.ToString` writes it, the serializer attribute's properties in the blob's order). A3 and A8 read the program's managed assembly (`ATests.ProgramFileName`, through `ProgramAssembly.Find`; `ProgramAssembly.cs` and `BuildOutput.cs` linked into WindowChecksTests). |
| `tests/ProjectFileTests/TestTelemetryTests.cs:113` `TheMSTestAdapter_UsesNoTelemetryAssembly` | `Assembly.ReflectionOnlyLoadFrom(path).GetReferencedAssemblies()` | `ReadReferencedAssemblyNames`: Mono.Cecil `AssemblyReferences` from a copy of the bytes. |
| `tests/ProjectFileTests/NuGetFilesTests.cs:132` `WriteAssembly` | `AppDomain.CurrentDomain.DefineDynamicAssembly(...,AssemblyBuilderAccess.Save,...)` and `Save` | Mono.Cecil `AssemblyDefinition.CreateAssembly` (a library module of the name and version, no types) and `Write`. `AssemblyBuilder.DefineDynamicAssembly` cannot be used: the test needs the file, and .NET's `AssemblyBuilder` cannot save one. |
| `tests/ToothChartImageTests/Support/SecondProcess.cs` `SecondProcess.Run` (the determinism, exit, cold-time, start and render-thread tests) | a console program compiled per run with `CSharpCodeProvider` (`/platform:x86`, `[MTAThread] Main` calling `SecondProcessEntry.Run`) into the test's folder, with the test's `.dll.config` | `ProbeProcess.RunOnce(folder,config,typeof(SecondProcessEntry),args,timeout)`: TestProbeHost started with `--run-once` is that program. Its `[MTAThread] Main` calls `public static int Run(string[])` and returns its result as the exit code, its console output is the process's, and the process ends as the compiled one did, once only background threads are left; the copy is deleted afterwards. `SecondProcessResult` is filled from `ProbeRunResult` (exit code, timed out, output with `stderr: ` lines, elapsed). |
| `tests/BINBTests/TopazWrapperTests.cs:64` `ClassInitialize` | the synthetic `Topaz.SigPlusNET` control compiled from a source string with `CSharpCodeProvider` into a search folder as `SigPlusNET.dll` | the same source as a prebuilt test-only project, `tests/TopazStandIn` (a library, not a test project, assembly name TopazStandIn so no build output holds a `SigPlusNET.dll`); `WriteStandIn` reads its `TopazStandIn.dll` with Mono.Cecil, renames the assembly and module to `SigPlusNET`/`SigPlusNET.dll` as Topaz's own file is named, and writes it into the search folder. The rename keeps it another assembly than the `TopazStandIn.dll` beside the test, which nothing loads. |
| Reference readers that named `OpenPmsDental.exe` (S3a-8 note section 6): `NoHqTests/CdtRetiredTests.cs` `CdtAssembly_IsNotReferencedByTheProgram`, `CloudEditionTests.cs` `CloudEditionAssemblies_AreNotReferenced`, `RemovedAssembliesTests.cs` `RemovedAssemblies_AreNotReferenced`, `tests/BINATests/ProprietaryBinariesRemovedTests.cs` `BuiltProgram_ReferencesNoneOfTheRemovedBinaries`, `tests/VendorDirectTests` `DdeBridgesTests.BuiltAssemblies_DoNotReferenceNDde`, `BridgesDllRemovedTests.Program_ReferencesNoRemovedAssembly`, `PayConnect1RemovedTests.DpsPosAndLog4net_AreNotReferencedByTheProgram`; and `ServiceManager.exe` with it in `tests/BINBTests/CompiledReferencesTests.cs` | the literal `OpenPmsDental\bin\Debug\OpenPmsDental.exe` (and `ServiceManager\bin\Debug\ServiceManager.exe`), which on .NET is the apphost Mono.Cecil refuses | `ProgramAssembly.FindRelative(root,folderRelative,programName)` (new in `tests/ProjectFileTests/ProgramAssembly.cs`): the managed file relative to the root (`.dll` when it is there, else `.exe`), or the `.exe`'s relative path when nothing is built, so each reader's own "not built" check and message stay as they were. `UnitTests\bin\Debug\Unit Tests.exe` in `BridgesDllRemovedTests` stays: it is a test program, not one of the programs, and moves with UnitTests in 3b. |

## 2. TestProbeHost

- **Run once** (`Program.RUN_ONCE`, `ProbeProcess.RunOnce`): `TestProbeHost.exe --run-once <assembly> <type> [args...]` loads the type as the
  request mode does and calls its `public static int Run(string[] args)` as its Main. Exit codes of its own: 90 (usage), 91 (the entry method
  cannot be loaded, the reason on standard error), 92 (Run threw: written to standard error and ended with the code, not left unhandled, which
  would start Windows Error Reporting for the process; `SecondProcessEntry.Run` catches its own exceptions, so no caller changes).
- **Stale copies** (`ProbeProcess.DeleteStaleCopies`, called by every `Start` and `RunOnce` before its copy): a test process that dies before
  `Dispose` (the runner kills an assembly after 20 minutes) leaves `TestProbeHost-<12 hex>.exe` and its `.exe.config` in the folder: a test's
  bin folder or, for WindowChecksTests' sessions, the program folder. Deleted are only files of exactly those names (`IsCopyName`), and only
  those created before this process started, so a copy that a child of this process, or of a test process started later, runs from is never
  touched. A copy whose child still runs cannot be deleted (the running exe is locked) and is left with its config. A config without its copy
  is deleted by the same rule. `CopyHost` sets the copy's creation time when it makes it, since `File.Copy` keeps the source's write time.
- `CallWithTimeout`: one call with its own wait (an in-process check runs longer than the other calls of its session).
- `Program` is internal: MiddleTierMockTests sees `OpenPmsDentalBusiness.Program` too.

New tests: `tests/ReleaseFlowTests/ProbeProcessTests.cs` (the copy names; the clean-up's rules with a held copy, a newer copy, an orphan config
and six other names; `Start` cleans first and `Dispose` leaves no copy, with a non-ASCII argument; `RunOnce` as Main: exit code, output, standard
error, MTA, a foreground thread that keeps the process, a background one that does not, an exception, a timeout, a type without Run),
`ProgramAssemblyTests.FindRelative_TheManagedFileRelativeToTheRoot_OrTheExeWhenNotBuilt`, and `tests/WindowChecksTests/HarnessTests.cs`
(no opt-in, no window: `AssemblyProbe` against reflection over the same synthetic assembly written with Mono.Cecil; the session process from
a scratch folder holding the test's copies of `OpenPmsDental.exe` and its config: Patch loads the program's assembly from there, makes it the
entry assembly, and the child's config is that config; `CheckResult` crosses as text unchanged).

## 3. Proof that each changed test still bites

Two negative runs, mutations in the working tree only, the touched projects built through the lane, the assemblies run Targeted, then
reverted with `git checkout` (nothing committed): `test-results/s3a8b-neg1.txt` and `s3a8b-neg2.txt` in the local tools folder.

| Test | Mutation | Result |
|---|---|---|
| `MiddleTierHostTests.NoHost_EveryValueIsWhatSystemWebGaveOutsideAHostedRequest` | the child first serves a `ConnectionStoreOnAnyThread` request (not a fresh worker) | failed: the content root was the earlier request's, not "(null)" |
| `MiddleTierHostTests.ProcessDto_HostForm_AMissingOrEmptyApplicationNameGetsTheRequestsName` | the child without the test's config | failed: "Could not load file or assembly 'Microsoft.Extensions.Logging.Abstractions, Version=8.0.0.2' ... manifest definition does not match" (the config reaches the child) |
| `HelixPackageTests.HelixTypes_TheChartWillUse_LoadFromTheProgramFolder` | the child from the test's own folder instead of the copies | failed (no "ok", the load failed there) |
| same | the child without `OpenPmsDental.exe.config` | failed: `FileLoadException` for System.Memory 4.0.1.2 in `ToWndMeshGeometry3D` (the program's redirects are what make it load) |
| `HarnessTests.AssemblyProbe_GivesTheTextsReflectionGave` | `FormatTypedArgument` without the quotes | failed at the first metadata line (`ForkBuildUtc = ...` against `"ForkBuildUtc" = "..."`) |
| `HarnessTests.SessionProcess_RunsFromTheProgramFolder_WithItsConfig` | `StartSessionProcess` passes no config | failed: the child's `.exe.config` does not exist |
| `HarnessTests.CheckResult_TextRoundTrip_KeepsEveryField` | `FromText` swaps the shots and the notes | failed |
| `TestTelemetryTests.TheMSTestAdapter_UsesNoTelemetryAssembly` | the Cecil list gains `Microsoft.ApplicationInsights` | failed: "MSTest.TestAdapter.dll references Microsoft.ApplicationInsights; ..." |
| `NuGetFilesTests.ReadCopiedFiles_TheRuntimeTarget_WithoutWhatTheFrameworkReplaces` | `WriteAssembly` writes build + 1 | failed: "No InvalidOperationException for is not modelled by NuGetFiles" (the same-version case is gone) |
| `WpfRendererTests.Process_Exits_WhileTheRenderThreadIsAlive` | the "exit" mode leaves a foreground thread | failed after the 2-minute timeout: "The process did not end: the render thread kept it alive." |
| `ProbeProcessTests.RunOnce_IsTheProcesssMain` | the host ends with `Environment.Exit` right after Run | failed: "The process waited for its foreground thread." |
| `ProbeProcessTests.DeleteStaleCopies_OnlyTheHostsOwnCopiesFromBeforeThisProcess` | no creation-time limit | failed: 4 deleted instead of 3 (the newer copy too) |
| `TopazWrapperTests.Installed_WrapperMakesTheSameCallsAsTheCompiledCode` | the stand-in records `SetTabletLogicalXSize=` | failed on the call list (the stand-in is what the wrapper loads) |
| `TopazWrapperTests` `Installed_*` (5 tests) | the stand-in is not written | all five failed (`TopazNotInstalledException`, empty path) |
| `CompiledReferencesTests.BuiltAssemblies_ReferenceNoneOfTheRemovedAssemblies` | `mscorlib` added to the removed list | failed, naming `OpenPmsDental\bin\Debug\OpenPmsDental.exe` and `ServiceManager\bin\Debug\ServiceManager.exe` as FindRelative gave them |
| the seven other reference readers, `ProgramAssemblyTests.FindRelative_...` | `FindRelative` always names the `.dll` (not built on .NET Framework) | the unit test failed; CdtRetired, CloudEdition, RemovedAssemblies (NoHqTests), ProprietaryBinariesRemoved (BINATests), BridgesDllRemoved and PayConnect1Removed went inconclusive through their not-built checks, and DdeBridges failed ("Build first: ...OpenPmsDental.dll"): each reads the file FindRelative gives |

Not proved by a run: the opt-in window checks themselves (`OPMS_WINDOW_CHECKS=1`). They start the program and show its windows on the
session's desktop, which this package must not do. What changed under them is covered by HarnessTests (the session process's base, config and
entry assembly, the result's way back) and by ProbeProcessTests; the lead or the owner runs them once (A2, A7, B1-B7, C1-C15, F1-F4, G1, G2,
I1-I11, J1-J17, K10 and Spike2 use in-process sessions) before the stage 3a gate.

## 4. Runs

- Lane builds: the touched projects while iterating, and the full `OpenDental.Local.slnf` build (Debug x86) at the end: 0 errors.
- Targeted run after the full build (`test-results/s3a8b-final.txt`): MiddleTierMockTests 132/132, ReleaseFlowTests 180 passed and 2 skipped
  (`StagedExes_CarryTheResourcesOfTheNet48Record` and `BuildIdFiles_AreAllInTheReleaseOutput`: no Release build in the worktree, as before),
  ProjectFileTests 149 passed and 1 skipped (`EveryProjectOfTheSolutionFilter_EvaluatesTheItemsMSBuildRecorded`, as before), WindowChecksTests
  3 passed (HarnessTests) and 29 skipped (the opt-in checks), ToothChartImageTests 207 passed and 4 skipped (opt-in, as before), BINBTests
  60/60, BINATests 23/23, VendorDirectTests 932/932, NoHqTests 63/63.
- `tools/Test-NoEolFlips.ps1`: no file changed its line ends.
- No database test was added or changed; the MiddleTierMockTests and ReleaseFlowTests scratch databases are their own as before.

## 5. Left for 3b

- **ProbeProcess on .NET** (S3a-8 section 6): a renamed copy of an apphost does not start and no `.config` applies, so `Start`, `RunOnce`
  and the session process start `TestProbeHost.dll` through the host's own apphost or `dotnet exec` with the test's `.runtimeconfig.json`.
  More tests depend on it now: MiddleTierHostTests, HelixPackageTests, every in-process window check, ToothChartImageTests' second process.
- The tests that prove a binding redirect (the Helix load with the program's config, the Middle Tier probe's config) prove nothing on .NET,
  which does not use them; they go with the binding-redirect tests in 3b.
- `ProgramDriver.PatchProcessIdentity` sets private fields of the .NET Framework's `Application` and `AppDomainManager`; on .NET the entry
  assembly of the session process is TestProbeHost's `.dll`, and the patch is redone there.
- The run-time compilers of shipped code (`FormReportComplex`, RdlEngine's and RdlDesign's VB `<Code>`) are not tests and not this package's.

## 6. Lead notes at the merge (from the reviews)

- **Readers outside S3a-8's list.** `tests/LibWebTests/HtmlGenerationTests.cs` (about lines 167 and 201, `ReadModule("OpenPmsDental.exe")`)
  and `tests/LibWebTests/IeBrowserRemovedTests.cs` (about lines 164-174) still name `OpenPmsDental.exe`. They go through
  `ProgramAssembly.Find`/`FindRelative` in 3b, so section 1's sentence on the readers covers S3a-8's list only.
- **The window checks are run at the stage 3a gate.** The opt-in window checks (`OPMS_WINDOW_CHECKS=1`) are headless and show no window
  on the desktop (WINCHK-HEADLESS). They are run once, end to end, with the process-based `InProcHost` as part of the gate, which settles
  this package's question without the owner.
- **A stale probe copy in the Release folder.** A window-check session writes `TestProbeHost-<hex>.exe`/`.exe.config` into
  `OpenPmsDental\bin\Debug` or `\Release`. If its test process dies, that copy stays until the next session starts, and in the meantime
  `tools/Build-Release.ps1` stops with "Unexpected file in the Release output". Delete the `TestProbeHost-*.exe*` files there, or start a
  window-check session first.
- **For a later cleanup:**
  - `HarnessTests.SessionProcess_RunsFromTheProgramFolder_WithItsConfig` should assert that the child's `Assembly.GetEntryAssembly()`
    location is the program. Today it only checks that the patch list names it.
  - `CheckResult_TextRoundTrip_KeepsEveryField` should fill `ListNotScripted` before the round trip.
  - `ProbeProcess.RunOnce` should quote arguments the way `CommandLineToArgvW` expects, so a trailing backslash does not join two
    arguments.
  - The unbounded `WaitForExit()` after a successful timed wait could hang on a grandchild that holds the pipes, as the old
    `SecondProcess` could.
  - `HelixPackageTests` checks for a System.* 4.3.x file only within the copied closure: say so at the call site.
