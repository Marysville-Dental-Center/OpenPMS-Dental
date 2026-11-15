# Native 64-bit integration survey (2026-11-15)

Repository: Open PMS - Dental, `main` at b9dbf4c35. Read-only survey: no code was changed, built or run. Web sources are public pages only. Nothing was downloaded and nothing was signed into.

## 1. The rule, and how to read the table

### Decision 23 (docs/specs/net10-plan.md:1719-1736)

> 23. **Native 64-bit first for every integration (owner, 2026-11-15: "For any integration, use native 64bit first. If the option is not available, fall back to 32bit").** This replaces the helper-first default of S3a-13/S3a-14 and answers O-14. For each integration that loads vendor code (an in-process DLL, a COM server, a driver):
> - the 64-bit program uses the vendor's 64-bit way when one exists: an AnyCPU or x64 DLL in process, or an out-of-process COM server, which works across bitness;
> - the 32-bit helper (`OpenPmsDentalHelper32`) is the fallback, chosen when the vendor has no 64-bit way, or at run time when the installed vendor piece is 32-bit only. The helper's modules stay under the no-cut rule.

### How to read the table

- There is one row per integration. That makes 18 rows. Five of them load no vendor code, and two are dead code. They are listed so that the survey is complete.
- **64-bit way** is one of these:
  - **yes**: a public source or a test in this repository shows that it works.
  - **no**: nothing public exists.
  - **unknown**: probably yes, but not proven.
  - **not needed**: no vendor code enters the program's process.
- **Choice** is one of these:
  - **native**: no helper path exists or is planned.
  - **native with helper fallback**: the program tries 64-bit first and goes to the helper when that fails.
  - **helper only**: the call always goes through `OpenPmsDentalHelper32`.
- **Run-time test** is the check that picks the path, once per program run, before the vendor is first called. "Build check" means that we ship the piece, so its bitness is fixed when the program is built.
- **Owner needs** lists only what the owner has to supply. Section 5 gathers all of it.
- On the x86 .NET Framework build of 3a, every in-process path stays in process, as it does today. The run-time tests only matter in the x64 program of 3b. They can still be written and unit-tested in 3a.

## 2. The table

