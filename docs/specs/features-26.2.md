# Open Dental 26.2: desktop feature specs for this fork

Status: written 2026-11-26, after the build; it records the design the 26.2 packages followed (their port notes carry the full detail and
the review history). Baseline: this fork at Open Dental 26.1.70 parity ([features-26.1.md](features-26.1.md)). Target: parity with Open
Dental 26.2.0 for the items chosen with the owner on 2026-11-07, on the 26.2.0 schema described in
[schema-changes-26.1-to-26.2.md](schema-changes-26.1-to-26.2.md).

This file classifies every change on the 26.2 release-notes page: 67 items in twelve sections (Major Changes 3, Appointments 4, Account 4,
Chart 5, Manage 3, Insurance 3, Tools 8, Reports 4, eServices 3, Miscellaneous 6, Program Links 8, API 16). Items are numbered 1 to 67 in the
page's order, the numbering the port notes use ("item 22"). Each desktop or vendor-direct item says what was built, what the public sources
leave open (marked **Inferred**), and where the work is recorded.

## How this spec was built (read first)

- **Sources used (all public).**
  - Release notes: <https://www.opendental.com/site/version26_2.html>, read in full on 2026-11-07 and again on 2026-11-26. 26.2 was released
    as stable on 2026-11-03; the page says features may still be added as the version progresses. Every item is one line, and none links to
    a manual page.
  - Published database documentation: the 26.2 diff document <https://www.opendental.com/OpenDentalDiffDocumentation26-2.xml> (17 tables,
    40 columns, 23 enum values in 13 enumerations, no deletion) and the full document
    <https://www.opendental.com/OpenDentalDocumentation26-2.xml> (430 tables). Their column descriptions are the most precise public
    statement of several items (the claim form per provider, the Carriers Match condition, the referral commlog types, the perio setting).
  - The public manual (`https://www.opendental.com/manual/*.html`), which still describes the windows before 26.2, and the public feature
    pages (`https://www.opendental.com/site/0_*.html`), which are current-version pages (the Perio Chart page shows the 26.2 exam panel).
  - Vendors' own public pages for the program links (Allied Star, OrthoComposite's Microsoft Store listing, Medit's partner page).
- **The 26.2 manual is not public.** The `help262/` pages answer only to customers on support. They were not used, no Open Dental program
  was installed, observed or decompiled, and no practice's data was used. Where the sources are silent, the behaviour is **our design**,
  marked **Inferred** here and in the port note.
- **Preference names.** New settings that Open Dental stores under an unpublished name are fork-owned rows (`Fork...`), read by name, so an
  Open Dental 26.2 database loads unchanged.
- **Schema.** All schema work is one foundation package, P262-SCHEMA; the feature packages add no column to Open Dental's tables.
- **IDs.** Each item is `v26_2-<module>-<short-name>`. Items 11 and 19 are 26.1 backports and keep their 26.1 IDs.
- **Packages.** Ten packages, P262-SCHEMA first, then nine feature packages in parallel (milestone M3 in [port-backlog.md](port-backlog.md)).
  Items 12 and 13 were built with the WPF tooth chart (TC-5, [wpf-tooth-chart-plan.md](wpf-tooth-chart-plan.md)).

## Summary

Counts: **35 built in the 26.2 work** (33 in the P262 packages, 2 in TC-5), **2 already built** as 26.1 backports, **1 not ported** by owner
decision, **29 excluded** (67 items).

