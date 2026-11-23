"""FormEhrMedicalOrderLabEdit on the grid (UI-12 slice 4, layout script on the planner's tree). The grid stood at 12 under the Instructions
box and Delete at 8: the grid at the content edge to 6 px before Attach and Remove."""


def layout(h):
    a = h.n("butAttach")
    h.set("gridMain", 8, None, a.nx - 6 - 8, None)
