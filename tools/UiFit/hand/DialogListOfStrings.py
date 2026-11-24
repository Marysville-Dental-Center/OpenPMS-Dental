"""The report designer's DialogListOfStrings on the grid (UI-12 slice 15, layout script; designer code in DialogListOfStrings.cs). Main cut
the caption's second line and left the buttons off the right padding. The caption on one line over the box, the box over the width down
to a section gap over the bar, OK and Cancel bottom right; main's 282 x 224."""
import importlib.util
import os

_spec = importlib.util.spec_from_file_location("_s15", os.path.join(os.path.dirname(os.path.abspath(__file__)), "_s15.py"))
s = importlib.util.module_from_spec(_spec)
_spec.loader.exec_module(s)



def layout(h):
    W0 = 282
    h.set(".", None, None, W0, 224)
    top = s.bar(h, right=("bOK", "bCancel"))
    y = s.caption(h, "label1", 8, 8, W0 - 16)
    h.set("tbStrings", 8, y, W0 - 16, top - s.SECTION - y)
