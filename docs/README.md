# Documentation

Every document of Open PMS - Dental, by who reads it. Start with the [how-to guides](how-to/README.md).

| Folder | For | What is there |
|---|---|---|
| [how-to/](how-to/README.md) | Office staff and the administrator | Short task guides with screenshots |
| [admin/](#admin-running-an-office) | The office's administrator | Installing, the database server, moving from Open Dental, capture setup |
| [develop/](#develop-building-and-releasing) | Contributors | Building, testing, releasing, the repository |
| [specs/](#specs-designs-and-plans) | Contributors | What each feature does and why, and the plans of the big moves |
| [decisions/](#decisions-the-owners-answers) | Everyone | The owner's decisions, with dates and the owner's words |
| [reports/](reports/README.md) | Everyone | Audits, validation runs and dated reviews, as they were |
| [release-notes/](release-notes/) | Everyone | What each release changed |
| [port-notes/](port-notes/) | Contributors | One note per package: what changed, the tests, the review |
| [screenshots/](screenshots/README.md) | Contributors | The pictures in these documents and how they are made |

Also here: the [roadmap](roadmap.md) and [how Open PMS - Dental differs from Open Dental](differences.md).

## admin: running an office

| Document | What it covers |
|---|---|
| [installing.md](admin/installing.md) | The database server, the workstations and updates |
| [db-connection-security.md](admin/db-connection-security.md) | The server side of encrypted database connections and the program's database account |
| [adoption-runbook.md](admin/adoption-runbook.md) | Moving an office from Open Dental, step by step, with checks and rollback |
| [intraoral-capture-setup.md](admin/intraoral-capture-setup.md) | Setting up intraoral X-ray capture in a clinic, and what its notices mean |
| [launch-checks-3b.md](admin/launch-checks-3b.md) | The owner's checklist for the 64-bit .NET 10 build |

## develop: building and releasing

| Document | What it covers |
|---|---|
| [building.md](develop/building.md) | Prerequisites, the build, the solution layout, a development copy |
| [testing.md](develop/testing.md) | What the tests cover and how to run them |
| [release-process.md](develop/release-process.md) | Versions, the release build, the setup, the version guard |
| [repository-layout.md](develop/repository-layout.md) | What each folder holds, the branches |
| [third-party-source-builds.md](develop/third-party-source-builds.md) | The libraries built here from their source |

## specs: designs and plans

| Area | Documents |
|---|---|
| Features of this program | [inventory tracking](specs/inventory-tracking.md), [routine maintenance](specs/routine-maintenance.md), [visuals in their own windows](specs/visuals-in-own-windows.md), [remembered SSH host keys](specs/ssh-host-keys.md), [database connection security](specs/db-connection-security.md), [tamper detection removed](specs/remove-tamper-detection.md), [unique patient e-mail](specs/unique-patient-email.md) |
| Imaging | [capture design](specs/imaging-capture-design.md), [sensor driver settings per device](specs/sensor-settings-per-device.md), [capture metadata](specs/capture-metadata-v1.md), [the DICOM reader](specs/dicom-reader.md) |
| Open Dental's features, re-implemented | [24.4](specs/features-24.4.md), [25.1](specs/features-25.1.md), [25.2](specs/features-25.2.md), [25.3](specs/features-25.3.md), [25.4](specs/features-25.4.md), [26.1](specs/features-26.1.md), [26.2](specs/features-26.2.md), [port backlog](specs/port-backlog.md), schema changes [24.3 to 26.1](specs/schema-changes-24.3-to-26.1.md) and [26.1 to 26.2](specs/schema-changes-26.1-to-26.2.md), [vendor-direct integrations](specs/vendor-direct/README.md) |
| Big moves | [decoupling from Open Dental](specs/decoupling-plan.md), [.NET 10](specs/net10-plan.md), [native 64-bit survey](specs/native64-survey.md), [UI refresh](specs/ui-refresh-plan.md), [WPF tooth chart](specs/wpf-tooth-chart-plan.md), [EHR rebuild](specs/ehr-rebuild-plan.md), [EHR 2027 measures](specs/ehr-2027-measures-plan.md) |

## decisions: the owner's answers

| Document | What it covers |
|---|---|
| [owner-questions.md](decisions/owner-questions.md) | Every owner decision of the project, with the date and the owner's words |
| [imaging-capture-owner-questions.md](decisions/imaging-capture-owner-questions.md) | The owner's decisions on native intraoral capture |
