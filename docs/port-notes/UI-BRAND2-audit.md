# UI-BRAND-2 audit: gallery G-UI3-uibrand2

The visual audit and fix round before the owner sees the gallery (the rule of 2026-11-21). Gallery:
`.git/od-tools/test-results/G-UI3-uibrand2` (zip beside it), 4 pictures at 100%, 3 changed, 1 the same. Each window was looked at before
and after; the changed pixels lie only on the changed texts. What changed and why: [UI-BRAND.md](UI-BRAND.md) section 10.

## The pictures

| Picture | Before | After | Verdict |
|---|---|---|---|
| `windows-100/FormQuickBooksSetup.png` (Setup QuickBooks) | "Push the connect button ... (Only need to do this once)" | "Push Connect with QuickBooks running in the background the first time you use QuickBooks with Open PMS - Dental (only once). QuickBooks' administrator then allows it, even where the old program was allowed." 3 lines, right-aligned beside Connect, as before | fine |
| `windows-100/FormPath.png` (Paths) | `C:\OpenDentImages`, `\\SERVER\OpenDentImages` | `C:\OpenPmsImages`, `\\SERVER\OpenPmsImages`; the label wraps as before, nothing cut | fine (closes UI-BRAND audit finding 2) |
| `windows-100/FormMain.png` (ServiceManager) | "begin with "OpenDent..." or are associated with "mysqld.exe"" and the old 9 lines | 8 lines: "begin with "OpenPmsDental..." or "OpenDent..." (Open Dental's, marked so) or use "mysqld.exe"", the rest shorter | fine after finding 1 |
| `windows-100/FormBackup.png` (Backup) | same pixels | same pixels | the same: finding 2 |

## Findings and what was done

1. **ServiceManager's help text ran under the list.** The first wording added a clause to the old first sentence and took 11 lines; the
   label (337 x 155) shows 9 above the list, so "in it's own separate folder." and "(double click to manage)" were hidden under it, and
   `GridWindows_FollowTheRules` reported it ("label1 Line 337x155 needs +0 x +10"). Fixed by rewording (no layout line changed): 8 lines,
   336 x 120 px (the old text 9 lines, 327 x 135), with the rest of the text shorter and its typo ("it's own") gone. The grid check passes.
2. **The Backup window's example is not visible.** `textBox5` (a read-only box, 396 x 27) shows one line, "Restore A-Z images to this
   folder: (example:)"; the example folder on its second line (now `C:\OpenPmsImages\`) is below the box's edge, before and after. Not
   changed by this package (layouts unchanged); listed for the lead.
3. **Create AtoZ Folder (WPF) has no picture yet.** `Brand2Tests.Gallery_UiBrand2_BeforeAndAfter` draws it off screen with
   RenderTargetBitmap, but this session (disconnected) draws nothing for WPF, so the test noted it (`G-UI3-uibrand2-src/gallery-notes.txt`)
   and left the picture out. The change is the text box's default, "OpenPmsImages" in place of "OpenDentImages" (shorter, in the same
   206 px box), checked by `Brand2Tests.AtoZFolder_NewInstallsDefaultToOpenPmsImages`. Run the gallery again at the next connected session
   for its picture.
