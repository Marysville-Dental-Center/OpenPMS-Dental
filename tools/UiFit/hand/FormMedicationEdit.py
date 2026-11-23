"""FormMedicationEdit on the grid (UI-12 slice 8, layout script on the planner's tree). The Dependencies group stood at x 43 (its combos
on the window's field column) and ended 108 px short of the right padding: the group from the padding to the right padding with its
combos still on the field column (its labels with them) and ending on its content edge; the Notes box on the right padding with Drug
name.
The Generic name box on its label's row (on the pitch under Drug name; it stood 4 px lower)."""
import importlib.util
import os

_spec = importlib.util.spec_from_file_location("_s8", os.path.join(os.path.dirname(os.path.abspath(__file__)), "_s8.py"))
s = importlib.util.module_from_spec(_spec)
_spec.loader.exec_module(s)


def layout(h):
    R = s.W(h) - 8
    g = h.n("groupBox1")
    dx = g.nx - 8
    gw = R - 8
    h.set("groupBox1", 8, None, gw, None)
    for k in g.children:
        if k.name:
            h.set(k.name, k.nx + dx, None)
    for name in ("comboPatients", "comboPatientAllergy", "comboBrands"):
        c = h.n(name)
        h.set(name, None, None, gw - 8 - c.nx, None)
    tn = h.n("textNotes")
    h.set("textNotes", None, None, R - tn.nx, None)
    s.bar(h, left=("butDelete",), right=("butSave",))
    l1 = h.n("label1")
    h.set("textGenericName", None, l1.ny)
