"""FormPath (Data Paths) on the grid (UI-12 slice 3, mode hand: the planner put the multiple paths check box on one line and the window at
1416 px). Main's arrangement on the grid's numbers, at main's width: the A to Z group at the padding to the right content edge, its three
storage choices a list from the content edge (FormPath.cs puts SFTP on the database choice's row when that choice is hidden), the storage tab
control under them to the content edges. The A to Z page: the path note, the path box with Browse after it to the right edge, the multiple
paths check box (three lines), the server and computer overrides as captioned boxes, 8 px between the blocks. The database and removed pages:
their note to the page's edges. The SFTP page: its caption and note, the four fields on one column (their labels 6 px before it), Clear after
the password, SSH Host Keys under it. Under the group the export and letter merge notes as captions of their path boxes, Browse after each,
Save at the bottom right."""

PAD = 8
GAP = 8


def caption_box(h, cap, box, but, x, y, w):
    """A caption (as tall as its lines) 2 px over a box with its Browse button 6 px after it, the button ending at x + w. Returns the bottom."""
    ch = h.note_h(cap, w, estimate=False)
    h.set(cap, x, y, w, ch)
    h.text_align(cap, "BottomLeft")
    yb = y + ch + 2
    bw = h.n(but).nw
    h.set(box, x, yb, w - bw - 6, h.H)
    h.set(but, x + w - bw, yb, bw, h.H)
    return yb + h.H


def layout(h):
    root = h.tree
    W = root.w
    R = W - PAD
    grp = h.n("groupbox1")
    top = (grp.title_bottom or 18) + 4
    gw = R - PAD
    y = top
    for r in ("radioUseFolder", "radioDatabaseStorage", "radioSftp"):
        h.check_left(r)
        h.set(r, PAD, y, h.cw(r), h.CH)
        y += h.CH + 4
    tx = PAD
    tw = gw - 2 * PAD
    y += GAP - 4
    tab = h.n("tabControlDataStorageType")
    strip = h.n("tabAtoZ").ny  # the tab strip over the page
    pw = tw - 4
    # The A to Z page.
    cw = pw - 2 * PAD
    py = PAD
    nh = h.note_h("labelPathSameForAll", cw)
    h.set("labelPathSameForAll", PAD, py, cw, nh)
    h.text_align("labelPathSameForAll", "BottomLeft")
    py += nh + 2
    bw = h.n("butBrowseDoc").nw
    h.set("textDocPath", PAD, py, cw - bw - 6, h.H)
    h.set("butBrowseDoc", PAD + cw - bw, py, bw, h.H)
    py += h.H + GAP
    ck = h.note_h("checkMultiplePaths", cw - 16)
    h.check_left("checkMultiplePaths")
    h.set("checkMultiplePaths", PAD, py, cw, ck)
    py += ck + GAP
    py = caption_box(h, "labelServerPath", "textServerPath", "butBrowseServer", PAD, py, cw) + GAP
    py = caption_box(h, "labelLocalPath", "textLocalPath", "butBrowseLocal", PAD, py, cw) + PAD
    ph = py
    # The database and removed pages: their note to the page's edges.
    h.set("label2", PAD, PAD, cw, ph - 2 * PAD)
    h.set("labelStorageRemoved", PAD, PAD, cw, ph - 2 * PAD)
    # The SFTP page.
    h.set("label10", PAD, PAD, cw, 17)
    h.text_align("label10", "BottomLeft")
    h.set("label11", PAD, PAD + 17 + 4, cw, h.note_h("label11", cw))
    h.text_align("label11", "TopLeft")
    fy = PAD + 17 + 4 + h.note_h("label11", cw) + GAP
    lw = max(h.lw(n) for n in ("label6", "label7", "label8", "label9"))
    F = PAD + lw + 6
    fw = 300
    b = h.box("tabSftp", top=fy)
    for lab, fld in (("label6", "textSftpAtoZ"), ("label7", "textSftpHostname"), ("label8", "textSftpUsername")):
        b.field(lab, fld, F, fw, lw=lw)
    b.field("label9", "textSftpPassword", F, fw, lw=lw, after=[("butSftpClear", None, h.n("butSftpClear").nw)])
    b.row(("butSshHostKeys", F, h.n("butSshHostKeys").nw))
    for p in ("tabAtoZ", "tabInDatabase", "tabStorageRemoved", "tabSftp"):
        h.set(p, None, None, pw, ph)
    th = strip + ph + 2
    h.set("tabControlDataStorageType", tx, y, tw, th)
    gh = y + th + PAD
    h.set("groupbox1", PAD, PAD, gw, gh)
    y = PAD + gh + GAP
    y = caption_box(h, "label1", "textExportPath", "butBrowseExport", PAD, y, gw) + GAP
    y = caption_box(h, "label3", "textLetterMergePath", "butBrowseLetter", PAD, y, gw) + PAD
    ok = h.n("butOK")
    h.set("butOK", R - ok.nw, None, ok.nw, 24)
    hh = max(root.h, y + 24 + PAD)
    h.set("butOK", None, hh - PAD - 24)
    h.set(".", None, None, W, hh)
    h.watch("groupbox1", "tabAtoZ", "tabInDatabase", "tabStorageRemoved")
