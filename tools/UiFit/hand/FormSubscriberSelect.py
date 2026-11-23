"""FormSubscriberSelect on the grid (UI-12 slice 9, layout script on the planner's tree): the note and the list stood at 32, the list 41 px
before More Patients and 15 px over OK, More Patients 35 px left of OK: More Patients at OK's x and width, the note and the list from the
padding, the list to 8 px before the button and 12 px over OK."""
import importlib.util
import os

_spec = importlib.util.spec_from_file_location("_s7", os.path.join(os.path.dirname(os.path.abspath(__file__)), "_s7.py"))
s = importlib.util.module_from_spec(_spec)
_spec.loader.exec_module(s)


def layout(h):
    ok = h.n("butOK")
    s.move(h, "butMore", x=ok.nx)
    s.span(h, "label1", x=8, r=ok.nx + ok.nw)
    s.span(h, "listPats", x=8, r=ok.nx - 8, b=ok.ny - 12)
