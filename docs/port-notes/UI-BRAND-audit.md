# UI-BRAND audit: gallery G-UI3-uibrand

The visual audit and fix round before the owner sees the gallery (the rule of 2026-11-21). Gallery:
`.git/od-tools/test-results/G-UI3-uibrand` (zip beside it), 4 pictures at 100%, 4 changed, 0 the same. Each window was looked at before
and after; the changed pixels lie only on the changed texts.

## The pictures

| Picture | Before | After | Verdict |
|---|---|---|---|
| `windows-100/FormWarnToCloseComputers.png` | "Close Other OD Programs Now!!"; "close all running opendental programs" | "Close Other Open PMS Programs Now!!" (347 of 368 px); "close all running Open PMS programs" | fine |
| `windows-100/FormXChargeReconcile.png` | Missing: "View X-Charge transactions with no payment in OD."; Extra: "View payments in OD with no X-Charge transaction." | "View X-Charge transactions not paid in Open PMS."; "View Open PMS payments not found in X-Charge." | fine after finding 1 |
| `windows-100/FormPath.png` | example folders `OpenDentalExport`, `OpenDentalLetters` | `OpenPmsExport`, `OpenPmsLetters`; both labels wrap as before, nothing cut | fine, finding 2 |
| `windows-100/FormInstallAllocator_Provider.png` | "provide you Opendental program" | "provide you Open PMS program" (the text box wraps and scrolls as before) | fine |

The before pictures of FormPath and FormInstallAllocator_Provider have the same bytes as the frozen reference pictures of
`ui-baseline-final`, so the before mode draws what `main` drew. FormXChargeReconcile's and FormWarnToCloseComputers' differ from the
reference outside the changed texts only by later packages (UI-CAL's calendars and today's date; UI-LOAD's ring).

## Findings and what was done

1. **The X-Charge labels did not fit.** The first wording, "View X-Charge transactions with no payment in Open PMS." and "View payments
   in Open PMS with no X-Charge transaction.", measured 320 px in labels of 296 and 288 px. Fixed by rewording (no layout line changed):
   274 and 271 px; the meaning (X-Charge transactions with no payment in the program; payments in the program with no X-Charge
   transaction) is kept. The "Payments" row above already read "View credit card payments in Open PMS - Dental." (from the product name),
   so the window now names the program one way.
2. **The Paths window's A to Z text still shows `C:\OpenDentImages` and `\\SERVER\OpenDentImages`** (the same window, first tab). That is
   Open Dental's folder name, which the A to Z folder tool still creates by default; left as it is and asked of the owner
   (UI-BRAND.md section 5, question 4).

Nothing is left for the lead: every changed text fits its control at 100%.
