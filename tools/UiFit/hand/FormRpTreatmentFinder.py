"""FormRpTreatmentFinder (Treatment Finder) on the grid (UI-12 slice 1 fix round 9, layout script of grid_hand.py, on the planner's tree).
Three touches on the View group:
- one right edge for the first field column: a date picker lays its box out inside its bounds (102 px from 63 px in), so Ins Month Start
  and Amount remaining over take the date boxes' width and end with them (they ended 6 px short);
- the two check boxes the planner flowed along the TP Date To row on columns: Include Hidden Billing Types under Billing Type's box (its
  row, the field pitch after it), Include patients with upcoming appointments in the check box column, on its pitch after Include secondary
  insurance;
- Refresh List on the Code Range box's right edge (it ended 21 px short)."""


def layout(h):
    dp = h.n("datePickerStart")
    # the date picker's box, as it lays it out (the tree's child of the picker)
    tb = [k for k in dp.children if k.name == "textDate"][0]
    right = dp.nx + tb.nx + tb.nw
    for name in ("comboMonthStart", "textOverAmount"):
        n = h.n(name)
        h.set(name, None, None, right - n.nx, None)
    mb, cn = h.n("comboBoxMultiBilling"), h.n("checkIncludeNoIns")
    ch = h.n("checkIncludeHiddenBillingTypes")
    h.check_left("checkIncludeHiddenBillingTypes")
    h.set("checkIncludeHiddenBillingTypes", mb.nx, mb.ny + h.H + 4 + (h.H - ch.nh) // 2, None, None)
    ca, ck2 = h.n("checkIncludePatsWithApts"), h.n("checkBenefitAssumeGeneral")
    h.check_left("checkIncludePatsWithApts")
    h.set("checkIncludePatsWithApts", cn.nx, cn.ny + (cn.ny - ck2.ny), None, None)
    cr, rf = h.n("codeRangeFilter"), h.n("butRefresh")
    h.set("butRefresh", cr.nx + cr.nw - rf.nw, None)
