"""FormBenefitEdit on the grid (UI-12 slice 5, layout script on the planner's tree). The planner put each Applies-to radio button's field
just after its text (95, 130 and 110 px into the window) and the Quantity box at 121: the three fields and the quantity box on the
window's field column (Type, Percent ...), the Qualifier label and box moved with the quantity box."""


def layout(h):
    fx = h.n("listBenefitType").nx
    g = h.n("groupBox1")
    for name in ("listCategory", "textProcCode", "comboCodeGroup"):
        h.set(name, fx - g.nx, None)
    # Teeth on the pitch under Treatment Area (the planner put it 4 px lower for its two-line hint, which starts on the row's top).
    ta = h.n("listTreatArea")
    y = ta.ny + ta.nh + 4
    for name in ("labelTeeth", "textTeeth", "labelTeethHint"):
        h.set(name, None, y)
    q = h.n("groupQuantity")
    dx = (fx - q.nx) - h.n("textQuantity").nx
    for name in ("textQuantity", "label8", "listQuantityQualifier"):
        h.set(name, h.n(name).nx + dx, None)
