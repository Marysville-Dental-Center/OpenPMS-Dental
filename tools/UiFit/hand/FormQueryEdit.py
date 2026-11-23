"""FormQueryEdit on the grid (UI-12 slice 6, layout script on the planner's tree). The left column kept main's 13 px margin and the Format
group and the query box ended 4 px short of Save: the content from the padding to the right edge."""
import importlib.util
import os

_spec = importlib.util.spec_from_file_location("_edges", os.path.join(os.path.dirname(os.path.abspath(__file__)), "_edges.py"))
e = importlib.util.module_from_spec(_spec)
_spec.loader.exec_module(e)


def layout(h):
    e.frame(h)
