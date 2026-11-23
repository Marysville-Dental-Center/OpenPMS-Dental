"""FormQueryParser on the grid (UI-12 slice 6, layout script on the planner's tree). The split panels kept main's 12 px margin, and the
planner moved the split container's two panels to 8, 8 (the container places them itself) and grew the window from main's 817 x 681 to
839 x 718, past the 720 px window height butShowHide_Click sets (it would hide OK): main's size, the split container from the padding to
the right edge down to 8 px over OK, its panels at 0, 0 and under the splitter, their content at the padding."""


def layout(h):
    W, H = 817, 681
    h.set(".", None, None, W, H)
    R, B = W - 8, H - 8
    h.set("butOK", R - 75, B - 23)
    sw = R - 8
    sh = B - 23 - 8 - 8
    h.set("splitContainer", 8, 8, sw, sh)
    h.set("splitterPanel1", 0, 0, sw, 212)
    h.set("splitterPanel2", 0, 216, sw, sh - 216)
    h.set("gridMain", 8, 8, sw - 16, 168)
    h.set("butShowHide", 8, 180)
    h.set("label1", 8, 8)
    h.set("textQuery", 8, 26, sw - 16, sh - 216 - 26 - 8)
