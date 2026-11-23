"""FormSubscriberMove on the grid (UI-12 slice 9, layout script on the planner's tree): each group's plan box started 85 px into its group
(main too), an empty band under the caption: the box from the group's padding to 6 px before its pick button."""
import importlib.util
import os

_spec = importlib.util.spec_from_file_location("_s7", os.path.join(os.path.dirname(os.path.abspath(__file__)), "_s7.py"))
s = importlib.util.module_from_spec(_spec)
_spec.loader.exec_module(s)


def layout(h):
    for box, pick in (("textCarrierNameInto", "butPickInsPlanInto"), ("textCarrierNameFrom", "butPickInsPlanFrom")):
        s.span(h, box, x=8, r=h.n(pick).nx - 6)
