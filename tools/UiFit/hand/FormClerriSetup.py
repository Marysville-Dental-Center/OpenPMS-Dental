"""FormClerriSetup on the grid (UI-12 slice 7, layout script on the planner's tree). The window has its own Clinic label (shown with clinics,
like the picker): the picker's own label is off (ShowLabel = false, set in the designer as FormGlobalPaymentsV2Setup's of slice 5) and its
box stands on the field column 6 px after the label (the two labels overlapped). The rows ended at three right edges (398, 479, 522): every
field and note to the content edge, Client Key's box to 6 px before Clear, Clear on the edge; the description on the content edge."""
import importlib.util
import os

_spec = importlib.util.spec_from_file_location("_s7", os.path.join(os.path.dirname(os.path.abspath(__file__)), "_s7.py"))
s = importlib.util.module_from_spec(_spec)
_spec.loader.exec_module(s)


def layout(h):
    R = s.right(h)
    F = h.n("textProgName").nx
    s.span(h, "labelDescription", x=8, r=R)
    h.set("labelDescription", None, None, None, h.note_h("labelDescription", R - 8))
    for n in ("textProgName", "textProgDesc", "textDashboardAddress", "labelDashboardHint", "labelToolbarNote", "labelKeyStatus"):
        s.span(h, n, r=R)
    h.set("comboClinic", F, None, 199, None)
    s.label_before(h, "labelClinic", "comboClinic")
    h.set("butClearKey", R - 75, None, 75, None)
    s.span(h, "textClientKey", r=R - 75 - 6)
