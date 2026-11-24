"""FormEhrLabImageEdit on the grid (UI-12 slice 11, layout script on the planner's tree). Its stock SplitContainer is now
OpenPmsDental.UI.SplitContainer (the lead's decision of slice 8: the stock one threw in LayoutManagerForms.SetNoAnchorDock). The planner
moved the two splitter panels to 8, 8 inside the container, which places them itself (0, 0 and under the splitter), and grew the window by
16 x 16 a pass for it (784 x 589 -> 807 x 625 and on; slice 7's BasicTemplate had the same). Main's size; the panels where the container puts
them (Panel1 the splitter distance tall, Panel2 under the 4 px splitter); the check box on the padding; the container from the padding to the
right padding, down to the section gap over Save; the grid and the preview 8 px inside their panels."""
import importlib.util
import os

_spec = importlib.util.spec_from_file_location("_s11", os.path.join(os.path.dirname(os.path.abspath(__file__)), "_s11.py"))
s = importlib.util.module_from_spec(_spec)
_spec.loader.exec_module(s)

WW, WH = 784, 589
SPLITTER = 4


def layout(h):
    h.set(".", None, None, WW, WH)
    yb = s.bar(h, right=("butSave",))
    c = h.n("checkWaitingForImages")
    h.set("checkWaitingForImages", 8, 8)
    top = 8 + c.nh + 8
    cw = WW - 16
    ch = yb - s.SECTION - top
    h.set("splitContainer", 8, top, cw, ch)
    p1 = h.n("splitterPanel1").nh
    h.set("splitterPanel1", 0, 0, cw, p1)
    p2 = ch - p1 - SPLITTER
    h.set("splitterPanel2", 0, p1 + SPLITTER, cw, p2)
    h.set("gridMain", 8, 8, cw - 16, p1 - 16)
    h.set("picturePreview", 8, 8, cw - 16, p2 - 16)
