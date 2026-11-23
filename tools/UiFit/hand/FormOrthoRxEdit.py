"""FormOrthoRxEdit on the grid (UI-12 slice 8, layout script on the planner's tree). The planner took the Comments label (the code gives
it the tooth example, "Example: 6,27,28") for the caption of the Upper, Lower and All buttons, and kept Delete at x 25: the example on the
field column under Teeth, left-aligned, as wide as its widest text; Upper, Lower and All on its row ending on the Teeth box's right edge,
6 px apart; Delete on the padding."""
import importlib.util
import os

_spec = importlib.util.spec_from_file_location("_s8", os.path.join(os.path.dirname(os.path.abspath(__file__)), "_s8.py"))
s = importlib.util.module_from_spec(_spec)
_spec.loader.exec_module(s)


def layout(h):
    tt = h.n("textToothRange")
    y = tt.ny + tt.nh + 4
    h.text_align("labelComments", "MiddleLeft")
    h.set("labelComments", tt.nx, y, h.lw("labelComments"), 23)
    x = tt.nx + tt.nw
    for name in ("butAll", "butLower", "butUpper"):
        n = h.n(name)
        x -= n.nw
        h.set(name, x, y)
        x -= 6
    s.bar(h, left=("butDelete",), right=("butSave",))
