"""FormSheetFieldCheckBox on the grid (UI-12 slice 14, layout script). Its field list's handler (listFields_SelectedIndexChanged) shows one
set of controls per field type, stacked on purpose in main's designer: a radio group's value list (groupRadio), a misc box's group and
Reportable Name (groupRadioMisc), a medical history allergy or problem (the medical list with its Add button, Yes / No, the mobile overrides),
a box with no radio values (its mobile item caption); Required with its note for most of them; the position rows and Tab Order always. Main:
labels 0 to 3 px from their boxes and off their centres, Required's box on the right, the groups' labels cut, the medical list 29 px from
the field list. Now, in main's 675 x 696:
- the field list and the medical list side by side from the padding, their captions 2 px over them, each down to its button row (Add
  Procedure under the field list, Add Allergy / Add Problem under the medical list: the code puts Add Problem on Add Allergy's place) a
  section gap over the bar;
- the right column 8 px beside the medical list to the right padding: X Pos / Y Pos and Width / Height as two label + field bands; Required
  (its box on the field column) with its note a section gap under them; under that one slot that each field type fills (the misc group,
  the radio group, Yes and No with their note and the two mobile override rows, or the mobile item caption: never two at once); Tab
  Order on the bottom row with Reportable Name and the misc note over it, a section gap over the bar;
- the groups' rows: one label column as wide as its widest label, the fields to the group's padding; the radio group's list with the
  'Also acts as' note beside it in the label column;
- Delete and Save on the bar."""

PAD = 8
GAP = 4
SECTION = 12
CAP = 2


def row(h, label, field, x_label, x_field, y, w):
    lw = x_field - 6 - x_label
    h.text_align(label, "MiddleRight")
    h.set(label, x_label, y, lw, h.H)
    h.set(field, x_field, y, w, h.H)


def layout(h):
    H = h.H
    W, HH = h.tree.nw, h.tree.nh
    R = W - PAD
    ybar = HH - PAD - 24
    h.set("butDelete", PAD, ybar, 77, 24)
    h.set("butSave", R - 75, ybar, 75, 24)
    # the two lists
    LW = 142
    ybut = ybar - SECTION - 24
    ylist = PAD + 17 + CAP
    for cap, lst, x in (("label2", "listBoxFields", PAD), ("labelMedical", "listMedical", PAD + LW + 8)):
        h.text_align(cap, "BottomLeft")
        h.set(cap, x, PAD, LW, 17)
        h.set(lst, x, ylist, LW, ybut - GAP - ylist)
    h.set("butAddProc", PAD, ybut, 124, 24)
    xm = PAD + LW + 8
    h.set("butAddAllergy", xm, ybut, 105, 24)
    h.set("butAddProblem", xm, ybut, 105, 24)
    # the right column
    x0 = xm + LW + 8
    lw = max(h.lw(n) for n in ("labelMobileMedicalNameOverride", "labelMobileCheckOverride", "labelUiLabelMobileCheckBoxNonMisc",
                               "labelReportableName", "labelTabOrder", "label5", "label7"))
    # one field column for the column and the groups' rows (GridRulesTests' columns: fields within 40 px share one left edge)
    glw_all = max(h.lw(n) for n in ("label3", "labelUiLabelMobileMisc", "labelUiLabelMobileRadioButtonMisc", "labelUiLabelMobile",
                                    "labelItemOverride"))
    fx = max(x0 + lw + 6, x0 + PAD + glw_all + 6)
    fw = R - fx
    NW = 68
    f2 = R - NW
    for (la, fa, lb, fb), y in ((("label5", "textXPos", "label6", "textYPos"), PAD), (("label7", "textWidth", "label8", "textHeight"), PAD + 27)):
        row(h, la, fa, x0, fx, y, NW)
        row(h, lb, fb, fx + NW + 16, f2, y, NW)
    yreq = PAD + 27 + H + SECTION
    h.check_left("checkRequired")
    cw = h.cw("checkRequired")
    h.set("checkRequired", fx, yreq, cw, 18)
    # fix round: Required's note and Yes / No's note on one left edge, 6 px after the wider of Required and the radio buttons
    RW = 56
    xn = fx + max(cw, RW) + 6
    nw = R - xn
    if h.note_h("labelRequired", nw) > h.note_h("labelRequired", 10000):
        # it wraps in the whole remaining width: the narrowest width with as few lines, so no word stands alone on the last line
        lines = h.note_h("labelRequired", nw)
        while nw > 40 and h.note_h("labelRequired", nw - 1) == lines:
            nw -= 1
        nw = min(R - xn, nw + 8)  # 8 px of slack, so the 150% font wraps it the same way
    h.set("labelRequired", xn, yreq + 1, nw, h.note_h("labelRequired", nw))
    yslot = yreq + max(18, h.n("labelRequired").nh) + SECTION
    # the misc group
    gw = R - x0
    gfx = fx - x0
    glw = gfx - 6 - PAD
    for i, (lb, f) in enumerate((("label3", "comboRadioGroupNameMisc"), ("labelUiLabelMobileMisc", "textUiLabelMobileMisc"),
                                 ("labelUiLabelMobileRadioButtonMisc", "textUiLabelMobileRadioButtonMisc"))):
        row(h, lb, f, PAD, gfx, 22 + 27 * i, gw - PAD - gfx)
    h.set("groupRadioMisc", x0, yslot, gw, 22 + 27 * 2 + H + PAD)
    # the radio group
    row(h, "labelUiLabelMobile", "textUiLabelMobile", PAD, gfx, 22, gw - PAD - gfx)
    yl = 22 + 27
    h.set("listRadio", gfx, yl, gw - PAD - gfx, 56)
    h.text_align("labelAlsoActs", "TopRight")
    h.set("labelAlsoActs", PAD, yl + 2, glw, 34)
    yo = yl + 56 + GAP
    row(h, "labelItemOverride", "textItemOverride", PAD, gfx, yo, gw - PAD - gfx)
    h.set("groupRadio", x0, yslot, gw, yo + H + PAD)
    # the medical history field: Yes and No, the note beside them, the mobile overrides
    for i, name in enumerate(("radioYes", "radioNo")):
        h.check_left(name)
        h.set(name, fx, yslot + 22 * i, RW, 18)
    h.set("labelYesNo", xn, yslot + 1, R - xn, h.note_h("labelYesNo", R - xn))
    ym = yslot + 22 + 18 + SECTION
    row(h, "labelMobileCheckOverride", "textMobileCheckOverride", x0, fx, ym, fw)
    row(h, "labelMobileMedicalNameOverride", "textMobileMedicalNameOverride", x0, fx, ym + 27, fw)
    # a box with no radio values: its mobile item caption
    row(h, "labelUiLabelMobileCheckBoxNonMisc", "textUiLabelMobileCheckBoxNonMisc", x0, fx, yslot, fw)
    # the bottom rows: the misc note, Reportable Name, Tab Order
    ytab = ybar - SECTION - H
    row(h, "labelTabOrder", "textTabOrder", x0, fx, ytab, NW)
    row(h, "labelReportableName", "textReportableName", x0, fx, ytab - 27, fw)
    nh = h.note_h("labelMiscInstructions", fw)
    h.set("labelMiscInstructions", fx, ytab - 27 - GAP - nh, fw, nh)
