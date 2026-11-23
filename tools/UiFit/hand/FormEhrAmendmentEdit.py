"""FormEhrAmendmentEdit on the grid (UI-12 slice 5, layout script on the planner's tree). The Description/Location label kept main's 60 px
and wrapped mid-word ("Descriptio" / "n/Location"): as wide as its text on one line, its right edge on the label column, on the box's
first line like the other labels of multi-line boxes."""


def layout(h):
    n = h.n("label5")
    w = h.lw("label5")
    right = n.nx + n.nw
    h.set("label5", right - w, h.n("textDescription").ny, w, h.H)
    h.text_align("label5", "MiddleRight")
