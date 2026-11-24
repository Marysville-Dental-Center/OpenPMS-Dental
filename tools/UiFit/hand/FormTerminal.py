"""FormTerminal on the grid (UI-12 slice 16): the kiosk screen's column centred on the window with the gaps its Load uses (a section
gap under the welcome, the caption gap over the list, a sibling gap under it, the item gap over Done); the connection note centred 8 px
over the bottom, the close panel bottom right 8 px from the edges. Load centres them on the screen at run time."""
import importlib.util
import os
_spec = importlib.util.spec_from_file_location("_s16", os.path.join(os.path.dirname(os.path.abspath(__file__)), "_s16.py"))
s = importlib.util.module_from_spec(_spec)
_spec.loader.exec_module(s)


KEEP_BAR_GAP = True  # no dialog bar: Done belongs to the column


def layout(h):
    W, H = h.tree.w, h.tree.h
    cx = W // 2
    y = h.n("labelWelcome").ny
    for name, gap in (("labelWelcome", None), ("labelForms", 12), ("listForms", 2), ("labelThankYou", 8), ("butDone", 6)):
        n = h.n(name)
        if gap is not None:
            y += gap
        h.set(name, cx - n.nw // 2, y)
        y += n.nh
    c = h.n("labelConnection")
    h.set("labelConnection", cx - c.nw // 2, H - s.PAD - c.nh)
    p = h.n("panelClose")
    h.set("panelClose", W - s.PAD - p.nw, H - s.PAD - p.nh)
