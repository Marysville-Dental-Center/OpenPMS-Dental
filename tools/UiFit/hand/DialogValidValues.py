"""The report designer's DialogValidValues on the grid (UI-12 slice 15, layout script; designer code in DialogValidValues.cs). The values
grid with Delete beside it on the right padding, the grid down to a section gap over the bar; OK and Cancel bottom right (main left 12 px
on the right); main's 392 x 222."""
import importlib.util
import os

_spec = importlib.util.spec_from_file_location("_s15", os.path.join(os.path.dirname(os.path.abspath(__file__)), "_s15.py"))
s = importlib.util.module_from_spec(_spec)
_spec.loader.exec_module(s)



def layout(h):
    W0 = 392
    R = W0 - 8
    h.set(".", None, None, W0, 222)
    top = s.bar(h, right=("bOK", "bCancel"))
    h.set("bDelete", R - 48, 8, 48, h.H)
    h.set("dgParms", 8, 8, R - 48 - 8 - 8, top - s.SECTION - 8)
