"""FormReport_ProviderIncomeReport on the grid (UI-12 slice 9, layout script on the planner's tree): the panel (8, 8, 821 x 512) ran 8 px
past the window's 821 x 520 client area and the grid stood 8 px inside it at the left only: the panel inside the padding, the grid 8 px
inside the panel under its docked title."""
import importlib.util
import os

_spec = importlib.util.spec_from_file_location("_s7", os.path.join(os.path.dirname(os.path.abspath(__file__)), "_s7.py"))
s = importlib.util.module_from_spec(_spec)
_spec.loader.exec_module(s)


def layout(h):
    CW, CH = 821, 520
    th = h.n("lblDgvHeader").nh
    h.set("panel1", 8, 8, CW - 16, CH - 16)
    h.set("dataGridView1", 8, th + 8, CW - 32, CH - 16 - th - 16)
