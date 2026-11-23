"""FormLoginFailed on the grid (UI-12 slice 8, layout script on the planner's tree). panel1 is the window's frame (at 0, 0 over the whole
client area, anchored on every side; the error label is drawn over it, the user and password rows are its children); the planner moved
it to 8, 8 and grew the window from main's 400 x 150: the frame back at 0, 0 in main's size; the error label from the padding to the right
padding; the User and Password columns 8 px apart from the padding to the right padding, their captions 2 px over their boxes, the block
centred between the error label and Login; Login on the bottom and right padding."""
import importlib.util
import os

_spec = importlib.util.spec_from_file_location("_s8", os.path.join(os.path.dirname(os.path.abspath(__file__)), "_s8.py"))
s = importlib.util.module_from_spec(_spec)
_spec.loader.exec_module(s)


MAIN_W, MAIN_H = 400, 150


def layout(h):
    s.size(h, MAIN_W, MAIN_H)
    h.set("panel1", 0, 0, MAIN_W, MAIN_H)
    R = MAIN_W - 8
    em = h.n("labelErrMsg")
    h.set("labelErrMsg", 8, 8, R - 8, None)
    lg = h.n("butLogin")
    yl = MAIN_H - 8 - lg.nh
    h.set("butLogin", R - lg.nw, yl)
    cap = h.n("labelUser").nh
    box = h.n("textUser").nh
    blk = cap + 2 + box
    top = 8 + em.nh
    y = top + (yl - top - blk) // 2
    wu = h.n("textUser").nw
    h.set("labelUser", 8, y, wu, None)
    h.set("textUser", 8, y + cap + 2, wu, None)
    xp = 8 + wu + 8
    h.set("labelPassword", xp, y, R - xp, None)
    h.set("textPassword", xp, y + cap + 2, R - xp, None)
