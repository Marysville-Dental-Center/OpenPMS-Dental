# ADOPT-CLAIMPAYMENT-CHECKSUM: claimpayment's table checksum after the Note type change

2026-11-25. Branch `adopt/claimpayment-checksum-1`. Found on the office's own adoption, at runbook step 6.1.

## The problem

Step 2026102403 (M2-01) turns `claimpayment.Note` from `varchar(255)` into `text NOT NULL` and makes NULL notes empty. `CHECKSUM TABLE`
follows the column type, so the table checksum changes even when no value does. The synthetic data always had NULL notes, so the
comparison listed `ColumnChanged claimpayment.Note`, which explained the table checksum. The office database has none: every column checksum
stayed the same, and `SyntheticData compare` reported `ChecksumTableChanged claimpayment` as the one unexpected difference, which the runbook
treats as a stop. Adopt's row check had already shown that only NULL notes change (none here) and every other note is unchanged.

## The change

- `tools/SyntheticData/allowed-differences.adoption.json`: a rule for `ChecksumTableChanged` on `claimpayment` only, next to the Note rule.
- `ReportComparer`: the comment now says a rule can name such a table.
- `docs/admin/adoption-runbook.md` step 6.1: names the line.
- Test: `ReportComparerTests.ClaimPaymentChecksumOnly_IsAllowed_OtherTablesAreNot` (the rule allows exactly claimpayment; a checksum-only change
  in another table stays unexpected).

## Checked

The office's own reports (trial of 2026-11-25): with the rule, `compare` of the copy and the adopted copy gives `Unexpected differences: 0`.
