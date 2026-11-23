"""FormSupplyInventory on the grid (UI-12 slice 6, layout script on the planner's tree). The content kept main's 12 px margin under the menu,
Stock Count stood 8 px under Supplies against the 4 px of the rows over it: the four buttons from the padding on one 28 px pitch with their
notes 6 px after them to the right edge (the stock count line, the text the code gives it, two lines centred on its button), the shortage
note and the grid under them, Add and Print 8 px over the bottom with the grid down to 8 px over them."""
import importlib.util
import os

_spec = importlib.util.spec_from_file_location("_edges", os.path.join(os.path.dirname(os.path.abspath(__file__)), "_edges.py"))
e = importlib.util.module_from_spec(_spec)
_spec.loader.exec_module(e)


def layout(h):
    R = e.right_edge(h)
    B = e.bottom_edge(h)
    y = h.n("butEquipment").ny
    nx = 8 + 80 + 6
    for but, lab in (("butEquipment", "label1"), ("butOrders", "label2"), ("butSupplies", "label3")):
        h.set(but, 8, y)
        h.set(lab, nx, y + 3, R - nx, None)
        y += 28
    h.set("butStockCount", 8, y)
    h.set("labelStockCount", nx, y - 4, R - nx, 32)
    y += 24 + 4 + 4
    h.set("label4", 8, y, R - 8, 32)
    y += 32 + 2
    ya = B - 26
    h.set("butAddNeeded", 8, ya)
    h.set("butPrint", 8 + 80 + 6, ya)
    h.set("gridMain", 8, y, R - 8, ya - 8 - y)
