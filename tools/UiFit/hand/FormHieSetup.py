"""FormHieSetup on the grid (UI-12 slice 7, layout script on the planner's tree). The clinic picker's box started 1 px before the field
column and the export path's pick button ended 28 px short of Save's edge: the picker's box on the field column, the pick button on the
right edge with the path box to 6 px before it."""
import importlib.util
import os

_spec = importlib.util.spec_from_file_location("_s7", os.path.join(os.path.dirname(os.path.abspath(__file__)), "_s7.py"))
s = importlib.util.module_from_spec(_spec)
_spec.loader.exec_module(s)


def layout(h):
    R = s.right(h)
    F = h.n("textExportPath").nx
    s.move(h, "comboClinic", x=F - h.clinic_w("comboClinic"))
    bw = h.n("butBrowse").nw
    s.move(h, "butBrowse", x=R - bw)
    s.span(h, "textExportPath", r=R - bw - 6)
