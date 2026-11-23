"""FormTransactionEdit on the grid (UI-12 slice 5 fix round 2, layout script on the planner's tree; the planner's layout otherwise).

The compound panel (shown by the code for every compound transaction, FillCompound; the gallery state "compound"): the button row under
the split grid (Add, Totals, the debit and credit totals, Memo Same For All, Export) ended at 516, 10 px past the grid's right edge (506),
which breaks the round 3 rule (a row of buttons under a list ends where the list ends): the grid widened to Export's right edge, the panel
8 px past its content (as the planner left it).

The Source Documents group: Attach for Deposit and Payment stood at 340 and Attach for Invoice at 324 (its box 16 px shorter for Open
after it); main had one Attach column: the three boxes one width, ending 6 px before the Attach column at Invoice's 324, Open after it
as before."""


def layout(h):
    e = h.n("butExport")
    gm = h.n("gridMain")
    h.set("gridMain", None, None, e.nx + e.nw - gm.nx, None)
    h.set("panelCompound", None, None, e.nx + e.nw + 8, None)
    ax = h.n("butAttachInvoice").nx
    for box, but in (("textSourceDeposit", "butAttachDep"), ("textSourcePay", "butAttachPay"), ("textSourceInvoice", "butAttachInvoice")):
        b = h.n(box)
        h.set(box, None, None, ax - 6 - b.nx, None)
        h.set(but, ax, None)
