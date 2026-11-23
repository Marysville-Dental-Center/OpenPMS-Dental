"""FormRepeatChargeEdit on the grid (UI-12 slice 5, layout script on the planner's tree). The Charge Amount row stood 29 px over Frequency
(the other rows 27): on the pitch over it (2 px down)."""


def layout(h):
    f = h.n("comboFrequencyTypes")
    y = f.ny - h.H - 4
    for name in ("labelChargeAmount", "textChargeAmt"):
        h.set(name, None, y)
