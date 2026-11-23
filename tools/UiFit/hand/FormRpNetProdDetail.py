"""FormRpNetProdDetail on the grid (UI-12 slice 5, layout script on the planner's tree). The planner kept main's 26 px left margin for the
two list columns and the note under them: the Providers and Clinics columns (caption, All, list) from the content edge, one width, 8 px
apart, ending 8 px before the right column's groups; the special-report note at the content edge."""


def layout(h):
    xr = h.n("groupDateRange").nx
    w = (xr - 8 - 8 - 8) // 2
    for i, (cap, chk, lst) in enumerate((("label1", "checkAllProv", "listProv"), ("labelClin", "checkAllClin", "listClin"))):
        x = 8 + i * (w + 8)
        h.set(cap, x, None)
        h.set(chk, x, None)
        h.set(lst, x, None, w, None)
    h.set("labelSpecialReport", 8, None)
