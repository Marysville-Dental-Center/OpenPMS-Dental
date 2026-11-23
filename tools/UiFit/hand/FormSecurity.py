"""FormSecurity on the grid (UI-12 slice 3, layout script of grid_hand.py, on the planner's tree). The planner put the user group control at
the padding under the menu (8, 32); FormSecurity.cs stretches it to the window's bottom at run time (now less the padding, UiGrid.PAD, not
5 px), so the designer gives it that height too: the picture shows the window as it opens. The control's own layout is its script
(UserControlSecurityUserGroup.py, run as a part of this window's tree)."""


def layout(h):
    uc = h.n("userControlSecurityUserGroup")
    h.set("userControlSecurityUserGroup", None, None, None, h.tree.nh - uc.ny - 8)
