"""FormQuickBooksSetup on the grid (UI-12 slice 5, layout script on the planner's tree). Enable QuickBooks Class Refs stood at 140, between
the label column and the lists' column (203): its box on the lists' column. The empty band under it holds the class list rows the check box
shows (hidden in the designer), as on main."""


def layout(h):
    h.set("checkQuickBooksClassRefsEnabled", h.n("listBoxIncomeAccountsQB").nx, None)