| # | ID | Title | Module | Category | 26.2 schema | Status | Package |
|---|---|---|---|---|---|---|---|
| 1 | v26_2-lab-glidewell-send | Send lab cases to Glidewell from Edit Lab Case | lab cases | vendor-direct | glidewellacct, glidewellmsg, labcaseproduct, provider.GlidewellAcctNum | built; transport blocked | P262-GLIDEWELL |
| 2 | v26_2-lab-glidewell-scanners | Glidewell cases with Medit or iTero scans | lab cases | vendor-direct | glidewellmsg.SentITero, SentMedit | built; transport blocked | P262-GLIDEWELL |
| 3 | v26_2-lab-glidewell-cloud | Glidewell integration on Open Dental Cloud | lab cases | excluded | none | excluded | none |
| 4 | v26_2-appt-search-multi-blockout | Advanced Appointment Search over several blockout types | appointments | desktop | none | built | P262-APPT |
| 5 | v26_2-appt-practice-notes | Practice and clinic schedule notes in the Appointments module | appointments | desktop | none | built | P262-APPT |
| 6 | v26_2-appt-search-slots-per-blockout | Search lists a slot for every selected blockout type per day | appointments | desktop | none | built | P262-APPT |
| 7 | v26_2-appt-estimates-on-plan-save | Appointment estimates updated when a plan is saved | appointments | desktop | none | built | P262-APPT |
| 8 | v26_2-account-statement-options-window | Statement dropdown options in their own window | account | desktop | none | built | P262-BILLING |
| 9 | v26_2-account-statement-window-redesign | Statement window redesigned | account | desktop | none | built | P262-BILLING |
| 10 | v26_2-account-claimedit-tabs | Status History and Misc moved in Edit Claim | account | desktop | none | built | P262-CLAIMS |
| 11 | v26_1-account-dxc-attach-pdf | DentalXChange attachments accept PDF (26.1.64) | account | vendor-direct | none | already built | V-DXC2 |
| 12 | v26_2-chart-extraction-graphic | Completed extractions drawn with an X | chart | desktop | none | built | TC-5 |
| 13 | v26_2-chart-paint-textbelow | Paint type TextBelow: text outside the tooth chart | chart | desktop | ToothPaintingType 18 | built | P262-SCHEMA, TC-5 |
| 14 | v26_2-perio-flags-selected-teeth | Flags on all selected teeth in the Perio Chart | chart | desktop | none | built | P262-PERIO |
| 15 | v26_2-perio-multi-cell-select | Several cells in any editable Perio Chart row | chart | desktop | none | built | P262-PERIO |
| 16 | v26_2-perio-past-exam-values | Settings to show past exams' values | chart | desktop | UserOdFkeyType 31 | built | P262-PERIO |
| 17 | v26_2-manage-ebill-window | Electronic Billing settings in their own window | manage | desktop | none | built | P262-BILLING |
| 18 | v26_2-manage-billing-defaults-clinic | Billing Defaults reconfigured, some settings per clinic | manage | desktop | none (clinicpref rows) | built | P262-BILLING |
| 19 | v26_1-manage-timecard-future-clockout | Future-dated clock-out events (26.1.63) | manage | desktop | none | already built | M2-EMP |
| 20 | v26_2-ins-claimform-per-provider | Default claim form per provider | insurance | desktop | provider.ClaimFormNum | built | P262-CLAIMS |
| 21 | v26_2-ins-combine-plans-mismatch | Detailed mismatch warning when combining plans | insurance | desktop | none | built | P262-CLAIMS |
| 22 | v26_2-ins-donotbill-automation | Automation trigger: procedure set to not bill insurance | insurance | desktop | EnumAutomationTrigger 11, AutoCondField 15 | built | P262-CLAIMS |
| 23 | v26_2-tools-dbm-income-transfer | DBM: payments with income transfer issues | tools | desktop | none | built | P262-TOOLS |
| 24 | v26_2-tools-dbm-conversion-pay-dates | DBM tool: incorrectly dated conversion payments | tools | desktop | none | built | P262-TOOLS |
| 25 | v26_2-tools-fee-copy-code | Fee Tool: copy fees from one code to another | tools | desktop | none | built | P262-TOOLS |
| 26 | v26_2-tools-latency-average | Test Latency measures the average | tools | desktop | none | built | P262-TOOLS |
| 27 | v26_2-tools-dbm-chat-duplicates | DBM: duplicate entry errors starting a chat | tools | excluded | none | excluded | none |
| 28 | v26_2-tools-mariadb-backup-standalone | Standalone MariaDB backup and restore tools | tools | excluded | none | excluded | none |
| 29 | v26_2-tools-mariadb-backup-automated | Automated MariaDB backups in the Backup tool | tools | excluded | AlertType 52, 53 | excluded | none |
| 30 | v26_2-tools-dbm-insverify-redundant | DBM: move redundant InsVerify entries | tools | desktop | none | built | P262-TOOLS |
| 31 | v26_2-reports-display-settings | Report Display Settings layout | reports | desktop | none | built | P262-REPORTS |
| 32 | v26_2-reports-security-permissions | Report Security Permissions layout | reports | desktop | none | built | P262-REPORTS |
| 33 | v26_2-reports-ppu-prior-balances | Patient Portion Uncollected: prior balances | reports | desktop | none | built | P262-REPORTS |
| 34 | v26_2-reports-ppu-payplan-column | Patient Portion Uncollected: Pay Plan column | reports | desktop | none | built | P262-REPORTS |
| 35 | v26_2-eservices-ereminder-eclipboard-tag | eReminder tag for the eClipboard URL | eServices | excluded | none | excluded | none |
| 36 | v26_2-eservices-websched-advance-limit | Web Sched booking-ahead limit | eServices | excluded | none | excluded | none |
| 37 | v26_2-eforms-vital-signs | Vital signs imported from eForms | eForms | desktop | none | built | P262-MISC |
| 38 | v26_2-misc-hq-global-alerts | Global alerts from Open Dental HQ in the title bar | misc | excluded | none | excluded | none |
| 39 | v26_2-misc-commlog-referral-types | Definition category Commlog Referral Types | misc | desktop | DefCat 57 | built | P262-MISC |
| 40 | v26_2-misc-chat-service-url | Chat service connection URL preference | misc | excluded | none | excluded | none |
| 41 | v26_2-recall-prophy-perio-switch | Automatic Prophy and Perio recall switch | recall | desktop | none (fork preference) | built | P262-MISC |
| 42 | v26_2-misc-econnector-hq-alert | Alert when the eConnector cannot reach HQ | misc | excluded | AlertType 54 | excluded | none |
| 43 | v26_2-misc-rxnorm-2026 | 2026 RxNorm codes to import | misc | excluded | none | excluded | none |
| 44 | v26_2-links-alliedstar-connect | Bridge to Allied Star Connect | program links | vendor-direct | none | built | P262-LINKS |
| 45 | v26_2-links-orthocomposite | Imaging bridge to OrthoComposite | program links | vendor-direct | none | built | P262-LINKS |
| 46 | v26_2-links-trojan-collect-retired | Trojan Express Collect deprecated | program links | vendor-direct | none | built | P262-LINKS |
| 47 | v26_2-links-suni-removed | Suni support removed | program links | vendor-direct | none | built | P262-LINKS |
| 48 | v26_2-links-ez3di-removed | Ez3D-i bridge removed | program links | vendor-direct | none | not ported | P262-LINKS |
| 49 | v26_2-cloud-payconnect2 | PayConnect 2.0 on Open Dental Cloud | cloud | excluded | none | excluded | none |
| 50 | v26_2-cloud-globalpayments-v2 | Global Payments V2 on Open Dental Cloud | cloud | excluded | none | excluded | none |
| 51 | v26_2-cloud-tigerview | TigerView on Open Dental Cloud | cloud | excluded | none | excluded | none |
| 52 to 67 | v26_2-api-* | Sixteen API changes (Definitions, Subscriptions, Benefits, Documents, ClaimProcs, Appointments, PayPlanCharges, CodeGroups, AppointmentTypes, InsPlans) | API | excluded | apisubscription columns (data only) | excluded | none |

