"""FormRpAccountingBalanceSheet on the grid (UI-12 slice 8, layout script on the planner's tree). The caption and the calendar stood at x
29: on the padding."""
import importlib.util
import os

_spec = importlib.util.spec_from_file_location("_s8", os.path.join(os.path.dirname(os.path.abspath(__file__)), "_s8.py"))
s = importlib.util.module_from_spec(_spec)
_spec.loader.exec_module(s)


def layout(h):
    h.set("labelTO", 8, None)
    h.set("monthCalendar", 8, None)
