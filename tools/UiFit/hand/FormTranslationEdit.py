"""FormTranslationEdit on the grid (UI-12 slice 10, layout script on the planner's tree). The four text boxes ended 49 px short of the right
padding: to it."""
import importlib.util
import os

_spec = importlib.util.spec_from_file_location("_s10", os.path.join(os.path.dirname(os.path.abspath(__file__)), "_s10.py"))
s = importlib.util.module_from_spec(_spec)
_spec.loader.exec_module(s)


def layout(h):
    R = s.right(h)
    for n in ("textEnglish", "textTranslation", "textOtherTranslation", "textComments"):
        s.span(h, n, r=R)
