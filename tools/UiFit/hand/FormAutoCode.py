"""FormAutoCode on the grid (UI-12 slice 8, layout script on the planner's tree). The list stood at x 26 with 31 px empty on the right and
a 49 px empty band under Add and Delete (the window keeps main's 235 x 403): the list from the padding to the right padding, Add on the
left and Delete on the right padding at the bottom, the list down to 8 px over them."""
import importlib.util
import os

_spec = importlib.util.spec_from_file_location("_s8", os.path.join(os.path.dirname(os.path.abspath(__file__)), "_s8.py"))
s = importlib.util.module_from_spec(_spec)
_spec.loader.exec_module(s)



def layout(h):
    yb = s.bar(h, left=("butAdd",), right=("butDelete",))
    h.set("listAutoCodes", 8, 8, s.W(h) - 16, yb - 8 - 8)
