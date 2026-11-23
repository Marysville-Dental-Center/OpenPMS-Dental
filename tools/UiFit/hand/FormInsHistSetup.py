"""FormInsHistSetup on the grid (UI-12 slice 8 fix round, ui/ui12-8-2; layout script on the planner's tree). The code makes the rows
(AddCategoryRows, with CodeBase.UiGrid): each row's label from the padding, right-aligned, ending the label gap before the first date
box; two date slots (box, Edit, Remove) the item gap apart with the second ending on the right padding; Save on the right padding
under the rows. The designer follows it: the heading from the padding to the right padding, the Most Recent and Previous captions over the
two slots (x 140 and 334 in the 530 px client area), Save on the right padding."""
import importlib.util
import os

_spec = importlib.util.spec_from_file_location("_s8", os.path.join(os.path.dirname(os.path.abspath(__file__)), "_s8.py"))
s = importlib.util.module_from_spec(_spec)
_spec.loader.exec_module(s)


SLOT_W = 80 + 6 + 40 + 6 + 56  # the date box, Edit and Remove, the item gap apart (FormInsHistSetup.AddCategoryRows)


def layout(h):
    R = s.W(h) - 8
    x1 = R - SLOT_W
    x0 = x1 - 6 - SLOT_W
    h.set("labelHeading", 8, None, R - 8, None)
    h.set("labelMostRecent", x0, None)
    h.set("labelPrevious", x1, None)
    sv = h.n("butSave")
    h.set("butSave", R - sv.nw, None)
