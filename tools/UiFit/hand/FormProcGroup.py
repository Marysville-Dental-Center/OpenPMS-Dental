"""FormProcGroup on the grid (UI-12 slice 2, layout script of grid_hand.py, on the planner's tree). The planner centred the first row on
the Provider combo box (3 px under its label) and left 8 px between it and Date Entry; Edit Auto Note wrapped its text in 84 px (main
too). The top rows on the 27 px pitch from 8 (Procedure Date and Provider, Date Entry with its note, User with its buttons, Notes); Edit
Auto Note 92 px wide (its text 85), Auto Note and Append after it 6 px apart; the Notes box keeps its bottom; the lock note as wide as its
longest text (403 px: grid-runtime-texts.tsv)."""


def layout(h):
    H = h.H
    P = H + 4
    y = 8
    for name in ("label2", "textProcDate", "labelProv", "comboProv"):
        h.set(name, None, y, None, H)
    h.set("butLock", None, y, None, H)
    y += P
    for name in ("label12", "textDateEntry"):
        h.set(name, None, y, None, H)
    d = h.n("textDateEntry")
    h.set("label26", d.nx + d.nw + 6, y, h.lw("label26"), H)
    h.text_align("label26", "MiddleLeft")
    y += P
    for name in ("label16", "textUser", "butChangeUser"):
        h.set(name, None, y, None, H)
    x = h.n("butChangeUser").nx + h.n("butChangeUser").nw + 6
    for name, w in (("butEditAutoNote", 92), ("buttonUseAutoNote", h.n("buttonUseAutoNote").nw), ("butAppend", h.n("butAppend").nw)):
        h.set(name, x, y, w, H)
        x += w + 6
    y += P
    n = h.n("textNotes")
    bottom = n.ny + n.nh
    h.set("label7", None, y, None, H)
    h.set("textNotes", None, y, None, bottom - y)
    h.set("labelPermAlert", None, None, max(h.n("labelPermAlert").nw, h.lw("labelPermAlert")), None)
