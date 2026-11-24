"""ControlImages (the Imaging module) on the grid (UI-12 slice 16). The module's LayoutControls places the toolbars, the tree, the image
area and the bars at run time (with UiGrid's numbers since this slice); here the parts the designer places inside them: the note panel
(the note and the signature, each a caption 2 px over its box, 8 px apart; the invalid-signature note stays on the signature box, shown
by the code) and the automatic import panel (its caption over the folder box, the four buttons a field's height, 6 px apart after it)."""
import importlib.util
import os
_spec = importlib.util.spec_from_file_location("_s16", os.path.join(os.path.dirname(os.path.abspath(__file__)), "_s16.py"))
s = importlib.util.module_from_spec(_spec)
_spec.loader.exec_module(s)

KEEP_BAR_GAP = True  # a module: no dialog bar


def layout(h):
    tn = h.n("textNote")
    h.set("label1", 0, 0, h.n("label1").nw, 16)
    h.set("textNote", 0, 18, tn.nw, 79)
    xs = tn.nw + s.GAP
    h.set("label15", xs, 0, h.n("label15").nw, 16)
    sb = h.n("sigBox")
    h.set("sigBox", xs, 18, sb.nw, 79)
    inv = h.n("labelInvalidSig")
    h.set("labelInvalidSig", xs + (sb.nw - inv.nw) // 2, 18 + (79 - inv.nh) // 2)
    h.set("label3", 0, 0, h.n("label3").nw, 16)
    ti = h.n("textImportAuto")
    h.set("textImportAuto", 0, 18, ti.nw, h.H)
    x = ti.nw + s.ITEM
    for b in ("butBrowse", "butGoTo", "butImportStart", "butImportClose"):
        h.set(b, x, 18, 60, h.H)
        x += 60 + s.ITEM
    # the import caption as wide as its panel (its text needs 677 px; it was cut at 616)
    h.set("label3", 0, 0, h.n("panelImportAuto").nw, 16)
    # the Pearl panel (LayoutControls gives it 70 px, from 62): its group fills it; Show and Tooth Parts a list under the title, Layers on the
    # first row at the group's right padding
    h.set("panelPearl", None, None, 200, 70)
    h.set("groupPearl", 0, 0, 200, 70)
    h.set("checkPearlShow", s.PAD, 22, 83, 18)
    h.set("checkPearlToothParts", s.PAD, 44, 83, 18)
    h.set("butPearlLayers", 200 - s.PAD - 54, 22, 54, s.BUTTON_H)
    h.set("labelAIProcessing", s.PAD, 22, 200 - 2 * s.PAD, 32)
