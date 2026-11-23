"""FormEhrMeasureEventEdit on the grid (UI-12 slice 8, layout script on the planner's tree). The planner grew the window from main's 261
to 270 px for the two tobacco rows hidden in the designer (the code shows them for tobacco events): main's height; the More information
box 51 px tall (it was 55; three lines), the tobacco rows on the pitch under it, the scale note centred on its row, the bottom bar on the
padding 8 px under them."""
import importlib.util
import os

_spec = importlib.util.spec_from_file_location("_s8", os.path.join(os.path.dirname(os.path.abspath(__file__)), "_s8.py"))
s = importlib.util.module_from_spec(_spec)
_spec.loader.exec_module(s)


MAIN_H = 261


def layout(h):
    s.size(h, None, MAIN_H)
    mi = h.n("textMoreInfo")
    h.set("textMoreInfo", None, None, None, 51)
    y1 = mi.ny + 51 + 4
    y2 = y1 + 27
    for name in ("labelTobaccoStartDate", "textTobaccoStartDate", "textTobaccoDuration"):
        h.set(name, None, y1)
    for name in ("labelTobaccoDesireToQuit", "textTobaccoDesireToQuit"):
        h.set(name, None, y2)
    sc = h.n("labelTobaccoDesireScale")
    h.set("labelTobaccoDesireScale", None, y2 + (23 - sc.nh) // 2)
    s.bar(h, left=("butDelete",), right=("butSave",))
