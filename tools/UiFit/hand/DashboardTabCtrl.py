"""DashboardTabCtrl on the grid (UI-12 slice 12, layout script on the planner's tree). Print Page stands on the tab strip's row at the top
right (anchored there); at main's 21 px its text's descenders were cut: 23 px tall from the top."""
import importlib.util
import os

_spec = importlib.util.spec_from_file_location("_s12", os.path.join(os.path.dirname(os.path.abspath(__file__)), "_s12.py"))
s = importlib.util.module_from_spec(_spec)
_spec.loader.exec_module(s)


def layout(h):
    h.set("butPrintPage", None, 0, None, 23)
