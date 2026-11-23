"""FormGlobalPaymentsV2 on the grid (UI-12 slice 4, layout script on the planner's tree). The planner sized Saved card to its designer text
(82 px), but Load names the card ("Saved card ending 1234", grid-runtime-texts), which it cut; and the three transaction types stood 46 and
6 px apart: Saved card as wide as its runtime text; Sale, Refund and Void 6 px apart.
Fix round: one right edge per container: the terminal combo to the Card group's content edge (as before the planner), the transaction box
and the result note to the window's content edge with the groups and Cancel."""


def layout(h):
    n = h.n("radioSavedCard")
    pad = h.cw("radioSavedCard") - n.text_w
    h.set("radioSavedCard", None, None, max(h.runtime_widths("radioSavedCard") + [n.text_w]) + pad, None)
    x = 8
    for name in ("radioSale", "radioRefund", "radioVoid"):
        h.check_left(name)
        h.set(name, x, None, h.cw(name), None)
        x += h.cw(name) + 6
    gc = h.n("groupCard")
    c = h.n("comboTerminal")
    h.set("comboTerminal", None, None, gc.nw - 8 - c.nx, None)
    R = h.tree.w - 8
    for name in ("textTransaction", "labelStatus"):
        h.set(name, None, None, R - h.n(name).nx, None)
