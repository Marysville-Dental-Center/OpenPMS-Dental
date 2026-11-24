"""The report designer's PropertyDialog on the grid (UI-12 slice 15, layout script; designer code in PropertyDialog.cs). Main docked
the tab control over the window (its frame on the window's top, left and right edges) and its bar panel left Apply 5 px from the right
edge and 9 under the buttons. The tab control on the padding as large as main's (no longer docked: anchored on all four sides; its pages
keep their 450 x 300, which the slice 13 and 15 property pages are laid out for), the window 16 px wider and 11 taller (474 x 377); the
bar panel 43 px: OK, Cancel and Apply 12 px under the tab control on the bottom and right padding, Delete (shown for report items) on the
left padding."""
import importlib.util
import os

_spec = importlib.util.spec_from_file_location("_s15", os.path.join(os.path.dirname(os.path.abspath(__file__)), "_s15.py"))
s = importlib.util.module_from_spec(_spec)
_spec.loader.exec_module(s)



KEEP_BAR_GAP = True


def layout(h):
    W0 = 458 + 16
    ph = s.SECTION + 23 + 8
    H0 = 8 + 326 + ph
    h.set(".", None, None, W0, H0)
    h.set("tcProps", 8, 8, 458, 326)
    h.set("panel1", 0, H0 - ph, W0, ph)
    x = W0 - 8 - 75
    for b in ("bApply", "bCancel", "bOK"):
        h.set(b, x, s.SECTION)
        x -= 75 + 8
    h.set("bDelete", 8, s.SECTION)
