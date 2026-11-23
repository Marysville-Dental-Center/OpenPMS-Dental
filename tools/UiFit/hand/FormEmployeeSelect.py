"""FormEmployeeSelect on the grid (UI-12 slice 4, layout script on the planner's tree). The note after Delete All stood top-aligned 29 px
tall from 1 px over the button (its text 6 px over the button's): the note on the button's row and height, centred on it. Fix round: the
two hidden working-place check boxes one list beside Furloughed and Non-Furloughed (the planner put Working Office under the bottom row and
grew the window for it), each one line; the window 8 px under the bottom row."""


def layout(h):
    b = h.n("butDelete")
    h.set("label1", b.nx + b.nw + 6, b.ny, h.lw("label1"), b.nh)
    h.text_align("label1", "MiddleLeft")
    f = h.n("checkFurloughed")
    nf = h.n("checkNonFurloughed")
    home = h.n("checkWorkingHome")
    h.set("checkWorkingHome", None, f.ny, None, f.nh)
    h.set("checkWorkingOffice", home.nx, nf.ny, None, nf.nh)
    h.set(".", None, None, None, b.ny + b.nh + 8)
