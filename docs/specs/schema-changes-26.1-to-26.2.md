# Schema changes: Open Dental 26.1.70 to 26.2.0

Status: written 2026-11-26 from the built step; the design the schema package P262-SCHEMA followed. Baseline: the 26.1.70 schema of
[schema-changes-24.3-to-26.1.md](schema-changes-24.3-to-26.1.md). Target: Open Dental 26.2.0, the schema `main` tracks.

Machine-readable twin: the `"range": "26.1-26.2"` entries of [`schema-changes.json`](schema-changes.json). The features that use these
changes are in [features-26.2.md](features-26.2.md).

## Summary

| Range | New tables | New columns | Type changes | Renames | Drops | Enum values | JSON entries |
|---|---|---|---|---|---|---|---|
| 26.1 → 26.2 | 3 | 7 | 0 | 0 | 0 | 23 in 13 enumerations | 10 |

- **Where it comes from.** The 26.2 diff document (`OpenDentalDiffDocumentation26-2.xml`, version 26.2.0.0) lists 17 tables: 3 new, 29 new
  columns (22 of them in the new tables), 11 columns listed for an enum change only, and no deletion.
- **Verified against the full 26.2 document** (`OpenDentalDocumentation26-2.xml`): 430 tables, and every table has the same column names in
  the same order with the same types as the reference, after the documentation tool's own type normalisation. The full document differs
  from the 26.1.70 reference in exactly the diff document's tables and columns.
- **Additive only.** Nothing is dropped, renamed or widened, so the step is a plain `ALTER TABLE ... ADD` and `CREATE TABLE`.
- **DDL confidence.** Only names, order and types are published. NOT NULL, defaults, keys and the storage engine follow the conventions of
  the 25.3.48 schema and are marked `inferred` (below and in the reference file). Owner decision of 2026-11-26: the published documents are
  the 26.2 schema; no dump of a real 26.2 database is needed.

## Sources

- <https://www.opendental.com/OpenDentalDiffDocumentation26-2.xml>, <https://www.opendental.com/OpenDentalDocumentation26-2.xml>.
- <https://www.opendental.com/site/version26_2.html> for the features (stable on 2026-11-03).
- The documents are not in the repository. `tests/P262SchemaTests` checks the reference, the migration chain and the enum values against them
  when `P262_OD_DOCS_DIR` names a folder holding both files.

## Columns

| Table.column | Change | MariaDB type / DDL | Purpose | Feature | Scope |
|---|---|---|---|---|---|
| `apisubscription`.`DateTimeLastFailure` | add-column | `datetime NOT NULL DEFAULT '0001-01-01 00:00:00'` | When the subscription last failed (MinVal means healthy). | API subscriptions: error log (API, item 53) | cut-feature |
| `apisubscription`.`FailureReason` | add-column | `text NOT NULL` | Why the last attempt failed. | API subscriptions: error log (item 53) | cut-feature |
| `apisubscription`.`SubsequentFailures` | add-column | `int(11) NOT NULL` | Failures in a row. | API subscriptions: error log (item 53) | cut-feature |
| `apisubscription`.`DateTimeNextRetry` | add-column | `datetime NOT NULL DEFAULT '0001-01-01 00:00:00'` | When the next attempt is due. | API subscriptions: error log (item 53) | cut-feature |
| `insbluebook`.`DateTStamp` | add-column | `timestamp NOT NULL DEFAULT current_timestamp() ON UPDATE current_timestamp()` | Last change of the row. *Inferred: the definition of every other `DateTStamp`; adding it fills existing rows with the time of the `ALTER TABLE`.* | No release-note entry; schema-only | core |
| `provider`.`ClaimFormNum` | add-column | `bigint(20) NOT NULL` + key `ClaimFormNum` | FK to claimform. The provider's default claim form; takes priority over the plan's and the practice default. | Default claim form per provider (Insurance, item 20) | core |
| `provider`.`GlidewellAcctNum` | add-column | `bigint(20) NOT NULL` + key `GlidewellAcctNum` | FK to glidewellacct. The provider's default Glidewell account. | Glidewell lab cases (item 1) | vendor-direct |

## New tables (DDL inferred)

