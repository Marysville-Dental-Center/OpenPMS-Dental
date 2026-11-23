"""FormAccountingAutoPayEdit on the grid (UI-12 slice 8, layout script on the planner's tree). Delete stood at x 28, the account list ended 2 px
short of the pay type combo and Add and Remove 4 px short of it, with a 75 px empty band over the bottom bar (the window keeps main's
size): Delete on the padding, the list on the combo's right edge and grown down so that Add and Remove (8 px apart, on that edge) stand
8 px over the bar."""
import importlib.util
import os

_spec = importlib.util.spec_from_file_location("_s8", os.path.join(os.path.dirname(os.path.abspath(__file__)), "_s8.py"))
s = importlib.util.module_from_spec(_spec)
_spec.loader.exec_module(s)


def layout(h):
    yb = s.bar(h, left=("butDelete",), right=("butOK",))
    cb = h.n("comboPayType")
    R = cb.nx + cb.nw
    ab = h.n("butAdd")
    yab = yb - 8 - ab.nh
    rm = h.n("butRemove")
    h.set("butRemove", R - rm.nw, yab)
    h.set("butAdd", R - rm.nw - 8 - ab.nw, yab)
    la = h.n("listAccounts")
    h.set("listAccounts", None, None, R - la.nx, yab - 4 - la.ny)
