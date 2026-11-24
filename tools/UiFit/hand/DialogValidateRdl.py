"""The report designer's DialogValidateRdl on the grid (UI-12 slice 15, layout script; designer code in DialogValidateRdl.cs). Main cut the
note's third line. The note over the width as tall as its three lines, the error list under it down to a section gap over the bar,
Validate and Close bottom right; 503 x 275 (main 503 x 261: the note's third line). Fix round: the error list without IntegralHeight (it snapped the list 12 px short of the section gap)."""
import importlib.util
import os

_spec = importlib.util.spec_from_file_location("_s15", os.path.join(os.path.dirname(os.path.abspath(__file__)), "_s15.py"))
s = importlib.util.module_from_spec(_spec)
_spec.loader.exec_module(s)



def layout(h):
    W0 = 503
    h.set(".", None, None, W0, 275)
    top = s.bar(h, right=("bValidate", "bClose"))
    y = s.note(h, "label1", 8, 8, W0 - 16) + 8
    h.set("lbSchemaErrors", 8, y, W0 - 16, top - s.SECTION - y)
    h.integral_height_off("lbSchemaErrors")
