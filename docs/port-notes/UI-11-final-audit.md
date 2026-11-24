# UI-11-final audit (gallery G-UI3-ui11final)

The pictures of `.git/od-tools/test-results/G-UI3-ui11final/` were opened before the owner sees them. `<W>` =
`.git/od-tools/test-results/G-UI3-ui11final-src`. Package notes: [UI-11-final.md](UI-11-final.md).

The gallery: every window the harness builds at 100% before (`main` at `e4eeb6760`, built x64) and after (the branch), and the densest
tenth at 150% (after only: the final reference set, section 3 of the notes). Before and after were compared pixel by pixel
(`New-UiGallery.ps1`, and a second count with PIL): only the windows this package changes differ.

## First pass

| Window(s) | Seen | Fix |
|---|---|---|
| FormInsPayFix | the description box framed like every input; its four lines fit inside the frame | none |
| FormLanguageAndRegion | the description box framed, but its third line ("...go to your machine's language and region settings.") was cut: the 3-D border the frame replaces takes 2 px a side, and the box was exactly three lines tall without a border | the box 12 px taller (47 to 59: three lines inside the frame) and the rows under it, the Save button and the window 12 px lower (`layout-snapshot/8.tsv` written again); drawn again |
| The other 1,021 windows at 100% | the same pixels as `main` (the comparison) | none |
| The report viewer's DialogMessages and DataSourcePassword (changed in the fix of section 1, drawn after the first pass) | the program's title bar and frame, the window background, the framed input (the messages box read-only, so in the disabled grey as every read-only input), OK filled Accent and Cancel in the button look; the window 14 px narrower and 11 px shorter because the program's frame replaces Windows' (the client area is the same) | none |
| The 103 windows at 150% (after only), on nine contact sheets of twelve, every one opened | the same look and layout as the earlier galleries' 150% pictures of these windows (UI-12, UI-13, UI-PLAIN); the text of the windows the harness scales by their fonts is as small as there (the owner's desktop check at 150% is still open, plan section 7) | none |
| The sweep's kept findings (`sweep-known.tsv`) | ContrTable's and the time pattern's scroll corner (hidden under the time bar), FormClaimsSend's toolbar (draws its own strip), the popups and covers that paint themselves, the screening tooth: in the pictures as the reasons say | none |

## Second pass (the changed pictures again, after the fix)

FormLanguageAndRegion (`final-100`): the three lines inside the frame, the rows 12 px lower on their 27 px pitch, Save 8 px from the edges;
FormInsPayFix, DialogMessages and DataSourcePassword as in the first pass. Nothing else differs from `main` (1,021 the same).

## Fix round (review)

The program's MsgBoxCopyPaste was in the first passes' kept findings and drawn the same as `main`, in the old look (Windows 7's grey
title bar, 3-D buttons, grey background, unframed text box), because the harness built it without its base constructor. Built by its
constructor now (UI-11-final.md section 1) and drawn again (`final-100`, the gallery and `ui-baseline-final`): the program's title bar
and frame, the framed text box with the thin scroll look, Copy All, Print (with its icon) and OK in the button look, the window
background; 704x672 as `MsgBoxCopyPaste@CodeBase`, which it now matches but for its text. Fix: none.

## Not drawn

The WPF pictures (UI-OVF's imaging toolbar and `wpf-focus`, and any WPF Frm): `ControlPictures.RendersWpf()` is false in this
disconnected session. The command is in UI-11-final.md section 2. The four types the harness cannot build (section 1) have no picture.
