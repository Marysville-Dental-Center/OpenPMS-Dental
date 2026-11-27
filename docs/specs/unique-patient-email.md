# A setting: each active patient has a different e-mail address

Owner decision of 2026-11-25 (roadmap, "A setting 'each active patient has a different e-mail address'"; owner question 12 in
[decisions/owner-questions.md](../decisions/owner-questions.md)). Package **UNIQUE-EMAIL**.

> **Status: deferred (owner, 2026-11-26).** Not implemented. The design below needs the program to add and drop a column and a key of
> Open Dental's `patient` table, which the schema rule forbids, and it must be coordinated with the patient portal that uses the column.
> The likely direction is to move the portal's column out of Open Dental's tables (a fork-only table). The design and the review's
> findings are kept here for that work; see "Review findings (2026-11-26)" at the end.

## Why

A patient portal that signs patients in by e-mail address needs one active patient per address. Some Open Dental databases enforce it
with a generated column and a unique key on `patient` (`_portal_active_email`, `portal_active_email_uniq`). Adoption keeps them
([port-notes/ADOPT-PORTAL-EMAIL.md](../port-notes/ADOPT-PORTAL-EMAIL.md)), but the program does not know about them: a save that
breaks the rule fails with MariaDB's "Duplicate entry" error. This package makes the rule a setting of the program, with clear messages.

## The setting

- Preference row `ForkPatientEmailUnique` (fork name, like the other `Fork*` rows): `1` on, missing or `0` off. Off by default.
- The setting is **on exactly when the column and key exist** with the expected definitions (`SchemaVerifier.PortalActiveEmail*`). The
  preference records the choice; the program reads the schema (once per start and after a change) and, when the two disagree, follows the
  schema and writes the preference to match, with a log line. So an adopted database that has the key is "on" without any step, and adoption
  itself writes the preference (`Adoption.KeepPortalActiveEmail`: `1` when the key is kept).
- The column and key are the only addition to an Open Dental table this program makes. The schema rule otherwise forbids it; this one is the
  owner's decision, and the verifier already allows exactly this column and key against every reference, so a database with them still
  matches Open Dental's reference schema and Open Dental's next schema still applies on top (it never touches the column).

## Where it is set

Setup, Preferences, Family, General: a row "Each active patient has a different e-mail address" with its state ("On" / "Off") and a
button **Change...**, which opens the window **Patient e-mail addresses** (a native standalone window, Setup and Security Admin
permissions):

- **Turning on.** The window lists the active patients (`PatStatus` Patient) whose e-mail address another active patient also has, grouped
  by address (name, PatNum, birthdate; double-click opens the patient). With any listed, **Turn on** is disabled and the text says to give
  each a different address first. With none, **Turn on** runs, with a progress window, `ALTER TABLE patient` + `SchemaVerifier.PortalActiveEmailAddDdl`
  (MariaDB builds the key; on a large table this takes a while and locks `patient`, which the window says), writes the preference and a
  Setup security log entry, and refreshes the preference cache everywhere (signal).
- **Turning off.** Asks for confirmation ("A patient portal that signs patients in by e-mail address may rely on this rule"), then drops the
  key and the column, writes the preference and the log entry.
- A failure (a duplicate added in the meantime, a lock timeout) leaves the schema as it was and shows the reason; the list refreshes.

## Where it is checked

When on, a save that would give an active patient an address another active patient has is stopped with one message:

> Another active patient, <LName, FName> (PatNum <n>), already has the e-mail address <address>. Each active patient must have a
> different e-mail address (Setup, Preferences, Family).

