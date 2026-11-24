"""FormAutoNoteControlEdit on the grid (UI-12 slice 11, layout script on the planner's tree). The planner centred the Possible responses
caption on its 293 px box (labels of multi-line boxes stand on their first line, as Prompt text does) and left the arrow buttons and Auto
Note Response 79 px short of the right padding. The button column ends on the right padding (Auto Note Response under the arrows, the code
shows it for a combo box); the two wide boxes end 6 px before it."""
import importlib.util
import os

_spec = importlib.util.spec_from_file_location("_s11", os.path.join(os.path.dirname(os.path.abspath(__file__)), "_s11.py"))
s = importlib.util.module_from_spec(_spec)
_spec.loader.exec_module(s)


def layout(h):
    R = s.W(h) - 8
    s.bar(h, left=("butDelete",), right=("butSave",))
    resp = h.n("butAutoNoteResp")
    bx = R - resp.nw
    up = h.n("butUp")
    h.set("butAutoNoteResp", bx, up.ny + up.nh + 4)
    h.set("butUp", bx, None)
    h.set("butDown", bx + up.nw + 6, None)
    for name in ("textBoxControlLabel", "textOptions"):
        n = h.n(name)
        h.set(name, None, None, bx - 6 - n.nx, None)
    t = h.n("textOptions")
    lw = h.lw("labelResponses")
    h.text_align("labelResponses", "MiddleRight")
    h.set("labelResponses", t.nx - 6 - lw, t.ny, lw, h.H)
