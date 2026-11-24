"""FormQuestionDefEdit on the grid (UI-12 slice 9 fix round, layout script on the planner's tree): Delete stood at 64 against Save 8 px from
the right edge, and the Question box stopped 47 px over the bottom bar. Delete at the padding; the Question box down to 12 px over the bar
(the section gap)."""
import importlib.util
import os

_spec = importlib.util.spec_from_file_location("_s7", os.path.join(os.path.dirname(os.path.abspath(__file__)), "_s7.py"))
s = importlib.util.module_from_spec(_spec)
_spec.loader.exec_module(s)


def layout(h):
    sv = h.n("butSave")
    s.move(h, "buttonDelete", x=8)
    s.span(h, "textQuestion", b=sv.ny - 12)
