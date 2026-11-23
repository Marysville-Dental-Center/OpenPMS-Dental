"""FormRpProcCodes on the grid (UI-12 slice 8, layout script on the planner's tree). The three lists stood 6 px apart, the first 2 px
higher than the others, and the Code / Categories group floated at x 217 under the Clinic list's middle: the lists from the padding to the
right padding 8 px apart on one top, 2 px under their captions; Show blank fees under the first list and the sort group under the Clinic
list, both 4 px under the lists."""
import importlib.util
import os

_spec = importlib.util.spec_from_file_location("_s8", os.path.join(os.path.dirname(os.path.abspath(__file__)), "_s8.py"))
s = importlib.util.module_from_spec(_spec)
_spec.loader.exec_module(s)


def layout(h):
    R = s.W(h) - 8
    w = (R - 8 - 16) // 3
    l1 = h.n("label1")
    top = l1.ny + l1.nh + 2
    x = 8
    for cap, lst in (("label1", "listBoxFeeSched"), ("label2", "listBoxClinics"), ("label3", "listBoxProviders")):
        ww = w if lst != "listBoxProviders" else R - x
        h.set(cap, x, None, ww, None)
        h.set(lst, x, top, ww, None)
        x += w + 8
    lb = h.n("listBoxFeeSched")
    y = top + lb.nh + 4
    h.check_left("checkShowBlankFees")
    h.set("checkShowBlankFees", 8, y)
    h.set("groupBox5", 8 + w + 8, y)
