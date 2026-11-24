"""ControlAppt (the Appointments module) on the grid (UI-12 slice 16). The module's LayoutControls places the book, the calendar column and
the tabs at run time (with UiGrid's numbers since this slice: 8 px from the module's edges, 8 px between the book and the column, the tabs
8 px under it); here the parts the designer places inside them.
The lower calendar panel (227 px, the calendar's width; LayoutControls gives it 351 px, from 317): the navigation strip at its top (a
segmented strip, its buttons edge to edge, unchanged); the day/week toggle, Clear and Advanced/Search in a column on the left, 4 px apart,
the pinboard on the right ending on the panel's edge, both 4 px under the strip; then the field rows at the 27 px pitch (View, Lab Cases,
Daily Prod, Daily Goal), labels and the Lab Cases button right-aligned 6 px before one field column ending on the panel's edge; the
confirmation list under them with its two-line caption before it.
The search group (shown by ShowSearch under the pinboard; 219 x 380, from 367): More and its close button on the first row under the
title, the results, the Date/Time Restrictions group (labels right-aligned 6 px before one field column, the am/pm choices 6 px after
their boxes), the providers' caption and list beside the Search by group, and Providers..., Search and Close on one row at the bottom."""
import importlib.util
import os
_spec = importlib.util.spec_from_file_location("_s16", os.path.join(os.path.dirname(os.path.abspath(__file__)), "_s16.py"))
s = importlib.util.module_from_spec(_spec)
_spec.loader.exec_module(s)

KEEP_BAR_GAP = True  # a module: no dialog bar


def layout(h):
    H = h.H
    B = s.BUTTON_H
    W = h.n("monthCalendarOD").nw  # the column is as wide as the calendar (LayoutControls)
    # the lower calendar panel
    y = 22 + 4
    h.set("toggleDayWeek", 0, y, 80, 40)
    h.set("pinBoard", W - 99, y, 99, 96)
    h.set("butClearPin", 0, y + 40 + 4, 75, B)
    yAdv = y + 40 + 4 + B + 4
    h.set("butAdvSearch", 0, yAdv, 60, B)
    h.set("butSearch", 60 + s.ITEM, yAdv, 114 - 60 - s.ITEM, B)
    fx = 84
    fw = W - fx
    yRow = y + 96 + s.GAP
    for label, field in (("label2", "comboView"), ("butLab", "textLab"), ("labelProduction", "textProduction"), ("labelProdGoal", "textProdGoal")):
        h.set(field, fx, yRow, fw, H)
        if label == "butLab":
            h.set(label, 0, yRow, fx - s.ITEM, H)
        else:
            lw = h.lw(label)
            h.text_align(label, "MiddleRight")
            h.set(label, fx - s.ITEM - lw, yRow, lw, H)
        yRow += 27
    yList = yRow
    h.set("label1", 0, yList, fx - s.ITEM, 30)
    h.set("listConfirmed", fx, yList, 86, 351 - yList)
    h.set("panelCalendarLower", None, None, W, 351)
    # the module's parts where LayoutControls puts them (the designer as the module draws at its designer size): the calendar column 8 px
    # from the right and under the toolbar, the tabs 8 px under it down to the bottom padding, the book 8 px before the column
    root = h.tree
    top = h.n("toolBarMain").nh + s.PAD
    pc = h.n("panelCalendar")
    hc = h.n("panelCalendarLower").ny + 351
    xc = root.w - s.PAD - W
    h.set("panelCalendar", xc, top, W, hc)
    h.set("tabControl", xc, top + hc + s.GAP, W, root.h - s.PAD - (top + hc + s.GAP))
    h.set("contrApptPanel", s.PAD, top, xc - s.GAP - s.PAD, root.h - s.PAD - top)
    # the search group
    G = 219
    R = G - s.PAD
    h.set("groupSearch", None, 741 - 8 - 380, G, 380)  # shown by ShowSearch; in the designer within the module (it ran 4 px past it)
    h.set("butSearchCloseX", R - 16, 22 + 4, 16, 16)
    h.set("butSearchNext", R - 16 - s.ITEM - 71, 22, 71, B)
    h.set("listSearchResults", s.PAD, 22 + B + 4, R - s.PAD, 89)
    y2 = 22 + B + 4 + 89 + s.GAP
    h.set("groupBox2", s.PAD, y2, R - s.PAD, 22 + 3 * 27 - 4 + s.PAD)
    lw = max(h.lw("label9"), h.lw("label10"), h.lw("label11"))
    gx = s.PAD + lw + s.ITEM
    for i, (label, field) in enumerate((("label9", "dateSearch"), ("label10", "textBefore"), ("label11", "textAfter"))):
        yy = 22 + i * 27
        h.text_align(label, "MiddleRight")
        h.set(label, s.PAD, yy, lw, H)
        h.set(field, gx, yy, 130 if field == "dateSearch" else 44, H)
    xr = gx + 44 + s.ITEM
    h.set("radioBeforeAM", xr, 22 + 27 + 1, 39, 20)
    h.set("radioBeforePM", xr + 45, 22 + 27 + 1, 39, 20)
    h.set("panel1", xr, 22 + 54 + 1, 84, 20)
    h.set("radioAfterAM", 0, 0, 39, 20)
    h.set("radioAfterPM", 45, 0, 39, 20)
    y3 = y2 + 22 + 3 * 27 - 4 + s.PAD + s.GAP
    xg = R - 89
    h.set("label8", s.PAD, y3, xg - s.GAP - s.PAD, 16)
    h.set("_listBoxProviders", s.PAD, y3 + 18, xg - s.GAP - s.PAD, 82 - 18)
    h.set("groupBox1", xg, y3, 89, 82)
    h.set("butProvDentist", s.PAD, 22, 73, B)
    h.set("butProvHygenist", s.PAD, 22 + B + 4, 73, B)
    yb = y3 + 82 + 4
    h.set("butProvPick", s.PAD, yb, 75, B)
    h.set("butRefresh", s.PAD + 75 + s.ITEM, yb, 58, B)
    h.set("butSearchClose", R - 58, yb, 58, B)
