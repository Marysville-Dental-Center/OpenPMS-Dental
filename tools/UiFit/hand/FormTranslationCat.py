"""FormTranslationCat on the grid (UI-12 slice 9, layout script on the planner's tree): the caption and the category list stood at 28 and
the list ended 31 px over the bottom: from the padding to 8 px before the right column (Language/Culture, Export All and its note), down to
the bottom padding."""
import importlib.util
import os

_spec = importlib.util.spec_from_file_location("_s7", os.path.join(os.path.dirname(os.path.abspath(__file__)), "_s7.py"))
s = importlib.util.module_from_spec(_spec)
_spec.loader.exec_module(s)


def layout(h):
    HT = h.n(".").nh
    c = h.n("labelLanguage")
    s.move(h, "label1", x=8)
    h.set("listCats", 8, None, c.nx - 8 - 8, HT - 8 - h.n("listCats").ny)
