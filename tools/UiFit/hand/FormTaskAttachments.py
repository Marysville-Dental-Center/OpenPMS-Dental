"""FormTaskAttachments on the grid (UI-12 slice 10, layout script on the planner's tree). Add and the grid stood 12 px from the left, the grid
12 px from the right and 13 over the bottom: on the padding."""
import importlib.util
import os

_spec = importlib.util.spec_from_file_location("_s10", os.path.join(os.path.dirname(os.path.abspath(__file__)), "_s10.py"))
s = importlib.util.module_from_spec(_spec)
_spec.loader.exec_module(s)


def layout(h):
    h.set("butAdd", 8, 8, None, None)
    s.fill(h, "gridMain", y=36)