- **Before saving**, through `Patients.GetActiveWithEmail(email,patNumExclude)` (a parameterized `SELECT ... WHERE PatStatus=0 AND
  Email=@email AND PatNum<>@patNum`, so the comparison uses the column's collation, as the key does: case and trailing spaces ignored):
  Edit Patient (FormPatientEdit OK), Add Family (FormPatientAddAll), the recall list's e-mail edits (FormRecallList), sheet import
  (FormSheetImport) and eForm import (FrmEFormFillEdit / the import rules), and reactivating a patient (a change of `PatStatus` to Patient:
  FormPatientEdit, FormPatientStatusTool, which reports each patient it skips).
- **Every other save** (HL7, X834, bridges, the API paths, the Middle Tier) goes through `Patients.Insert` and `Patients.Update`. There a
  `MySqlException` with error 1062 on `portal_active_email_uniq` becomes an `ODException` with the same message (looking up the other
  patient), so the caller's usual error handling shows or logs it. No other duplicate-key error is changed.
- Merging patients (FormPatientMerge) is not affected: the patient merged from becomes inactive.

## Tests

- Unit: the message text; `IsPortalEmailDuplicateKeyError` recognizes 1062 on that key only.
- Database (new suite `UniqueEmailTests`, scratch databases `test_uniqueemail_*`): turning on with duplicates is refused and lists them;
  turning on creates exactly the verifier's definitions and `SchemaVerifier` still reports a match; the pre-save check finds a duplicate
  ignoring case and trailing spaces and ignores inactive patients and the patient itself; `Patients.Update` with a duplicate throws the
  message; turning off drops both; the preference follows the schema when they disagree; adoption of a database with the key writes `1`.
- UI: the Family General row and the new window on the layout grid at 100% and 150% (`GridRulesTests`), a before/after gallery.

## Documents

Port note `docs/port-notes/UNIQUE-EMAIL.md`; a how-to guide with screenshots, "Require a different e-mail address for each patient"
([how-to/](../how-to/README.md)); ADOPT-PORTAL-EMAIL and the runbook say adoption turns the setting on; the roadmap entry moves to "Done".

## Open points

- The window's permission: Setup, plus Security Admin because turning off can break a portal's sign-in. Recommended as written.

## Review findings (2026-11-26)

A separate review agent checked this design before implementation. Its findings, to settle when the work resumes:

1. **The schema rule.** Owner decision 12 lets adoption *keep* an existing `_portal_active_email` column and its key. It does not let the
   program add them to `patient`, or drop an office's key. Either the owner makes an explicit exception (recorded with the owner's words,
   and in the standing rules), or the rule's data moves to a fork-only table. An Open Dental conversion that rebuilds `patient` (`CONVERT TO`,
   a changed `Email`) would also change a generated column's definition, and `SchemaVerifier` would report it.
2. **Not every save goes through `Patients.Insert`/`Update`.** `Patients.UpdateEmailPhoneForFam` writes `Email` for a whole family with
   raw SQL (Edit Patient's "same for family" box). With a unique key on, it fails for any family with two active members, after the
   patient row itself was saved (MyISAM, no transaction). The design must skip or pre-check that propagation.
3. **The save paths.** Add Family is `WpfControlsOD/Frms/FrmPatientAddAll.xaml.cs`; it copies member 1's address to the others and inserts
   in a loop, so its check must also catch duplicates among its own members before the first insert. The Recall List edits no patient
   e-mail (drop it from the list). The eForm import saves in `OpenPmsDentalBusiness/EForms/EFormImport.cs`. The Patient Status tool can
   reactivate patients, so its check stays.
4. **The duplicate query.** `Db`/`DataCore` reads take no parameters, so a read either uses `POut` or a new parameterized read. Compare on
   the key's own expression (`_portal_active_email`, utf8mb3_general_ci, PAD SPACE) so the check and the key agree, and test a non-ASCII
   address (the connection is utf8mb4).
5. **The duplicate-key error** is detectable: `RunDbAction` rethrows `MySqlException` unchanged, and the existing portal tests already see
   `DuplicateKeyEntry` and the key name. Translate it on the server side of the Middle Tier, because `DtoProcessor` passes only an exact
   `ODException` to the client.
6. **The preference duplicates the schema.** Every workstation writing it when the two disagree is racy; derive "on" from one server-side
   method, or let only the setting window and adoption write it.
