"""FormReportComplex on the grid (UI-12 slice 16): the preview under the toolbar with the grid's padding on every side (it stood edge
to edge over a 28 px empty band); the window's LayoutPreview does the same. The toolbar stays docked on top."""
import importlib.util
import os
_spec = importlib.util.spec_from_file_location("_s16", os.path.join(os.path.dirname(os.path.abspath(__file__)), "_s16.py"))
s = importlib.util.module_from_spec(_spec)
_spec.loader.exec_module(s)


def layout(h):
    s.surface(h, "printPreviewControl2", top=h.n("ToolBarMain").nh)
