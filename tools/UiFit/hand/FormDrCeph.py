"""FormDrCeph on the grid (UI-12 slice 8 fix round, ui/ui12-8-2; layout script on the planner's tree). The planner packed every row to
the top (the lists from y 35, the field rows ending at 224) over a 72 px empty band, centred the two captions over their lists, and ended
the Browse buttons 38 px short of Save. The two lists take the spare height (they hold the races and the treatment phases): the captions
start where their lists start, 2 px over them; the three field rows stand at the row pitch with the section gap (12) over Save; the lists
grow down to the section gap over the first row; the path boxes and their Browse buttons end on the right padding."""
import importlib.util
import os

_spec = importlib.util.spec_from_file_location("_s8", os.path.join(os.path.dirname(os.path.abspath(__file__)), "_s8.py"))
s = importlib.util.module_from_spec(_spec)
_spec.loader.exec_module(s)

CAP_H = 18  # a caption's height (one line of Segoe UI 9 with its bottom edge on the text)


def layout(h):
    R = s.W(h) - 8
    yb = s.bar(h, right=("butSave",))
    rows = (("labelXRayPath", "textCephLocation", "butCephBrowse"), ("labelPhotoPath", "textPhotoLocation", "butPhotoBrowse"),
            ("labelDate", "dateXRay", None))
    y = yb - 12 - 3 * h.H - 2 * 4
    first = y
    for label, field, browse in rows:
        h.set(label, None, y, None, h.H)
        if browse:
            b = h.n(browse)
            bx = R - b.nw
            f = h.n(field)
            h.set(browse, bx, y, None, h.H)
            h.set(field, None, y, bx - 6 - f.nx, h.H)
        else:
            h.set(field, None, y)
        y += h.H + 4
    top = 8 + CAP_H + 2
    for cap, lst in (("labelRace", "listBoxRace"), ("labelTxPhase", "listBoxTxPhase")):
        n = h.n(lst)
        h.text_align(cap, "BottomLeft")
        h.set(cap, n.nx, 8, n.nw, CAP_H)
        h.set(lst, None, top, None, first - 12 - top)