| # | Vendor | Office uses it for | Today's mechanism | 64-bit way | Conditions | Choice | Run-time test | Owner needs |
|---|---|---|---|---|---|---|---|---|
| 1 | Patterson / Dentsply Sirona: Schick CDR DICOM 5 | Schick button: opens CDR's exam window, and creates the patient or exam when CDR does not know it. The office does not use Schick. | COM through embedded interop (Interop.CDRDicom, Interop.CDRDATALib), called directly from the program's process. The server is probably out-of-process (CDRDicom.exe). | **Unknown (likely yes).** CDRDicom.exe is very probably a 32-bit LocalServer32. A 64-bit client reaches it through COM, preferably late-bound through IDispatch. | CDR 5.x installed by its normal setup, in bridge mode. Launch check S1 must find LocalServer32 rather than InprocServer32. The code moves to late binding (Type.GetTypeFromCLSID, dynamic). OnClose is kept only as best effort. | Native with helper fallback (S3a-14h) | Read the CLSID `{E7D0A248-0C4C-11D6-8294-00B0D0403E92}` in both registry views. No key means "not installed". LocalServer32 means direct. Only InprocServer32 means helper. If CreateInstance or the first call fails with CLASSNOTREG, SERVER_EXEC_FAILURE, NOINTERFACE, IIDNOTREG or LIBNOTREGISTERED, use the helper. | A test PC with CDR 5.x (5.14 or later), with media and licence from Patterson or Dentsply. No sign-in. No sensor. |
| 2 | Topaz Systems: SigPlusNET.dll | Signature pad on treatment plans, procedures, sheets, the ortho chart, image documents, eForms and commlogs. The office does not use Topaz today. | In-process managed DLL: Assembly.LoadFrom, then Activator.CreateInstance (a WinForms control). | **Yes.** The current SigPlusNET 2.1.5.6 is "AnyCPU" and loads in process. | The current Topaz install. An older copy may be x86-only. Serial pads on .NET 10 need System.IO.Ports, which is not approved yet. USB HID pads (the default, type 6) do not need it. | Native with helper fallback (S3a-14h) | Read each SigPlusNET.dll found with PEReader. AnyCPU or x64 loads in process. Requires32Bit, or x86 mixed mode, goes to the helper. A BadImageFormatException at load is the safety net and also goes to the helper. | A test PC with SigPlusNET 2.1.5.x and a USB pad. Approval of System.IO.Ports, for serial pads only. |
| 3 | Guru (Reality Engineering, sold by Henry Schein): MedVisorInterface.dll | Guru button: starts Guru and sends the patient. | In-process native DllImport (MVStart, MVSendPatient). | **Unknown, none known.** No public documentation exists. Guru's listings name Windows XP to 7. | It would need a 64-bit MedVisorInterface.dll with the same exports. After 3b, the helper in Helper32\ must still find a DLL placed in the program folder (launch check G3). | Helper only. An in-process branch is ready for a 64-bit DLL, but it is unproven. | Find the DLL the way DllImport would and read the PE Machine field. 0x8664 means in process. 0x14C means helper. If the DLL is not found, show today's message. | A question to Henry Schein / Guru support. A workstation with Guru, for launch checks G1-G3. |
| 4 | Intuit: QuickBooks Desktop, QBFC SDK | Accounting > QuickBooks: connection test, account and class lists, deposits. The office does not use QuickBooks Desktop. | Helper module (S3a-13, merged): QBFC 10 in-process COM inside the x86 helper, reached over a named pipe. | **Yes, with conditions.** QBFC 15/16/17 are 64-bit (SDK 15 and later), for QuickBooks Desktop 2022 and later. They are not backward-compatible with the 32-bit SDK. | QuickBooks Desktop 2022 or later. The 64-bit QBFC redistributable on every workstation. A new interop built from QBFC17 or 16. A second session class beside the helper's QBFC 10 module. The qbXML versions and the app authorization need a check on a test PC. | Native with helper fallback. The native path is not written yet. | Create QBFC17's QBSessionManager, then QBFC16's. On CLASSNOTREG or a load error, use the helper. If OpenConnection or BeginSession fails because QuickBooks is too old, use the helper for the rest of the run. | Intuit Developer sign-in, the SDK 17.0 download and its licence. Who installs the redistributable. A test PC with QuickBooks 2022 or later (O-14). |
| 5 | TWAIN Working Group DSM, plus each scanner maker's TWAIN data source | Document scanning in the Images module and the Images window, and the source picker in imaging setup. | In-process native: NativeTwainDsm loads TWAINDSM.dll or twain_32.dll, and the DSM loads the vendor's .ds into the program's process. | **Yes for the DSM** (TWAINDSM 2.5.1 x64 in System32). **Per vendor for the data source:** only a 64-bit .ds in twain_64 loads. | S3b-7 installs the 64-bit TWAINDSM. The fallback needs a 32-bit DSM (SysWOW64\TWAINDSM.dll or twain_32.dll) and the vendor's .ds in twain_32. Needs a new helper TWAIN module, a merged source list and a picker drawn by the program. | Native with helper fallback. The helper path is new 3b work. | List sources from the 64-bit DSM and from the helper's 32-bit DSM. Merge them by ProductName, Manufacturer and ProductFamily. A 64-bit source runs in process. A source that is only 32-bit runs in the helper. Store the default source with its bitness. The "No 64-bit DSM" error appears only when both lists are empty. | O-16 approval for TWAINDSM 2.5.1. One scanner with a 64-bit driver and one with only a 32-bit driver for launch checks. |
| 6 | X-ray sensor TWAIN data sources, including the clinic's woodpacker-twain drivers 1.2 (32-bit and 64-bit) | Native radiograph capture (CAP) with TWEI_TWAINDIRECTMETADATA. | In-process native: TwainCaptureSession loads the DSM and runs the sensor's DS on its own message-loop thread. | **Yes** for the clinic's 1.2 x64 DS through the 64-bit DSM. The structures are x64-safe. Most third-party sensor DSes are 32-bit only. | TWAINDSM 2.5.1 x64 in System32. The DS in twain_64. The clinic's 32-bit package is also installed, because the 64-bit DS uses it. The fallback needs a 32-bit TWAINDSM in SysWOW64. The helper ITwainCaptureSession is new. The TW_INFO four-byte rule is still to be rechecked. | Native with helper fallback. The helper path is new 3b work. | Once per device at Open, before MSG_ENABLEDS. Load the 64-bit DSM, find the stored TwainName and open it. If that fails, ask the helper's 32-bit DSM. If neither finds it, report the source as missing in both bitnesses. Never switch paths after Enable. When both lists have the name, 64-bit wins. | The x64 package of drivers 1.2 (not packaged yet). The clinic PC with the sensor, for both paths. A decision on the 32-bit TWAINDSM in SysWOW64. |
| 7 | Intraoral and USB camera makers (DirectShow, UVC) | Intraoral camera video and stills (FormVideo, ImagingCapture camera sessions). The clinic's camera is plain UVC. | In-process COM: devenum, quartz, qcap and qedit, with the capture filter bound through IMoniker.BindToObject (ksproxy). | **Yes.** Windows' 64-bit DirectShow filters. UVC works through usbvideo.sys and ksproxy. 64-bit Windows loads only 64-bit kernel drivers. | Windows 10/11 x64, with qedit registered in the 64-bit view (it is deprecated, but present). A vendor's user-mode filter must be registered for 64-bit. ImagingCapture needs an x64 platform in 3b. | Native. A helper camera module is not planned; it would be added only if a camera with a 32-bit-only filter turns up. | Enumerate the video input category in the 64-bit process. If the saved camera is missing, compare the Registry32 and Registry64 Instance keys. If it is registered only for 32-bit, show a clear "32-bit only" message. A missing qedit is not a bitness problem. | The clinic's UVC camera on a test PC for the 3b launch check. |
| 8 | Mediadent / CADI (ImageLevel, Corilus) | CADI bridge: opens CADI and sends the name, birth date, age, sex and image folder. | Late-bound COM (IDispatch) through the ProgID `Mediadent.Application`. Creating the object starts CADI. | **Unknown (likely yes).** It is very probably a 32-bit LocalServer32 EXE, which a 64-bit client reaches with pure IDispatch. | CADI or Mediadent installed and registering LocalServer32. No code change for the native path. | Native with helper fallback. The helper CADI module would be new. | GetTypeFromProgID returns null: not installed, so show today's message. CreateInstance fails with CLASSNOTREG, SERVER_EXEC_FAILURE or NOINTERFACE: use the helper and log it once. Optionally read LocalServer32 in both views first. | A read-only reg query and one launch check on a PC with CADI. Optional dealer question. |
| 9 | Microsoft Word | Letter merge (print, open in Word) and chart letters saved as PDF. | Out-of-process COM: late-bound Word.Application (WINWORD.EXE). | **Yes.** Out-of-process COM works with 32-bit or 64-bit Office. | Word installed. Orphaned TypeLib keys left by an Office 32-to-64 move can break calls; Microsoft's remediation script fixes that. DISABLE_MICROSOFT_OFFICE must not be defined in the x64 build. | Native | The existing null-ProgID check gives "Microsoft Word is not installed." A CLASSNOTREG, SERVER_EXEC_FAILURE or TYPE_E_* error is logged and shown as "Word's registration is broken". The helper would hit the same registration, so it is not used. | A test PC with Word (64-bit, and 32-bit if available) and a printer. |
| 10 | Third-party plug-in authors (PluginBase DLLs) | Program plug-ins. None are known at the office; the feature stays under the no-cut rule. | In-process managed: Assembly.LoadFile, then Activator.CreateInstance of `<name>.Plugin`. | **Yes.** An AnyCPU or x64 net10.0-windows rebuild by the author. | PluginExample is rebuilt AnyCPU (today x86). Both loaders check the PE kind before loading. The Middle Tier's empty catch becomes a logged reason. | Native. A helper is not possible, because hooks pass live in-process forms and objects. | PE kind and TargetFramework read before LoadFile. Required32Bit, Preferred32Bit or x86 gets a clear "32-bit only, rebuild AnyCPU or x64" refusal. .NET Framework gets the decision-16 message. | Nothing. |
| 11 | Microsoft WebView2 SDK loader | Embedded browser: PDFs in Images, EdgeExpress pay pages, vendor sign-in pages. | WebView2Loader.dll in process (the native per-architecture loader). The Edge runtime runs out of process. | **Yes.** The x64 loader is already in the pinned package 1.0.2420.47. | Directory.Build.targets must link the win-x64 loader flat next to the exe. Build-Release checks 0x8664. The text in ODWebView2.cs:197 and the package comments needs updating. | Native | Build check only (PE 0x8664). At run time, the existing runtime presence test stays. | Nothing. A bump to 1.0.4314.35 is optional. |
| 12 | Microsoft DirectX 9 / D3DX9, and the GPU makers' D3D9 and OpenGL drivers | DirectX and OpenGL tooth chart modes, and the graphical perio chart. | In-process native: SharpDX.Direct3D9 and Tao.OpenGl (both AnyCPU), plus a LoadLibrary probe for d3dx9_43.dll. | **Yes.** The x64 d3dx9_43.dll comes from the June 2010 End-User Runtime or from Microsoft.DXSDK.D3DX. S3a-15 ran all three modes in a 64-bit process. | An office with only the 32-bit D3DX loses DirectX and the perio graphic, and falls back to OpenGL. The setup should make the x64 D3DX present. | Native. The fallback is in process: DirectX, then OpenGL, then Simple 2D. | The existing probe (IsDirectXRuntimeAvailable), then a device test, then StartFirstWorkingMode. | A decision on chaining the runtime or shipping D3DX app-local (needs counsel). A GPU workstation and a clean VM for the launch check. |
| 13 | Open PMS - Dental's own Direct2dWrapper.dll | Direct2D drawing of the program's windows. | In-process native DllImport (66 entry points). | **Yes.** The x64 build exists (S3a-16) and the signatures are checked (S3a-12). | S3b-4 copies the x64 build into the output. S3b-7 takes it from bin\x64\Release and checks 0x8664. | Native | Build check (PE 0x8664, exports match the imports). | Nothing. |
| 14 | Belgian government eID middleware (eidlib.dll), plus winscard.dll | Patient import from a Belgian eID card. This is dead code: SmartCardWatcher has no caller. | In-process native DllImport (BEID_* calls of the old SDK). | **Yes, through a port.** eidlib.dll was never 64-bit. The current middleware's beidpkcs11.dll (PKCS#11) is 64-bit in System32. | The 64-bit Belgium eID software and a PC/SC reader. When the feature is turned on: port it to PKCS#11, fix ReaderState and SCardListReaders, and detect the card by the token label "BELPIC". | Native with helper fallback, only when the feature is turned on. | NativeLibrary.TryLoad of beidpkcs11.dll from System32. If only SysWOW64 has it, use the helper. If neither has it, ask the user to install the 64-bit eID software. | Nothing now. For a later launch check: a card, a reader and the free middleware. |
| 15 | VisiQuick (Thomsystems, now Citodent): vqdde32.dll | VisiQuick imaging bridge. This is dead code: the whole file is a comment. | In-process native DllImport (DDE client to VisiQuick.exe). Nothing is compiled. | **No.** Only a Win32 x86 DLL is known. VDDS-media (an exe plus an INI file) might be a 64-bit route, but that is not confirmed. | Only if the bridge is revived. Code fixes are then needed in either bitness: the HWND cast, the string returns and the buffers. | Helper only, if revived. VDDS-media first, if VisiQuick supports it. | Read the vqdde DLL's PE header: 0x8664 means in process, 0x14C means helper. If no DLL is found and VDDS_MMI.INI has a VisiQuick PATDATIMPORT entry, use VDDS. | Nothing now. If revived, a question to Citodent. |
| 16 | Trojan Benefit Service | Insurance plan import and the startup plan-update check. | A registry read of INSTALLDIR (pinned to Registry32), then text files, and a separate-process launch. | **Not needed.** No vendor code is loaded. | None for x64. Optional: try Registry64 after Registry32. The key-name mismatch with Open Dental's manual is a separate question. | Native | None. Optionally, a Registry32-then-Registry64 lookup order. | Optional: a decision on the key name. |
| 17 | About 88 process-start bridges (Apteryx/XVWeb, Dexis, Dolphin, Romexis, Sirona, Carestream, VixWin, Planmeca, Trophy, Owandy, DentX, iCat, PandaPerio, 3Shape, XCharge and others) | Program-link buttons that start the vendor's exe with arguments or a file. | A separate process (Process.Start and its variants). A few use DDE, WM_SETTEXT or ini functions. | **Not needed.** | None. Paths come from program properties. | Native | None. | Nothing. |
| 18 | Other cross-process or network bridges (DDE and file-drop imaging, EvaSoft UDP, cloud payment and web vendors, the eclaims modem, printers) | Imaging hand-offs, payments, eclaims, printing. | DDE, file drop, UDP, HTTP, managed SerialPort, GDI printing. | **Not needed.** | RDL reports refuse every provider except MySqlConnector, so no ODBC or OLE DB driver loads. On .NET 10, TerminalConnector's SerialPort needs the System.IO.Ports package; to be checked. | Native | None. | Probably the same System.IO.Ports approval as Topaz serial pads (to be confirmed). |

