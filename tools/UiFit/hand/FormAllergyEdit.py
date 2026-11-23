"""FormAllergyEdit on the grid (UI-12 slice 7, layout script on the planner's tree). Fix round (the lead's review of 2026-11-23): the planner
cut the Patient entered note (the code sets 'Patient entered: ' and what the patient typed, FormAllergyEdit.cs Load) to 96 px right-aligned
before Convert: from the field column (the Allergy box's left edge) to 6 px before Convert, on its row, left-aligned (its long sample in
grid-runtime-texts/7.tsv)."""
import importlib.util
import os

_spec = importlib.util.spec_from_file_location("_s7", os.path.join(os.path.dirname(os.path.abspath(__file__)), "_s7.py"))
s = importlib.util.module_from_spec(_spec)
_spec.loader.exec_module(s)


def layout(h):
    c = h.n("butConvertOther")
    x = h.n("comboAllergies").nx
    h.set("labelOtherEntered", x, c.ny, c.nx - 6 - x, c.nh)
    h.text_align("labelOtherEntered", "MiddleLeft")
