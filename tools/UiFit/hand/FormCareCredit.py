"""FormCareCredit on the grid (UI-12 slice 7, layout script on the planner's tree). The clinic picker's box ended 8 px before the provider
box, the two runtime notes (closed merchant number, restricted location: Load shows either or both) overlapped and Transactions stood 66
px left of the button columns: the clinic box on the provider box's edges, the notes one under the other from the field column to the
right edge, Transactions on the field column 8 px under them."""
import importlib.util
import os

_spec = importlib.util.spec_from_file_location("_s7", os.path.join(os.path.dirname(os.path.abspath(__file__)), "_s7.py"))
s = importlib.util.module_from_spec(_spec)
_spec.loader.exec_module(s)


def layout(h):
    F = h.n("comboProviders").nx
    R = h.n("comboProviders").nx + h.n("comboProviders").nw
    cw = h.clinic_w("comboClinics")
    h.set("comboClinics", F - cw, None, cw + R - F, None)
    W = s.right(h) - F
    y = h.n("butLookup").ny + 24 + 4
    nh1 = h.note_h("labelMerchantClosedDescription", W)
    h.set("labelMerchantClosedDescription", F, y, W, nh1)
    y += nh1 + 4
    nh2 = h.note_h("labelApplyRestricted", W)
    h.set("labelApplyRestricted", F, y, W, nh2)
    y += nh2 + 8
    h.set("butTransactions", F, y, None, None)
