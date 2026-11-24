"""FormWikiLists on the grid (UI-12 slice 9 fix round, layout script on the planner's tree): the list stood at 12 with 8 px on top and 15
at the bottom. The list from the padding to 8 px over the bottom and 8 px before Add; Add on the right content edge."""
import importlib.util
import os

_spec = importlib.util.spec_from_file_location("_s7", os.path.join(os.path.dirname(os.path.abspath(__file__)), "_s7.py"))
s = importlib.util.module_from_spec(_spec)
_spec.loader.exec_module(s)


def layout(h):
    add = h.n("butAdd")
    xa = s.right(h) - add.nw
    s.move(h, "butAdd", x=xa)
    h.set("listWikiLists", 8, 8, xa - 8 - 8, s.bottom(h) - 8)
