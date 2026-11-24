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
| The other 1,024 windows at 100% | the same pixels as `main` (the comparison) | none |

TBD
