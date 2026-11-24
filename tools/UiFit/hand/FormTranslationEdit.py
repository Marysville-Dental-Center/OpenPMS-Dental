"""FormTranslationEdit on the grid (UI-12 slice 10, layout script on the planner's tree). The four text boxes ended 49 px short of the right
padding: to it. The planner moved them up 27 px and kept the height: the window 12 px under Comments for Save (621 -> 586 high; the code
does not size it)."""
import importlib.util
import os

_spec = importlib.util.spec_from_file_location("_s10", os.path.join(os.path.dirname(os.path.abspath(__file__)), "_s10.py"))
s = importlib.util.module_from_spec(_spec)
_spec.loader.exec_module(s)


def layout(h):
    R = s.right(h)
    for n in ("textEnglish", "textTranslation", "textOtherTranslation", "textComments"):
        s.span(h, n, r=R)
    c = h.n("textComments")
    s.size(h, h.n(".").nw, c.ny + c.nh + 12 + h.n("butSave").nh + 8)
    s.bar(h, right_=("butSave",))
