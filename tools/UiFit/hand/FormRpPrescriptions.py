"""FormRpPrescriptions on the grid (UI-12 slice 8, layout script on the planner's tree). The Patient / Drug panel ended 4 px short of
the right padding: on it.
UI-12F (the final consistency pass): the label-centre rule in every slice: the input box centred on its two-line label (it stood at
the label's top, 4 px over its centre)."""
import importlib.util
import os

_spec = importlib.util.spec_from_file_location("_s8", os.path.join(os.path.dirname(os.path.abspath(__file__)), "_s8.py"))
s = importlib.util.module_from_spec(_spec)
_spec.loader.exec_module(s)


def layout(h):
    p = h.n("panel1")
    h.set("panel1", s.W(h) - 8 - p.nw, None)
    lab, box = h.n("labelInstruct"), h.n("textBoxInput")
    h.set("textBoxInput", None, lab.ny + (lab.nh - box.nh) // 2, None, None)  # UI-12F: the label-centre rule
