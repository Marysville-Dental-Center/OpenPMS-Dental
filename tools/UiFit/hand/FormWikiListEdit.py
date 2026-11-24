"""FormWikiListEdit on the grid (UI-12 slice 10, layout script on the planner's tree). The planner grew the window 952 -> 962 wide; the search
row started at 130 and the grid at 12, 12 px before the button groups and 21 px over Delete: main's size, the search row from the padding
(its caption as wide as 'Advanced Search', the text the code gives it), the groups on the right padding, the grid from the padding to 8 px
before them and 12 px over Delete."""
import importlib.util
import os

_spec = importlib.util.spec_from_file_location("_s10", os.path.join(os.path.dirname(os.path.abspath(__file__)), "_s10.py"))
s = importlib.util.module_from_spec(_spec)
_spec.loader.exec_module(s)


def layout(h):
    s.size(h, 952, 628)
    R = s.right(h)
    for gname in ("groupBox3", "groupBox1", "groupBox2"):
        s.move(h, gname, x=R - h.n(gname).nw)
    y = s.bar(h, left=("butDelete",))
    lw = h.lw("labelSearch")
    h.set("labelSearch", 8, 8, lw, 23)
    s.row_left(h, ("textSearch", "butAdvSearch", "butClearAdvSearch", "radioButHighlight", "radioButFilter", "checkBoxIncludeHiddenColumns"), 8 + lw + 6)
    s.fill(h, "gridMain", y=35, r=h.n("groupBox3").nx - 8, over="butDelete")
