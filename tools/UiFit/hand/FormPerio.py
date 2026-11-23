"""FormPerio on the grid (UI-12 slice 5, layout script on the planner's tree). The F, L, L and F labels beside the perio chart mark its
four facial and lingual bands (main: 94, 215, 408 and 542 on the chart's rows); the planner stacked them 25 px apart at the chart's top.
Each back on its band (the chart is owner-drawn and keeps its place; its top did not move)."""

BANDS = (("label2", 94), ("label5", 215), ("label4", 408), ("label3", 542))


def layout(h):
    for name, y in BANDS:
        h.set(name, None, y)
