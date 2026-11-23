"""FormTxtMsgEdit on the grid (UI-12 slice 6 fix round, layout script on the planner's tree). The planner moved the recipient group to the
padding (8) and kept the fields at main's 25 (main drew the group and the fields at 28), the phone box 14 px and the character count 10 px
short of the right edge: the fields from the padding under the group (Patient, Find, Text Message); the phone box on the right edge with its
caption over its left edge and the patient box 6 px before it; the character count box on the right edge on the Find row (its top and
height), its caption 6 px before it."""
import importlib.util
import os

_spec = importlib.util.spec_from_file_location("_edges", os.path.join(os.path.dirname(os.path.abspath(__file__)), "_edges.py"))
e = importlib.util.module_from_spec(_spec)
_spec.loader.exec_module(e)


def layout(h):
    R = e.right_edge(h)
    ph = h.n("textWirelessPhone")
    xp = R - ph.nw
    h.set("label4", 8, None)
    h.set("textPatient", 8, None, xp - 6 - 8, None)
    h.set("label1", xp, None, ph.nw, None)
    h.set("textWirelessPhone", xp, None)
    f = h.n("butPatFind")
    h.set("butPatFind", 8, None)
    c = h.n("textCharCount")
    xc = R - c.nw
    h.set("textCharCount", xc, f.ny, None, f.nh)
    lc = h.n("labelCharCount")
    h.set("labelCharCount", xc - 6 - lc.nw, f.ny, None, f.nh)
    h.set("label2", 8, None)
    h.set("textMessage", 8, None, R - 8, None)
    e.icon_buttons(h)
