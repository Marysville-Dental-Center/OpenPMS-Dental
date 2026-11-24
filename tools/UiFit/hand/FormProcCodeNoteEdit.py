"""FormProcCodeNoteEdit on the grid (UI-12 slice 10, layout script on the planner's tree). The planner grew the window 646 -> 685 high (the
bottom bar under the Minutes row). Main's 646: the time bar and the provider list from one top under the two-line Time Pattern caption,
Provider and Note 2 px over their boxes (their captions), the minutes box and its label on the bottom bar's row, the note box to the right
padding and down to 8 px over the bar. The time bar's slider is placed by the code from the bar (TableTimeBar.GetSliderLocation): it moves
with it."""
import importlib.util
import os

_spec = importlib.util.spec_from_file_location("_s10", os.path.join(os.path.dirname(os.path.abspath(__file__)), "_s10.py"))
s = importlib.util.module_from_spec(_spec)
_spec.loader.exec_module(s)


def layout(h):
    s.size(h, 646, 646)
    h.set("label6", 8, 8, None, None)
    tb = h.n("tbTime")
    dy = 44 - tb.ny
    h.set("tbTime", None, 44, None, None)
    h.set("butSlider", None, h.n("butSlider").ny + dy, None, None)
    h.set("label15", 91, 24, None, 18)
    h.set("listProv", 91, 44, None, None)
    lp = h.n("listProv")
    h.set("label10", 91, lp.ny + lp.nh + 8, None, 18)
    y = s.bar(h, left=("butDelete",), right_=("butSave",))
    h.set("butDelete", 207, None, None, None)
    h.set("textNote", 91, h.n("label10").ny + 20, None, None)
    s.span(h, "textNote", r=s.right(h), b=y - 8)
    h.set("textTime2", 8, y + (h.n("butSave").nh - h.n("textTime2").nh) // 2, None, None)
    s.vcenter(h, "label11", "textTime2")