## Major Changes: Glidewell lab cases (items 1 to 3)

**What 26.2 says.** Lab cases can be sent to Glidewell electronically from the Edit Lab Case window, with Medit or iTero scans, also on Open
Dental Cloud. The documents add `glidewellacct` (account number, encrypted API key, expiry), `glidewellmsg` (one row per sent case: status,
Glidewell's id, the raw request and response, which scanner), `labcaseproduct` (product, material, shade, teeth, notes),
`provider.GlidewellAcctNum` (a provider's default account) and the eight order statuses.

**Built (P262-GLIDEWELL).** A Glidewell program link (off by default and hidden in Program Links until a transport exists); Glidewell
accounts with the API key in this program's own encrypted format and a provider default; a Glidewell group in Edit Lab Case (account,
scanner, products, Send, Check Status, Details), shown only while the link is on; the request built from the documented fields; one send
per lab case; patient merge carries the rows. The transport is an interface (`IGlidewellClient`) whose shipped implementation is blocked:
it never opens a connection. Sending patient data also waits for a business associate agreement recorded in the Glidewell setup.

**Inferred.** The link's name and texts, the request's formats, the order status numbers (the documents list the eight names in order),
the 127-byte key limit, and everything about the transport. `EnumGlidewellProductName` has no published values, so it has no members and a
stored number is kept as it is.

**Blocked.** Glidewell's interface addresses, sign-in scheme and product list are not public; each office asks Glidewell for its own API
access. Item 3 is excluded: this program has no cloud edition.

Port note: [P262-GLIDEWELL.md](../port-notes/P262-GLIDEWELL.md).

## Appointments Module (items 4 to 7)

- **Items 4 and 6, several blockout types in Advanced Appointment Search.** The Blockout list is multi-select, with None exclusive. For
  each date the search lists the first slot of every selected type that has one that date, with a Blockout column; with providers the slot
  must also be open for one of them in the same operatory. One type gives exactly the 26.1 result. More continues after the last date
  listed. **Inferred:** one row per type per date (not per blockout), 15 dates rather than 15 rows, the row order and the column.
- **Item 5, practice and clinic notes.** The full schema document says practice and holiday notes show in the upper left of the operatory
  bar with a hover effect. Built that way: a "Notes" corner that shows the practice's notes and holidays (and the selected clinic's) in the
  operatory header's tip. **Inferred:** the corner's look, the click, the tip's wording and which notes are included.
