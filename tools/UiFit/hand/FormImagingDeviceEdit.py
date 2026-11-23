"""FormImagingDeviceEdit on the grid (UI-12 slice 4 fix round, layout script on the planner's tree). The planner put the Computer Name box
and This 4 px under their label's row (a 31 px pitch), the device type group 5 px right of the field column and the Device Type label 5 px
wider than the other labels: the box and This on the label's row, the group on the field column, the label right-aligned with the others."""


def layout(h):
    lab = h.n("label2")
    for name in ("textComputerName", "butThis"):
        h.set(name, None, lab.ny, None, None)
    col = h.n("textDescription").nx
    h.set("groupBoxOD1", col, None, None, None)
    right = lab.nx + lab.nw
    h.set("label4", right - 100, None, 100, None)
