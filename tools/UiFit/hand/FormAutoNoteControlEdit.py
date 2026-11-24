"""FormAutoNoteControlEdit on the grid (UI-12 slice 11, layout script on the planner's tree). The planner centred the Possible responses
caption on its 293 px box (labels of multi-line boxes stand on their first line, as Prompt text does) and left the arrow buttons and Auto
Note Response 79 px short of the right padding. The button column ends on the right padding (Auto Note Response under the arrows, the code
shows it for a combo box); the two wide boxes end 6 px before it. The Possible responses caption wraps TopRight beside its box (fix round:
its run-time text is wider than the label column)."""
import importlib.util
import os

_spec = importlib.util.spec_from_file_location("_s11", os.path.join(os.path.dirname(os.path.abspath(__file__)), "_s11.py"))
s = importlib.util.module_from_spec(_spec)
_spec.loader.exec_module(s)
import grid_tools as g  # noqa: E402

RESPONSES = "Possible responses (one line per item)"


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
    # Fix round: Load (comboType_SelectedIndexChanged) captions the box "Possible responses (one line per item)" for a OneResponse or
    # MultiResponse control (grid-runtime-texts/11.tsv, 205 px), wider than the label column (the padding to 6 px before the field column,
    # 172 px): as on main, TopRight and wrapped beside the multi-line box, from the padding, its first line on the box's first line (4 px
    # down, as a MiddleRight label of the field height draws it), as tall as the wrap of the longest text.
    t = h.n("textOptions")
    lw = t.nx - 6 - 8
    lines = g.measured_lines(RESPONSES, max(h.runtime_widths("labelResponses")), lw)
    h.text_align("labelResponses", "TopRight")
    h.set("labelResponses", 8, t.ny + 4, lw, lines * 16 + 2)
