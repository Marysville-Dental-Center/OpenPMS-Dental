"""FormRpUnearnedIncome (Unearned Income reports) on the grid (UI-12 slice 1, layout script of grid_hand.py, on the planner's tree). Each tab
page has its own OK button at the page's bottom right; the planner moved each onto the row of the page's first list. The script puts each OK
8 px from its page's bottom and right edges, and on the Line Item page moves the Clinics caption, its All box and its list under the month
calendars' drawn height (a month calendar takes its own height, about 175 px at the program's font, not the designer's 155)."""


def layout(h):
    # The tab control from the content edge to the right content edge (the planner kept main's 12 and 10 px); its pages take its width less
    # their frame. Each page's content from the page's 8 px edge (the lead's answer of 2026-11-22: the report windows on the grid's one 8 px
    # margin; main's pages started at 18), each OK 8 px from its page's bottom and right edges.
    tc = h.n("tabControlMain")
    wt = h.tree.w - 16
    pages = (("tabPageUnearnedAccounts", "butUnearnedAcctOk"), ("tabPageUnallocatedBalances", "butUnearnedAllocationOK"),
             ("tabPageNetUnearned", "butNetUnearnedOK"), ("tabPageLineItemUnearned", "butLineItemOK"))
    for page, ok in pages:
        p = h.n(page)
        pw = wt - (tc.w - p.w)
        kids = [k for k in p.children if k.name and k.name != ok]
        dx = min(k.x for k in kids) - 8
        for k in kids:
            h.set(k.name, k.x - dx)
        h.set(ok, pw - 8 - 75, p.h - 8 - 24, 75, 24)
    cal = h.n("dateLineItemFrom")
    y = cal.y + 176 + 8
    lst = h.n("listLineItemClins")
    bottom = lst.y + lst.h
    h.set("labelLineItemClins", None, y)
    h.set("checkLineItemAllClins", None, y + 18 + 2)
    h.set("checkLineItemShowProv", None, y + 18 + 2)
    yl = y + 18 + 2 + h.CH + 2
    h.set("listLineItemClins", None, yl, None, bottom - yl)
    h.set("tabControlMain", 8, None, wt, None)
