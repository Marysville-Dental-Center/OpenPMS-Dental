"""FormSheetImport on the grid (UI-12 slice 10, layout script on the planner's tree). The grid stood 27 px from the left and 16 px before the
insurance card pictures, 12 px over their bottom: from the padding to 8 px before the pictures (on the right padding), down to their bottom."""
import importlib.util
import os

_spec = importlib.util.spec_from_file_location("_s10", os.path.join(os.path.dirname(os.path.abspath(__file__)), "_s10.py"))
s = importlib.util.module_from_spec(_spec)
_spec.loader.exec_module(s)


def layout(h):
    R = s.right(h)
    for n in ("pictureBoxPrimaryInsuranceFront", "pictureBoxPrimaryInsuranceBack", "pictureBoxSecondaryInsuranceFront", "pictureBoxSecondaryInsuranceBack"):
        s.move(h, n, x=R - h.n(n).nw)
    p = h.n("pictureBoxSecondaryInsuranceBack")
    s.fill(h, "gridMain", y=8, r=h.n("pictureBoxPrimaryInsuranceFront").nx - 8, b=p.ny + p.nh)
    s.vcenter(h, "label1", "butOK")
