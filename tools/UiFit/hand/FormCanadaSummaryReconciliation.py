"""FormCanadaSummaryReconciliation on the grid (UI-12 slice 6, layout script on the planner's tree). The Treating Provider list ended at 284
and the Reconciliation Date row at 297 against the group and Save at 310: both on the right content edge."""
import importlib.util
import os

_spec = importlib.util.spec_from_file_location("_edges", os.path.join(os.path.dirname(os.path.abspath(__file__)), "_edges.py"))
e = importlib.util.module_from_spec(_spec)
_spec.loader.exec_module(e)


def layout(h):
    R = e.right_edge(h)
    lst = h.n("listTreatingProvider")
    h.set("listTreatingProvider", None, None, R - lst.nx, None)
    t = h.n("textDateReconciliation")
    h.set("textDateReconciliation", R - t.nw, None)
    h.set("label4", R - t.nw - 6 - h.n("label4").nw, None)
