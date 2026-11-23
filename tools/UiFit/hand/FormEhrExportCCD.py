"""FormEhrExportCCD on the grid (UI-12 slice 5, layout script on the planner's tree). The sections caption (its text 372 px) was cut at
the window's 353 px ("Documen"), and Result stood 6 px after Functional Status instead of on the right column (177): the window as wide as
the caption (the group, the instructions and Save follow its right edge), Result on the right column."""


def layout(h):
    lw = h.lw("labelSections")
    W = max(h.tree.nw, 8 + lw + 8)
    dw = W - h.tree.nw
    h.set(".", None, None, W, None)
    R = W - 8
    for name in ("labelSections", "groupBox1", "textInstructions"):
        h.set(name, 8, None, R - 8, None)
    for name in ("butCheckAll", "butCheckNone", "butSave"):
        h.set(name, h.n(name).nx + dw, None)
    h.set("checkResult", h.n("checkProblem").nx, None)
