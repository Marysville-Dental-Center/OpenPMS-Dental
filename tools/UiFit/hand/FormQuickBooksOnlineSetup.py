"""FormQuickBooksOnlineSetup on the grid (UI-12 slice 11, layout script on the planner's tree). The Status row: its caption stood at the
padding (not on the label column) and the status text right-aligned 340 px away under the buttons' row. The caption right-aligned on the
label column; the status text (the code gives it up to a two-line sentence) at the field column, left-aligned on the caption's line, up to
6 px before Connect; Connect and Disconnect stay at the row's right."""
import importlib.util
import os

_spec = importlib.util.spec_from_file_location("_s11", os.path.join(os.path.dirname(os.path.abspath(__file__)), "_s11.py"))
s = importlib.util.module_from_spec(_spec)
_spec.loader.exec_module(s)


def layout(h):
    x = h.n("textClientId").nx
    c = h.n("butConnect")
    lw = h.lw("labelStatus")
    h.set("labelStatus", x - 6 - lw, c.ny, lw, h.H)
    v = h.n("labelStatusValue")
    h.text_align("labelStatusValue", "TopLeft")
    h.set("labelStatusValue", x, c.ny + 3, c.nx - 6 - x, v.nh)
