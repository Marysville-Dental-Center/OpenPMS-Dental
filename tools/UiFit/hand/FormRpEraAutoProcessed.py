"""FormRpEraAutoProcessed (ERAs Automatically Processed) on the grid (UI-12 slice 1, layout script of grid_hand.py, on the planner's tree). The
planner kept main's 12 px frame on both sides (the date range, Acknowledge and the three grids at x 12, Refresh and the grids ending 12 px
from the right): the lead's answer of 2026-11-22 puts the report windows on the grid's one 8 px margin. The script moves the left part of the
filter row (the date range, Acknowledge and the hidden Select EOB) 4 px left, the rest of the row 4 px right (its gaps stay), and the three
grids from the content edge to the right content edge; their tops and heights stay (the grids are anchored)."""


def layout(h):
    root = h.tree
    R = root.w - 8
    grids = ("gridEras", "gridClaims", "gridClaimProcedureAdjustments")
    for name in grids:
        h.set(name, 8, None, R - 8, None)
    for k in root.children:
        if not k.name or k.name in grids or k.dock not in ("None", ""):
            continue
        if k.name in ("dateRangePicker", "butAcknowledge", "butSelectEob"):
            h.set(k.name, k.x - 4)
        else:
            h.set(k.name, k.x + 4)
