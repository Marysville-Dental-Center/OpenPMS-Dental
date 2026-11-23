"""FormTimeCardManage on the grid (UI-12 slice 4, layout script on the planner's tree). Clear Manual Adjusts' text (120 px) filled its
120 px button edge to edge, and Clear Auto Adjusts was 3 px narrower: both 132 px (6 px each side of the text), right-aligned on the right
content edge."""


def layout(h):
    R = h.tree.w - 8
    for name in ("butClearManual", "butClearAuto"):
        h.set(name, R - 132, None, 132, None)
