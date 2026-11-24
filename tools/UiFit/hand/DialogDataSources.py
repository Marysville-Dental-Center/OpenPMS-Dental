"""The report designer's DialogDataSources on the grid (UI-12 slice 15, layout script on the planner's tree; designer code in
DialogDataSources.cs). The planner's edit template wrapped Data Source Name's caption, put the shared reference's box past its check box
and left the bar 89 px over the window's bottom. Top down: the data source list on the padding with Add and Remove beside it, Data
Source Name's caption over its box in the right column; Shared Data Source Reference with its file box and "..." on the right padding;
Data provider (label + box) with Use integrated security after it; Connection string's caption with fx on the right padding on its row,
the connection box under them, taking the window's spare height; the prompt's caption over its box a section gap over the bar; Test
Connection bottom left, OK and Cancel bottom right; main's 456 x 374."""
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
    h.set("lbDataSources", 8, 8, 120, 27 + h.H)
    bx = 8 + 120 + 8
    h.set("bAdd", bx, 8, 64, h.H)
    h.set("bRemove", bx, 35, 64, h.H)
    rx = bx + 64 + s.BAND
    y = s.caption(h, "label1", rx, 8, R - rx)
    h.set("tbDSName", rx, y, R - rx, h.H)
    y = 35 + h.H + 8
    x = s.chk(h, "chkSharedDataSource", 8, y) + s.ITEM
    h.set("bGetFilename", R - 24, y, 24, h.H)
    h.set("tbFilename", x, y, R - 24 - s.ITEM - x, h.H)
    y += 27
    lw = h.lw("lDataProvider")
    x = s.row(h, "lDataProvider", "cbDataProvider", 8, lw, 144, y) + s.BAND
    s.chk(h, "ckbIntSecurity", x, y)
    y += 27
    h.set("bExprConnect", R - s.FX, y, s.FX, h.H)
    h.text_align("lConnectionString", "BottomLeft")
    h.set("lConnectionString", 8, y + h.H - 17, R - s.FX - s.BAND - 8, 17)
    cy = y + h.H + 2
    py = top - s.SECTION - h.H
    pcy = py - 2 - 17
    s.caption(h, "lPrompt", 8, pcy, R - 8)
    h.set("tbPrompt", 8, py, R - 8, h.H)
    h.set("tbConnection", 8, cy, R - 8, pcy - 8 - cy)
