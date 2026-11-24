"""FormPrintTrojan on the grid (UI-12 slice 10, layout script on the planner's tree). The page panel stood at 35 and the scroll bar at 33 (the
code puts the bar at the window's width less 29, at 33: now at the panel's top, UI-12): both 8 px over the bottom. The window keeps the
planner's width (the bar's right edge and the padding; the code sets the width)."""
import importlib.util
import os

_spec = importlib.util.spec_from_file_location("_s10", os.path.join(os.path.dirname(os.path.abspath(__file__)), "_s10.py"))
s = importlib.util.module_from_spec(_spec)
_spec.loader.exec_module(s)


def layout(h):
    B = s.bottom(h)
    p = h.n("printPanel")
    h.set("printPanel", None, None, None, B - p.ny)
    h.set("vScroll", None, p.ny, None, B - p.ny)
