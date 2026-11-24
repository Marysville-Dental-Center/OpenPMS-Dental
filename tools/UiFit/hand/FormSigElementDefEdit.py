"""FormSigElementDefEdit on the grid (UI-12 slice 10, layout script on the planner's tree). The Type list (106) and the Text box (104) under it
ended 2 px apart, and Record New (105) 3 px short of Windows Recorder (108) over it: the list as wide as the box, Record New as wide as
Windows Recorder. The empty label7 (no text in the designer or the code, as on main) held a 47 px empty band over Delete and Save: it spans
the Light rows, the bar 12 px under Light Color (369 -> 334 high; the code does not size the window).
UI-12F (the final consistency pass; a window never shrinks, the lead's decision at slice 6): main's client size 599 x 363 again, the room the
content does not fill shared out as equal gaps between the Type list, the Text row, the Sound group and the Light rows and over the bar (_s10.keep_main), no empty band at the bottom.
The Text label on its box's row (the label-centre rule; grid-known/10.tsv listed it)."""
import importlib.util
import os

_spec = importlib.util.spec_from_file_location("_s10", os.path.join(os.path.dirname(os.path.abspath(__file__)), "_s10.py"))
s = importlib.util.module_from_spec(_spec)
_spec.loader.exec_module(s)


def layout(h):
    h.set("listType", None, None, h.n("textSigText").nw, None)
    h.set("butRecordNew", None, None, h.n("butRecord").nw, None)
    r, c = h.n("textLightRow"), h.n("butColor")
    h.set("label7", None, r.ny, None, c.ny + c.nh - r.ny)
    h.set("label2", None, h.n("textSigText").ny, None, None)  # UI-12F: the label-centre rule (it stood 4 px over its box)
    s.keep_main(h, 599, 363, [("label1", "listType"), ("label2", "textSigText", "label3"), ("groupBox1",),
                              ("label5", "textLightRow", "label7", "label6", "butColor")],
                left=("butDelete",), right_=("butSave",))
