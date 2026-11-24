"""FormTaskListEdit on the grid (UI-12 slice 9 fix round, layout script on the planner's tree): the planner centred the Date box (113) on
the two-line note beside it (109..141) and left its label at 109, so the label's text stood 4 px over the box's centre. The box on the
label's row at the row pitch (109, 27 px after Filter at 82), the note's first line on it; Date Type at 145 still clears the note."""
import importlib.util
import os

_spec = importlib.util.spec_from_file_location("_s7", os.path.join(os.path.dirname(os.path.abspath(__file__)), "_s7.py"))
s = importlib.util.module_from_spec(_spec)
_spec.loader.exec_module(s)


def layout(h):
    lab = h.n("labelDate")
    s.move(h, "textDateTL", y=lab.ny)
    h.set("labelDate", None, None, None, h.n("textDateTL").nh)
