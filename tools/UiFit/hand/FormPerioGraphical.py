"""FormPerioGraphical on the grid (UI-12 slice 10, layout script on the planner's tree). The window says 'Do NOT change the size of this form'
(the tooth chart's picture is printed at its size): the planner grew it 768 x 696 -> 772 x 712; main's size. Its three buttons one column
(Save to Images was 2 px left of the others and 4 px wider), 4 px apart from the padding; the hidden note under them."""
import importlib.util
import os

_spec = importlib.util.spec_from_file_location("_s10", os.path.join(os.path.dirname(os.path.abspath(__file__)), "_s10.py"))
s = importlib.util.module_from_spec(_spec)
_spec.loader.exec_module(s)


def layout(h):
    s.size(h, 768, 696)
    y = s.column(h, ("butSetup", "butSave", "butPrint"), 672, 8, w=88)
    h.set("label1", 672, y + 8, 88, None)
