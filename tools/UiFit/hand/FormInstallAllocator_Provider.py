"""FormInstallAllocator_Provider on the grid (UI-12 slice 10, layout script on the planner's tree). The radio buttons' texts were cut ('I Have
NOT Read The Above Tex': the planner's 198 and 168 px; this window draws its own fonts): as wide as their panel's content. The welcome note
and its text box stood at 9, the radio panel at 12: on the padding."""
import importlib.util
import os

_spec = importlib.util.spec_from_file_location("_s10", os.path.join(os.path.dirname(os.path.abspath(__file__)), "_s10.py"))
s = importlib.util.module_from_spec(_spec)
_spec.loader.exec_module(s)


def layout(h):
    h.set("label1", 8, None, None, None)
    h.set("richTextBox1", 8, None, None, None)
    h.set("panel2", 8, None, None, None)
    w = h.n("panel2").nw - 16
    for n in ("rbIhaveNotRead", "rbutIHaveRead"):
        h.set(n, None, None, w, None)
