"""FormPrinterSetup on the grid (UI-12 slice 10, layout script on the planner's tree). The planner grew the window 404 -> 407 high and kept the
note and the grid 12 px from the left: main's size, on the padding, the grid 12 px over the bar, the simple-interface check box centred on
Save's row."""
import importlib.util
import os

_spec = importlib.util.spec_from_file_location("_s10", os.path.join(os.path.dirname(os.path.abspath(__file__)), "_s10.py"))
s = importlib.util.module_from_spec(_spec)
_spec.loader.exec_module(s)


def layout(h):
    s.size(h, 941, 404)
    R = s.right(h)
    s.bar(h, right_=("butSave",))
    h.set("checkSimple", 8, None, None, None)
    s.vcenter(h, "checkSimple", "butSave")
    h.set("label1", 8, None, None, None)
    s.fill(h, "gridPrinters", y=28, over="butSave")
