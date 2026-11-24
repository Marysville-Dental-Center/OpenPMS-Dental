"""FormReportCustom on the grid (UI-12 slice 14, layout script). Its Layout handler puts the report viewer under the toolbar over the whole client area
(the viewer's dock fill did not work); the viewer is a page surface with its own scroll bars, kept edge to edge as the grid keeps the print
previews and the other drawn surfaces (UI-12.md section 1, Untouched). Main's designer had the viewer at 45, 56 (856 x 453), where the code
never leaves it, so the planner refused the window (its tree is not the designer's): the designer now has the viewer where the code puts it."""


def layout(h):
    W, HH = h.tree.nw, h.tree.nh
    top = h.n("ToolBarMain").ny + h.n("ToolBarMain").nh
    h.set("viewer", 0, top, W, HH - top)
