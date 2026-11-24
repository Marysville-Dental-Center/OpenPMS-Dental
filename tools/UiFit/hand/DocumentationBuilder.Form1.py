"""DocumentationBuilder's Form1 on the grid (UI-12 slice 12, layout script on the planner's tree; hand/<namespace>.<class>.py, as
xCrudGenerator has a Form1 too). One column of steps: each note across the content and as tall as its lines, the connection string box
across the content, the version rows with their labels from the padding, each Build button (24 px) centred under its note; 4 px between
the parts of a step, 8 px between steps. The window grows only when the column needs it."""
import importlib.util
import os

_spec = importlib.util.spec_from_file_location("_s12", os.path.join(os.path.dirname(os.path.abspath(__file__)), "_s12.py"))
s = importlib.util.module_from_spec(_spec)
_spec.loader.exec_module(s)


def layout(h):
    R = s.W(h) - s.PAD
    cw = R - s.PAD
    y = s.note(h, "label1", s.PAD, s.PAD, cw) + s.GAP
    y = s.note(h, "label2", s.PAD, y, cw) + s.GAP
    y = s.note(h, "label3", s.PAD, y, cw) + s.ROWGAP
    h.set("textConnStr", s.PAD, y, cw, h.H)
    y += h.H + s.ROWGAP
    fx = s.col_x(h, ["label6", "labelPrevVersion"])
    yb = s.rows(h, [("label6", "textVersion"), ("labelPrevVersion", "textPrevVersion")], y, fx)
    y = s.note(h, "label4", s.PAD, yb + s.GAP, cw) + s.ROWGAP
    for lab, but in (("label7", "butBuild"), ("label5", "butBuildUnitTest"), ("labelPrefDocumentation", "buttonPrefDocumentation")):
        y = s.note(h, lab, s.PAD, y, cw) + s.ROWGAP
        b = h.n(but)
        h.set(but, s.PAD + (cw - b.nw) // 2, y, None, 24)
        y += 24 + s.GAP
    need = y - s.GAP + s.PAD
    if need > s.H(h):
        s.size(h, s.W(h), need)
