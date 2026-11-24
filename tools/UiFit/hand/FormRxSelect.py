"""FormRxSelect on the grid (UI-12 slice 10, layout script on the planner's tree). The instructions (470 px at 8, the text 464) ran under Blank
(at 435; main too): Blank 6 px after the text, the note centred on its row; the grid from the padding to 8 px before the Search group (on
the right padding), 12 px over the bar."""
import importlib.util
import os

_spec = importlib.util.spec_from_file_location("_s10", os.path.join(os.path.dirname(os.path.abspath(__file__)), "_s10.py"))
s = importlib.util.module_from_spec(_spec)
_spec.loader.exec_module(s)


def layout(h):
    R = s.right(h)
    s.move(h, "groupBox1", x=R - h.n("groupBox1").nw, y=8)
    y = s.bar(h, right_=("butOK",))
    lw = h.lw("labelInstructions")
    h.set("labelInstructions", 8, None, lw, 18)
    h.set("butBlank", 8 + lw + 6, y, None, None)
    s.vcenter(h, "labelInstructions", "butBlank")
    s.fill(h, "gridMain", y=8, r=h.n("groupBox1").nx - 8, over="butOK")
