"""FormSecurityLock on the grid (UI-12 slice 4, layout script on the planner's tree). The explanation is twelve lines (the list of what
the lock covers and a two-line paragraph) and was 167 px tall, its last paragraph cut: the note from the content edge to the right content
edge, as tall as its lines (182); the Date and Days rows with their notes 6 px after the boxes and the two check boxes as a list on the
field column under it; the window 13 px taller for it, Save at the bottom right."""


def layout(h):
    W = h.tree.w
    h.set("label1", 8, 8, W - 16, 182)
    b = h.box(".")
    b.y = 8 + 182 + 8
    F = h.n("textDate").nx
    b.field("label3", "textDate", F, 100, after=[("label5", None, h.lw("label5"), h.CH)])
    b.field("label4", "textDays", F, 48, after=[("label2", None, h.lw("label2"), h.CH)])
    b.checks(["checkAdmin", "checkPayPlansExcluded"], F)
    save = h.n("butSave")
    ysave = b.y - 4 + 8
    h.set("butSave", W - 8 - save.nw, ysave)
    h.set(".", None, None, None, ysave + save.nh + 8)
