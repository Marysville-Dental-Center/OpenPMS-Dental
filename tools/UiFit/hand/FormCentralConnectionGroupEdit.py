"""FormCentralConnectionGroupEdit on the grid (UI-12 slice 12, layout script on the planner's tree). The two grids from main's 12 px
margin, ending 49 px short of Cancel, and 65 px over the bar: one width, one top and one bottom, from the padding to the right padding
down to the section gap over the bar, Add and Remove between them centred on them; the description row on the first row with its note."""
import importlib.util
import os

_spec = importlib.util.spec_from_file_location("_s12", os.path.join(os.path.dirname(os.path.abspath(__file__)), "_s12.py"))
s = importlib.util.module_from_spec(_spec)
_spec.loader.exec_module(s)


def layout(h):
    R = s.W(h) - s.PAD
    yb = s.bar(h, left=("butDelete",), right=("butOK", "butCancel"), bh=24)
    fx = s.col_x(h, ["label1"])
    s.row(h, "label1", "textDescription", s.PAD, fx)
    d = h.n("textDescription")
    h.set("label2", d.nx + d.nw + s.LGAP, s.PAD, h.lw("label2"), h.H)
    h.text_align("label2", "MiddleLeft")
    top = s.PAD + h.H + s.GAP
    bw = 81
    gw = (R - s.PAD - bw - 2 * s.GAP) // 2
    bottom = yb - s.SECTION
    h.set("gridMain", s.PAD, top, gw, bottom - top)
    h.set("gridAvail", R - gw, top, gw, bottom - top)
    bx = s.PAD + gw + (R - gw - (s.PAD + gw) - bw) // 2
    mid = (top + bottom) // 2
    h.set("butAdd", bx, mid - 24 - 2, bw, 24)
    h.set("butRemove", bx, mid + 2, bw, 24)
