"""FormDashboardEditTab on the grid (UI-12 slice 12, layout script on the planner's tree). The planner moved the menu and the dashboard
8 px in and grew the window 16 x 19: the dashboard stays edge to edge under its menu as on main (1061 x 520, the menu at the corner, the
split container under it to the edges; the dashboard tab control, its own designer, fills the second panel). In the first panel: the help
text (set by the code) from the panel's padding down to the Graph Types caption, the caption 2 px over the list, the list and Save Changes
across the panel's content, Save Changes on its bottom padding."""
import importlib.util
import os

_spec = importlib.util.spec_from_file_location("_s12", os.path.join(os.path.dirname(os.path.abspath(__file__)), "_s12.py"))
s = importlib.util.module_from_spec(_spec)
_spec.loader.exec_module(s)


def layout(h):
    s.size(h, 1061, 520)
    h.set("menuStrip", 0, 0)
    h.set("splitContainer1", 0, 27, 1061, 520 - 27)
    pw = 140
    ph = 520 - 27
    cw = pw - 2 * s.PAD
    sb = ph - s.PAD - 24
    h.set("butSaveChanges", s.PAD, sb, cw, 24)
    lt = sb - s.GAP - 121
    h.set("listItems", s.PAD, lt, cw, 121)
    h.set("label2", s.PAD, lt - s.CAPGAP - 17, cw, 17)
    h.text_align("label2", "BottomLeft")
    h.set("labelHelp", s.PAD, s.PAD, cw, lt - s.CAPGAP - 17 - s.GAP - s.PAD)
