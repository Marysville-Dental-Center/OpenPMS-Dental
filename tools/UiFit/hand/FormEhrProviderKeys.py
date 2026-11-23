"""FormEhrProviderKeys on the grid (UI-12 slice 7, layout script on the planner's tree). The grid stood 41 px from the left under a note at
12, and 35 px over the bottom: the note and the grid on the content edge, the grid 8 px over the bottom (6 px before Add kept)."""
import importlib.util
import os

_spec = importlib.util.spec_from_file_location("_s7", os.path.join(os.path.dirname(os.path.abspath(__file__)), "_s7.py"))
s = importlib.util.module_from_spec(_spec)
_spec.loader.exec_module(s)


def layout(h):
    s.span(h, "label1", x=8)
    s.span(h, "gridMain", x=8, b=s.bottom(h))
