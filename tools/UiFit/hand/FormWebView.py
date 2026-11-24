"""FormWebView on the grid (UI-12 slice 16): the browser under the docked toolbar with the grid's padding on every side (it stood edge to
edge); the page inside is the web view's."""
import importlib.util
import os
_spec = importlib.util.spec_from_file_location("_s16", os.path.join(os.path.dirname(os.path.abspath(__file__)), "_s16.py"))
s = importlib.util.module_from_spec(_spec)
_spec.loader.exec_module(s)


def layout(h):
    s.surface(h, "webViewMain", top=h.n("ToolBarMain").nh)
