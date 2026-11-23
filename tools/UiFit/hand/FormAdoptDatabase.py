"""FormAdoptDatabase on the grid (UI-12 slice 7, layout script on the planner's tree). The planner left the window's content at 12 against
the buttons' 8 px margin and the confirm row's label 52 px in: the notes, the check box, the confirm row and the log on the content edges
(8 to 616; the check box 8 px wider than its measured text, which GDI+ wraps at its exact width), the confirm label at the content edge with its box 6 px after it. The notes' texts are set in Load (grid-runtime-texts)."""
import importlib.util
import os

_spec = importlib.util.spec_from_file_location("_s7", os.path.join(os.path.dirname(os.path.abspath(__file__)), "_s7.py"))
s = importlib.util.module_from_spec(_spec)
_spec.loader.exec_module(s)


def layout(h):
    R = s.right(h)
    for n in ("labelExplain", "labelDatabase", "labelStatus", "textLog"):
        s.span(h, n, x=8, r=R)
    h.check_left("checkRestoredCopy")
    h.set("checkRestoredCopy", 8, None, h.cw("checkRestoredCopy") + 8, None)
    lw = h.lw("labelConfirmName")
    h.set("labelConfirmName", 8, None, lw, None)
    s.move(h, "textConfirmName", x=8 + lw + 6)
