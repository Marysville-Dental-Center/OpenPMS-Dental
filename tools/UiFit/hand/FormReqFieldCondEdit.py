"""FormReqFieldCondEdit on the grid (UI-12 slice 9, layout script on the planner's tree): the planner put the relationship list above the
Operator caption and its first box, and left the Condition Type column at 12 against Delete's 8. Main's operator column on the grid: the
Operator caption, the first operator box 2 px under it, the relationship list and the second operator box 4 px apart (the code puts the two
value boxes on the operator boxes' rows and takes 70 px off the window); the Condition Type caption and list at the padding."""
import importlib.util
import os

_spec = importlib.util.spec_from_file_location("_s7", os.path.join(os.path.dirname(os.path.abspath(__file__)), "_s7.py"))
s = importlib.util.module_from_spec(_spec)
_spec.loader.exec_module(s)


def layout(h):
    top = h.n("listConditionType").ny
    s.move(h, "label1", x=8)
    s.move(h, "listConditionType", x=8)
    y = top
    h.set("label2", None, y, None, 17)
    y += 17 + 2
    s.move(h, "comboOperator1", y=y)
    y += h.n("comboOperator1").nh + 4
    s.move(h, "listRelationships", y=y)
    y += h.n("listRelationships").nh + 4
    s.move(h, "comboOperator2", y=y)
