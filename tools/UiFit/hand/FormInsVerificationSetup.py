"""FormInsVerificationSetup on the grid (UI-12 slice 2, mode hand: the planner pushed the Standard group to the window's middle and left the
check boxes at main's x 1). The Standard and Medicaid groups side by side from the content edge, 8 px apart, sharing the content's width;
in both one field column (6 px after the widest label of both: each label right-aligned before its days box, "days" 6 px after the box) on
the 27 px pitch; the Medicaid group's filing code list on the column under its four rows; each group as tall as its content + 8; the five
check boxes a list at the content edge 8 px under the Standard group; Save at the bottom right."""

ROWS = (("labelScheduledAppointment", "textScheduledAppointmentDays", "labelSchedApptDays"),
        ("labelPlanBenefits", "textInsBenefitEligibilityDays", "labelPlanBenefitsDays"),
        ("labelPatientEligibility", "textPatientEnrollmentDays", "labelPatientEligibilityDays"),
        ("labelPastDue", "textPastDueDays", "labelPastDueDays"))


def layout(h):
    H = h.H
    CH = h.CH
    P = H + 4
    root = h.tree
    R = root.w - 8
    gw = (R - 8 - 8) // 2
    lw = max(h.lw(a + s) for a, _, _ in ROWS for s in ("Standard", "Medicaid"))
    lw = max(lw, h.lw("labelInsuranceFilingCodeInMedicaid"))
    F = 8 + lw + 6
    heights = {}
    for grp, sfx, gx in (("groupStandard", "Standard", 8), ("groupMedicaid", "Medicaid", 8 + gw + 8)):
        top = (h.n(grp).title_bottom or 18) + 4
        y = top
        for lab, box, days in ROWS:
            h.set(lab + sfx, 8, y, lw, H)
            h.text_align(lab + sfx, "MiddleRight")
            h.set(box + sfx, F, y, 32, H)
            h.set(days + sfx, F + 32 + 6, y, h.lw(days + sfx), H)
            h.text_align(days + sfx, "MiddleLeft")
            y += P
        if sfx == "Medicaid":
            h.set("labelInsuranceFilingCodeInMedicaid", 8, y, lw, H)
            h.text_align("labelInsuranceFilingCodeInMedicaid", "MiddleRight")
            h.set("listBoxInsFilingCodes", F, y, gw - 8 - F, 212)
            y += 212 + 4
        gh = y - 4 + 8
        h.set(grp, gx, 8, gw, gh)
        h.watch(grp)
        heights[grp] = gh
    y = 8 + heights["groupStandard"] + 8
    for name in ("checkInsVerifyUseCurrentUser", "checkInsVerifyExcludePatVerify", "checkFutureDateBenefitYear",
            "checkFutureDatePatEnrollmentYear", "checkExcludePatientClones"):
        h.check_left(name)
        h.set(name, 8, y, h.cw(name), CH)
        y += CH + 4
    hr = max(root.h, 8 + heights["groupMedicaid"] + 8 + 24 + 8, y + 4 + 24 + 8)
    h.set("butSave", R - 75, hr - 8 - 24, 75, 24)
    h.set(".", None, None, root.w, hr)
