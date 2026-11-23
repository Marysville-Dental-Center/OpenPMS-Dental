"""FormEhrPatientSelectSimple on the grid (UI-12 slice 6, layout script on the planner's tree). The search fields stood at 120, a third down
the grid, and Search ended 4 px short of the fields: the two field rows and Search at the grid's top on the 27 px pitch, Search on the
fields' right edge."""
import importlib.util
import os

_spec = importlib.util.spec_from_file_location("_edges", os.path.join(os.path.dirname(os.path.abspath(__file__)), "_edges.py"))
e = importlib.util.module_from_spec(_spec)
_spec.loader.exec_module(e)


def layout(h):
    y = h.n("gridMain").ny
    for lab, fld in (("label1", "textLName"), ("label3", "textFName")):
        h.set(fld, None, y)
        h.set(lab, None, y)
        y += 27
    R = e.right_edge(h)
    h.set("butSearch", R - 75, y)
