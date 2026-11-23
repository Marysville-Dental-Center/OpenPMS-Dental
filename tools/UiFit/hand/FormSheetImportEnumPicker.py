"""FormSheetImportEnumPicker on the grid (UI-12 slice 9, layout script on the planner's tree): the list stood at 12, 98 px before Clear Val,
which stood 2 px under the list's top: the list from the padding to 8 px before the button column, Clear Val on the list's top, OK under it
on the bar."""
import importlib.util
import os

_spec = importlib.util.spec_from_file_location("_s7", os.path.join(os.path.dirname(os.path.abspath(__file__)), "_s7.py"))
s = importlib.util.module_from_spec(_spec)
_spec.loader.exec_module(s)


def layout(h):
    ok = h.n("butOK")
    s.move(h, "labelPrompt", x=8)
    s.span(h, "listResult", x=8, r=ok.nx - 8, b=ok.ny - 12)
    s.move(h, "butClear", x=ok.nx, y=h.n("listResult").ny)
