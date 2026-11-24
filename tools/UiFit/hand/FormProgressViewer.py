"""FormProgressViewer on the grid (UI-12 slice 9, layout script on the planner's tree): the planner stacked OK and Cancel under Start in the
button panel, 37 px over the messages' bottom. Main's arrangement: View Log and Start at the top of the panel, OK and Cancel on the
messages' bottom edge.
Fix round: the messages stood at 12 with 8 px on top and 14 at the bottom, and the button panel ran 5 px past the client area (Cancel 3 px
from the edge). Every edge at the padding: the messages from (8, 8), the button panel as wide as its buttons (View Log and Start, OK and
Cancel 6 px apart) on the right content edge, 8 px after the messages; both down to 8 px over the bottom."""
import importlib.util
import os

_spec = importlib.util.spec_from_file_location("_s7", os.path.join(os.path.dirname(os.path.abspath(__file__)), "_s7.py"))
s = importlib.util.module_from_spec(_spec)
_spec.loader.exec_module(s)


def layout(h):
    CW, CH = 596, 144  # the designer's ClientSize (a FormODBase without the client panel: the tree's root is the outer size)
    ok, cancel = h.n("butOK"), h.n("butCancel")
    pw = ok.nw + 6 + cancel.nw
    ph = CH - 16
    px = CW - 8 - pw
    h.set("panel3", px, 8, pw, ph)
    h.set("panel1", 8, 8, px - 8 - 8, ph)
    s.move(h, "butViewLog", x=0, y=0)
    s.move(h, "butStart", x=0, y=h.n("butViewLog").nh + 4)
    yb = ph - ok.nh
    s.move(h, "butOK", x=0, y=yb)
    s.move(h, "butCancel", x=ok.nw + 6, y=yb)
