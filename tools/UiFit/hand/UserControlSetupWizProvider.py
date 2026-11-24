"""UserControlSetupWizProvider on the grid (UI-12 slice 10, layout script on the planner's tree). The planner grew the wizard page 530 -> 540
high (the wizard gives every page 930 x 530): 530. The note and the grid stood 20 px from the left, the right column of notes 2 px past the
right padding and Add 4 px over the grid's top: on the padding, the column on the right padding from the grid's top, the grid 8 px before it
and to the bottom padding."""
import importlib.util
import os

_spec = importlib.util.spec_from_file_location("_s10", os.path.join(os.path.dirname(os.path.abspath(__file__)), "_s10.py"))
s = importlib.util.module_from_spec(_spec)
_spec.loader.exec_module(s)


def layout(h):
    s.size(h, 930, 530)
    R = s.right(h)
    x = R - 129
    y = 60
    h.set("butAdd", x, y, None, None)
    y += h.n("butAdd").nh + 4
    h.set("label2", x, y, 129, None)
    y += h.n("label2").nh + 4
    h.set("label1", x, y, 129, None)
    y += h.n("label1").nh + 4
    h.set("butAdvanced", x, y, None, None)
    h.set("pictureAdd", x + h.n("butAdd").nw + 6, 63, None, None)
    h.set("label8", 8, 8, None, None)
    h.set("label3", 8 + h.n("label8").nw + 6, 8, None, None)
    s.fill(h, "gridMain", y=60, r=x - 8)
