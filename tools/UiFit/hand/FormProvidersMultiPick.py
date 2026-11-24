"""FormProvidersMultiPick on the grid (UI-12 slice 9, layout script on the planner's tree): the Providers and Hygienists buttons stood 21 px
left of OK and the grid at 12, 36 px before them: the buttons at OK's x, the grid from the content edge to 8 px before them."""
import importlib.util
import os

_spec = importlib.util.spec_from_file_location("_s7", os.path.join(os.path.dirname(os.path.abspath(__file__)), "_s7.py"))
s = importlib.util.module_from_spec(_spec)
_spec.loader.exec_module(s)


def layout(h):
    ok = h.n("butOK")
    s.move(h, "butProvDentist", x=ok.nx)
    s.move(h, "butProvHygenist", x=ok.nx)
    s.span(h, "gridMain", x=8, r=ok.nx - 8)
