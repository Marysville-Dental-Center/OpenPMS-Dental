"""FormSheetDefEdit on the grid (UI-12 slice 13, layout script on the planner's tree; the harness builds it with an empty letter sheet,
Scan/SampleData.cs). The sheet (panelMain in the scrolling panelLeft) is drawn by the code and placed by SetPanelMainSize; the window's
size is the code's (the sheet's width + 315 and its height + 65, within the screen), so the tool column panelRight keeps main's 284 px.
The planner grew the window to 1208 x 1130 and parted the Add Field buttons from their Show check boxes (one row each, the pairs main lines
up). In the column, from its padding: Description; Edit Properties and Show Mobile, Undo and Redo with Blue, Copy and Paste with Grid Snap
on one three-column grid; Align's four buttons spread over the column; under it the Fields list (its caption over it), Add Field and Show
side by side: Show's All and None first, then each Show check box on its Add Field button's row (28 px pitch, 8 px more between the
kinds of field, the hidden Special and ScreenChart buttons keeping their rows, which the code shows by sheet type); MobileHeader as wide as
its text; Language (Sync matched fields over the language box) and Pages under them, the internal-sheet note on Tab Order's row (Load shows
one or the other), Delete and Save on the bar under the section gap. The column needs 954 px (main's 696 cut nothing only because the
rows touched): the window grows to it; the code's height is the screen's for a letter sheet."""
import importlib.util
import os

_spec = importlib.util.spec_from_file_location("_s13", os.path.join(os.path.dirname(os.path.abspath(__file__)), "_s13.py"))
s = importlib.util.module_from_spec(_spec)
_spec.loader.exec_module(s)

PW = 284  # panelRight (main)
CW = PW - 16
PITCH = 28
KIND_GAP = 8
ADD = ("butOutputText", "butInputField", "butStaticText", "butCheckBox", "butComboBox", None, "butImage", "butPatImage", None, "butLine",
       "butRectangle", None, "butSigBox", "butSigBoxPractice", None, "butSpecial", "butGrid", "butScreenChart", "labelMobileHeader")
SHOW = ("checkShowOutputText", "checkShowInputField", "checkShowStaticText", "checkShowCheckBox", "checkShowComboBox", None,
        "checkShowImage", "checkShowPatImage", None, "checkShowLine", "checkShowRectangle", None, "checkShowSigBox", "checkShowSigBoxPractice",
        None, "checkShowSpecial", "checkShowGrid", "checkShowScreenChart", "checkShowMobileHeader")


def layout(h):
    W0 = s.W(h)
    h.set("textDescription", 8, 8, CW, h.H)
    # the three-column button grid
    bw = 90
    cols = (8, 8 + bw + 8, 8 + 2 * (bw + 8))
    y = 8 + h.H + 8
    rows = (("butEdit", "butMobile", None), ("butUndo", "butRedo", "checkBlue"), ("butCopy", "butPaste", "butAutoSnap"))
    for row in rows:
        for x, name in zip(cols, row):
            if not name:
                continue
            if name.startswith("check"):
                h.check_left(name)
                h.set(name, x, y + (24 - h.CH) // 2, None, h.CH)
            else:
                h.set(name, x, y, (PW - 8 - x) if name == "butAutoSnap" else bw, 24)
        y += PITCH
    y += 4
    # Align: four buttons over the column
    aw = (CW - 16 - 3 * 8) // 4
    for i, name in enumerate(("butAlignLeft", "butAlignCenterH", "butAlignRight", "butAlignTop")):
        h.set(name, 8 + i * (aw + 8), 23, aw, 24)
    h.set("groupAlignH", 8, y, CW, 23 + 24 + 8)
    y += 23 + 24 + 8 + 8
    # Fields | Add Field | Show
    add_w = 8 + max(h.n("butOutputText").nw, h.lw("labelMobileHeader")) + 8
    show_w = 8 + 40 + 8
    list_w = CW - add_w - show_w - 16
    top = y
    h.text_align("label2", "BottomLeft")
    h.set("label2", 8, top, list_w, 17)
    # Show: All and None, then its check boxes on the Add Field rows
    h.set("butShowAll", 8, 23, 40, 24)
    h.set("butShowNone", 8, 23 + PITCH, 40, 24)
    add_top = top + 2 * PITCH
    ry = 23
    for add, show in zip(ADD, SHOW):
        if add is None:
            ry += KIND_GAP
            continue
        if add.startswith("label"):
            h.text_align(add, "MiddleLeft")
            h.set(add, 8, ry, add_w - 16, 24)
        else:
            h.set(add, 8, ry, add_w - 16, 24)
        h.set(show, 8 + (40 - 13) // 2, 2 * PITCH + ry + (24 - h.n(show).nh) // 2)
        ry += PITCH
    group_h = ry - 4 + 8
    h.set("groupAddField", 8 + list_w + 8, add_top, add_w, group_h)
    h.set("groupShowField", 8 + list_w + 8 + add_w + 8, top, show_w, group_h + 2 * PITCH)
    bottom = add_top + group_h
    h.set("listBoxFields", 8, top + 17 + 2, list_w, bottom - (top + 17 + 2))
    y = bottom + 8
    # Language and Pages
    h.check_left("checkSynchMatchedFields")
    h.set("checkSynchMatchedFields", 8, 23, CW - 16, h.CH)
    h.set("comboLanguages", 8, 23 + h.CH + 4, CW - 16, h.H)
    lang_h = 23 + h.CH + 4 + h.H + 8
    h.set("groupBoxSubViews", 8, y, CW, lang_h)
    y += lang_h + 8
    h.set("butPageAdd", 8, 23, None, 24)
    h.set("butPageRemove", 8 + h.n("butPageAdd").nw + 8, 23, None, 24)
    h.set("groupPage", 8, y, CW, 23 + 24 + 8)
    y += 23 + 24 + 8 + 8
    # Tab Order and tips, or the internal-sheet note in their place
    h.set("butTabOrder", 8, y, None, 24)
    t = h.n("linkLabelTips")
    h.set("linkLabelTips", PW - 8 - t.nw, y + (24 - t.nh) // 2)
    nh = h.note_h("labelInternal", CW)
    h.text_align("labelInternal", "TopLeft")
    h.set("labelInternal", 8, y, CW, nh)
    y += max(24, nh) + s.SECTION
    hh = y + 24 + 8
    h.set("butDelete", 8, y, None, 24)
    h.set("butSave", PW - 8 - h.n("butSave").nw, y, None, 24)
    # the window: main's width; the column's height
    h.set(".", None, None, 828, hh)
    h.set("panelRight", 828 - PW, 0, PW, hh)
    h.set("panelLeft", 0, 0, 828 - PW, hh)
