"""FormPrinterEdit on the grid (UI-12 slice 6, layout script on the planner's tree). The rule line under Printer (labelPrinterRule, the text
the code gives it) was 14 px tall for its two lines and the rows stood at main's places: the caption at the padding; Situation and Printer
on the 27 px pitch with the status after the combo; the rule as tall as its lines on the field column to the right edge; Prompt with its
note beside it, Virtual Printer, and File Extension with its note, each 8 px under the one before."""
import importlib.util
import os

_spec = importlib.util.spec_from_file_location("_edges", os.path.join(os.path.dirname(os.path.abspath(__file__)), "_edges.py"))
e = importlib.util.module_from_spec(_spec)
_spec.loader.exec_module(e)


def layout(h):
    R = e.right_edge(h)
    F = 117
    h.set("label2", 8, 8)
    y = 35
    h.set("textSituation", F, y + 1)
    h.set("labelSituation", F - 6 - h.lw("labelSituation"), y, h.lw("labelSituation"), None)
    y += 27
    h.set("comboPrinter", F, y)
    h.set("labelPrinter", F - 6 - h.lw("labelPrinter"), y, h.lw("labelPrinter"), None)
    h.set("labelStatus", F + 280 + 6, y + 3, R - (F + 280 + 6), None)
    y += 27
    rh = h.note_h("labelPrinterRule", R - F)
    h.set("labelPrinterRule", F, y, R - F, rh)
    y += rh + 8
    nx = F + 63 + 6
    h.set("label11", nx, y, R - nx, 32)
    h.check_left("checkPrompt")
    h.set("checkPrompt", F, y + 6)
    y += 32 + 4
    h.check_left("checkVirtualPrinter")
    h.set("checkVirtualPrinter", F, y)
    y += 20 + 8
    h.set("textFileExtension", F, y + 1)
    h.set("labelExtension", F - 6 - h.lw("labelExtension"), y, h.lw("labelExtension"), None)
    h.set("labelExtensionNote", nx, y - 5, R - nx, 33)