- **Item 7, estimates on plan save.** Saving an insurance plan (with Insurance Plan Edit) recomputes the estimates of every covered patient
  with a scheduled appointment from today on, also when the plan is edited from Lists. **Inferred:** whose estimates and when.

No schema change. Port note: [P262-APPT.md](../port-notes/P262-APPT.md).

## Account Module (items 8 to 11)

- **Item 8, Statement Options.** The Statement button's dropdown arrow opens a Statement Options window listing the default statement and
  every former menu entry with a description; the button's click still prints the default statement.
- **Item 9, the Statement window.** Laid out in boxes (Statement, Invoice, Date Range, Contents, notes); no control added or removed, every
  rule unchanged, including the limited custom statement's enabled controls.
- **Item 10, Edit Claim.** The tabs are General, Status History, Misc, Attachments, Medical, Canadian. **Inferred:** the whole layout; the
  release note says only that the two moved. It is the item least likely to match Open Dental.
- **Item 11** is the 26.1.64 backport `v26_1-account-dxc-attach-pdf`, specified in [features-26.1.md](features-26.1.md) and built in V-DXC2.

Port notes: [P262-BILLING.md](../port-notes/P262-BILLING.md) (items 8 and 9), [P262-CLAIMS.md](../port-notes/P262-CLAIMS.md) (item 10),
[V-DXC2.md](../port-notes/V-DXC2.md) (item 11). **Inferred** for items 8 and 9: the windows' layout and descriptions.

## Chart Module (items 12 to 16)

- **Item 12, the extraction graphic.** Setup > Preferences > Chart - General, "Show completed extractions with an X on the tooth chart", on
  by default as in 26.2: a completed extraction draws an X in its status's colour on the 3D and 2D charts. Built with the WPF tooth chart.
- **Item 13, TextBelow (paint type 18).** The procedure code windows offer it; its text is shown in a line under the Chart module's date
  slider, not in the tooth picture, as 26.2 does. The enum value came with P262-SCHEMA; the drawing with TC-5.
- **Item 14, flags on selected teeth.** With teeth selected and no cell selected, Bleeding, Suppuration, Plaque and Calculus (buttons or
  keys) add the flag to all six probing sites of every selected tooth; the same click removes it when every site has it. Skipped teeth get
  nothing.
- **Item 15, multi-cell selection.** A drag selects cells in any editable row (probing, Mobility, Furc, Ging Marg, MGJ) of the row type
  where it started; one number fills them all, Delete clears them.
