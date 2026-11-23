"""FormEmailMessageEdit (Edit Email) on the grid (UI-12 slice 1 fix round 9, layout script of grid_hand.py, on the planner's tree). One
touch on the planner's layout: the Save / Refresh / Edit Text / Edit HTML row starts on the body box's left edge (the email preview control
moved 3 px with the grid and the row stayed, 3 px left of the box; main: on it), its buttons 6 px apart, at Delete's top and height on the
bottom bar."""


def layout(h):
    ep, body, dl = h.n("emailPreview"), h.n("textBodyText"), h.n("butDelete")
    x = ep.nx + body.nx
    for name in ("butSave", "butRefresh", "butEditText", "butEditHtml"):
        b = h.n(name)
        h.set(name, x, dl.ny, b.nw, dl.nh)
        x += b.nw + 6
