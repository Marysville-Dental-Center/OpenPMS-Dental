"""FormEhrVaccines from main's arrangement on the grid (UI-12 slice 7, layout script on the planner's tree). The planner moved Export HL7
(anchored to the bottom and the right) to the top of the button column, left the export row's label touching its yes/no list and grew the
window from 516 to 563 px: main's 516 px window, Add at the top of the button column, Export HL7 and Submit HL7 at its bottom (Submit 8 px
over the bottom, 4 px apart), the export row on Submit's row with its label 6 px before the list, the grid from the content edge to 8 px
before the buttons and down to 8 px over the export row."""
import importlib.util
import os

_spec = importlib.util.spec_from_file_location("_s7", os.path.join(os.path.dirname(os.path.abspath(__file__)), "_s7.py"))
s = importlib.util.module_from_spec(_spec)
_spec.loader.exec_module(s)


def layout(h):
    H = 516
    h.set(".", None, None, None, H)
    bx = h.n("butAddVaccine").nx
    y = H - 8 - 23
    h.set("butSubmitImmunization", bx, y, None, 23)
    h.set("butExport", bx, y - 4 - 23, None, 23)
    lw = h.lw("label45")
    ly = y + (23 - 17) // 2
    h.set("label45", 8, ly, lw, 17)
    h.text_align("label45", "MiddleRight")
    h.set("listVacShareOk", 8 + lw + 6, ly, None, 17)
    s.span(h, "gridVaccine", x=8, y=8, r=bx - 8, b=y - 8)
