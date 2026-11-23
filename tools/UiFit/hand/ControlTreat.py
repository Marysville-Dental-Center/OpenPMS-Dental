"""ControlTreat (the Treatment Plan module) on the grid where its designer places the controls (UI-12 slice 1, layout script of grid_hand.py,
as ControlAccount and ControlChart got in the pilot). What the module's code does at run time, checked in ControlTreat.cs: it moves and sizes
none of these controls (no Location, Bounds, Size or LayoutManager.Move* of them; the print preview's tooth chart wrapper is its own); it
switches by Visible the insurance summaries (userControlFamIns and userControlIndIns) against the discount plan summary (userControlIndDis,
FillSummary: one or the other), Plan Appt, Refresh with the Estimates as of date and its caption, the Ins Rem button and some of the Show
page's check boxes; LayoutManager keeps the anchored Procedures grid (top, bottom, left) and the note (bottom, left) on the module's edges as
it is sized. So the script places every part where the designer has it, on the module's own numbers (the grids against the module's edge as
the other modules have them, 4 px between the parts):
- the top row on one top (under the toolbar) and one height: the Treatment Plans grid, the button column (New TP and Save TP from the top,
  Refresh and Plan Appt ending on the row's bottom), the Show / Sort by / Printing tabs, the Pre Authorizations grid to the right column's
  edge (main: 3, 6 and 2 px between them, the Pre Authorizations grid 5 px short of the row);
- the Procedures grid 4 px under the row, 4 px from the right column, down to 4 px over the note;
- the right column: Set Priority and Estimates as of captions 2 px over their list and date (main: the captions off their boxes by 2 and
  3 px, the caption wider than its date box), the list down to 4 px over the summaries, the summaries stacked from the Procedures
  grid's bottom 4 px apart (the discount summary on the family summary's place: the code shows one or the other), Ins Rem on the
  column's right edge 4 px over them;
- the tab pages: the check boxes and radio buttons at the grid's check box height (18) one under the other from 3 px under the page's top,
  so the Show page's seven need the row 4 px taller than main's (155: the page 132); the Sort and Printing pages from the Show page's x,
  the Sort page's note as on main (159 px: the width its fit fix gave it for the scales up to 200%).
The summaries' insides are their own designers (DashFamilyInsurance, DashIndividualInsurance, DashIndividualDiscount)."""


def layout(h):
    H = h.H
    h.watch("tabPageShow", "tabPageSort", "tabPagePrint")
    gap = 4
    # ---- The top row ------------------------------------------------------------------------------------------------------------------
    y0, ht = 29, 155
    R = 992
    h.set("gridPlans", 0, y0, 426, ht)
    xb, wb = 426 + gap, 90
    h.set("butNewTP", xb, y0, wb, H)
    h.set("butSaveTP", xb, y0 + H + gap, wb, H)
    yplan = y0 + ht - H
    h.set("butPlannedAppt", xb, yplan, wb, H)
    h.set("butRefresh", xb, yplan - H - gap, wb, H)
    xt = xb + wb + gap
    h.set("tabControlShowSort", xt, y0, 166, ht)
    xp = xt + 166 + gap
    h.set("gridPreAuth", xp, y0, R - xp, ht)
    # ---- The Procedures grid and the note -----------------------------------------------------------------------------------------------
    y1 = y0 + ht + gap
    xr = 799  # the right column: the summaries are 193 px wide (their own designers) and end on the top row's right edge
    wm = xr - gap
    note = h.n("textNote")
    h.set("textNote", 0, note.y, wm, note.h)
    ybot = note.y - gap
    h.set("gridMain", 0, y1, wm, ybot - y1)
    # ---- The right column ---------------------------------------------------------------------------------------------------------------
    hc = 17
    ybox = y1 + hc + 2
    wl = 92
    h.set("label1", xr, y1, wl, hc)
    h.text_align("label1", "BottomLeft")
    xd = xr + wl + 8
    h.set("labelCheckInsFrequency", xd, y1, R - xd, hc)
    h.text_align("labelCheckInsFrequency", "BottomLeft")
    h.set("dateTimeTP", xd, ybox, R - xd, H)
    ind = h.n("userControlIndIns")
    fam = h.n("userControlFamIns")
    dis = h.n("userControlIndDis")
    yind = ybot - ind.h
    h.set("userControlIndIns", xr, yind, ind.w, ind.h)
    yfam = yind - gap - fam.h
    h.set("userControlFamIns", xr, yfam, fam.w, fam.h)
    h.set("userControlIndDis", xr, yfam, dis.w, dis.h)
    # Set Priority's list down to 4 px over the summaries (main: 199 px tall, over the family summary's top).
    h.set("listSetPr", xr, ybox, wl, yfam - gap - ybox)
    rem = h.n("butInsRem")
    h.set("butInsRem", R - rem.w, yfam - gap - rem.h, rem.w, rem.h)
    # ---- The Show, Sort by and Printing pages -------------------------------------------------------------------------------------------
    # The grid's check box row (GRID["check_height"], 18; the module's tree measures 20, which seven rows cannot have). The pages take the
    # tab control's height less its tab strip (main: 151 and 128).
    tc = h.n("tabControlShowSort")
    for page in ("tabPageShow", "tabPageSort", "tabPagePrint"):
        p = h.n(page)
        h.set(page, None, None, None, p.h + ht - tc.h)
    yt, pitch, hk = 3, 18, 18
    for i, (name, x) in enumerate((("checkShowCompleted", 5), ("checkShowMaxDed", 5), ("checkShowFees", 5), ("checkShowIns", 23),
                                   ("checkShowDiscount", 23), ("checkShowSubtotals", 23), ("checkShowTotals", 23))):
        k = h.n(name)
        h.set(name, x, yt + i * pitch, k.w, hk)
    lb = h.n("label6")
    h.set("label6", lb.x, yt, lb.w, lb.h)
    yr = yt + lb.h + 2
    for i, name in enumerate(("radioTreatPlanSortTooth", "radioTreatPlanSortOrder")):
        k = h.n(name)
        h.set(name, 5, yr + i * pitch, k.w, hk)
    k = h.n("checkPrintClassic")
    h.set("checkPrintClassic", 5, yt, k.w, hk)
