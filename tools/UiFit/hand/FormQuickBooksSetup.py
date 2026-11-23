"""FormQuickBooksSetup on the grid (UI-12 slice 5, layout script on the planner's tree). Enable QuickBooks Class Refs stood at 140, between
the label column and the lists' column (203): its box on the lists' column. The deposit note's third line was cut (32 px for three lines
at 510 px): from the content edge to the right content edge, three lines tall, the rows under it 16 px down (the empty band under the
check box holds the class list rows it shows, hidden in the designer, as on main)."""

BELOW = ("label7", "listBoxDepositAccountsQB", "butAddDepositQB", "butRemoveDepositQB", "labelIncomeAccountQB", "listBoxIncomeAccountsQB",
         "butAddIncomeQB", "butRemoveIncomeQB", "checkQuickBooksClassRefsEnabled", "labelClass", "listBoxClassRefsQB", "buttonAddClassRefQB",
         "buttonRemoveClassRefQB", "labelWarning")


def layout(h):
    h.set("checkQuickBooksClassRefsEnabled", h.n("listBoxIncomeAccountsQB").nx, None)
    R = h.tree.nw - 8
    d = h.n("labelDepositsQB")
    h.set("labelDepositsQB", 8, None, R - 8, 48)
    dy = 48 - d.nh
    for name in BELOW:
        h.set(name, None, h.n(name).ny + dy)
