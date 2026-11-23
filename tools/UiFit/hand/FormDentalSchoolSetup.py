"""FormDentalSchoolSetup on the grid (UI-12 slice 7, layout script on the planner's tree). The Students and Instructors boxes and their pick
buttons ended 68 px inside the group: the boxes to 6 px before the pick buttons, the buttons on the group's content edge, the note to it."""
import importlib.util
import os

_spec = importlib.util.spec_from_file_location("_s7", os.path.join(os.path.dirname(os.path.abspath(__file__)), "_s7.py"))
s = importlib.util.module_from_spec(_spec)
_spec.loader.exec_module(s)


def layout(h):
    R = s.right(h, "groupBox1")
    for t, b in (("textStudents", "butStudentPicker"), ("textInstructors", "butInstructorPicker")):
        bw = h.n(b).nw
        h.set(b, R - bw, None, None, None)
        s.span(h, t, r=R - bw - 6)
    s.span(h, "label3", r=R)
    h.set("label3", None, None, None, h.note_h("label3", R - h.n("label3").nx))
