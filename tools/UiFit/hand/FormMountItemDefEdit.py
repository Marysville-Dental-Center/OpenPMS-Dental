"""FormMountItemDefEdit on the grid (UI-12 slice 11, layout script on the planner's tree). The text box stood 8 px left of the field
column (174 against 182) and ended 15 px short of the right padding, the camera guide box 4 px short: both on the field column and the
right padding."""
import importlib.util
import os

_spec = importlib.util.spec_from_file_location("_s11", os.path.join(os.path.dirname(os.path.abspath(__file__)), "_s11.py"))
s = importlib.util.module_from_spec(_spec)
_spec.loader.exec_module(s)


def layout(h):
    R = s.W(h) - 8
    x = h.n("textToothNumbers").nx
    h.set("textTextShowing", x, None, R - x, None)
    g = h.n("textGuideLabel")
    h.set("textGuideLabel", None, None, R - g.nx, None)
