"""FormRegistrationKey on the grid (UI-12 slice 10, layout script on the planner's tree). The caption and the registration list stood 12 px
from the left, the list 1 px over the agreement's top: on the padding, both from one top 2 px under the caption, the agreement 8 px after
the list to the right padding, both 12 px over OK."""
import importlib.util
import os

_spec = importlib.util.spec_from_file_location("_s10", os.path.join(os.path.dirname(os.path.abspath(__file__)), "_s10.py"))
s = importlib.util.module_from_spec(_spec)
_spec.loader.exec_module(s)


def layout(h):
    h.set("labelLicenseAgree", 8, None, None, None)
    l = s.fill(h, "listBoxRegistration", y=27, r=8 + h.n("listBoxRegistration").nw, over="butOK")
    s.fill(h, "richTextAgreement", x=8 + l.nw + 8, y=27, over="butOK")
