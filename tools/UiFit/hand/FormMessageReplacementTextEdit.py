"""FormMessageReplacementTextEdit on the grid (UI-12 slice 8 fix round, ui/ui12-8-2; layout script on the planner's tree). The editor box
stood at 12..492 and ended 14 px over the bar while Replacements starts at 8 and Save ends at 496: the box from the padding to the right
padding, its top kept and its bottom 8 px over the bar."""
import importlib.util
import os

_spec = importlib.util.spec_from_file_location("_s8", os.path.join(os.path.dirname(os.path.abspath(__file__)), "_s8.py"))
s = importlib.util.module_from_spec(_spec)
_spec.loader.exec_module(s)


def layout(h):
    R = s.W(h) - 8
    yb = s.bar(h, left=("butReplacementText",), right=("butSave",))
    t = h.n("textBoxEditor")
    h.set("textBoxEditor", 8, None, R - 8, yb - 8 - t.ny)
