"""FormImagePicker on the grid (UI-12 slice 7, layout script on the planner's tree). The search box ended 23 px short of the grid under it,
the grid stood 12 px from the left, the preview 7 px above the grid's top and 7 px above its bottom, Import 6 px left of OK's column: the
search row from the content edge to the grid's right edge, the grid on the content edge, the preview 8 px after the grid (main's 4) on its top and bottom to 8 px
before Import, Import on OK's column, the size caption on the search row (its text is set in Load)."""
import importlib.util
import os

_spec = importlib.util.spec_from_file_location("_s7", os.path.join(os.path.dirname(os.path.abspath(__file__)), "_s7.py"))
s = importlib.util.module_from_spec(_spec)
_spec.loader.exec_module(s)


def layout(h):
    g = h.n("gridMain")
    R = g.nx + g.nw
    lw = h.lw("label1")
    h.set("label1", 8, None, lw, None)
    s.span(h, "textSearch", x=8 + lw + 6, r=R)
    s.span(h, "gridMain", x=8)
    ok = h.n("butOK")
    s.move(h, "butImport", x=ok.nx)
    s.span(h, "picturePreview", x=g.nx + g.nw + 8, y=g.ny, b=g.ny + g.nh, r=ok.nx - 8)
    s.move(h, "labelImageSize", x=g.nx + g.nw + 8)
    h.set("labelImageSize", None, 8, None, h.H)
    h.text_align("labelImageSize", "MiddleLeft")
