"""The report designer's StyleBorderCtl on the grid (UI-12 slice 13, layout script on the planner's tree; its designer code is in
StyleBorderCtl.cs, read and edited in InitializeComponent's body). The planner laid out the five rows (Default, Left, Right, Top, Bottom) on
one grid but left the Color and Width captions over the fx buttons before their columns (28 px left of the boxes). Each caption starts
where its column's boxes start."""
import importlib.util
import os

_spec = importlib.util.spec_from_file_location("_s13", os.path.join(os.path.dirname(os.path.abspath(__file__)), "_s13.py"))
s = importlib.util.module_from_spec(_spec)
_spec.loader.exec_module(s)


def layout(h):
    for cap, box in (("label2", "cbStyleDefault"), ("label7", "cbColorDefault"), ("label6", "tbWidthDefault")):
        h.set(cap, h.n(box).nx, None)
