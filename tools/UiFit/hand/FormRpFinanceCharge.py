"""FormRpFinanceCharge on the grid (UI-12 slice 4 fix round, layout script on the planner's tree). The Providers and Billing Types lists
stand side by side: one caption top and height (8, 16), one All row (2 px under the captions) and one list top (main's 50), the Date Range
group on the lists' top (the planner left the Billing Types column 2 px lower than the Providers column)."""


def layout(h):
    cap = h.n("label1")
    yc = cap.ny
    ya = h.n("checkAllProv").ny
    yl = h.n("listProv").ny
    h.set("label4", None, yc, None, cap.nh)
    h.set("checkAllBilling", None, ya, None, None)
    h.set("listBillingType", None, yl, None, None)
    h.set("groupBox2", None, yl, None, None)
