"""FormReplicationEdit on the grid (UI-12 slice 10, layout script on the planner's tree). The notes of the two check boxes stood at 353 and
420: one x, 6 px after the wider check box, to the right padding. The wider window put the notes beside the check boxes: the window 12 px
under the last note for Delete and Save (353 -> 328 high; the code does not size it).
UI-12F (the final consistency pass; a window never shrinks, the lead's decision at slice 6): main's client size 353 high (the width stays the 796 its notes need) again, the room the
content does not fill shared out as equal gaps between the six rows and over the bar (_s10.keep_main), no empty band at the bottom."""
import importlib.util
import os

_spec = importlib.util.spec_from_file_location("_s10", os.path.join(os.path.dirname(os.path.abspath(__file__)), "_s10.py"))
s = importlib.util.module_from_spec(_spec)
_spec.loader.exec_module(s)


def layout(h):
    R = s.right(h)
    h.set("label7", 420, None, R - 420, None)
    h.set("label11", 420, None, R - 420, None)
    s.keep_main(h, h.n(".").nw, 353, [("label2", "textDescript", "butThisComputerDesc"), ("label3", "textServerId", "label1"),
                                      ("label4", "textRangeStart", "label5", "textRangeEnd", "label8"), ("label6", "textAtoZpath"),
                                      ("checkUpdateBlocked", "label7"), ("checkTempTablesAllowedServer", "label11")],
                left=("butDelete",), right_=("butSave",))
