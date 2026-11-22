"""FormPayPlanDynamic (Dynamic Payment Plan) on the grid (UI-12 slice 1, layout script of grid_hand.py, on the planner's tree). One touch on
the planner's layout: Change Guarantor on the Guarantor row, 6 px after Go To at its top and height (the planner left it 2 px lower)."""


def layout(h):
    g = h.n("butGoToGuar")
    h.set("butChangeGuar", g.x + g.w + 6, g.y, None, g.h)
