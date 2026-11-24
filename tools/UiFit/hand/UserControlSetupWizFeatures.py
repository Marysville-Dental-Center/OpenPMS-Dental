"""UserControlSetupWizFeatures on the grid (UI-12 slice 10, layout script on the planner's tree). The planner packed the check boxes (24 px
apart) and the info icons beside them (22 px apart) separately: the seventh icon stood 12 px over its check box. Each icon now on its check
box's row in a column 6 px after the widest check box, the Feature Information group 8 px after it to the right padding; the explanation
label covered the group's title (main too): inside the group's content box; the further-modifications note and Advanced under the group on
the right padding."""
import importlib.util
import os

_spec = importlib.util.spec_from_file_location("_s10", os.path.join(os.path.dirname(os.path.abspath(__file__)), "_s10.py"))
s = importlib.util.module_from_spec(_spec)
_spec.loader.exec_module(s)


def layout(h):
    R = s.right(h)
    h.set("labelFieldType", 8, 8, None, None)
    checks = ("checkCapitation", "checkMedicaid", "checkInsurance", "checkClinical", "checkNoClinics", "checkMedicalIns", "checkEhr")
    icons = ("labelInfoCapitation", "labelInfoMedicaid", "labelInfoInsurance", "labelInfoClinical", "labelInfoClinics", "labelInfoMedIns", "labelInfoEHR")
    y0 = 8 + h.n("labelFieldType").nh + 8
    for i, c in enumerate(checks):
        h.set(c, 8, y0 + i * 24, None, 20)
    wmax = max(h.n(c).nw for c in checks)
    px = 8 + wmax + 6 - 8
    h.set("panelInfo", px, y0 - 8, 36, 8 + 6 * 24 + 20 + 8)
    for i, n in enumerate(icons):
        h.set(n, 8, 8 + i * 24, 20, 20)
    gx = px + 36 + 8
    h.set("groupBox1", gx, y0 - 8, R - gx, None)
    g = h.n("groupBox1")
    h.set("labelExplanation", 8, 23, g.nw - 16, g.nh - 23 - 8)
    h.set("label1", R - h.n("label1").nw, g.ny + g.nh + 8, None, None)
    h.set("butAdvanced", R - h.n("butAdvanced").nw, h.n("label1").ny + h.n("label1").nh + 4, None, None)
