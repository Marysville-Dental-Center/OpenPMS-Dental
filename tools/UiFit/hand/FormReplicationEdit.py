"""FormReplicationEdit on the grid (UI-12 slice 10, layout script on the planner's tree). The notes of the two check boxes stood at 353 and
420: one x, 6 px after the wider check box, to the right padding. The wider window put the notes beside the check boxes: the window 12 px
under the last note for Delete and Save (353 -> 328 high; the code does not size it)."""
import importlib.util
import os

_spec = importlib.util.spec_from_file_location("_s10", os.path.join(os.path.dirname(os.path.abspath(__file__)), "_s10.py"))
s = importlib.util.module_from_spec(_spec)
_spec.loader.exec_module(s)


def layout(h):
    R = s.right(h)
    h.set("label7", 420, None, R - 420, None)
    h.set("label11", 420, None, R - 420, None)
    n = h.n("label11")
    s.size(h, h.n(".").nw, n.ny + n.nh + 12 + h.n("butSave").nh + 8)
    s.bar(h, left=("butDelete",), right_=("butSave",))
