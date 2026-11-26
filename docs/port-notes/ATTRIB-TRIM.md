# ATTRIB-TRIM: Open Dental credited once where its licence needs it

Package ATTRIB-TRIM, on `main`. The owner asked on 2026-11-26 to remove the references to Open Dental, then chose to trim them to the
minimum once told that the GPL requires Open Dental's copyright notices to be kept ([owner-questions.md](../decisions/owner-questions.md),
item 14).

## What changed

- **The copyright line of the program's files** (`SharedAssemblyInfo.cs`, `Direct2dWrapper/xDirect2dWrapper.rc`, the setup's `AppCopyright`):
  the program's copyright first, then "Portions copyright (C) 2003 and later years, Jordan Sparks, D.M.D., Open Dental Software, Inc. and
  contributors", then the licence.
- **About box:** the second copyright line is the program's own ("Copyright 2026-<year>, Open PMS - Dental contributors"); Open Dental is
  credited once, in the attribution line above it.
- **README:** the attribution paragraph is replaced by one line that points to `LICENSE` and `NOTICE`.
- **Setup information page:** `installer/setup-info.txt` no longer repeats the attribution; `NOTICE`, shown on the same page, starts with it.
- **Kept as they are:** `NOTICE`, the source files' own copyright headers, the About box's attribution (`AboutBoxInfo.Attribution`), the
  release notes of published releases and the project site's footer ("not affiliated with or endorsed by").

## Tests

`AttributionTests` (the README points to `NOTICE` without repeating it; `FormAbout.cs` names Open Dental only through the attribution),
`ReleaseFlowTests` (`SharedAssemblyInfo` puts the program's copyright first and keeps Open Dental's; the setup template does not repeat
`NOTICE`), and `NoHqTests`' list of kept literals.
