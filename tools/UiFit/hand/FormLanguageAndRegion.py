"""FormLanguageAndRegion on the grid (UI-12 slice 8, layout script on the planner's tree). The description box stood at x 36 and the
"Do not show" check box at 85 (neither the padding nor the field column; its text does not fit after the field column): both on the
padding, the description to the right padding."""
import importlib.util
import os

_spec = importlib.util.spec_from_file_location("_s8", os.path.join(os.path.dirname(os.path.abspath(__file__)), "_s8.py"))
s = importlib.util.module_from_spec(_spec)
_spec.loader.exec_module(s)


def layout(h):
    h.set("textBoxDescript", 8, None, s.W(h) - 16, None)
    h.check_left("checkNoShow")
    h.set("checkNoShow", 8, None)
