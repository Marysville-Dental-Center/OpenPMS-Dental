"""FormWiki on the grid (UI-12 slice 16): the wiki page under the docked toolbar with the grid's padding on every side (it stood
edge to edge), the status line under it (4 px, the row gap) on the bottom padding. The window's code sizes the window from the screen."""
import importlib.util
import os
_spec = importlib.util.spec_from_file_location("_s16", os.path.join(os.path.dirname(os.path.abspath(__file__)), "_s16.py"))
s = importlib.util.module_from_spec(_spec)
_spec.loader.exec_module(s)


def layout(h):
    W, H = h.tree.w, h.tree.h
    st = h.n("labelStatus")
    yStatus = H - s.PAD - st.nh
    h.set("labelStatus", s.PAD, yStatus, W - 2 * s.PAD, st.nh)
    s.surface(h, "webViewWiki", top=h.n("ToolBarMain").nh, bottom=yStatus - 4)
