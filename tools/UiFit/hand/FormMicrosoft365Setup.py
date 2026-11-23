"""FormMicrosoft365Setup on the grid (UI-12 slice 7, layout script on the planner's tree). The two ID boxes ended 8 px past the Advanced
group's boxes (503 against 495) and the steps box 1 px short of the group's frame: every field to the group's content edge (504), the
description and the steps box to the window's content edge with the group."""
import importlib.util
import os

_spec = importlib.util.spec_from_file_location("_s7", os.path.join(os.path.dirname(os.path.abspath(__file__)), "_s7.py"))
s = importlib.util.module_from_spec(_spec)
_spec.loader.exec_module(s)


def layout(h):
    R = s.right(h)
    for n in ("textClientId", "textTenantId"):
        s.span(h, n, r=R - 8)
    s.span(h, "textRedirectUri", r=s.right(h, "groupAdvanced"))
    s.span(h, "textSteps", r=R)
    s.span(h, "labelDescription", r=R)
