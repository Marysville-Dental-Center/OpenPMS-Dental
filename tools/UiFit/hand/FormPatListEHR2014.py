"""FormPatListEHR2014 on the grid (UI-12 slice 10, layout script on the planner's tree). The planner grew the window 747 x 530 -> 749 x 543:
main's size; the button column on the right padding, the grid 8 px before it (16) and to the bottom padding; Results (anchored to the
bottom, at 445 on main, moved to 224 by the planner) on the bottom padding."""
import importlib.util
import os

_spec = importlib.util.spec_from_file_location("_s10", os.path.join(os.path.dirname(os.path.abspath(__file__)), "_s10.py"))
s = importlib.util.module_from_spec(_spec)
_spec.loader.exec_module(s)


def layout(h):
    s.size(h, 747, 530)
    R = s.right(h)
    for n in ("butClear", "butBirthdate", "butDisease", "butMedication", "butLabResult", "butGender", "butCommPref", "butAllergy", "butShow"):
        s.move(h, n, x=R - h.n(n).nw)
    s.move(h, "butShow", y=s.bottom(h) - h.n("butShow").nh)
    s.fill(h, "gridMain", y=8, r=R - h.n("butClear").nw - 8)
