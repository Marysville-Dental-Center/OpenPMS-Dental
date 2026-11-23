"""FormDiseaseDefMerge on the grid (UI-12 slice 8, layout script on the planner's tree). The explanation and the two choices stood at x
12, 4 px short of the right padding: from the padding to the right padding (the choices' texts are set by the code; two lines each)."""
import importlib.util
import os

_spec = importlib.util.spec_from_file_location("_s8", os.path.join(os.path.dirname(os.path.abspath(__file__)), "_s8.py"))
s = importlib.util.module_from_spec(_spec)
_spec.loader.exec_module(s)


def layout(h):
    R = s.W(h) - 8
    for name in ("labelExplain", "radioKeep1", "radioKeep2"):
        h.set(name, 8, None, R - 8, None)
