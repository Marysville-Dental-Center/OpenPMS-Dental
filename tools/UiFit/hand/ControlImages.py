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
    wt = 228  # the tree's width (_widthTree96)
    h.set("groupPearl", 0, 0, wt, 70)
    h.set("checkPearlShow", s.PAD, 22, 83, 18)
    h.set("checkPearlToothParts", s.PAD, 44, 83, 18)
    h.set("butPearlLayers", wt - s.PAD - 54, 22, 54, s.BUTTON_H)
    h.set("labelAIProcessing", s.PAD, 22, wt - 2 * s.PAD, 32)
    # the module's parts where LayoutControls puts them at the designer's size (the note, Pearl, import and drawing panels hidden, at the
    # places the code shows them), so the designer draws as the module does
    W, H = h.tree.w, h.tree.h
    P, G, I = s.PAD, s.GAP, s.ITEM
    h.set("elementHostToolBarMain", 0, 0, W, 25)
    h.set("elementHostWindowingSlider", P, 27, 154, 20)
    h.set("elementHostZoomSlider", P + 154 + I, 25, 231, 25)
    xp = P + 154 + I + 231 + I
    h.set("toolBarPaint", xp, 25, W - xp, 25)
    top, bottom = 50 + P, H - P
    h.set("elementHostImageSelector", P, top, wt, bottom - top)
    h.set("panelPearl", P, bottom - 70, wt, 70)
    h.set("panelSplitter", P + wt + 1, top, 10, bottom - top)
    xr = P + wt + 1 + 10 + 1
    wr = W - P - xr
    h.set("panelNote", xr, bottom - 97, wr, 97)  # as tall as its content (18 + 79; LayoutControls, scaled)
    h.set("elementHostUnmountedBar", xr, bottom - 200, wr, 200)
    h.set("panelImportAuto", xr, top, wr, 44)
    h.set("panelDraw", xr, top, wr, 25)
    h.set("controlImageDock", xr, top, wr, bottom - 200 - G - top)
