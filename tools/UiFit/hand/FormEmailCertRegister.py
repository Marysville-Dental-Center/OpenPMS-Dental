"""FormEmailCertRegister on the grid (UI-12 slice 5, layout script on the planner's tree). The rows ended at six x (Email Address 463, the
file boxes' ... buttons 599 in groups whose content ends at 612, the certificate box and Import 595, the in-store box 608, Close 620): the
window's boxes to the right content edge (620), in each group the ... button, the certificate box and the Import buttons on the group's
content edge with the file boxes 6 px before the ... buttons."""


def layout(h):
    R = h.tree.nw - 8
    for name in ("textEmailAddress", "textInStore", "labelAddressNote"):
        n = h.n(name)
        h.set(name, None, None, R - n.nx, None)
    GR = h.n("groupCertificate").nw - 8
    for box, browse, imp in (("textCertFilePath", "butBrowse", "butImport"), ("textAnchorFilePath", "butBrowseAnchor", "butImportAnchor")):
        b = h.n(browse)
        h.set(browse, GR - b.nw, None)
        t = h.n(box)
        h.set(box, None, None, GR - b.nw - 6 - t.nx, None)
        i = h.n(imp)
        h.set(imp, GR - i.nw, None)
    d = h.n("textDetails")
    h.set("textDetails", None, None, GR - d.nx, None)
    a = h.n("labelAnchorExplanation")
    h.set("labelAnchorExplanation", None, None, GR - a.nx, None)
