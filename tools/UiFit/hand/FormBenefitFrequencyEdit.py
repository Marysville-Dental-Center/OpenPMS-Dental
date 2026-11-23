"""FormBenefitFrequencyEdit on the grid (UI-12 slice 8, layout script on the planner's tree). The note stood at x 12, the Patient Override
check box at 76 (neither the padding nor the field column) and the Treatment Area list ended 12 px above the Code Group list: the note on
the padding, both lists end on one bottom, the check box's box on the Code Group list's column 8 px under the lists."""
import importlib.util
import os

_spec = importlib.util.spec_from_file_location("_s8", os.path.join(os.path.dirname(os.path.abspath(__file__)), "_s8.py"))
s = importlib.util.module_from_spec(_spec)
_spec.loader.exec_module(s)



def layout(h):
    h.set("label2", 8, None)
    cg = h.n("listBoxCodeGroup")
    bot = cg.ny + cg.nh
    ta = h.n("listBoxTreatArea")
    h.set("listBoxTreatArea", None, None, None, bot - ta.ny)
    h.check_left("checkPat")
    h.set("checkPat", cg.nx, bot + 8)
