"""FormRecordAudio on the grid (UI-12 slice 9 fix round, layout script on the planner's tree): Record, Play and Save stood at 36, 36 and 37,
76, 75 and 75 px wide (a ragged column), 36 px in against OK 8 px from the right edge. The three at the padding, 75 px wide; the timer 6 px
after Record; OK stays on the right padding."""
import importlib.util
import os

_spec = importlib.util.spec_from_file_location("_s7", os.path.join(os.path.dirname(os.path.abspath(__file__)), "_s7.py"))
s = importlib.util.module_from_spec(_spec)
_spec.loader.exec_module(s)


def layout(h):
    for name in ("butStart", "butPlay", "butSave"):
        h.set(name, 8, None, 75, None)
    s.move(h, "labelTimer", x=8 + 75 + 6)
