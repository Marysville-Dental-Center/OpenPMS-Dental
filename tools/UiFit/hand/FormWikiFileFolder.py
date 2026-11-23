"""FormWikiFileFolder on the grid (UI-12 slice 9, layout script on the planner's tree): the Path label stood 42 px in and the box and its
browse button ended 173 px before the window's edge: the label at the padding, the box 6 px after it to 6 px before the browse button on the
right content edge; the read-only note the code shows under the box on the box's column, as tall as its lines."""
import importlib.util
import os

_spec = importlib.util.spec_from_file_location("_s7", os.path.join(os.path.dirname(os.path.abspath(__file__)), "_s7.py"))
s = importlib.util.module_from_spec(_spec)
_spec.loader.exec_module(s)


def layout(h):
    W = h.n(".").nw
    lw = h.lw("label1")
    h.set("label1", 8, None, lw, None)
    h.text_align("label1", "MiddleRight")
    F = 8 + lw + 6
    b = h.n("butBrowse")
    s.move(h, "butBrowse", x=W - 8 - b.nw)
    s.span(h, "textLink", x=F, r=W - 8 - b.nw - 6)
    nw = W - 8 - F
    h.set("labelReadOnlyCopy", F, None, nw, h.note_h("labelReadOnlyCopy", nw))
