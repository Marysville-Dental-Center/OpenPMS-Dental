"""FormAbout on the grid (UI-12 slice 8, layout script on the planner's tree). The planner kept the connection group at x 60, the logo and
the buttons 11 px short of the right padding, the version label 10 px above the product name's centre, and parted the copyright lines
(20, 38 and 20 px apart): the group from the padding to the right padding under the logo, the logo and the buttons on the right padding,
the version label centred on the product name, the four copyright lines on one 20 px pitch with the last one centred on the button row
(main's even lines), the attribution as wide as the content."""
import importlib.util
import os

_spec = importlib.util.spec_from_file_location("_s8", os.path.join(os.path.dirname(os.path.abspath(__file__)), "_s8.py"))
s = importlib.util.module_from_spec(_spec)
_spec.loader.exec_module(s)


def layout(h):
    R = s.W(h) - 8
    pn = h.n("labelProductName")
    lv = h.n("labelVersion")
    h.set("labelVersion", None, pn.ny + (pn.nh - lv.nh) // 2)
    lg = h.n("pictureLogo")
    h.set("pictureLogo", R - lg.nw, 8)
    h.set("groupBox3", 8, None, R - 8, None)
    h.set("panelHL", 8, None, R - 8, None)
    h.set("labelAttribution", 8, None, R - 8, None)
    yb = s.bar(h, right=("butDiagnostics", "butLicense"))
    bh = h.n("butLicense").nh
    y = yb + (bh - 20) // 2
    for name in ("labelMariaDBCopyright", "labelMySQLCopyright", "label9", "labelCopyright"):
        h.set(name, 8, y)
        y -= 20
