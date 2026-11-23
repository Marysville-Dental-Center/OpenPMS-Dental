"""FormEhrAptObsEdit on the grid (UI-12 slice 8, layout script on the planner's tree). Delete stood at x 5: on the padding (the bottom
bar on the padding). The Value caption takes the code's texts ("SNOMEDCT Value", "Facility Address"): as wide as the widest, its right
edge kept.
Fix round (ui/ui12-8-2): the rows ended at y 158 over an 82 px empty band: the height the content does not fill shared out as equal
gaps between the Observation row, the Value Type list with its code buttons, the Value row, the Value Units row and the bar (s.spread)."""
import importlib.util
import os

_spec = importlib.util.spec_from_file_location("_s8", os.path.join(os.path.dirname(os.path.abspath(__file__)), "_s8.py"))
s = importlib.util.module_from_spec(_spec)
_spec.loader.exec_module(s)


def layout(h):
    s.bar(h, left=("butDelete",), right=("butSave",))
    lv = h.n("labelValue")
    w = h.lw("labelValue")
    h.set("labelValue", lv.nx + lv.nw - w, None, w, None)
    yb = h.n("butSave").ny
    s.spread(h, [["label2", "comboObservationQuestion"],
                 ["label1", "listValueType", "butPickValueLoinc", "butPickValueSnomedct", "butPickValueIcd9", "butPickValueIcd10"],
                 ["labelValue", "textValue"], ["label5", "comboUnits", "labelValueUnitsNote"]], 8, yb)
