"""FormRxEdit (Edit Rx) on the grid (UI-12 slice 1 fix round 8, layout script of grid_hand.py, on the planner's tree). The planner moved Audit
Trail and Pat Instr. of the bottom bar 4 px left with Delete and kept Print and View where main has them (gaps 3, 10 and 6 px): the script
puts the four buttons 6 px apart from Audit Trail on, their widths kept."""


def layout(h):
    x = h.n("butAudit").nx
    for name in ("butAudit", "butPrintPatInstructions", "butPrint", "butView"):
        h.set(name, x, None)
        x += h.n(name).nw + 6
