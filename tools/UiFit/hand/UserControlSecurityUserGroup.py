"""UserControlSecurityUserGroup on the grid (UI-12 slice 3, layout script of grid_hand.py, run as a part of FormSecurity's tree:
grid_hand.py --window FormSecurity --part userControlSecurityUserGroup --part-class UserControlSecurityUserGroup). The control adds controls
in code, which LayoutManager allows only in its host, so the harness cannot build it alone; its rows are written under FormSecurity.
Users page: the User Filters group at the page's padding, its fields captioned in three columns (Show Only, Group, Clinic; Username, Class,
Show hidden users on the second row; Clinic and Class are shown by the code with clinics and the dental school), as wide as the users grid
under it; the effective permissions tree in the right column with its caption, the user group panel (Add User, Copy User, the captions and the
list) between the grid and the tree, 8 px apart; everything ends 8 px from the page's edges. User Groups page: the group list, the group's
permission tree and the associated users list as three captioned columns 8 px apart, the buttons on one row 8 px from the bottom (Add Group
and Edit Group under the list, Set All and Set None at the tree's left, Collapse All and Expand All ending on its right edge). The control's
anchors stretch it to its host (FormSecurity.cs sets its height)."""

PAD = 8
GAP = 8
CAP = 17


def layout(h):
    page = h.n("tabPageUsers")
    W, H = page.nw, page.nh
    R, B = W - PAD, H - PAD
    F = h.H
    # The right column: the effective permissions tree and its caption.
    tree = h.n("securityTreeUser")
    tx = R - tree.nw
    h.set("labelPerm", tx, PAD, tree.nw, CAP)
    h.set("securityTreeUser", tx, PAD + CAP + 2, tree.nw, B - (PAD + CAP + 2))
    # The user group panel left of it.
    panel = h.n("panelUserGroups")
    px = tx - GAP - panel.nw
    # The User Filters group: three captioned columns, two rows.
    grp = h.n("groupBox2")
    top = (grp.title_bottom or 18) + 4
    cw = h.n("comboShowOnly").nw
    cols = [PAD, PAD + cw + 6, PAD + 2 * (cw + 6)]
    y1 = top
    f1 = y1 + CAP + 2
    y2 = f1 + F + 4
    f2 = y2 + CAP + 2
    for cap, fld, c in (("label5", "comboShowOnly", 0), ("labelPermission", "comboGroups", 1), ("labelClinic", "comboClinic", 2)):
        h.set(cap, cols[c], y1, cw, CAP)
        h.text_align(cap, "BottomLeft")
        h.set(fld, cols[c], f1, cw, F)
    for cap, fld, c in (("labelFilterType", "textPowerSearch", 0), ("labelSchoolClass", "comboSchoolClass", 1)):
        h.set(cap, cols[c], y2, cw, CAP)
        h.text_align(cap, "BottomLeft")
        h.set(fld, cols[c], f2, cw, F)
    h.check_left("checkShowHidden")
    h.set("checkShowHidden", cols[2], f2 + (F - h.CH) // 2, h.cw("checkShowHidden"), h.CH)
    gw = px - GAP - PAD
    gh = f2 + F + PAD
    h.set("groupBox2", PAD, PAD, gw, gh)
    gy = PAD + gh + GAP
    h.set("gridUsers", PAD, gy, gw, B - gy)
    # The panel: Add User, Copy User (shown by the code) under it, the two caption lines, the list to the panel's bottom.
    ph = B - gy
    h.set("panelUserGroups", px, gy, panel.nw, ph)
    h.set("butAddUser", 0, 0, None, 24)
    h.set("butCopyUser", 0, 28, None, 24)
    h.set("labelUserTabUserGroups", 0, 60, panel.nw, CAP)
    h.set("labelUserCurr", 0, 60 + CAP, panel.nw, CAP)
    ly = 60 + 2 * CAP + 2
    h.set("listUserTabUserGroups", 0, ly, panel.nw, ph - ly)
    # User Groups page.
    pg = h.n("tabPageUserGroups")
    W2, H2 = pg.nw, pg.nh
    R2, B2 = W2 - PAD, H2 - PAD
    by = B2 - 24
    lb = by - GAP
    ly2 = PAD + CAP + 2
    lst = h.n("listUserGroupTabUserGroups")
    assoc = h.n("listAssociatedUsers")
    h.set("label3", PAD, PAD, lst.nw, CAP)
    h.set("listUserGroupTabUserGroups", PAD, ly2, lst.nw, lb - ly2)
    t2x = PAD + lst.nw + GAP
    ax = R2 - assoc.nw
    t2w = ax - GAP - t2x
    h.set("label6", t2x, PAD, t2w, CAP)
    h.set("userControlSecurityTree", t2x, ly2, t2w, lb - ly2)
    h.set("label4", ax, PAD, assoc.nw, CAP)
    h.set("listAssociatedUsers", ax, ly2, assoc.nw, lb - ly2)
    x = PAD
    for b in ("butAddGroup", "butEditGroup"):
        h.set(b, x, by, None, 24)
        x += h.n(b).nw + 6
    x = t2x
    for b in ("butSetAll", "butSetNone"):
        h.set(b, x, by, None, 24)
        x += h.n(b).nw + 6
    x = t2x + t2w
    for b in ("butExpandAll", "butCollapseAll"):
        x -= h.n(b).nw
        h.set(b, x, by, None, 24)
        x -= 6
    h.watch("tabPageUsers", "groupBox2", "panelUserGroups", "tabPageUserGroups")
