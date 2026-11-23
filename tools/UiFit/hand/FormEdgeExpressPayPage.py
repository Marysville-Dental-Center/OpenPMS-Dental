"""FormEdgeExpressPayPage on the grid (UI-12 slice 8, layout script on the planner's tree). The browser and its wait note stood at x 12
and the browser ended 18 px over Close: both from the padding to the right padding, the browser down to 8 px over Close."""
import importlib.util
import os

_spec = importlib.util.spec_from_file_location("_s8", os.path.join(os.path.dirname(os.path.abspath(__file__)), "_s8.py"))
s = importlib.util.module_from_spec(_spec)
_spec.loader.exec_module(s)


def layout(h):
    yb = s.bar(h, right=("butFinished", "butClose"))
    R = s.W(h) - 8
    h.set("webViewMain", 8, 8, R - 8, yb - 8 - 8)
    h.set("labelBrowserWait", 8, 8, R - 8, None)
