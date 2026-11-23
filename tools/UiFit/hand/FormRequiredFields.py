"""FormRequiredFields on the grid (UI-12 slice 6, layout script on the planner's tree). The three columns started at four heights (the list at
75, the required grid at 24, the conditions grid at 44) from main's 12 px margin, 21 px apart: the Field Type caption and combo, then the
Available Fields caption and list in the left column; the required grid on the combo's top with the arrows centred on it; the conditions
caption at the top and its grid on the combo's top 8 px after the required grid, to the right edge, Add and Delete under it on its right
edge; the explanation line (the text the code gives it) and the Medicaid check box (shown by the code) under the required grid, the list and
the grid down to the bottom padding."""
import importlib.util
import os

_spec = importlib.util.spec_from_file_location("_edges", os.path.join(os.path.dirname(os.path.abspath(__file__)), "_edges.py"))
e = importlib.util.module_from_spec(_spec)
_spec.loader.exec_module(e)


def layout(h):
    R = e.right_edge(h)
    B = e.bottom_edge(h)
    h.set("labelFieldType", 8, 8, 158, 17)
    h.set("comboFieldTypes", 8, 27)
    h.set("label1", 8, 58, 158, 17)
    h.set("listAvailableFields", 8, 77, 158, B - 77)
    xa = 8 + 158 + 8
    xg = xa + 35 + 8
    gb = B - 20 - 4 - 18 - 4
    h.set("gridMain", xg, 27, 236, gb - 27)
    mid = 27 + (gb - 27) // 2
    h.set("butRight", xa, mid - 28)
    h.set("butLeft", xa, mid + 4)
    h.set("labelExplanation", xg, gb + 4)
    h.set("checkMedicaidLength", xg, gb + 4 + 18 + 4)
    xc = xg + 236 + 8
    h.set("label2", xc, 8, R - xc, 17)
    h.set("gridConditions", xc, 27, R - xc, 240)
    h.set("butDelete", R - 83, 27 + 240 + 4)
    h.set("butAdd", R - 83 - 6 - 75, 27 + 240 + 4)
