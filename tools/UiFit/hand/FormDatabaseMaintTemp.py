"""FormDatabaseMaintTemp on the grid (UI-12 slice 6, layout script on the planner's tree). The planner ran the Step 1 caption into its combo
(main too), right-aligned the Step 2 and Step 4 captions at other x, and left the Fix group 105 px short of the right edge: the intro at the
top as tall as its line; each step's caption at the padding with its control 6 px after its text (the combo to the right edge), Step 3's
caption over the results box from the padding to the right edge; the Fix group from the padding to the right edge on the bottom padding,
Step 5's note at its content edge with Backup 6 px after it, the three fix rows right-aligned on the 27 px pitch with their Fix buttons on
its content edge; Step 4 8 px over the group, the results down to 8 px over it."""
import importlib.util
import os

_spec = importlib.util.spec_from_file_location("_edges", os.path.join(os.path.dirname(os.path.abspath(__file__)), "_edges.py"))
e = importlib.util.module_from_spec(_spec)
_spec.loader.exec_module(e)


def layout(h):
    R = e.right_edge(h)
    B = e.bottom_edge(h)
    lh = h.note_h("linkLabel1", R - 8)
    h.set("linkLabel1", 8, 8, R - 8, lh)
    y = 8 + lh + 8
    lw = h.lw("label1")
    h.set("label1", 8, y, lw, 23)
    h.text_align("label1", "MiddleLeft")
    h.set("comboDbs", 8 + lw + 6, y, R - (8 + lw + 6), 23)
    y += 27
    lw = h.lw("label2")
    h.set("label2", 8, y, lw, None)
    h.text_align("label2", "MiddleLeft")
    h.set("butRun", 8 + lw + 6, y)
    y += 27 + 4
    h.set("label3", 8, y, None, 17)
    y += 17 + 2
    g = h.n("groupBox1")
    gy = B - g.nh
    h.set("groupBox1", 8, gy, R - 8, None)
    y4 = gy - 8 - 23
    h.set("textResults", 8, y, R - 8, y4 - 8 - y)
    lw = h.lw("label4")
    h.set("label4", 8, y4, lw, None)
    h.text_align("label4", "MiddleLeft")
    h.set("butPrint", 8 + lw + 6, y4)
    cr = R - 8 - 8
    fx = cr - 66
    yy = 23
    for lab, but in (("label6", "butFix1"), ("label7", "butFix2"), ("label8", "butFix3")):
        h.set(but, fx, yy)
        h.set(lab, fx - 6 - h.n(lab).nw, yy)
        yy += 27
    h.set("label5", 8, 23)
    h.set("butBackup", 8 + h.n("label5").nw + 6, 23)
