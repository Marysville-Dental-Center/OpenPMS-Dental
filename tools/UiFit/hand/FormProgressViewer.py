"""FormProgressViewer on the grid (UI-12 slice 9, layout script on the planner's tree): the planner stacked OK and Cancel under Start in the
button panel, 37 px over the messages' bottom. Main's arrangement: View Log and Start at the top of the panel, OK and Cancel on the
messages' bottom edge."""
import importlib.util
import os

_spec = importlib.util.spec_from_file_location("_s7", os.path.join(os.path.dirname(os.path.abspath(__file__)), "_s7.py"))
s = importlib.util.module_from_spec(_spec)
_spec.loader.exec_module(s)


def layout(h):
    m = h.n("panel1")
    h.set("panel3", None, m.ny, None, m.nh)
    s.move(h, "butViewLog", x=8, y=0)
    s.move(h, "butStart", x=8, y=h.n("butViewLog").nh + 4)
    yb = m.nh - h.n("butOK").nh
    s.move(h, "butOK", x=8, y=yb)
    s.move(h, "butCancel", x=8 + h.n("butOK").nw + 6, y=yb)
