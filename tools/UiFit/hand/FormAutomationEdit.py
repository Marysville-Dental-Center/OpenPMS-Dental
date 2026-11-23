"""FormAutomationEdit on the grid (UI-12 slice 6, layout script on the planner's tree). The two-line Procedure Code(s) label was 29 px tall for
its 32 px of lines and its row 30 px under Trigger, and Delete (a local variable the planner does not read) stayed at main's 48, 393 beside
Save at 405: the field rows on the 27 px pitch with the two-line label centred on its field, the Conditions grid a pitch under it with Add
at the right content edge, the bottom rows 8 px under the grid, Delete on Save's row at the content edge."""
import importlib.util
import os

_spec = importlib.util.spec_from_file_location("_edges", os.path.join(os.path.dirname(os.path.abspath(__file__)), "_edges.py"))
e = importlib.util.module_from_spec(_spec)
_spec.loader.exec_module(e)


def layout(h):
    R = e.right_edge(h)
    F = 162
    y = 8
    for lab, fld in (("label1", "textDescription"), ("label2", "comboTrigger")):
        h.set(fld, F, y)
        h.set(lab, F - 6 - h.lw(lab), y, h.lw(lab), None)
        y += 27
    h.set("textProcCodes", F, y)
    h.set("butProcCode", None, y)
    h.set("labelProcCodes", 8, y + (23 - 32) // 2, 148, 32)
    y += 27
    h.set("butAdd", R - 65, y)
    h.set("gridMain", F, y, R - 65 - 6 - F, None)
    y += h.n("gridMain").nh + 8
    for lab, fld in (("label4", "comboAction"), ("labelActionObject", "comboActionObject"), ("labelMessage", "textMessage")):
        h.set(fld, F, y)
        h.set(lab, F - 6 - h.lw(lab), y, h.lw(lab), None)
        y += 27
    h.set("butDelete", 8, h.n("butSave").ny)
