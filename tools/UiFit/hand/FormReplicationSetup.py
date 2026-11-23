"""FormReplicationSetup on the grid (UI-12 slice 4, layout script on the planner's tree). The Set Ranges and Test notes were drawn into
each other (the Test note 2 px over the end of the Set Ranges note, which wraps onto three lines at 550 px and was cut): the two buttons
at the content edge, each note 6 px after its button, its first line on the button's text, wrapping to 6 px before the Replication Failure
group, which stands at the right of the band on its top (main's place, beside the notes). Fix round: Add is anchored to the bottom
(Bottom|Right) and stands on the grid's bottom edge 6 px after the grid (the planner had put it at the grid's top; a window resize moved it
off the top edge). FormReplicationSetup_Load hides the buttons and notes without random primary keys and takes the band's height off the
window (the designer's width, the Force Sync and Replication Failure groups anchored to the bottom; the grid grows back into the band when
the failure group is hidden), so the layout here is the designer's for both states."""
import grid_tools as g

LINE = 15  # a line of Segoe UI 9 in a label (the tree's text height)


def note_h(h, name, room):
    n = h.n(name)
    return g.measured_lines(n.text, n.text_w, room) * LINE + 1


def layout(h):
    grid = h.n("gridMain")
    y0 = 460  # the band's top (main's grid bottom, 459, on the grid)
    gb = y0 - 8
    h.set("gridMain", None, None, None, gb - grid.ny)
    add = h.n("butAdd")
    h.set("butAdd", grid.nx + grid.nw + 6, gb - add.nh, None, None)
    root = h.tree
    R = root.w - 8
    fail = h.n("groupBoxReplicationFailure")
    xf = R - fail.nw
    xl = 8 + 90 + 6
    room = xf - 6 - xl
    h.set("groupBoxReplicationFailure", xf, y0, None, None)
    h1 = note_h(h, "label1", room)
    h2 = note_h(h, "label2", room)
    y1 = max(y0 + 24 + 4, y0 + 4 + h1 + 4)
    h.set("butSetRanges", 8, y0, 90, 24)
    h.set("label1", xl, y0 + 4, room, h1)
    h.text_align("label1", "TopLeft")
    h.set("butTest", 8, y1, 90, 24)
    h.set("label2", xl, y1 + 4, room, h2)
    h.text_align("label2", "TopLeft")
    band = max(y1 + 24, y1 + 4 + h2, y0 + fail.nh)
    sync = h.n("groupBox1")
    ys = band + 8
    h.set("groupBox1", 8, ys, R - 8, None)
    h.set(".", None, None, None, ys + sync.nh + 8)
