"""The allocator's FormInstallAllocator_Provider on the grid (UI-12 slice 14, layout script; its maroon REPORTS header and report buttons keep
their colours, UI-12F). Main: the welcome label 9 px in, the text 12 px in and 69 px down, the report buttons' panel at 475, 69 touching
nothing on the right, the read / not read choices and Run Allocator Tool on a row 6 px under the text with an empty band under them. Now:
the welcome label on the padding, the text a row gap under it down to the bottom row, the report panel (its buttons 4 px apart inside its
8 px padding, as the planner put them) on the right padding at the text's top; the bottom row on the bottom padding: the two choices (a
radio list) and, a sibling gap beside them, Run Allocator Tool with the tool's status under it."""

PAD = 8
GAP = 4


def layout(h):
    W, HH = h.tree.nw, h.tree.nh
    lab = h.n("label1")
    h.set("label1", PAD, PAD)
    yt = PAD + lab.nh + GAP
    pw = 159
    h.set("panel1", W - PAD - pw, yt, pw, 8 + 4 * 23 + 3 * GAP + 8)
    for i, name in enumerate(("button4", "butProviderIncomeReport", "butGuarnDetailReport", "butUneardedIncomeReport")):
        h.set(name, 8, 8 + i * (23 + GAP), 143, 23)
    rows = 23 + GAP + 20
    yr = HH - PAD - rows
    tw = W - PAD - pw - 8 - PAD
    h.set("richTextBox1", PAD, yt, tw, yr - 8 - yt)
    for i, name in enumerate(("rbIhaveNotRead", "rbutIHaveRead")):
        h.set(name, 0, i * 22, 204, 18)
    h.set("panel2", PAD, yr, 234, 40)
    xb = PAD + 234 + 8
    h.set("butRunAllocatorTool", xb, yr, 143, 23)
    h.set("lblAllocatorStatus", xb, yr + 23 + GAP, 143, 20)
