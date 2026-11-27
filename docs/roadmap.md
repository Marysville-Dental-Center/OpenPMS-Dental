# Roadmap

Planned work, as agreed with the owner. Each item is scoped with the owner and starts with a design document in
[specs/](specs/) before any code (CLAUDE.md, "Design before code"). The owner's decisions are in
[decisions/owner-questions.md](decisions/owner-questions.md).

Status on 2026-11-26.

## Planned

Not scheduled yet; the owner sets the order.

### CRM: leads, calls and texts

Owner, 2026-11-26: "Add CRM functionality with lead tracking and in program VOIP calls (integrating with Unifi Talk) and text".

- **Lead tracking.** Prospective patients before they have a chart: source (referral, web, phone, campaign), status through a pipeline
  (new, contacted, scheduled, converted, lost), follow-up tasks and reminders, notes and the history of every contact. A lead becomes a
  patient without typing the data again, and keeps its history. Reports on lead sources and conversion.
- **Calls from the program.** Click to call a patient or lead, see who is calling (the caller's chart or lead on an incoming call), and
  log each call as a communication item, through UniFi Talk. The design first settles what UniFi Talk's published interfaces allow (call
  control, call events, call records) and where they run (the console on the practice's network, the workstation).
- **Texts.** Send and receive texts with patients and leads from the program, threaded with the calls, through UniFi Talk where it
  supports messaging or another provider the office chooses.
- **Rules that apply.** Off until the office turns it on; the program talks to the office's own phone system or provider with the office's
  own account, never through Open Dental's servers; consent and opt-out for texts are recorded; data goes in fork-only tables
  (`ForkTableSet`).
- Design: a new spec in `specs/` (data model, the UniFi Talk interface, consent, the windows), then packages.

### Scribing with a local speech-to-text model

Owner, 2026-11-26: "add some scribing features with OSS voice-to-text local model".

- **Dictation and visit notes.** Speech to text for clinical notes, procedure notes and the perio chart, with the transcript reviewed and
  edited before it is saved; nothing is filed without the user's OK.
- **Local only.** An open-source speech-to-text model running on the workstation (candidates for the design: Whisper models through
  whisper.cpp, both MIT-licensed), so no audio or text leaves the computer. Audio is not kept after the transcript is saved unless the
  office chooses to keep it.
- **To settle in the design:** the model and its size against workstation hardware (CPU and GPU), dental vocabulary (tooth numbers,
  surfaces, CDT terms), the microphone and push-to-talk, how it relates to the existing voice perio charting, recording consent, and how
  the model files are shipped with their licences.
- Design: a new spec in `specs/`, then packages.

### A Middle Tier in Rust, on Linux

Owner, 2026-11-26: "This one should be Rust Middle Tier. Server component is just mariaDB." The only server component the program needs
today is MariaDB. The Middle Tier (today an ASP.NET Core server on Windows, `OpenPmsDentalServer`, which the release leaves out) is
planned as a Rust service that runs on Linux, carrying the same messages, so the workstations need no change. A plan first: which of the
business methods it serves, how it shares their logic with the .NET business library, authentication and TLS, and the tests that prove
both servers answer alike.

### Open Dental 26.3

When Open Dental marks it stable: `main` moves to its schema and its features are re-implemented from its public documents, as 24.4 to
26.2 were ([specs/features-26.2.md](specs/features-26.2.md)).

### Smaller items

- **A setting "each active patient has a different e-mail address"** (owner, 2026-11-25; **deferred**, owner, 2026-11-26). Off by
  default; when on, checked before saving, with a clear message, wherever a patient's e-mail or status changes. It must be coordinated with
  the patient portal that uses `patient._portal_active_email`, and will likely move that column out of Open Dental's tables. The design and
  its review's findings: [specs/unique-patient-email.md](specs/unique-patient-email.md). Until then adoption keeps such a column and key,
  and a duplicate shows MariaDB's error ([ADOPT-PORTAL-EMAIL](port-notes/ADOPT-PORTAL-EMAIL.md)).
- **A sensor's driver settings saved per device in the database** (owner, 2026-11-24). Today Capture settings keeps them in each computer's
  capture.ini, so a USB sensor moved between computers needs the same `Source.*` values on each one. Stored with the imaging device, every
  computer would use them after one save; only the driver install, the calibration files and the hardware binding stay per computer.
  Needs moving the values already saved in capture.ini and keeping the gated-settings check and its log working across computers.
  Designed and reviewed, not built yet: [specs/sensor-settings-per-device.md](specs/sensor-settings-per-device.md).
- **Compressed DICOM decoders**, only if such files turn up: JPEG Extended and Lossless, JPEG 2000 and JPEG-LS, through a thin layer over
  libjpeg-turbo, OpenJPEG and CharLS, plugged into the in-house reader's decoder hook. Today the reader opens uncompressed DICOM, RLE and
  JPEG Baseline, and names the format of any other compressed file
  ([reports/2026-11-02-dicom-codecs-research.md](reports/2026-11-02-dicom-codecs-research.md)).
- **The next release**, when the owner asks for one ([develop/release-process.md](develop/release-process.md)).

## Done in the next version

Built after 0.1 (tagged on 2026-11-25); the release notes collect them in [release-notes/next.md](release-notes/next.md).

- **Adoption**: Open Dental databases whose version labels differ from their schema version (25.3.47.0 / 25.3.54.0 with the 25.3.48
  schema) ([ADOPT-LABELS](port-notes/ADOPT-LABELS.md)); Open Dental 26.2 databases, checked against the published 26.2 schema
  ([ADOPT-262](port-notes/ADOPT-262.md)); a patient portal's unique e-mail column kept as it is
  ([ADOPT-PORTAL-EMAIL](port-notes/ADOPT-PORTAL-EMAIL.md)); `claimpayment`'s table checksum change
  ([ADOPT-CLAIMPAYMENT-CHECKSUM](port-notes/ADOPT-CLAIMPAYMENT-CHECKSUM.md)); the runbook brought up to date with the code
  ([RUNBOOK-R4](port-notes/RUNBOOK-R4.md)).
- **Visuals in their own windows**: the camera, the X-ray preview and the graphical perio chart open in ordinary windows
  ([specs/visuals-in-own-windows.md](specs/visuals-in-own-windows.md)).
- **Camera and sensor settings**: every driver setting shown readably, flexible camera buttons, light and brightness
  ([IMG-SRCSET](port-notes/IMG-SRCSET.md), [FB-1](port-notes/FB-1.md)).
- **Feedback fixes** (FB-1): hover flicker, the tooltip crash, crash reports.
- **The new database file**: no third-party copyrighted content, and program link passwords start blank
  ([INITSQL-COPYRIGHT](port-notes/INITSQL-COPYRIGHT.md)).
- **Tamper detection removed**: the program neither computes nor checks the `SecurityHash` columns (kept, with their values); no warning
  triangles, no Security Hashes tool, no rehash after adoption ([specs/remove-tamper-detection.md](specs/remove-tamper-detection.md),
  [NOHASH](port-notes/NOHASH.md)).
- **Documentation**: how-to guides with screenshots, the docs arranged by reader, the link check, the standing rules in CLAUDE.md, and the
  Open Dental 26.2 design documents ([specs/features-26.2.md](specs/features-26.2.md),
  [specs/schema-changes-26.1-to-26.2.md](specs/schema-changes-26.1-to-26.2.md)) ([DOCS-HOWTO](port-notes/DOCS-HOWTO.md)).

## Released in 0.1

Released on 2026-11-25; what each one changes is in [release-notes/v0.1.md](release-notes/v0.1.md).

1. **.NET 10, 64-bit and self-contained**: the program, its libraries, tools and services on `net10.0-windows` x64 with their own runtime,
   the 32-bit helper on its own x86 runtime, the Middle Tier as an ASP.NET Core server, the 64-bit setup and self-update
   ([specs/net10-plan.md](specs/net10-plan.md), [specs/native64-survey.md](specs/native64-survey.md)). The 32-bit-only vendor components
   (QuickBooks Desktop, Schick CDR, Topaz, Guru) work through 32-bit helpers.
2. **The tooth chart on WPF 3D** with Helix Toolkit, replacing the DirectX 9 and OpenGL engines, with free text, orthodontic hardware, the
   extraction graphic and TextBelow ([specs/wpf-tooth-chart-plan.md](specs/wpf-tooth-chart-plan.md)).
3. **EHR rebuilt as open code**, on the 2027 quality measures ([specs/ehr-rebuild-plan.md](specs/ehr-rebuild-plan.md),
   [specs/ehr-2027-measures-plan.md](specs/ehr-2027-measures-plan.md)).
4. **The UI refresh**: tokens, the window frame, Fluent icons, Segoe UI 9 pt, every window on one layout grid, final review approved on
   2026-11-24 ([specs/ui-refresh-plan.md](specs/ui-refresh-plan.md), [release-notes/ui-refresh.md](release-notes/ui-refresh.md)).
5. **Native intraoral capture**, with the sensor driver's twain-contract 1.3 and 1.4
   ([specs/imaging-capture-design.md](specs/imaging-capture-design.md)).
6. **Open Dental 26.2 features** on its 26.2.0 schema: appointments, billing and statements, claims and insurance, the Perio Chart, tools,
   reports, eForms and recalls, program links, and Glidewell lab cases without a connection to Glidewell yet
   ([specs/features-26.2.md](specs/features-26.2.md)), with Open Dental 25.3's settings read under their own names and its eight options.
7. **Library updates (security)** and client-side database security: SSH.NET, MailKit/MimeKit 4, Newtonsoft.Json 13, MSAL and the Google
   APIs, remembered SSH host keys, and encrypted database connections with the program's own account.
8. **Library updates (maintenance)**: WebView2 in place of Internet Explorer, WeCantSpell.Hunspell, PDFsharp/MigraDoc 6.5 built from
   source, and the in-house DICOM reader in place of fo-dicom.
9. **Routine maintenance** and **inventory tracking** ([specs/routine-maintenance.md](specs/routine-maintenance.md),
   [specs/inventory-tracking.md](specs/inventory-tracking.md)).
10. **Storage rename**: `OpenPmsDentalCloud` became `OpenPmsDentalStorage`.
11. **Fixes from the owner's answers of 2026-11-07**: a declined PayConnect 2 charge is a decline, an edited X-ray attached to a claim is
    saved, the Insurance History step leaves the frequency code groups unchanged, the DentX, Dexis Integrator and Vipersoft links work
    without NDde.dll, capture is on for new installs, and the backup to another server works.
