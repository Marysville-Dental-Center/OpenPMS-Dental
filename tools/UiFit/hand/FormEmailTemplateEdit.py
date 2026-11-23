"""FormEmailTemplateEdit on the grid (UI-12 slice 6, layout script on the planner's tree). The planner put Description and Subject under Attach
(52 px down), the body 9 px right of the fields and the Edit buttons, and cut the three-line "Add Replacement Fields" caption: Attach and
the Attachments grid at the top of a right column on the right edge, the Description and Subject rows at the top on the 27 px pitch 8 px
before that column, the body (and the HTML view stacked on it) 8 px under the grid from one field column to the right edge, down to 8 px
over the bottom bar; the replacement fields group under the Body label, its caption as tall as its three lines and centred, its two buttons
under it."""
import importlib.util
import os

_spec = importlib.util.spec_from_file_location("_edges", os.path.join(os.path.dirname(os.path.abspath(__file__)), "_edges.py"))
e = importlib.util.module_from_spec(_spec)
_spec.loader.exec_module(e)


def layout(h):
    R = e.right_edge(h)
    F = 108
    gx = R - 220
    h.set("butAttach", gx, 8)
    h.set("gridAttachments", gx, 8 + 20 + 4)
    y = 8
    for lab, fld in (("label3", "textDescription"), ("label2", "textSubject")):
        h.set(fld, F, y, gx - 8 - F, None)
        h.set(lab, F - 6 - h.lw(lab), y + (20 - 23) // 2, h.lw(lab), None)
        y += 27
    yb = max(y - 4, 8 + 20 + 4 + 62) + 8
    bar = h.n("butSave").ny
    for name in ("textBodyText", "webViewHtml"):
        h.set(name, F, yb, R - F, bar - 8 - yb)
    h.set("label1", F - 6 - h.lw("label1"), yb, h.lw("label1"), None)
    gw = F - 8 - 8
    lh = h.note_h("label4", gw - 16)
    h.set("label4", 8, 8, gw - 16, lh)
    bx = (gw - 65) // 2
    h.set("butSubjectFields", bx, 8 + lh + 4)
    h.set("butBodyFields", bx, 8 + lh + 4 + 28)
    h.set("groupBoxOD1", 8, yb + 20 + 8, gw, 8 + lh + 4 + 28 + 24 + 8)
    h.set("butEditText", F, None)
    h.set("butEditHtml", F + 75 + 6, None)
