"""FormImages on the grid (UI-12 slice 16): under the toolbar (edge to edge, chrome), the file list on the left and the zoom slider
over the image on the right, with the grid's padding and 8 px between them; the image area under the slider (8 px). The window's code puts
the web view on the image area's bounds; the image is drawn as before."""
import importlib.util
import os
_spec = importlib.util.spec_from_file_location("_s16", os.path.join(os.path.dirname(os.path.abspath(__file__)), "_s16.py"))
s = importlib.util.module_from_spec(_spec)
_spec.loader.exec_module(s)


def layout(h):
    W, H = h.tree.w, h.tree.h
    top = h.n("elementHostToolBarMain").nh + s.PAD
    lst = h.n("listFiles")
    h.set("listFiles", s.PAD, top, lst.nw, H - s.PAD - top)
    x = s.PAD + lst.nw + s.GAP
    z = h.n("elementHostZoomSlider")
    h.set("elementHostZoomSlider", x, top, z.nw, z.nh)
    yImage = top + z.nh + s.GAP
    h.set("panelImage", x, yImage, W - s.PAD - x, H - s.PAD - yImage)