## 3. Notes per integration, with sources

### 3.1 Schick CDR DICOM 5

**Evidence that CDR is an out-of-process 32-bit server**

- No vendor document describes CDR's automation or its bitness.
- CDR 5.4 and later installs under `C:\Program Files (x86)\Sirona Dental\CDR DICOM for Windows\`, so CDRDicom.exe is almost certainly 32-bit.
- Creating the Application object is what starts CDR. That points to a LocalServer32 server.
- Microsoft documents that a 32-bit out-of-process COM server works with a 64-bit client.
- Microsoft also documents that a 64-bit client falls back to a 32-bit server when no 64-bit one is registered.

**Registry risk**

- Classes\CLSID and Classes\Interface are redirected per bitness. Classes\TypeLib is shared.
- The interface casts (ExamDocument, the event DIID, ICDRPatient) may fail in the 64-bit view.
- The robust native way is late binding through IDispatch for every call, including the patient object.
- Before reusing a cached exam, check that it is still alive: IsWindow on its hWnd, or catch RPC_E_DISCONNECTED.
- Keep OnClose only as best effort, inside a try.
- Keep the S3a-12 fix that sign-extends the hWnd.
- The reference at OpenPmsDental.csproj:49-56 has no user and can go.

**Corrections to the plan's text**

- Decision 23 (net10-plan.md:1729) calls CDR "an out-of-process server" as a fact. It should say "probably; proven by S1".
- `{8D82B9C6-374C-48F9-AE2A-A51FB7F44752}` is the assembly GUID of Interop.CDRDATALib, so it is most likely CDRDATALib's TypeLib ID, not ICDRPatient's IID as S3a-14.md:59 says. Check S1's TypeLib key against this.

**Launch checks**

- **S1** is extended to read both registry views and the TypeLib's win32 and win64 subkeys.
- **New S4:** the x64 build presses the Schick button with CDR not running, then running, then after its exam window was closed.
- **New S5:** the same with the helper forced.

**The fallback.** The S3a-14 SchickModule is merged as it is. It uses SHOW_EXAM, the message-pumping wait, CallPumping and AllowSetForeground. On main, Helper32Host.cs:74-78 still lists only Host and QuickBooks.

**Sources**

- WpfControlsOD/Bridges/Schick.cs:14-15, 53, 72-73, 76, 81
- WpfControlsOD/WpfControlsOD.csproj:27-34
- OpenPmsDental/OpenPmsDental.csproj:49-56
- OpenPmsDentalHelper32/Helper32Host.cs:74-78
- Required dlls/Interop.CDRDATALib.dll and Interop.CDRDicom.dll (GUID strings)
- `git show net10/s3a14g-1:OpenPmsDentalHelper32/Schick/SchickModule.cs:13-20`
- `git show net10/s3a14n-1:docs/port-notes/S3a-14.md:51-65, :85-138, :288-305`
- docs/specs/net10-plan.md:1710-1726
- https://pattersonsupport.custhelp.com/app/answers/detail/a_id/7111/~/schick-cdr-dicom-files-install-locations
- https://liveddm.freshdesk.com/support/solutions/articles/9000155187-how-to-bridge-schick-cdr-dicom-imaging-software
- https://www.opendental.com/manual/bridgeschick.html
- https://assets.dentsplysirona.com/websites/dentsplysironasupport/schick-brand-software/CDRDICOMSystem_Requirements.pdf (only the search summary was readable)
- https://learn.microsoft.com/en-us/windows/win32/winprog64/process-interoperability
- https://learn.microsoft.com/en-us/windows/win32/com/preferredserverbitness
- https://learn.microsoft.com/en-us/windows/win32/winprog64/shared-registry-keys

### 3.2 Topaz SigPlusNET

**What Topaz publishes**

- Topaz's developer FAQ (page 15) says the current SigPlusNET is "AnyCPU" and runs in either bitness.
- The current version is 2.1.5.6 (September 2023). It is built on .NET Framework 4.7.1, and Topaz says it supports .NET Core.
- Nothing public says which older versions were x86-only, so the run-time PE check is needed.

**Notes for the code**

- AssemblyName.ProcessorArchitecture is obsolete on .NET (SYSLIB0037) and cannot do the check. Use PEReader instead.
- TopazWrapper should collect every copy it finds, take the first that can load in process, and otherwise give the helper the path of the first 32-bit-only copy.
- The public TopazWrapper API does not change.
- FillSignatureEncodings reflects on the private field Sig and on SetAutoKeyData. It keeps its existing fallback when that reflection fails.
- Open Dental's tests say that the 32-bit and 64-bit SigPlusNET handle encoding differently. SignatureBoxWrapperTests must therefore pass in an x64 run.

**The fallback** is the whole S3a-14 Topaz module: Load, Create, Release, Call, CanSetAutoKeyBytes, SetAutoKeyBytes, Picture and Sign, with TopazSignatureView on the program's side.

**Sources**

- https://topazsystems.com/information/SoftwareDevFAQ.pdf (page 15)
- https://www.topazsystems.com/itextnet.html
- https://www.topazsystems.com/software/download/sigplusnet.pdf
- OpenPmsDentalBusiness/UI/TopazWrapper.cs:12-17, 23-25, 38-54, 105, 112-114, 202, 376-401
- OpenPmsDentalBusiness/UI/SignatureBoxWrapper.cs:418
- UnitTests/UnitTests/SignatureBoxWrapperTests.cs:115-116, 126-127
- docs/specs/net10-plan.md:736, :1709-1735
- docs/owner-questions.md:996-1000
- branch net10/s3a14g-1: docs/port-notes/S3a-14.md sections 2 and 5
- branch net10/s3a14g-1: OpenPmsDentalHelper32/Topaz/TopazModule.cs:119, :201-227, and TopazCommands.cs

### 3.3 Guru

**What is known**

- No public documentation of MedVisorInterface.dll exists.
- The patient structure has only fixed char arrays and no pointers, so its layout is the same on x64.
- The existing quirk stays: 921 of the 1320 bytes are written.
- Put the declarations in one shared file that both the program and the helper compile. A 64-bit DLL then needs no code change.

**Fix needed when the helper moves to Helper32\ in 3b**

- A DLL that a site copied into the program folder is no longer found by the helper.
- Either the program passes the folder to the helper and the helper calls SetDllDirectory or LoadLibrary with it, or the helper adds its parent folder to the search path.
- Otherwise launch check G3 fails.

**Sources**

- WpfControlsOD/Bridges/Guru.cs:15-28, 49-117, 150-154
- `git show net10/s3a14g-1:OpenPmsDentalHelper32/Guru/GuruModule.cs:1-60`
- `git show net10/s3a14g-1:docs/port-notes/S3a-14.md:80-83, :141-159`
- docs/specs/net10-plan.md:1709, :1715-1727
- https://www.opendental.com/manual/bridgeguru.html
- https://www.dentalcommunity.com.au/products/guru/2d09be63-2c15-ddc2-293a-4ef8015e77fd
- Search results only, pages not fetched:
  - https://www.dentalcompare.com/News/151702-New-Dental-Product-Guru-6-Case-Presentation-Software-from-Guru-Dental/
  - https://www.aegisdentalnetwork.com/id/products/reality-engineering/guru
  - https://www.henryschein.com/us-en/Dental/PracticeSolutions/PatientEdu.aspx

### 3.4 QuickBooks Desktop (QBFC)

**What Intuit publishes**

- A 64-bit SDK exists from 15.0 on. The current SDK is 17.0, and Intuit recommends its 64-bit installer.
- QBFC15, 16 and 17, and the request processor QBXMLRP2, are 64-bit in-process COM.
- The 64-bit SDK works only with QuickBooks Desktop 2022 and later.
- It is not backward-compatible with the 32-bit SDK, so the 64-bit path is a second module, not a recompile.
- No public out-of-process QBFC server exists. The helper is the only way to reach QBFC 10 from x64.
- Intuit's pages are drawn by script. Their content comes from search snippets and from the earlier S3a-13 vendor check.

**Notes for the code**

- The new interop is built from the 64-bit type library with embedded types and is never shipped. Build-Release already denies Interop.*.dll.
- The message-set versions passed to CreateMsgSetRequest need checking against the newer qbXML.
- The integrated-application authorization may ask the user again when the 64-bit path first connects. That is not confirmed publicly.

**The fallback.** The helper's QBFC 10 module stays exactly as it is, and the helper stays x86 after 3b. S3b-0 item 12 must prove that QBFC 10 loads in the helper's self-contained .NET 10 x86 process.

**Sources**

- https://developer.intuit.com/app/developer/qbdesktop/docs/get-started/desktop-sdk-features
- https://developer.intuit.com/app/developer/qbdesktop/docs/get-started/download-and-install-the-sdk
- https://developer.intuit.com/app/developer/qbdesktop/docs/get-started/older-versions-of-the-desktop-sdk
- https://developer.intuit.com/app/developer/qbdesktop/docs/get-started/quickbooks-products-supported-by-the-desktop-sdk
- https://developer.intuit.com/app/developer/qbdesktop/docs/get-started/sdk-compatibility-with-quickbooks-releases
- https://developer.intuit.com/app/developer/qbdesktop/docs/go-live/redistributing-sdk-components-with-your-application
- Community reports:
  - https://tek-tips.com/threads/calling-qbfc16-64-bit-interface-from-vfp-32-bit-code.1821251
  - https://www.tek-tips.com/threads/quickbooks-qbfc16-sdk-64-bit-usage-from-vfp9-vfpa.1832521
- docs/port-notes/S3a-13.md:34-66
- docs/specs/net10-plan.md:27, :1715-1722, :1859-1866
- WpfControlsOD/Bridges/QuickBooks.cs:62-65
- OpenPmsDentalHelper32/OpenPmsDentalHelper32.csproj:21-24, :33-41
- OpenPmsDentalHelper32/QuickBooks/QuickBooksModule.cs:22, :70-79
- CodeBase/Utilities/Helper32Client.cs:62, :79-89

### 3.5 TWAIN document scanning

**The rule**

- A data source loads only into a process of its own bitness. There is no thunking layer.
- The 64-bit DSM looks for sources in `%windir%\twain_64`. The 32-bit DSM uses `%windir%\twain_32`.
- Commercial toolkits (VintaSoft, NAPS2) reach 32-bit drivers through a separate 32-bit process. That is the helper's design.
- WIA's TWAIN compatibility source is reported only in twain_32. WIA-only scanners will in practice come through the helper.

**The helper TWAIN module must do this**

- Run its own message pump.
- Parent the source's window on the program's HWND, passed as a number. This needs a launch check.
- Return pages as files or bytes, because a DIB handle (handleDIB at ControlImages.cs:2637) cannot cross processes.
- Keep the DSM search order, listing and selection, Hide UI and Show UI, the capability settings, the multi-page loop and EZTEC_USER_CANCEL.

**The program-side picker.** The DSM's MSG_USERSELECT shows only one bitness, so SelectImageSource needs a picker drawn by the program over the merged list.

**Sources**

- https://github.com/twain/twain-dsm
- https://github.com/twain/twain-dsm/releases
- https://api.github.com/repos/twain/twain-dsm/releases
- https://help.accusoft.com/TwainPro/v9.4/dotnet/topic65.html
- https://www.vintasoft.com/docs/vstwain-dotnet/Programming-Twain-Device_Manager.html
- https://www.vintasoft.com/docs/vstwain-dotnet/Programming-Twain-How_To_Use_32bit_TWAIN_driver_in_64bit_Application.html
- https://www.nuget.org/packages/naps2.sdk
- Search snippets only:
  - https://twain.org/forums/topic/64-bit-twain-only-sees-64-bit-twain-drivers/
  - https://www.twain.org/forums/topic/how-to-access-32-bit-twain-drivers-from-64-bit-twaindsm/
  - https://learn.microsoft.com/en-us/windows/win32/wia/-wia-twain-compatibility
- ImagingDeviceManager/TwainProtocol/NativeTwainDsm.cs:33-46, 58, 63, 75-77
- ImagingDeviceManager/EZTwain.cs:50-63
- OpenPmsDental/Main Modules/ControlImages.cs:2615-2637
- OpenPmsDental/Forms/FormImagingSetup.cs:90-96
- docs/port-notes/S3a-12.md:28-133
- docs/specs/net10-plan.md:1054-1060, :1079, :1709-1729

### 3.6 TWAIN sensor capture (CAP)

**What the TWAIN 2.5 header confirms.** The official twain.h has pack(2), TW_UINTPTR = UINT_PTR, the TW_INFO layout, and TWEI_TWAINDIRECTMETADATA = 0x124B.

**The TW_INFO.Item four-byte rule (MaxInlineItemBytes=4) is still open**

- twain.h does not settle it.
- The TWAIN 2.5 specification PDF could not be read here.
- It does not block CAP: the metadata JSON is far larger than 8 bytes and always arrives as a handle.

**The helper path**

- It is a new ITwainCaptureSession implementation that proxies to a TwainCaptureSession hosted in the helper.
- CaptureSessionController does not change; only FuncCreateSession picks the implementation.
- It must stream pixels (8-bit DIB and 16-bit memory transfer), source info and MetadataJson.
- It must keep the journal, capture log and quarantine writes.
- The topmost Status/Cancel window and the foreground restore (W7) must work across processes.

**The 32-bit DSM.** The 64-bit DSM keeps its own registry state (TWAIN64). The clinic DS's DF_DS2 identity under twain_32.dll is unconfirmed, so the 32-bit TWAINDSM in SysWOW64 is the safer fallback DSM.

**Sources**

- ImagingDeviceManager/Capture/TwainCaptureSession.cs:66-83
- ImagingDeviceManager/TwainProtocol/NativeTwainDsm.cs:9-11, 26-40, 51-80
- ImagingDeviceManager/TwainProtocol/TwainStructs.cs:8-13, 164-209
- ImagingDeviceManager/TwainProtocol/TwainConstants.cs:133
- ImagingDeviceManager/TwainProtocol/TwainSession.cs:474-480, 551, 591-594
- ImagingDeviceManager/Capture/ITwainCaptureSession.cs:22-60
- ImagingCapture/Session/CaptureSessionLauncher.cs:79-80, 114
- ImagingCapture/Session/CaptureSessionController.cs:54, 582-605
- docs/port-notes/S3a-12.md:34-35, 55, 63-70, 106-110
- docs/specs/net10-plan.md:734, 1064-1070, 1082, 1089, 1719-1735
- docs/imaging-capture-design.md:97, 145, 195, 394-396, 1043
- https://raw.githubusercontent.com/twain/twain-dsm/master/TWAIN_DSM/src/twain.h
- https://github.com/twain/twain-dsm
- https://github.com/twain/twain-dsm/releases
- https://github.com/twain/twain-cs
- https://raw.githubusercontent.com/twain/twain-dsm/master/TWAIN_DSM/src/dsm.cpp
- https://asprise.com/c%23-vb.net-scanner-api/64bit-applications-twain-64bit-os.html
- https://www.twain.org/forums/topic/how-to-access-32-bit-twain-drivers-from-64-bit-twaindsm/ (summary only)
- https://www.vintasoft.com/docs/vstwain-dotnet/Programming-Twain-How_To_Use_32bit_TWAIN_driver_in_64bit_Application.html (summary only)
- https://twain.org/wp-content/uploads/2021/11/TWAIN-2.5-Specification.pdf (tried; not readable here)

### 3.7 Intraoral and USB cameras (DirectShow)

**Where bitness matters**

- Bitness matters only for user-mode pieces that a vendor registers as in-process COM: a software source filter, a KS proxy plug-in or a decoder.
- Classes\CLSID and Classes\DirectShow are redirected per bitness, so a piece registered only for 32-bit is invisible to the x64 program.
- On Windows 1607 and later, DShow Bridge (the Camera FrameServer service) also serves UVC devices across bitness.

**Notes**

- The Sample Grabber is deprecated by Microsoft. If it goes missing, the cure is a Media Foundation backend behind IUvcBackend, not the helper.
- At 3b, update the comment in SampleGrabberInterfaces.cs:7-9 and add the x64 platform to xImagingCapture.csproj:17.

**Sources**

- ImagingCapture/Uvc/Interop/DsUtil.cs:13-18
- ImagingCapture/Uvc/Interop/Ds.cs:11-22
- ImagingCapture/Uvc/DirectShow/DeviceEnumerator.cs:9, 58-63
- ImagingCapture/Uvc/DirectShow/DirectShowFrameSource.cs:226-234, 529, 542
- ImagingCapture/Uvc/Interop/SampleGrabberInterfaces.cs:7-9
- ImagingCapture/Uvc/UvcErrors.cs:23
- ImagingCapture/xImagingCapture.csproj:17
- docs/imaging-capture-design.md:93, :692
- docs/specs/net10-plan.md:734, :1709-1725
- https://learn.microsoft.com/en-us/windows/win32/directshow/sample-grabber-filter
- https://learn.microsoft.com/en-us/windows/win32/winprog64/shared-registry-keys
- https://learn.microsoft.com/en-us/windows-hardware/drivers/stream/dshow-bridge-implementation-guidance-for-usb-video-class-devices
- https://learn.microsoft.com/windows-hardware/drivers/stream/usb-video-class-driver-overview (summary only)
- https://learn.microsoft.com/en-us/windows-hardware/drivers/stream/usb-video-class-implementation (summary only)

### 3.8 Mediadent / CADI

**Evidence for an out-of-process server**

- Creating the COM object starts CADI.
- The interface is application-level: OleBringToFrontApp, and an exit notice posted as WM_CLOSE to a window in the client.
- Mediadent installs under Program Files (x86).
- All of this points to a 32-bit LocalServer32. Pure IDispatch is marshalled by Windows, so no vendor type library needs registering for 64-bit callers.

**Notes**

- The window handle passed as an int stays safe.
- A read-only check settles it on any PC. `reg query "HKCR\Mediadent.Application\CLSID"` gives the CLSID; then look for LocalServer32 or InprocServer32 under both CLSID views.
- The helper CADI module would be new. It would repeat today's call order and keep the COM object alive between calls.
- The public Mediadent v8 manual's section 8.7 ("Mediadent automation") may answer the question. It is over 10 MB and could not be fetched here.

**Sources**

- WpfControlsOD/Bridges/CADI.cs:30-31, 33, 56-61, 67-84, 116-129
- WpfControlsOD/Logic/ProgramL.cs:79-80
- OpenPmsDentalBusiness/Misc/ConvertDatabases5.cs:5255-5292
- docs/specs/net10-plan.md:736, 1448-1449
- docs/port-notes/S3a-12.md:356
- https://www.opendental.com/manual/bridgecadi.html
- https://opendental.com/manual/bridgeMediadent.html (search result only)
- https://corilus.be/hubfs/Imagelevel/min%20%20system%20requirements.pdf
- https://info.corilus.be/hubfs/Imagelevel/Mediadent%20V8%20EN%20-%20new-1.pdf
- https://www.corilus.be/hubfs/Mediadent%20Manuals/Mediadent%20user%20manual%20V8%20Part%201%20v8.24.2.1.5.0%20en.pdf (not fetched)
- https://www.advanceduninstaller.com/Mediadent-2524c67d6cf3135abbb09698f70542e6-application.htm (search result only)
- https://learn.microsoft.com/en-us/windows/win32/com/preferredserverbitness
- https://learn.microsoft.com/en-us/windows/win32/winprog64/shared-registry-keys

### 3.9 Microsoft Word

- No interop or Office PIA is shipped. The calls are late bound.
- The C# dynamic COM binder exists in modern .NET's Microsoft.CSharp (seen as a search-result title only).
- The Middle Tier server never calls Word. GetWordApp runs on the client.
- Add a test that the x64 build compiles the `#if !DISABLE_MICROSOFT_OFFICE` code.

