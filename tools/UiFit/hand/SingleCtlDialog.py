"""The report designer's SingleCtlDialog on the grid (UI-12 slice 15, layout script; designer code in SingleCtlDialog.cs). Main's bar
panel was 56 px with the buttons 8 px from its top (25 px of empty band under them) and Apply 21 px from the right edge. The bar panel
43 px: OK, Cancel and Apply 12 px under the hosted page on the bottom and right padding; the page's panel fills the rest; main's
458 x 366."""
import importlib.util
import os

_spec = importlib.util.spec_from_file_location("_s15", os.path.join(os.path.dirname(os.path.abspath(__file__)), "_s15.py"))
s = importlib.util.module_from_spec(_spec)
_spec.loader.exec_module(s)



KEEP_BAR_GAP = True


def layout(h):
    W0, H0 = 458, 366
    h.set(".", None, None, W0, H0)
    ph = s.SECTION + 23 + 8
    h.set("panel1", 0, H0 - ph, W0, ph)
    x = W0 - 8 - 75
    for b in ("bApply", "bCancel", "bOK"):
        h.set(b, x, s.SECTION)
        x -= 75 + 8
