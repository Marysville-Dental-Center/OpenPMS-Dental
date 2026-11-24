"""The report designer's DialogNew on the grid (UI-12 slice 15, layout script; designer code in DialogNew.cs). Main docked the choices
list over the whole window (its frame on the window's edges). The list on the padding (no longer docked: anchored on all four sides) down
to the bar panel; the bar panel 43 px with OK and Cancel 12 px under the list, on the bottom and right padding; main's 448 x 366."""
import importlib.util
import os

_spec = importlib.util.spec_from_file_location("_s15", os.path.join(os.path.dirname(os.path.abspath(__file__)), "_s15.py"))
s = importlib.util.module_from_spec(_spec)
_spec.loader.exec_module(s)



KEEP_BAR_GAP = True


def layout(h):
    W0, H0 = 448, 366
    h.set(".", None, None, W0, H0)
    ph = s.SECTION + 23 + 8
    h.set("panel1", 0, H0 - ph, W0, ph)
    h.set("btnCancel", W0 - 8 - 75, s.SECTION)
    h.set("btnOK", W0 - 8 - 75 - 8 - 75, s.SECTION)
    h.set("listNewChoices", 8, 8, W0 - 16, H0 - ph - 8)
