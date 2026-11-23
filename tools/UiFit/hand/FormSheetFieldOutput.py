"""FormSheetFieldOutput on the grid (UI-12 slice 5, layout script on the planner's tree). The note, the Field Name caption and the list
stood at 13-15: at the content edge."""


def layout(h):
    for name in ("label1", "label2", "listFields"):
        h.set(name, 8, None)
