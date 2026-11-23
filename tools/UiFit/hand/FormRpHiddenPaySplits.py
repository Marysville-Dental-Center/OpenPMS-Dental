"""FormRpHiddenPaySplits on the grid (UI-12 slice 2, layout script of grid_hand.py, on the planner's tree). The planner left the three list
columns at two widths (160, 163) and their captions 1 px apart. Three list columns (Providers, Unearned Types, Clinics) from the content
edge, 8 px apart and one width, to the right content edge; each caption on one row 8 px under the date range, its All box 2 px under it
and its list 2 px under the box, down to 8 px over the bottom bar; OK at the bottom right."""


def layout(h):
    CH = h.CH
    root = h.tree
    R = root.w - 8
    yb = root.h - 8 - 26
    yc = h.n("odDateRangePicker").ny + h.n("odDateRangePicker").nh + 8
    yl = yc + 18 + 2 + CH + 2
    W = (R - 8 - 16) // 3
    for i, (cap, chk, lst) in enumerate((("label1", "checkAllProv", "listBoxProv"), ("label3", "checkAllUnearnedTypes", "listBoxUnearnedTypes"),
            ("labelClinic", "checkAllClinics", "listBoxClinic"))):
        x = 8 + i * (W + 8)
        w = W if i < 2 else R - x
        h.set(cap, x, yc, w, 18)
        h.text_align(cap, "BottomLeft")
        h.check_left(chk)
        h.set(chk, x, yc + 18 + 2, h.cw(chk), CH)
        h.set(lst, x, yl, w, yb - 8 - yl)
    h.set("butOK", R - 75, yb, 75, 26)
