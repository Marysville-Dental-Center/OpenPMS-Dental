"""FormReport_ProviderIncomeReport on the grid (UI-12 slice 9, layout script on the planner's tree): the panel (8, 8, 821 x 512) ran 8 px
past the window's 821 x 520 client area and the grid stood 8 px inside it at the left only: the panel inside the padding, the grid 8 px
inside the panel under its docked title.
Fix round: the bold title (13 px tall, main too) cut the descender of its "p", and its text started 6 px left of the grid: the title band
18 px (the bold line), the grid 4 px under it from the panel's left edge, where the title starts, across and down the panel (8 px from the
window's edges, as the panel)."""
import importlib.util
import os

_spec = importlib.util.spec_from_file_location("_s7", os.path.join(os.path.dirname(os.path.abspath(__file__)), "_s7.py"))
s = importlib.util.module_from_spec(_spec)
_spec.loader.exec_module(s)


def layout(h):
    CW, CH = 821, 520
    th = 18
    h.set("panel1", 8, 8, CW - 16, CH - 16)
    h.set("lblDgvHeader", 0, 0, CW - 16, th)
    h.set("dataGridView1", 0, th + 4, CW - 16, CH - 16 - th - 4)
