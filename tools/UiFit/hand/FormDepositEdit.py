"""FormDepositEdit on the grid (UI-12 slice 3, layout script of grid_hand.py on the planner's tree). The planner left the right column's
boxes ending at 890 and the Show group at 959 while Send QB ends on the window's right content edge (966): the boxes (Batch #, Bank Account
Info, Memo, Deposit into Account, the auto deposit account shown by the code), the Show group and, in the group, the insurance payment type
list, the class box and Refresh end on that edge; the group's content starts at its padding (8, the planner kept 14), its two list
columns (with the clinic picker and the class box over them) one width."""


def layout(h):
    R = h.tree.w - 8
    for name in ("textBatch", "textBankAccountInfo", "textMemo", "comboDepositAccount", "labelDepositAccount", "comboDepositAccountNum"):
        n = h.n(name)
        h.set(name, None, None, R - n.nx, None)
    g = h.n("groupSelect")
    h.set("groupSelect", None, None, R - g.nx, None)
    cr = (R - g.nx) - 8
    # The group's content from its padding (the planner kept main's 14): two list columns one width, 8 px apart, to the content edge.
    lw = (cr - 8 - 8) // 2
    x2 = cr - lw
    for name in ("label5", "textDateStart", "labelClinic", "label2"):
        h.set(name, 8, None)
    for name in ("comboClinic", "listPayType"):
        h.set(name, 8, None, lw, None)
    h.set("label2", 8, None, lw, None)
    for name in ("listInsPayType", "comboClassRefs", "label6", "labelClassRef"):
        h.set(name, x2, None, lw, None)
    h.set("butRefresh", cr - h.n("butRefresh").nw, None)
