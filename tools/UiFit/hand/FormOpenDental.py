"""FormOpenDental (the main window) on the grid (UI-12 slice 14, layout script). Its constructor makes the module bar (51 px, docked left
under the menu), the toolbar (25 px, docked top) and the modules, and LayoutControls lays the window out at run time: the modules' split
container (the module and the patient dashboard) from the module bar's right edge and the toolbar's bottom over the rest, the task list
docked at the bottom (or right) behind a 7 px splitter the user drags, the signal grid over the module bar under its buttons. The parts
stand edge to edge on purpose (a main window: the module bar, the toolbar and the modules meet). LayoutControls now puts the signal grid a
section gap under the module bar's buttons (ModuleBar.GetButtonsHeight; main's fixed 489 stood under the taller buttons of Open Dental's
bar, 59 px of empty bar over the grid with the fork's 57 px buttons). The designer has the parts where LayoutControls puts them with the
tasks docked at the bottom (Load's default, the splitter at 540): the split container from 51, 49 to the splitter, the splitter 7 px over
the tasks' place, the signal grid at its place for the seven module buttons (24 + 7 x 58 + 12); main's designer had them in the middle
of the window where the code never leaves them."""

MENU = 24
TOOLBAR = 25
BAR = 51
SPLIT_Y = 540


def layout(h):
    W, HH = h.tree.nw, h.tree.nh
    top = MENU + TOOLBAR
    h.set("splitContainer", BAR, top, W - BAR, SPLIT_Y - top)
    h.set("panelSplitter", BAR, SPLIT_Y, W - BAR, 7)
    ys = MENU + 7 * 58 + 12
    h.set("lightSignalGrid1", 0, ys, BAR - 1, HH - ys)
