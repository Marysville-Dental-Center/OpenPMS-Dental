"""FormEmployeeSelect on the grid (UI-12 slice 4, layout script on the planner's tree). The note after Delete All stood top-aligned 29 px
tall from 1 px over the button (its text 6 px over the button's): the note on the button's row and height, centred on it."""


def layout(h):
    b = h.n("butDelete")
    h.set("label1", b.nx + b.nw + 6, b.ny, h.lw("label1"), b.nh)
    h.text_align("label1", "MiddleLeft")
