"""ControlFamilyEcw on the grid (UI-12 slice 9, layout script on the planner's tree): the Fee Schedule label stood 35 px in (its box 80 px
for a 76 px text, right-aligned): the label at the padding, as wide as its text, the box 6 px after it."""
import importlib.util
import os

_spec = importlib.util.spec_from_file_location("_s7", os.path.join(os.path.dirname(os.path.abspath(__file__)), "_s7.py"))
s = importlib.util.module_from_spec(_spec)
_spec.loader.exec_module(s)


def layout(h):
    lw = h.lw("label1")
    h.set("label1", 8, None, lw, None)
    h.text_align("label1", "MiddleRight")
    s.move(h, "comboFeeSched", x=8 + lw + 6)
