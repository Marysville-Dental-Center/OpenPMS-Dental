"""FormPharmacyEdit on the grid (UI-12 slice 2, layout script of grid_hand.py, on the planner's tree). In Canada Load gives the city label
"City, Province, Postal Code" (149 px; grid-runtime-texts.tsv), which the planner's 76 px label cut: the label column is as wide as that text
(Hand.lw: 156 px from the content edge), so every label and field of the window moves 10 px right; the bottom bar stays."""


def layout(h):
    lw = h.lw("label11")
    dx = 8 + lw + 6 - h.n("textStoreName").nx
    for k in h.tree.children:
        if k.name in ("butDelete", "butSave") or not k.name:
            continue
        h.set(k.name, k.nx + dx, None)
    h.set("label11", 8, None, lw, None)
