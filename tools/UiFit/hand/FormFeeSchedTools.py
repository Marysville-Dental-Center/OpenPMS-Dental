"""FormFeeSchedTools on the grid (UI-12 slice 11, layout script on the planner's tree). The planner put the two fee schedule group combos
(stacked on the clinic rows, shown in their place when Show Fee Schedule Groups is checked; the code moved them to literal (100, 46) and
(100, 45), now to the clinic row at the field column) at x -160, left both clinic pickers' boxes 8 px off the field column, the group pick
buttons 8 px off the other pick buttons, and the six tab pages' groups at six widths (216 to 433). The group combos at the field column on
the clinic row at the field width and height, their captions before them, their pick buttons on the pick column; the clinic pickers' boxes
on the field column, ending on the fields' right edge; every tab page's group spanning the page's content width; the notes that grow with
it (Clear, Global Updates, Copy Code) one line or less taller than their text, what stands under them following at the item gap."""
import importlib.util
import os

_spec = importlib.util.spec_from_file_location("_s11", os.path.join(os.path.dirname(os.path.abspath(__file__)), "_s11.py"))
s = importlib.util.module_from_spec(_spec)
_spec.loader.exec_module(s)

def group_row(h, sched, clinic, group, label, pick_sched, pick_group):
    f = h.n(sched)
    c = h.n(clinic)
    lw = h.clinic_w(clinic)
    h.set(clinic, f.nx - lw, None, lw + f.nw, h.H)
    h.set(group, f.nx, c.ny, f.nw, h.H)
    s.label_before(h, label, group)
    h.set(pick_group, h.n(pick_sched).nx, c.ny, None, h.H)


def note_then(h, note, w, under, group):
    """The note w wide and as tall as its lines; the controls under it move by the change, the group too."""
    n = h.n(note)
    nh = h.note_h(note, w)
    dy = nh - n.nh
    h.set(note, 8, None, w, nh)
    for name in under:
        u = h.n(name)
        h.set(name, None, u.ny + dy)
    g = h.n(group)
    h.set(group, None, None, None, g.nh + dy)


def layout(h):
    group_row(h, "comboFeeSched", "comboClinic", "comboGroup", "labelGroup", "butPickSched", "butPickGroup")
    group_row(h, "comboFeeSchedTo", "comboClinicTo", "comboGroupTo", "labelGroupTo", "butPickSchedTo", "butPickGroupTo")
    page = h.n("tabPageCopy")
    gw = page.nw - 16
    inner = gw - 16
    for g in ("groupBox1", "groupBox3", "groupBox2", "groupBox5", "groupGlobalUpdateFees", "groupBoxCopyCode"):
        h.set(g, 8, 8, gw, None)
    n = h.n("labelDateAsOfNote")
    h.set("labelDateAsOfNote", None, None, inner + 8 - n.nx, None)
    note_then(h, "label11", inner, ("butClear",), "groupBox2")
    h.set("butClear", 8, None)
    h.set("comboGlobalUpdateClinics", 8, None)
    for name in ("label4", "butUpdate", "butUpdateWriteoffs"):
        h.set(name, 8, None)
    h.text_align("label4", "MiddleLeft")
    h.set("label4", None, None, inner, None)
    h.text_align("label9", "TopLeft")
    note_then(h, "label9", inner, ("butUpdateWriteoffs",), "groupGlobalUpdateFees")
    note_then(h, "labelCopyCodeExplain", inner, ("labelCodeFrom", "textCodeFrom", "butPickCodeFrom", "labelCodeTo", "textCodeTo",
                                                  "butPickCodeTo", "radioCopyCodeAllScheds", "radioCopyCodeOneSched", "butCopyCode"),
              "groupBoxCopyCode")
