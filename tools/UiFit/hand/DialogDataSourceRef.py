"""The report designer's DialogDataSourceRef on the grid (UI-12 slice 15, layout script on the planner's tree; designer code in
DialogDataSourceRef.cs). Main cut the note's third line; the planner ran Data provider's label into its box and left the bar 51 px over
the window's bottom. Top down: the note over the width as tall as its three lines; the two pass phrase boxes on one row (Repeat's label
between them); the file caption over its box with "..." on the right padding; Data provider (label + box) with ODBC Data Sources (shown
for ODBC) after it to the right padding; Connection string's caption with Use integrated security at the right padding on its row, the
connection box under them, taking the window's spare height; the prompt's caption over its box a section gap over the bar; Test
Connection bottom left, OK and Cancel bottom right; main's 456 x 374. Fix round: Data provider at the content edge, as wide as its text."""
import importlib.util
import os

_spec = importlib.util.spec_from_file_location("_s15", os.path.join(os.path.dirname(os.path.abspath(__file__)), "_s15.py"))
s = importlib.util.module_from_spec(_spec)
_spec.loader.exec_module(s)



def layout(h):
    W0, H0 = 456, 374
    R = W0 - 8
    h.set(".", None, None, W0, H0)
    top = s.bar(h, left=("bTestConnection",), right=("bOK", "bCancel"))
    y = s.note(h, "label1", 8, 8, R - 8) + 8
    rw = h.lw("label6")
    tw = (R - 8 - s.BAND - rw - s.ITEM) // 2
    h.set("tbPassword", 8, y, tw, h.H)
    s.row(h, "label6", "tbPassword2", 8 + tw + s.BAND, rw, R - (8 + tw + s.BAND + rw + s.ITEM), y)
    y = s.caption(h, "label2", 8, y + h.H + 8, R - 8)
    h.set("bGetFilename", R - 24, y, 24, h.H)
    h.set("tbFilename", 8, y, R - 24 - s.ITEM - 8, h.H)
    y += h.H + 8
    x = s.lead(h, "label3", "cbDataProvider", 8, 120, y) + s.BAND
    ow = h.lw("lODBC")
    s.row(h, "lODBC", "cbOdbcNames", x, ow, R - (x + ow + s.ITEM), y)
    y += h.H + 8
    cw = s.check_w(h, "ckbIntSecurity")
    s.chk(h, "ckbIntSecurity", R - cw, y)
    h.text_align("label4", "BottomLeft")
    h.set("label4", 8, y + h.H - 17, R - cw - s.BAND - 8, 17)
    cy = y + h.H + 2
    py = top - s.SECTION - h.H
    pcy = py - 2 - 17
    s.caption(h, "label5", 8, pcy, R - 8)
    h.set("tbPrompt", 8, py, R - 8, h.H)
    h.set("tbConnection", 8, cy, R - 8, pcy - 8 - cy)
