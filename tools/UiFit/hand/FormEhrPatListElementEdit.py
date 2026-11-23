"""FormEhrPatListElementEdit on the grid (UI-12 slice 8, layout script on the planner's tree). The Restriction and Operand lists were 75
px wide and cut their items (Medication, GreaterThan): both as wide as the Compare string box (117). The Compare string caption takes the code's texts ("For display and sorting"): as
wide as the widest, its right edge kept.
Fix round (ui/ui12-8-2): the lists cut their last row (Operand showed the top of Equals, Restriction the bottom of Lab result) over a 68 px
empty band: each list a whole number of rows (UI.ListBox: 2 px borders and a 17 px line at Segoe UI 9, ceil(font height) + 1), Operand
all three operators with no scroll bar, Restriction four of its seven; the rows under them (Compare string with its example, Order by,
and the Lab value row the code shows for a lab result) follow at the row gap."""
import importlib.util
import os

_spec = importlib.util.spec_from_file_location("_s8", os.path.join(os.path.dirname(os.path.abspath(__file__)), "_s8.py"))
s = importlib.util.module_from_spec(_spec)
_spec.loader.exec_module(s)


def layout(h):
    w = h.n("textCompareString").nw
    h.set("listRestriction", None, None, w, None)
    h.set("listOperand", None, None, w, None)
    lc = h.n("labelCompareString")
    lw = h.lw("labelCompareString")
    h.set("labelCompareString", lc.nx + lc.nw - lw, None, lw, None)
    y = h.n("listRestriction").ny
    for label, field, hh in (("labelRestriction", "listRestriction", rows_h(4)), ("labelOperand", "listOperand", rows_h(3)),
                             ("labelCompareString", "textCompareString", h.H), (None, "checkOrderBy", h.n("checkOrderBy").nh),
                             ("labelLabValue", "textLabValue", h.H)):
        if label:
            h.set(label, None, y)
        if field == "textCompareString":
            ex = h.n("labelExample")
            h.set("labelExample", None, y + (h.H - ex.nh) // 2)
        h.set(field, None, y, None, hh)
        y += hh + 4


LINE = 17  # UI.ListBox's line at Segoe UI 9: ceil(LayoutManager.ScaleMS(font.GetHeight())) + 1
BORDER = 4  # its top and bottom border (Height - 2 - 2 is the room for rows)


def rows_h(n):
    """A UI.ListBox's height for n whole rows."""
    return BORDER + LINE * n