- **Item 16, past exams.** The exams panel has Show previous exams and Show previous probing values (the public feature page's names),
  stored per user (`UserOdFkeyType` 31 `PerioShowCurrentProbingOnly`, and value 8 with its 26.2 meaning).

**Inferred:** the keys, the remove-when-all rule, the drag rules, and how the two settings combine. Port notes:
[TC-5.md](../port-notes/TC-5.md) (items 12 and 13), [P262-PERIO.md](../port-notes/P262-PERIO.md) (items 14 to 16).

## Manage Module (items 17 to 19)

- **Item 17, Electronic Billing.** The Electronic Billing box of Billing Defaults is its own window, opened from Billing Defaults and from
  the Billing List's Defaults button; same settings and `ebill` rows as before.
- **Item 18, Billing Defaults per clinic.** Two boxes: Statements (practice-wide) and Messages (General Message, e-mail subject, body,
  body type, autograph, invoice note). With clinics on, a clinic picker and "Use the practice defaults for this clinic"; a clinic's own
  value is a `clinicpref` row under the preference's name, and a message without a row follows the practice. Billing Options shares the
  General Message row. **Inferred:** which settings are per clinic, and the copy-on-uncheck rule.
- **Item 19** is the 26.1.63 backport `v26_1-manage-timecard-future-clockout`, built in M2-EMP.

Port notes: [P262-BILLING.md](../port-notes/P262-BILLING.md), [M2-EMP.md](../port-notes/M2-EMP.md).

## Insurance (items 20 to 22)

- **Item 20, claim form per provider.** Edit Provider has a Default Claim Form. A new claim or preauthorization takes the billing
  provider's form, then the treating provider's, then the plan's (owner decision of 2026-11-07); existing claims are not changed; a claim
  form in use as a provider's default cannot be deleted.
- **Item 21, Combine Insurance Plans.** Before combining, a window lists every field of each plan that differs from the kept plan (every
  `insplan` column of the 26.2 document except the key and the security columns), plus one Benefits row when the benefits differ.
- **Item 22, Do Not Bill to Ins trigger.** Automation trigger 11 fires when Do Not Bill to Ins is checked in Edit Procedure. Condition 15,
  Carriers Match, holds a CarrierNum, works only with that trigger, and several of them are ORed (as the documents say), then ANDed with the
  other conditions. Combining carriers moves the conditions to the kept carrier.

**Inferred:** the windows, the provider order before the owner's answer, and when the trigger fires (checked now, not before). Port note:
[P262-CLAIMS.md](../port-notes/P262-CLAIMS.md).

## Tools (items 23 to 30)

- **Item 23**, Database Maintenance check `PaymentIncomeTransferIssues`: finds income transfer payments whose splits do not balance and
  fixes the safe group automatically. **Inferred:** which problems count and which are fixed.
- **Item 24**, Tools tab, Conversion Payment Dates: on demand, moves the income transfers dated after a conversion date to that date.
  **Inferred:** the selection rule and the window.
- **Item 25**, Fee Tools, Copy Code: copies one code's fees to another, per fee schedule, effective dates included.
- **Item 26**, Test Latency: an Average latency group (round trips, fastest, slowest).
- **Item 30**, check `InsVerifyRedundantEntries`: keeps one `insverify` row per plan and moves the rest to `insverifyhist`.
- **Items 27 to 29** are excluded (below).

No schema change. Port note: [P262-TOOLS.md](../port-notes/P262-TOOLS.md).

## Reports (items 31 to 34)

- **Items 31 and 32**, Report Setup: Display Settings and Security Permissions show one list of every report, with a shaded row per
  category and an Internal Name column, instead of one small grid per category. Every 26.1 rule is kept; two 26.1 bugs were fixed on the way
  (Up and Down with gaps in the order; "every report allowed" decided by count).
- **Item 33**, Patient Portion Uncollected, "Show prior uncollected balances": also lists, for the patients of the date range, their earlier
  procedures with an uncollected patient portion, shaded and totalled apart.
- **Item 34**, a Pay Plan column: the amount of the procedure on a patient or dynamic payment plan.
- The adjustment subquery inherited from 26.1 counted an adjustment once per payment split; fixed (owner, 2026-11-07).

**Inferred:** the layouts, which earlier procedures count, and the Pay Plan amount of a dynamic plan link without an override. Port note:
[P262-REPORTS.md](../port-notes/P262-REPORTS.md).

## eServices and Miscellaneous (items 35 to 43)

- **Item 37, vital signs from eForms.** Five eForm text field links (height, weight, systolic, diastolic, pulse), read with their units and
  checked against plausible ranges, and a Vital Signs section in the eForm import window that writes a `vitalsign` row with the documents'
  LOINC exam codes.
- **Item 39, Commlog Referral Types.** Definition category 57; the referral communication window has a Type list, and Edit Referral's
  commlog grid a Type column.
- **Item 41, automatic Prophy and Perio switch.** A setting in Recall Types (off by default; the fork's own preference row): when a
  completed procedure triggers only the other type, the patient's Prophy or Perio recall switches type and interval on the next recall sync,
  as Set Perio and Set Prophy do.
- **Items 35, 36, 38, 40, 42 and 43** are excluded (below).

**Inferred:** the field names, units and ranges, the referral type defaults, and every switch rule. Port note:
[P262-MISC.md](../port-notes/P262-MISC.md).

## Program Links (items 44 to 51)

- **Item 44, Allied Star Connect.** A Chart module link (off by default) that writes a patient info file (JSON, the patient's id, names,
  birthdate, gender and first e-mail address) and starts Connect; the file is removed when the program closes. **Inferred:** the file's
  layout, which Allied Star does not publish.
- **Item 45, OrthoComposite.** An imaging link (off by default) with OrthoComposite's public launch arguments (last name, first name,
  birthdate) as its default.
- **Item 46, Trojan Express Collect.** Retired as the DXC Patient Credit Score link was: listed as "(discontinued)", refused at launch, its
  windows removed; a migration step disables the row and deletes its toolbar button. The Trojan benefit link stays.
- **Item 47, Suni.** Removed; its capture had already gone in v0.0. Its columns stay.
- **Item 48, Ez3D-i.** Not ported: the link stays (owner decision).
- **Items 49 to 51** are excluded (below).

Port note: [P262-LINKS.md](../port-notes/P262-LINKS.md), with the vendor follow-up from public documentation (OrthoComposite's arguments,
Medit Link's Partner ID).

## Excluded

29 items. The schema foundation still creates every column, table and enum value they bring, so an Open Dental 26.2 database loads.

| Reason | Items |
|---|---|
| Open Dental Cloud (this program has no cloud edition) | 3, 49, 50, 51 |
| Open Dental hosted services and HQ (nothing phones home) | 35 eReminder eClipboard tag, 36 Web Sched, 38 HQ global alerts, 42 eConnector alert, 43 the 2026 RxNorm import, which Open Dental delivers from its servers |
| Intra-office chat (owner decision of 2026-10-18) | 27, 40 |
| MariaDB backup tools: server-side, left out on 2026-11-07 | 28, 29 |
| Local REST API (owner decision of 2026-10-18) | 52 to 67 |

**Kept only for data compatibility:** `apisubscription.DateTimeLastFailure`, `FailureReason`, `SubsequentFailures`, `DateTimeNextRetry`
(item 53), `insbluebook.DateTStamp`, `AlertType` 52 to 54, `DefLinkType` 6, `EnumPermType` 277 (HQ only) and `TsiFKeyType` 6
([P262-SCHEMA.md](../port-notes/P262-SCHEMA.md)).

## Open questions

- Every **Inferred** point can only be checked against a 26.2 install, which the project does not use. The owner accepted them as built on
  2026-11-07 ("all as recommended"); [owner-questions.md](../decisions/owner-questions.md), "Open Dental 26.2 port".
- Changes in 26.2.x point releases after 26.2.0 have no published document and are not covered.
- Glidewell: the transport, its addresses and the product list wait for Glidewell's documentation.
- Allied Star Connect: the info file layout is unpublished.
