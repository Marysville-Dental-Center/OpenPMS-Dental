"""FormDepositEdit on the grid (UI-12 slice 3, layout script of grid_hand.py on the planner's tree). The planner left the right column's
boxes ending at 890 and the Show group at 959 while Send QB ends on the window's right content edge (966): the boxes (Batch #, Bank Account
Info, Memo, Deposit into Account, the auto deposit account shown by the code), the Show group and, in the group, the insurance payment type
list, the class box and Refresh end on that edge."""


def layout(h):
    R = h.tree.w - 8
    for name in ("textBatch", "textBankAccountInfo", "textMemo", "comboDepositAccount", "labelDepositAccount", "comboDepositAccountNum"):
        n = h.n(name)
        h.set(name, None, None, R - n.nx, None)
    g = h.n("groupSelect")
    h.set("groupSelect", None, None, R - g.nx, None)
    cr = (R - g.nx) - 8
    for name in ("listInsPayType", "comboClassRefs", "label6"):
        n = h.n(name)
        h.set(name, None, None, cr - n.nx, None)
    h.set("butRefresh", cr - h.n("butRefresh").nw, None)
