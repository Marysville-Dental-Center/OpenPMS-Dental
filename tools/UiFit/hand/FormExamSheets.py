"""FormExamSheets from main's arrangement on the grid (UI-12 slice 7, layout script on the planner's tree). The planner moved the window's
menu bar to 8, 8 (it is the left column's top band), the divider and the sheet preview 8 px down and grew the window from 974 x 696 to
996 x 749 px: main's window, menu bar, divider and owner-drawn preview; the filter caption 8 px under the menu with its list 2 px under
it, the grid 8 px under the list from the content edge to 4 px before the divider, down to 8 px over Add, Add 8 px over the bottom."""
import importlib.util
import os

_spec = importlib.util.spec_from_file_location("_s7", os.path.join(os.path.dirname(os.path.abspath(__file__)), "_s7.py"))
s = importlib.util.module_from_spec(_spec)
_spec.loader.exec_module(s)


def layout(h):
    W, H = 974, 696
    h.set(".", None, None, W, H)
    h.set("menuMain", 0, 0, 435, 24)
    h.set("labelVerticalDivider", 435, 0, 1, H)
    h.set("panelSheetPreview", 439, 3, 534, 692)
    h.set("labelFilterTypes", 8, 32, None, 17)
    h.text_align("labelFilterTypes", "BottomLeft")
    h.set("listExamTypes", 8, 51, None, None)
    h.set("butAdd", 8, H - 8 - 24, None, None)
    s.span(h, "gridMain", x=8, r=431, y=51 + h.n("listExamTypes").nh + 8, b=H - 8 - 24 - 8)
