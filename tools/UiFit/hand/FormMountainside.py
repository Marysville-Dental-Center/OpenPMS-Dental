"""FormMountainside on the grid (UI-12 slice 8 fix round, ui/ui12-8-2; layout script on the planner's tree). The HL7 Sync Folder group
spans the content width (8..554) with its note on its 8 px content edge, but the Out from Mountainside box ended 32 px in from its right
edge, and the Internal Name and Description boxes above ended at 522 while Save ends at 554: the group's box on the group's content edge,
the two boxes above on the same edge (one right edge for the field column)."""
import importlib.util
import os

_spec = importlib.util.spec_from_file_location("_s8", os.path.join(os.path.dirname(os.path.abspath(__file__)), "_s8.py"))
s = importlib.util.module_from_spec(_spec)
_spec.loader.exec_module(s)


def layout(h):
    g = h.n("groupBox1")
    cr = g.nw - 8  # the group's content edge (group coordinates)
    f = h.n("textHL7FolderOut")
    h.set("textHL7FolderOut", None, None, cr - f.nx, None)
    edge = g.nx + cr  # the same edge in the window
    for name in ("textProgName", "textProgDesc"):
        n = h.n(name)
        h.set(name, None, None, edge - n.nx, None)