```sql
CREATE TABLE `glidewellacct` (
  `GlidewellAcctNum` bigint(20) NOT NULL AUTO_INCREMENT,
  `AccountNumber` varchar(255) NOT NULL,
  `ApiKey` varchar(255) NOT NULL,
  `DateTimeExpires` datetime NOT NULL DEFAULT '0001-01-01 00:00:00',
  PRIMARY KEY (`GlidewellAcctNum`)
) ENGINE=MyISAM DEFAULT CHARSET=utf8mb3 COLLATE=utf8mb3_general_ci;

CREATE TABLE `glidewellmsg` (
  `GlidewellMsgNum` bigint(20) NOT NULL AUTO_INCREMENT,
  `LabCaseNum` bigint(20) NOT NULL,
  `OrderStatus` tinyint(4) NOT NULL,
  `VendorRxId` varchar(255) NOT NULL,
  `PatNum` bigint(20) NOT NULL,
  `GlidewellAcctNum` bigint(20) NOT NULL,
  `RawJsonResponse` text NOT NULL,
  `RawJsonRequest` text NOT NULL,
  `SentITero` tinyint(4) NOT NULL,
  `SentMedit` tinyint(4) NOT NULL,
  PRIMARY KEY (`GlidewellMsgNum`),
  KEY `LabCaseNum` (`LabCaseNum`),
  KEY `PatNum` (`PatNum`),
  KEY `GlidewellAcctNum` (`GlidewellAcctNum`)
) ENGINE=MyISAM DEFAULT CHARSET=utf8mb3 COLLATE=utf8mb3_general_ci;

CREATE TABLE `labcaseproduct` (
  `LabCaseProductNum` bigint(20) NOT NULL AUTO_INCREMENT,
  `LabCaseNum` bigint(20) NOT NULL,
  `PatNum` bigint(20) NOT NULL,
  `GlidewellProduct` tinyint(4) NOT NULL,
  `Material` varchar(255) NOT NULL,
  `Shade` varchar(255) NOT NULL,
  `ToothRange` varchar(255) NOT NULL,
  `Notes` text NOT NULL,
  PRIMARY KEY (`LabCaseProductNum`),
  KEY `LabCaseNum` (`LabCaseNum`),
  KEY `PatNum` (`PatNum`)
) ENGINE=MyISAM DEFAULT CHARSET=utf8mb3 COLLATE=utf8mb3_general_ci;
```

The engine and character set are those of every table of the 26.1.70 reference; a 26.2 database made on a server with another default
engine would differ only in that table option, and the verifier would report it.

## Enum values (no DDL)

| Enum | Value | Member | Use here |
|---|---|---|---|
| `AlertType` | 52, 53, 54 | `MariaDbFullBackupError`, `MariaDbIncrementalBackupError`, `EConnectorCritical` | Reserved: raised by Open Dental's services |
| `EnumAutomationTrigger` | 11 | `DoNotBillInsuranceCheck` | Item 22 |
| `AutoCondField` | 15 | `CarriersMatch` | Item 22 |
| `DefCat` | 57 | `CommLogReferralTypes` | Item 39 |
| `DefLinkType` | 6 | `User` | Reserved: Open Dental HQ only |
| `EnumPermType` | 277 | `AdvancedTextingAccountEdit` | HQ only: hidden, never granted |
| `ToothPaintingType` | 18 | `TextBelow` | Item 13 |
| `TsiFKeyType` | 6 | `PayPlanLink` | Loaded, not written |
| `UserOdFkeyType` | 31 | `PerioShowCurrentProbingOnly` | Item 16 |
| `EnumGlidewellOrderStatus` | 0 to 7 | `Initialized` to `Credited` | Item 1; *numbers inferred from the published order* |
| `EnumGlidewellProductName` | none | none | Item 1; *no values are published* |

None collides with a fork-only value ([FORK-ENUMS.md](../port-notes/FORK-ENUMS.md)).

## The migration step

`OpenPmsDentalBusiness/Misc/ForkMigrations/OdParity26_2.cs`, level 2026110701 ("Open Dental 26.2 schema"), after the 26.1 steps. Idempotent:
it adds only missing columns and keys (one statement per table) and creates missing tables, so a 26.1.70 database ends exactly at the 26.2.0
reference and a 26.2.0 database is not touched. No data migration. Levels 2026110702 to 2026110709 are kept for 26.2 schema corrections.

`SchemaVerifier` checks against `Reference/OdSchema-26.2.0.sql` by default; 26.1.70 and 25.3.48 stay embedded under their own names.

## Gaps

- NOT NULL, defaults, keys and the engine are inferred (accepted as built, owner, 2026-11-07).
- 26.2.x point releases after 26.2.0 have no published document.
- Open Dental 26.3 moves the target when Open Dental marks it stable.

Port notes: [P262-SCHEMA.md](../port-notes/P262-SCHEMA.md), [ADOPT-262.md](../port-notes/ADOPT-262.md) (adopting a 26.2 database).
