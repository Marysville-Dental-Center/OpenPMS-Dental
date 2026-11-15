# Port note: X12-SENDER, the practice is the sender of an X12 batch

Branch `claims/x12-sender-1`, cut from `main` (7f41666bc). Owner decision of 2026-11-15 (`docs/owner-questions.md`, "The 837 claim sender
when a clearinghouse has no Sender TIN"): **"send your practice's own TIN"**.

## 1. What upstream did, and every site

When a clearinghouse's Sender TIN is blank (Edit Clearinghouse, the radio upstream labels 'This software is the "sender"'), upstream names
Open Dental Inc as the sender of every X12 batch it writes: its TIN in ISA06 and GS02, its name, TIN and telephone in loop 1000A (the
submitter). That names a company the office has no agreement with, and since the fork is not Open Dental, the batch claims to come from a
software vendor that never sent it.

The sites on `main` (7f41666bc), found by grepping the whole shipped code for `810624427`, `204944520`, `8776861248`, `8662390469`,
`OPEN DENTAL SOFTWARE`, `Open Dental Software Inc.`, `JORDAN SPARKS`, `GetISA06`, `GetGS02` and the `SenderTIN==""` / `SenderTIN!=""` tests:

| File:line on main | What it wrote with a blank Sender TIN | Now |
|---|---|---|
| `X12/X12Generator.cs:8-25` `GetISA06`, `GetGS02` | `810624427` (Open Dental Inc's TIN) | `GetISA06(X12Sender)`, `GetGS02(X12Sender)`: the sender's TIN |
| `X12/X837_5010.cs:55` ISA06, `:96` GS02 | through `GetISA06`/`GetGS02` | the batch's `X12Sender` |
| `X12/X837_5010.cs:2287-2309` `Write1000A_NM1` | `OPEN DENTAL SOFTWARE`, `810624427`; Emdeon Medical: `Open Dental Software Inc.`, `204944520` | the sender's name and TIN |
| `X12/X837_5010.cs:2312-2336` `Write1000A_PER` | `OPEN DENTAL SOFTWARE`, `8776861248`; Emdeon Medical: `Open Dental Software Inc.`, `8662390469` | the sender's name and telephone (PER02 still only for ClaimConnect and Emdeon Medical) |
| `X12/X837_4010.cs:27` ISA06, `:62` GS02 (in a commented-out medical block), `:73` GS02 | through `GetISA06`/`GetGS02` | the batch's `X12Sender` |
| `X12/X837_4010.cs:1366-1381` `Write1000A_NM1` | `OPEN DENTAL SOFTWARE`, `810624427` | the sender's name and TIN |
| `X12/X837_4010.cs:1384-1397` `Write1000A_PER` | `JORDAN SPARKS`, `8776861248` | the sender's name and telephone |
| `X12/X270.cs:36` ISA06, `:51` and `:56` GS02 | through `GetISA06`/`GetGS02` (Emdeon Dental's ISA06 is its vendor number and login, unchanged) | the request's `X12Sender` |
| `X12/X12Validate.cs:17` `ISA` | skipped every sender check ("we'll be using OD's info") | lists what the practice sender is missing |
| `TableTypes/Clearinghouse.cs:26,53,56` and `DocumentationBuilder/OpenDentalDocumentation.xml` (the same three summaries) | "810624427 is used", "OPEN DENTAL SOFTWARE is used", "8776861248 is used" | the practice's TIN, title and telephone |
| `OpenPmsDental/Forms/FormClearinghouseEdit.designer.cs:471` | 'This software is the "sender"' | 'The practice is the "sender"' (same radio, same stored value: Sender TIN blank) |
| `OpenPmsDental/Forms/FormClearinghouseEdit.cs:491` | "When using Emdeon, this software must be the sender." | "When using Emdeon, the practice must be the sender. Emdeon may need the practice registered as its submitter first." |

Not senders, left as they are: the samples of received files in comments (`X12/X277.cs`, `X12/X835.cs`, `X12/X997.cs`, where `810624427`
is the receiver, ISA08 of an incoming file), the 271 responses of `UnitTests/IntegrationTests/InsVerifiesIntegrationTests.cs`,
`UnitTests/Resources/x271Test.txt` and its `Resources.Designer.cs` entry, and `tests/AttributionTests/ReleaseIdentityTests.cs` (it lists
`OPEN DENTAL SOFTWARE` among the names the release must not carry). There are no other X12 writers: the only `ISA*` writers in the shipped
code are the 837 5010, the 837 4010 and the 270 (`ClaimConnect.cs:316` is a sample response).

## 2. The rule and its source

One class, `OpenPmsDentalBusiness/X12/X12Sender.cs`, decides the sender for every writer, for HQ and clinic-level clearinghouses alike
(the writers get the clinic-level clearinghouse from `Clearinghouses.OverrideFields`, so a clinic's own Sender TIN counts as before):

- **Sender TIN set:** the clearinghouse's Sender TIN, Name and Telephone, exactly as before (validated by `X12Validate.ISA` as before).
- **Sender TIN blank: the practice.**
  - **TIN:** the SSN/TIN field of the practice's billing provider, which is the source the claims themselves use: every claim names its
    billing provider in 2010AA NM1 and sends that provider's SSN/TIN in 2010AA REF\*EI, and the claim's billing provider comes from
    `Providers.GetBillingProvNum`: the clinic's Default Insurance Billing Provider (Lists > Clinics) with clinics on, otherwise the
    practice's (Setup > Practice). The same setting picks the sender:
    - a specific provider: that provider;
    - Default Practice Provider (0): the practice's Default Provider, as `GetBillingProvNum` reads it (for a clinic too);
    - Treating Provider (-1): names no single provider, so the clinic's Default Provider, or the practice's Default Provider when the
      clinic has none or clinics are off.

    The provider must be marked TIN, not SSN (a social security number is never sent as an interchange sender ID), and the number must be 9
    digits with no dashes, as the claims' own validator (`X12Validate.BillProv`) wants it. Why not a new setting: the practice's TIN already
    lives on that provider and is already sent on every claim; a second copy could drift from it. Why not each claim's own billing
    provider: ISA06, GS02 and 1000A are one per batch, and a batch can hold claims of several billing providers (for example Treating
    Provider billing), so the batch needs one practice-level answer.
  - **Name:** the practice title (Setup > Practice, "Provider Name or Practice Title"). The claims send it as the billing contact's name
    (2010AA PER02) for every clinic, in 5010 and 4010; a clinic's description is a location label, not the name the practice is known by.
  - **Telephone:** the one the claims send in 2010AA PER04 (5010): the practice's billing phone when "Use on Claims" of the billing address
    is checked, otherwise the clinic's phone with clinics on, otherwise the practice's phone. It must be 10 digits, as for a clearinghouse
    sender (1000A PER04).
- **The clinic** of a batch is its claims' clinic (`ClaimSendQueueItem.ClinicNum`; every batch has one clinic), of a 270 the patient's
  clinic, of a claim's validation the claim's clinic. With clinics off the clinic is ignored, as `GetBillingProvNum` and the claims do.
- **The Emdeon Medical variant** goes: Emdeon Medical gets the practice like every other clearinghouse (PER02 is still written for it).
- Open Dental Inc's TIN, name or telephone is never written: the constants are gone from the code (a source scan test keeps it so).

`X12SenderSource` carries the values the rule reads; `FromCache` fills it from the preference, clinic and provider caches (no database
call, so it works the same on a Middle Tier client), and the tests fill it directly.

## 3. Refused before anything is written or sent

When the Sender TIN is blank and the practice's TIN, name or telephone is missing, the batch is refused:

- `Eclaims.SendBatch` checks first, for the three X12 formats, before the batch number is taken; `x837Controller.SendBatch` checks again
  before the export folder or file is made (`x837Controller.CanMakeSender`, which shows the message unless the send is automatic);
  `x837Controller.GenerateBatch` (also used by ClaimConnect's validation) and the writers throw an `ODException` with the message, so no
  caller can write a batch without a sender. `X270.GenerateMessageText` gets its sender before it takes the batch number.
- The message (`X12Sender.GetRefusalMessage`), for example:

  ```
  The batch was not created and nothing was sent.
  The clearinghouse <description> has no Sender TIN, so the practice is the sender of the batch. Missing:
  - the 9 digit TIN of the practice's billing provider <Abbr>, marked TIN, not SSN (Lists > Providers > <Abbr> > SSN or TIN)
  - the practice title (Setup > Practice > Provider Name or Practice Title)
  - a 10 digit telephone number (Setup > Practice > Phone)
  Fill in what is missing, or enter the Sender TIN, name and telephone your clearinghouse gave you in Setup > Family/Insurance > Clearinghouses.
  ```

  Other entries: "the practice's billing provider, whose TIN is the sender TIN (Setup > Practice > Default Insurance Billing Provider and
  Default Provider)" or, with a clinic, "(Lists > Clinics > <clinic> > Default Insurance Billing Provider and Default Provider)"; the phone
  entry names "Setup > Practice > Billing Address > Phone" or "Lists > Clinics > <clinic> > Phone" when that is the phone the claims send.
  The provider's number itself is never put in a message.
- Before the send, the same entries show in each claim's Missing Info (Send Claims, Edit Claim) as "Practice as sender: ..." through
  `X12Validate.ISA`, which the 837 5010 and 4010 validators and the 270's validator call.

## 4. Tests and fixtures

Targeted runs (2026-11-15, all passed): GoldenOutputTests 27, BCLAIMTests 79, CLAIMDCMTests 41, VEDSTests 83, VendorDirectTests 937,
P262ClaimsTests 42, NoHqTests 84, MiddleTierMockTests 151.

- **New, `tests/BCLAIMTests/X12SenderTests.cs`** (no database): the TIN from the chosen source (Default Practice Provider, a specific
  provider, Treating Provider; with clinics the clinic's setting, its Default Provider, the practice's fallback; clinics off ignore the
  clinic), the telephone (practice, billing, clinic), the name, missing provider / SSN / dashes / blank TIN, title and telephone refuse with
  what and where, all three at once, the refusal message, Sender TIN set keeps the clearinghouse's values, and two source scans: no X12
  writer (or `Clearinghouse.cs`) carries Open Dental Inc's values, and every writer gets its sender from `X12Sender.GetSender`.
- **New, `tests/GoldenOutputTests/X12SenderGoldenTests.cs`** (the fixture database, `gold_*`, synthetic): with a blank Sender TIN the 5010
  and 4010 batches equal the recorded fixtures with exactly four lines changed (ISA06, GS02, 1000A NM1, 1000A PER) to the practice's TIN
  123456789 (the fixture's Default Provider), title GOLDEN SYNTHETIC DENTAL and phone 5035550100; the Emdeon Medical variant (ISA08
  133052274) names the practice in NM1 and PER (with PER02); the 270's ISA06 and GS02 carry the practice's TIN; a blank practice title, or a
  short phone and no Default Provider, refuse the 5010 and 4010 batches (and `CanMakeSender`) and show in the claim's Missing Info; the
  preferences are put back afterwards. None of the outputs contains Open Dental Inc's values.
- **Fixtures not re-recorded.** The golden fixtures `x837-5010-dental.txt` and `x837-4010-dental.txt` were recorded with a clearinghouse
  whose Sender TIN is set (991234567, "Golden Synthetic Billing", 5035550199), so they pin the unchanged "Sender TIN set" path and stay
  byte for byte. The practice-sender batches are checked against them with only the four sender lines replaced (the diff, per batch):

  ```
  ISA ... *ZZ*991234567      *  ->  *ZZ*123456789      *
  GS*HC*991234567*            ->  GS*HC*123456789*
  NM1*41*2*GOLDEN SYNTHETIC BILLING*****46*991234567~  ->  NM1*41*2*GOLDEN SYNTHETIC DENTAL*****46*123456789~
  5010: PER*IC**TE*5035550199~                          ->  PER*IC**TE*5035550100~
  4010: PER*IC*GOLDEN SYNTHETIC BILLING*TE*5035550199~  ->  PER*IC*GOLDEN SYNTHETIC DENTAL*TE*5035550100~
  ```
- **Existing tests that were thought to pin the old sender do not.** `BCLAIMTests ClaimNumberTextTests` scans the 837 sources for amount
  formatting, `VEDSTests EdsLocalChecksTests` tests EDS's own checks on a snapshot, `VendorDirectTests ClaimAttachedFlagsTests` scans the
  837 sources for the attachment helpers: none reads ISA06, GS02 or 1000A, and none needed a change. The database tests that validate
  claims against a clearinghouse with a blank Sender TIN (VEDSTests `EdsDbTests`) can now also get "Practice as sender: ..." entries in
  Missing Info when their scratch practice has no title, phone or TIN provider; they assert on the EDS part, which still comes last, and
  pass unchanged.

## 5. What the office must check with its clearinghouse

- A clearinghouse that registered the office under Open Dental's submitter ID (ETIN 810624427, or 204944520 for Emdeon Medical) will now
  receive the practice's TIN as the interchange sender (ISA06), application sender (GS02) and submitter (1000A NM109). Ask the
  clearinghouse whether the practice must be registered as its own submitter with that TIN, or whether it issued a submitter ID of its
  own; in the second case enter it as the Sender TIN, with the name and telephone it has on file (Edit Clearinghouse, 'The information
  below identifies the sender').
- ISA05 (the sender ID qualifier) stays as set: ZZ (mutually defined) is usual; some clearinghouses want 30 (federal TIN) for a TIN.
- Emdeon (ISA08 0135WCH00) still requires 'The practice is the "sender"'; Emdeon may need the practice registered as its submitter first.
- Send one test batch (ISA15 T) after the change and check the clearinghouse's acknowledgement (997/999) before sending production claims.

## Lead fixes and notes at the merge (from the reviews)

- **ClaimConnect.ValidateClaim** checks the sender before it takes a batch number, so a refused validation no longer uses one up.
- **Emdeon in FormClearinghouseEdit.** The refusal of a Sender TIN for Emdeon (ISA08 `0135WCH00`) is gone. The practice is always the
  sender now: through its billing provider (blank Sender TIN) or through a submitter ID Emdeon gives it, entered under "The information
  below identifies the sender". The row for `FormClearinghouseEdit.cs:491` in section 1 is superseded by this.
- **Golden test.** `PhoneAndTinMissing_BatchRefused_ListsBoth` restores the practice phone it read before the test, not a fixed value.
- **No x12s_ database.** The rule tests need no database. The batch tests use GoldenOutputTests' own synthetic per-run `gold_` database on
  127.0.0.1, which `GoldenDb` drops at the end, so no x12s_ database was made.
- **Owner questions, in docs/owner-questions.md:**
  - a billing provider marked SSN (not TIN) is refused as the sender;
  - clinics that are separate businesses are named with the practice title;
  - the Emdeon Dental 270's ISA06 `316:`+login (`X270.cs:33`), where 316 may be Open Dental Inc's vendor number at Emdeon.
  - Answered on 2026-11-15: the SSN refusal and the practice title stay; the Emdeon Dental 270's ISA06 is the login alone, padded to 15
    ("Login only"; lead change on `main`, test `X12SenderGoldenTests.X270_EmdeonDental_Isa06IsTheLoginOnly`).
- **Later cleanups:**
  - The 270 is refused for a missing title or phone, although it writes only the TIN. That follows the one rule for every writer; relax it
    if the owner prefers.
  - Name the Treating Provider case in the refusal text.
  - Build the refusal from whole translatable sentences.
  - Add a `gold_` test with clinics turned on.
