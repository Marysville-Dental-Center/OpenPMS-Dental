"""The report designer's DialogDatabase (New Report from Database) on the grid (UI-12 slice 13, layout script on the planner's tree; its
designer code is in DialogDatabase.cs, read and edited in InitializeComponent's body). The tab control fills the window over the docked
button panel; SQL, Report Syntax and Report Preview are docked editors and a viewer (unchanged). The planner left the Report Info groups
at x 16 and 112 px short of the right padding, the radio buttons too narrow for their text (the stock circle and gap are 24 px), the
Parameters page's Name and Datatype boxes beside captioned boxes, Allow blank on two lines and Check columns you want to subtotal over its
list. On each page from the content edge: Report Info's groups over the width, Name, Description, Author and Orientation on one label
column; Connection's Connection Type and ODBC Data Sources bands, Connection with its "..." button, the connection string over the width,
Test Connection; Parameters' list with Add and Remove under it and Up and Down beside it, then Name and Datatype rows, the captioned
Prompt, Valid Values ("..." after it) and Default Value boxes and the two check boxes; Grouping's two captioned columns. OK and Cancel stay
on the docked panel's padding."""
import importlib.util
import os

_spec = importlib.util.spec_from_file_location("_s13", os.path.join(os.path.dirname(os.path.abspath(__file__)), "_s13.py"))
s = importlib.util.module_from_spec(_spec)
_spec.loader.exec_module(s)


LEAD = 24


def radio(h, name, x, y):
    h.check_left(name)
    w = LEAD + h.n(name).text_w
    h.set(name, x, y, w, h.CH)
    return x + w


def lab(h, name, x, y, w):
    h.text_align(name, "MiddleRight")
    h.set(name, x, y, w, h.H)


def cap(h, name, x, y, w):
    h.text_align(name, "BottomLeft")
    h.set(name, x, y, w, 17)
    return y + 17 + 2


def layout(h):
    pw, ph = h.n("ReportType").nw, h.n("ReportType").nh
    R = pw - 8
    # Report Info
    radio(h, "rbTable", 8, 23)
    radio(h, "rbList", 8, 23 + h.CH + 4)
    mx = 8 + max(LEAD + h.n("rbTable").text_w, LEAD + h.n("rbList").text_w) + 16
    radio(h, "rbMatrix", mx, 23)
    radio(h, "rbChart", mx, 23 + h.CH + 4)
    gh = 23 + 2 * h.CH + 4 + 8
    h.set("groupBox1", 8, 8, R - 8, gh)
    y = 8 + gh + 8
    lw = max(h.lw(n) for n in ("label1", "label2", "label3", "label6"))
    fx = 8 + lw + 6
    for i, (l, f) in enumerate((("label1", "tbReportName"), ("label2", "tbReportDescription"), ("label3", "tbReportAuthor"),
                                ("label6", "cbOrientation"))):
        lab(h, l, 8, y + 27 * i, lw)
        h.set(f, fx, y + 27 * i, (R - fx) if i < 3 else 168, h.H)
    y += 3 * 27 + h.H + 8
    x = 8
    for r in ("rbSchemaNo", "rbSchema2003", "rbSchema2005"):
        x = radio(h, r, x, 23) + 16
    h.set("groupBox2", 8, y, R - 8, 23 + h.CH + 8)
    # Connection
    lw = max(h.lw("label7"), h.lw("lConnection"))
    fx = 8 + lw + 6
    lab(h, "label7", 8, 8, lw)
    h.set("cbConnectionTypes", fx, 8, 128, h.H)
    ow = h.lw("lODBC")
    ox = fx + 128 + 16
    lab(h, "lODBC", ox, 8, ow)
    h.set("cbOdbcNames", ox + ow + 6, 8, R - (ox + ow + 6), h.H)
    lab(h, "lConnection", 8, 35, lw)
    h.set("bShared", fx, 35, 24, h.H)
    h.set("tbConnection", 8, 62, R - 8, h.H)
    h.set("bTestConnection", 8, 62 + h.H + 8, None, h.H)
    # Parameters
    bh = h.H
    h.set("bAdd", 8, ph - 8 - bh, 56, bh)
    h.set("bRemove", 8 + 56 + 8, ph - 8 - bh, 64, bh)
    lw2 = 8 + 56 + 8 + 64 - 8
    h.set("lbParameters", 8, 8, lw2, ph - 8 - bh - 8 - 8)
    ax = 8 + lw2 + 6
    h.set("bParmUp", ax, 8, 24, h.H)
    h.set("bParmDown", ax, 35, 24, h.H)
    rx = ax + 24 + 16
    nw = max(h.lw("lParmName"), h.lw("lParmType"))
    lab(h, "lParmName", rx, 8, nw)
    h.set("tbParmName", rx + nw + 6, 8, R - (rx + nw + 6), h.H)
    lab(h, "lParmType", rx, 35, nw)
    h.set("cbParmType", rx + nw + 6, 35, 96, h.H)
    y = 35 + h.H + 8
    y = cap(h, "lParmPrompt", rx, y, R - rx)
    h.set("tbParmPrompt", rx, y, R - rx, h.H)
    y = cap(h, "lbParmValidValues", rx, y + h.H + 8, R - rx)
    h.set("bValidValues", R - 24, y, 24, h.H)
    h.set("tbParmValidValues", rx, y, R - 24 - 6 - rx, h.H)
    y = cap(h, "lDefaultValue", rx, y + h.H + 8, R - rx)
    h.set("tbParmDefaultValue", rx, y, R - rx, h.H)
    y += h.H + 8
    x = radio(h, "ckbParmAllowNull", rx, y) + 16
    radio(h, "ckbParmAllowBlank", x, y)
    # Grouping
    cw = (R - 8 - 16) // 2
    y = cap(h, "label4", 8, 8, cw)
    h.set("cbColumnList", 8, y, cw, h.H)
    h.check_left("ckbGrandTotal")
    h.set("ckbGrandTotal", 8, y + h.H + 8, LEAD + h.n("ckbGrandTotal").text_w, h.CH)
    y = cap(h, "label5", 8 + cw + 16, 8, R - (8 + cw + 16))
    h.set("clbSubtotal", 8 + cw + 16, y, R - (8 + cw + 16), None)
    # OK and Cancel on the docked panel's padding
    pw1 = h.n("panel1").nw
    h.set("btnCancel", pw1 - 8 - 75, 8, 75, None)
    h.set("btnOK", pw1 - 8 - 75 - 8 - 75, 8, 75, None)