**Sources**

- https://learn.microsoft.com/en-us/windows/win32/winprog64/process-interoperability
- https://learn.microsoft.com/en-us/office365/troubleshoot/administration/com-net-errors-after-migration
- https://learn.microsoft.com/en-us/office/client-developer/shared/compatibility-between-the-32-bit-and-64-bit-versions-of-office
- https://source.dot.net/Microsoft.CSharp/Microsoft/CSharp/RuntimeBinder/ComInterop/ComInvokeBinder.cs.html (title only)
- OpenPmsDentalBusiness/Data Interface/LetterMerges.cs:77-106
- OpenPmsDental/Logic/ChartLetterL.cs:596-630
- OpenPmsDental/Forms/FormLetterMerges.cs:288-335
- OpenPmsDental/Forms/FormLetterMergeEdit.cs:215-229
- docs/port-notes/BIN-B.md:35
- docs/third-party-binaries-audit.md:164, 369
- docs/specs/net10-plan.md:736, 1448, 1709

### 3.10 Plug-ins

**Notes**

- The sample PluginExample is x86 against .NET Framework 4.7.2, so it must change.
- Microsoft's documented .NET 10 way to load a plug-in is a per-plug-in AssemblyLoadContext with AssemblyDependencyResolver and EnableDynamicLoading. Assembly.LoadFile still works.
- Plug-ins from the Open Dental era fail anyway, because they bind to the old assembly names (adoption D5).
- The gate check at net10-plan.md:1268 gains a case: an x86 plug-in gets the clear message.
- Correction (N64-A, docs/port-notes/N64-A.md section 2): a 64-bit .NET Framework process loads an IL-only DLL with both 32BITREQUIRED and 32BITPREFERRED ("any CPU, 32-bit preferred"; the lane's x64 test run), so only 32BITREQUIRED without 32BITPREFERRED, mixed-mode x86 or PE32+ of another machine is refused; row 10's "Preferred32Bit" does not apply to such a DLL.

**Sources**

- OpenPmsDentalBusiness/Plugins/Plugins.cs:66-138
- WpfControlsOD/Bridges/PluginLoader.cs:15-96
- OpenPmsDentalBusiness/Plugins/PluginBase.cs:8-40
- PluginExample/Plugin.cs:18-50
- PluginExample/PluginExample.csproj:13, 44, 55
- docs/specs/net10-plan.md:1076, 1226, 1268, 1652-1653, 1709-1725
- docs/reviews/2026-11-01-rename-inventory.md:180-184, 414
- docs/adoption-runbook.md:322-325
- https://learn.microsoft.com/en-us/dotnet/framework/tools/corflags-exe-corflags-conversion-tool
- https://learn.microsoft.com/en-us/dotnet/api/system.reflection.portableexecutablekinds
- https://learn.microsoft.com/en-us/dotnet/core/tutorials/creating-app-with-plugin-support
- https://learn.microsoft.com/en-us/dotnet/core/porting/net-framework-tech-unavailable
- Search-result text only (404 to the fetch tool):
  - https://opendental.com/site/programmingassistance.html
  - https://www.OpenDental.com/site/integrationtypes.html
  - https://opendental.com/site/pluginwhitelisting.html

### 3.11 WebView2 loader

**What the package already does**

- The package's build\Common.targets sets EffectivePlatform to x64 for an x64 build (lines 39-40) and links the win-x64 loader (lines 141-142).
- WebView2LoaderPreference=Static applies only to native C++ projects, not to this program.

**The trap at x64**

- Directory.Build.targets:79-81 updates only the win-x86 item.
- At x64, the package's own item would write a runtimes sub folder. Build-Release refuses sub folders (lines 9 and 29), so the release would fail.

**The fix**

- Name the win-x64 path, or win-$(EffectivePlatform).
- Keep PlatformTarget x64, not AnyCPU.
- Change the Build-Release check to 0x8664.

**Sources**

- https://learn.microsoft.com/en-us/microsoft-edge/webview2/concepts/distribution
- https://learn.microsoft.com/en-us/microsoft-edge/webview2/how-to/static
- https://learn.microsoft.com/en-us/dotnet/api/microsoft.web.webview2.core.corewebview2environment.setloaderdllfolderpath
- https://www.nuget.org/packages/Microsoft.Web.WebView2
- The local NuGet cache, microsoft.web.webview2\1.0.2420.47: build\Common.targets and runtimes\win-x64\native\WebView2Loader.dll
- Directory.Build.targets:66-81
- Directory.Packages.props:23-24, 67-68
- OpenPmsDental/OpenPmsDental.csproj:59-63
- CodeBase/Controls/ODWebView2.cs:192-200
- tools/Build-Release.ps1:9, 29, 209, 245, 258-260
- docs/specs/net10-plan.md:577, 1150-1151, 1277, 1492

### 3.12 DirectX and OpenGL tooth chart

**Notes**

- With safe DLL search mode on, the program folder is searched before System32. An app-local x64 D3DX is therefore found with no code change.
- WDDM drivers register a separate 64-bit OpenGL ICD (OpenGLDriverName) and 32-bit ICD (OpenGLDriverNameWow).
- SharpDX.Direct3D9 4.2.0 has a netstandard1.3 build. Whether to use it or the 4.0.1 net40 files is S3b-3's choice, settled by the S3b-0 spike.
- The license of Microsoft.DXSDK.D3DX lists the files that may be redistributed and has an excluded-licence clause. Counsel must read it next to the GPL 3.0 before the app-local option is chosen.
- My preference is to chain the June 2010 runtime silently (`dxsetup.exe /silent`).

**Sources**

- SparksToothChart/ToothChartWrapper.cs:586-610, 794-813
- SparksToothChart/ToothChartDirectX.cs:290-305, 358
- SparksToothChart/SparksToothChart.csproj:24-44
- WpfControlsOD/UI/ToothChart/ToothChartRelay.cs:340-344, 356-372, 383-394
- docs/port-notes/S3a-15.md:21-25, 44-52, 89-104, 108-111
- docs/port-notes/BIN-A.md:80-95, 123
- docs/owner-questions.md:23
- docs/release-notes/next.md:428-436
- docs/release-process.md:179
- docs/specs/net10-plan.md:737, 1709-1726
- https://www.microsoft.com/en-us/download/details.aspx?id=8109
- https://learn.microsoft.com/en-us/windows/win32/dxtecharts/directx-setup-for-game-developers
- https://learn.microsoft.com/en-us/windows/win32/directx-sdk--august-2009-
- https://www.nuget.org/packages/Microsoft.DXSDK.D3DX
- https://www.nuget.org/packages/Microsoft.DXSDK.D3DX/9.29.952.8/License
- https://walbourn.github.io/legacy-d3dx-on-nuget
- https://learn.microsoft.com/en-us/windows/win32/dlls/dynamic-link-library-search-order
- https://learn.microsoft.com/en-us/windows-hardware/drivers/display/loading-an-opengl-installable-client-driver
- https://www.nuget.org/packages/SharpDX.Direct3D9

### 3.13 Direct2dWrapper (own code)

- The x64 build keeps the /we4311 and /we4312 warnings as errors.
- Direct2dWrapperX64Tests flips: the program folder's copy must be 0x8664, and the Win32 comparison goes.
- An x86 DLL beside the x64 program would throw BadImageFormatException, and nothing in OpenPmsDental/UI catches it.

**Sources**

- https://learn.microsoft.com/en-us/windows/win32/direct2d/direct2d-portal
- https://learn.microsoft.com/en-us/windows/win32/winprog64/running-32-bit-applications
- Direct2dWrapper/Direct2dWrapper.vcxproj:4-18, 33/39/45/52, 85/96, 112/136/156/180
- Direct2dWrapper/pch.h:14-21
- OpenPmsDental/UI/Direct2d/Direct2d.cs:186-706
- docs/port-notes/S3a-16.md sections 1-6
- docs/port-notes/S3a-12.md:187
- docs/specs/net10-plan.md:738, 1076
- tools/Build-Release.ps1:137, 260
- OpenDental.sln:50

### 3.14 Belgian eID

**What the middleware developers say**

- The old BEID_* library was "only available in 32 bit", and "No 64 bit version of beidlib.dll was ever made".
- They said in 2017 that the old SDK is no longer supported. The PKCS#11 module is the SDK now.
- The vendor's own C# sample reads CKO_DATA objects by label ("surname", "firstnames"), and its project has an x64 configuration.

**A correction to S3a-12**

- ReaderState is CharSet.Auto, so the reader name is probably marshalled as Unicode and is not wrong.
- The real second fault is SCardListReaders, which passes an immutable string as its output buffer.
- ATR matching should give way to the token label "BELPIC".

**Sources**

- OpenPmsDental/SmartCards/Belgium/BelgianIdentityCard.cs:11-12, 62, 66-75, 86-91
- OpenPmsDental/SmartCards/Windows/WindowsSmartCardManager.cs:13-22, 43-55
- OpenPmsDental/SmartCards/Windows/ReaderState.cs:7-15
- OpenPmsDental/Main Modules/FormOpenDental.cs:49
- docs/port-notes/S3a-12.md:201-202, 333
- docs/specs/net10-plan.md:736
- https://groups.google.com/g/eid-middleware-dev/c/4C3NFaf3wyo
- https://groups.google.com/g/eid-middleware-dev/c/5DKd85RxtyY
- https://groups.google.com/g/eid-middleware-dev/c/n0Lj0hkEtFw
- https://www.aideacces.be/212.php?langue=EN
- https://github.com/Fedict/eid-mw
- https://github.com/Fedict/eid-mw/tree/master/doc/sdk
- https://raw.githubusercontent.com/Fedict/eid-mw/master/doc/sdk/readme.txt
- https://github.com/Fedict/eid-mw/blob/master/doc/sdk/examples/CS/EidSamples/ReadData.cs
- https://github.com/Fedict/eid-mw/blob/master/doc/sdk/examples/CS/EidSamples/EidSamples.csproj
- https://github.com/Fedict/eid-mw/blob/master/doc/sdk/examples/CS/EidSamples/tests/DataTests.cs
- https://github.com/Fedict/eid-mw/blob/master/cardcomm/pkcs11/src/beidlabels.h
- https://github.com/geersch/BelgianEidVB
- https://dist.eid.belgium.be/releases/5.1.32/RN5132.pdf (search snippet only)
- https://eid.belgium.be/en/technical-documentation

### 3.15 VisiQuick

**What is public**

- The current download tree has no SDK or VqDde package.
- The 4.11.0 change history mentions VqDde fixes, but nothing about 64-bit.
- The "WIN32 x86 dll" wording of the Link SDK is known only from the search engine's summary. Those PDFs now return 404.
- vqdde carries DDE between processes, so a helper copy can drive VisiQuick.exe whatever bitness VisiQuick has.

**Status.** The file stays as it is under the no-cut rule. Record it in O-14 as "no 64-bit vqdde; dormant; helper fallback if revived".

**Sources**

- OpenPmsDental/Misc/VisiQuick.cs:1-2, 14-31, 122, 126, 149, 153, 167, 258
- docs/port-notes/S3a-12.md:203, 333
- docs/specs/net10-plan.md:736, 1163, 1455-1456, 1719-1730
- tools/project-baseline/items.txt:8535
- https://citodent.com/downloads
- Directory listings:
  - https://downloads.citodent.com/pub/VisiQuick/
  - https://downloads.citodent.com/pub/VisiQuick/Latest
  - https://downloads.citodent.com/pub/VisiQuick/Latest/Manuals/
  - https://downloads.citodent.com/pub/VisiQuick/Tools/
  - https://downloads.citodent.com/pub/VisiQuick/Tools/IStart/
  - https://downloads.citodent.com/pub/VisiQuick/Tools/WebLink/
  - https://downloads.citodent.com/pub/VisiQuick/Tools/VDDS/
- https://downloads.citodent.com/pub/VisiQuick/Latest/ChangeHistory.pdf
- https://downloads.citodent.com/pub/VisiQuick/Tools/VDDS/VDDS%20specs%201.4%20Monitor.pdf
- https://citodent.com/visiquick/support
- https://www.citodent.com/visiquick-2/
- Now HTTP 404:
  - https://downloads.citodent.com/pub/VisiQuick/Tools/VisiQuick%20Link%20SDK.pdf
  - https://downloads.citodent.com/pub/VisiQuick/Tools/Integration-SDK-VisiQuick-Link_CDI.pdf
  - https://downloads.citodent.com/pub/VisiQuick/VQDDE%20Integration%20VisiQuick.pdf

### 3.16 Trojan

- The code reads `Software\TROJAN BENEFIT SERVICE`. Open Dental's manual names `HKEY_LOCAL_MACHINE\SOFTWARE\Trojan`.
- That mismatch is the same in either bitness, so it is an owner question, not part of the x64 move.

**Sources**

- WpfControlsOD/Bridges/Trojan.cs:24-31, 33-64
- OpenPmsDental/Forms/FormInsPlan.cs:1353-1389
- docs/port-notes/S3a-12.md:214-216
- docs/specs/net10-plan.md:734
- docs/port-notes/P262-LINKS.md:11
- https://www.opendental.com/manual/trojan.html

### 3.17 Process-start bridges

- No web research was done. The vendor code runs in its own process.
- Owandy's SendMessage already takes an IntPtr wParam (S3a-12).
- Paths come from program properties, not from SpecialFolder.ProgramFiles.

**Sources**

- WpfControlsOD/Bridges/*.cs: 87 of 111 files start a process, plus Iap.cs
- OpenPmsDental/Forms/FormXchargeTokenTool.cs:114
- OpenPmsDental/Forms/FormPayment.cs:4033-4087

### 3.18 Other cross-process or network bridges

- No web research was done. No vendor code enters the process.
- Printer drivers on 64-bit Windows are x64, so the 64-bit program is their native caller.
- The PayConnect DpsPos.dll and RS232.dll paths were already removed.

**Sources**

- WpfControlsOD/Bridges/DexisIntegrator.cs, VixWinOld.cs, DBSWin.cs, EvaSoft.cs:31
- WpfControlsOD/Bridges/PayConnectTerminal.cs:6-7
- OpenPmsDentalBusiness/Eclaims/TerminalConnector.cs:14-22
- CodeBase/Utilities/ODDdeClient.cs
- RdlEngine/Runtime/RdlEngineConfig.cs:334-340
- DataConnectionBase/DbConnectionSecurity.cs:487-491

## 4. Work this creates, as packages in order

The labels A1-A5 and B1-B8 below are only for this survey. Existing plan IDs are named where they apply.

### Stage 3a (.NET Framework 4.8, x86 program; tests can also run x64 on net48 as S3a-15 did)

**A1. S3a-14h: Topaz and Schick native, with the helper as the fallback. Do this first.**
- Merge the S3a-14 helper modules (Schick, Topaz, Guru) into `OpenPmsDentalHelper32`, and register them in Helper32Modules.CreateModules.
- **Topaz.**
  - Collect every SigPlusNET.dll in the search folders.
  - PEReader check in an x64 process. In process when the DLL is AnyCPU or x64. Helper when it is 32-bit only.
  - BadImageFormatException leads to the helper.
  - One decision per process, logged.
  - TopazStandIn DLLs built AnyCPU, x64 and x86-32BITREQUIRED in TopazWrapperTests.
  - SignatureBoxWrapperTests in an x64 run.
- **Schick.**
  - Keep the direct call, but late-bound: create through Type.GetTypeFromCLSID, and make every exam and patient call through IDispatch.
  - A liveness check replaces the dependence on OnClose.
  - Registry pre-check in both views (LocalServer32, InprocServer32, none).
  - HRESULT fallback: retry once in the helper if nothing has run yet; otherwise use the helper from the next click on.
  - Remove the unused interop reference at OpenPmsDental.csproj:49-56.
- **Guru.**
  - Put the declarations in one shared file.
  - PE Machine check: 0x8664 runs in process, but is unproven. Everything else goes to the helper.
  - Fix the helper's DLL search before 3b moves the helper to Helper32\ (launch check G3).
- **Docs.**
  - Correct decision 23's Schick sentence.
  - Correct the TypeLib/IID wording at S3a-14.md:59.
  - Extend launch check S1, and add S4 and S5.

**A2. Plug-in loaders.** This is independent of A1 and can run in parallel in its own worktree. **Done** on `net10/n64a-1` (N64-A, docs/port-notes/N64-A.md): `Plugins.LoadPlugin` with `PluginImageCheck` and `CodeBase.PortableExecutableFile`, tests in `tests/N64ATests` (x86 and x64).
- Add the PE-kind and TargetFramework check before LoadFile in both loaders.
- Replace the empty catch in Plugins.cs with a logged reason.
- Catch BadImageFormatException and FileLoadException in PluginLoader.cs and show a clear text.
- Log a hook-time DllNotFoundException or BadImageFormatException with the plug-in name.

**A3. CADI fallback.** Do this after A1, which sets the helper module pattern. **Done** on `net10/n64a3-1` (N64-A3,
docs/port-notes/N64-A3.md): `NativeFirstChooser.Cadi` reads the ProgID's LocalServer32 and InprocServer32 in both registry views first
(`ComServerRegistration`); the four HRESULTs (CLASSNOTREG, SERVER_EXEC_FAILURE, NOINTERFACE, LIBNOTREGISTERED) move a 64-bit process to
the new helper Cadi module, retried once when no call reached CADI; the in-process path is main's code call for call; tests in
NativeFirstTests (x86 and x64) and Helper32Tests. The registry check of section 5 (launch check C1) is still to do on a PC with CADI.
- Add the HRESULT test in CADINativeWindow.SendData, and optionally the LocalServer32 read.
- Add a new helper CADI module that repeats today's calls and message-only window.
- This can wait for the CADI registry check (section 5) if the owner prefers.

**A4. Small items.** Any time, independent. **Done** on `net10/n64a-1` (N64-A, docs/port-notes/N64-A.md): all three, with Software\Trojan read after the old key name.
- Trojan: optionally look in Registry32, then Registry64, with a test.
- Word: a test that the x64 build compiles the `!DISABLE_MICROSOFT_OFFICE` code, and the HRESULT message for a broken registration.
- DirectX: optionally, a ToothChart log line naming the folder and bitness of the D3DX that loaded.

**A5. Record.** In O-14 and the plan, record:
- VisiQuick as dormant with no 64-bit vqdde;
- Belgian eID as a PKCS#11 port when the feature is turned on;
- cameras as native, with a helper module only if a camera with a 32-bit-only filter appears.

### Stage 3b (.NET 10 x64)

**B1. S3b-0 spike checks.** These come first and gate the rest.
- Item 11: S3a-12's TWAIN code in an x64 net10 process against the 64-bit TWAINDSM 2.5.1 and the driver project's 64-bit sources in fake mode.
- Item 12: the Schick late-bound call against a fake on net10 x64.
- Item 12: QBFC 10 loads in the helper's self-contained net10 x86 process.
- SigPlusNET 2.1.5.x loads in a net10 x64 process, where a test PC has it.
- SharpDX.Direct3D9 4.0.1 net40 versus 4.2.0 netstandard1.3.
- Recheck the TW_INFO.Item four-byte rule against the TWAIN 2.5 specification text.

**B2. S3b-3.** Choose the SharpDX version, from B1's result.

**B3. S3b-4: build changes.**
- Copy the x64 Direct2dWrapper build and drop Win32.
- Link the WebView2 loader for win-x64, flat next to the exe, and fix the comments and ODWebView2.cs:197.
- Rebuild PluginExample as AnyCPU for net10, and add the gate case for an x86 plug-in.
- Give ImagingCapture an x64 platform and fix the SampleGrabber comment.

**B4. S3b-7: setup and release.**
- Install TWAINDSM 2.5.1 x64 into System32, and the 32-bit copy into SysWOW64 if the owner agrees.
- Native-file checks expect 0x8664: Direct2dWrapper, WebView2Loader, and an app-local D3DX if chosen.
- The D3DX path the owner chooses: chain the runtime, or ship it app-local.
- Direct2dWrapper taken from bin\x64\Release.

**B5. Helper TWAIN module.** This is new. Do it after B1 item 11.
- One x86 TWAIN stack in `OpenPmsDentalHelper32`, with its own message pump. Its search order is TWAINDSM beside the helper (for scanning only; CAP refuses an app-local DSM), then SysWOW64, then twain_32.dll.
- **B5a, document scanning.**
  - A source list merged from both DSMs, with the default source stored with its bitness.
  - A picker drawn by the program.
  - Pages returned as files.
  - The "No 64-bit DSM" error only when both lists are empty.
- **B5b, CAP.**
  - A helper ITwainCaptureSession that streams pixels, metadata and status.
  - FuncCreateSession picks the path at Open.
  - Windows and focus across processes.
- **Tests.** FakeTwainDsm in both bitnesses, plus a fake helper.

**B6. Native 64-bit QBFC module.** The owner downloaded the newest QuickBooks Desktop SDK, 18.0 (64-bit installer, signed by Intuit),
on 2026-11-15; it is kept outside the repository with its SHA-256 (`.git/od-tools/downloads/qbsdk18/`). Build on the newest QBFC it
carries (owner rule: always the latest version).
- A new embedded interop from the SDK's newest QBFC (18, or the highest it ships).
- A 64-bit session class behind the same public QuickBooks methods.
- The run-time test: create the newest QBFC first, then each older 64-bit one down to QBFC15, else the helper; fall back to the helper when the connection fails.
- The helper's QBFC 10 module stays.

**B7. Launch checks on test PCs, never at the office.**
- Schick: S1-S5.
- Topaz: sign and show, and stored signatures.
- Guru: G1-G3.
- CADI.
- Word: merge, print, SaveAs PDF.
- TWAIN: a 64-bit scanner, a 32-bit-only scanner, the clinic sensor on both paths.
- The UVC camera.
- DirectX: with the runtime, and on a clean VM.
- WebView2: an eForm, the e-mail preview, a PDF in Images.
- The Direct2D windows.
- QuickBooks 2022 or later, if a PC exists.

**B8. Later, only when triggered.**
- The Belgian eID PKCS#11 port, plus the ReaderState and SCardListReaders fixes and a helper module for the 32-bit middleware.
- VisiQuick, if revived: VDDS-media, else a helper module.
- A helper camera module, if a camera with a 32-bit-only filter appears.

## 5. What the owner must supply

### Sign-ins and the downloads behind them

- **Intuit Developer** (only the owner can do this):
  - sign in;
  - download the newest QuickBooks Desktop SDK 64-bit installer (done 2026-11-15: SDK 18.0, `QBSDK180_x64.exe`);
  - accept the SDK and redistribution licence.
  - Decide whether our setup or the office installs the 64-bit QBFC runtime.
- **No other sign-in is needed.** TWAINDSM 2.5.1 (GitHub), the DirectX runtime, Microsoft.DXSDK.D3DX, the Belgian eID middleware and scanner drivers are all public.

### Decisions

- **O-16.** Approve the TWAINDSM 2.5.1 download. The download rule of 2026-11-14 may already cover it. Take both the 64-bit and the 32-bit DLLs.
- **The 32-bit TWAINDSM.** Should the setup also install the 32-bit TWAINDSM into SysWOW64? S3b-7 names only the 64-bit one. **Answered
  2026-11-15: yes**; S3b-7 installs both.
- **DirectX** (owner-questions.md:23). Chain Microsoft's June 2010 End-User Runtime, or ship the x64 D3DX9_43.dll app-local. **Answered
  2026-11-15: neither** ("we can get rid of Directx after our switch"): TC-8, TC-9a and TC-9b land before the 64-bit build, so it
  carries no SharpDX and needs no D3DX. B1's SharpDX check and B2 (S3b-3's SharpDX version) fall away; no counsel question.
- **System.IO.Ports.** Approve the package, for Topaz serial pads. TerminalConnector probably needs the same approval. **Approved
  2026-11-15** (both).
- **Optional, Trojan.** Should the program also look under `SOFTWARE\Trojan` and in the 64-bit view?

### Vendor questions (optional; the owner's contacts)

- Henry Schein / Guru: does a 64-bit MedVisorInterface.dll exist?
- Patterson or Dentsply Sirona: is there a 64-bit CDR or an automation SDK? Ask only if S1 shows InprocServer32 or the native call fails.
- Corilus / ImageLevel: is Mediadent.Application an EXE server? Or read section 8.7 of the public v8 manual.
- Citodent: a 64-bit vqdde, or VDDS-media patient import. Only if VisiQuick is revived.

### Hardware and software for launch checks (on test PCs, never at the office)

- **Schick:** a Windows 10/11 x64 PC with CDR DICOM 5.x (5.14 or later is best). Media and licence come through Patterson or Dentsply. No sensor is needed.
- **Topaz:** a PC with SigPlusNET 2.1.5.x and a USB HID pad. An old x86-only SigPlusNET, if one can be found.
- **Guru:** a workstation with Guru installed.
- **QuickBooks:** a PC with QuickBooks Desktop 2022 or later and a sample company file. For the fallback, older QuickBooks or only the 32-bit QBFC.
- **TWAIN scanning:** one scanner with a 64-bit driver and one with only a 32-bit driver.
- **TWAIN capture:**
  - the x64 package of drivers 1.2, which is not packaged yet;
  - the clinic PC with the sensor, for the native path and the helper path.
- **CADI:** a PC with CADI or Mediadent, for the reg query and one launch check.
- **Word:** a PC with Word (64-bit Office, and 32-bit if available) and a printer.
- **DirectX:** a 64-bit workstation with a real GPU and the runtime, and a clean VM without System32's d3dx9_43.dll.
- **Camera:** the clinic's UVC intraoral camera on a test PC.
- **Belgian eID, only when the feature is turned on:** a Belgian eID card or test card, a PC/SC reader and the 64-bit eID software.
- **Not needed:** WebView2, Direct2dWrapper and plug-ins need nothing beyond the ordinary x64 launch check.
