"""FormRpUnearnedIncome (Unearned Income reports) on the grid (UI-12 slice 1, layout script of grid_hand.py, on the planner's tree). Each tab
page has its own OK button at the page's bottom right; the planner moved each onto the row of the page's first list. The script puts each OK
8 px from its page's bottom and right edges, and on the Line Item page moves the Clinics caption, its All box and its list under the month
calendars' drawn height (a month calendar takes its own height, about 175 px at the program's font, not the designer's 155)."""


def layout(h):
    for page, ok in (("tabPageUnearnedAccounts", "butUnearnedAcctOk"), ("tabPageUnallocatedBalances", "butUnearnedAllocationOK"),
                     ("tabPageNetUnearned", "butNetUnearnedOK"), ("tabPageLineItemUnearned", "butLineItemOK")):
        p = h.n(page)
        h.set(ok, p.w - 8 - 75, p.h - 8 - 24, 75, 24)
    cal = h.n("dateLineItemFrom")
    y = cal.y + 176 + 8
    lst = h.n("listLineItemClins")
    bottom = lst.y + lst.h
    h.set("labelLineItemClins", None, y)
    h.set("checkLineItemAllClins", None, y + 18 + 2)
    h.set("checkLineItemShowProv", None, y + 18 + 2)
    yl = y + 18 + 2 + h.CH + 2
    h.set("listLineItemClins", None, yl, None, bottom - yl)
    # The tab control from the content edge to the right content edge (the planner kept main's 12 and 10 px).
    h.set("tabControlMain", 8, None, h.tree.w - 16, None)
