"""FormPerio on the grid (UI-12 slice 5, layout script on the planner's tree). The F, L, L and F labels beside the perio chart mark its
four facial and lingual bands (main: 94, 215, 408 and 542 on the chart's rows); the planner stacked them 25 px apart at the chart's top.
Each back on its band (the chart is owner-drawn and keeps its place; its top did not move).

The probing keypad (fix round 2): the planner left the 8 and 9 keys 2 and 4 px right of the keys under them, 6 px gaps in the top row and
4 px in the others, and the rows 36, 36 and 39 px apart. Main's one 3 x 4 keypad back on one grid: columns at 1005, 1041 and 1077 in every
row (32 px keys, 4 px gaps), 0 across the first two columns (68 px, to 1073) and 10 in the third, the rows on one 36 px pitch (137, 173,
209, 245). The voice controls beside it follow its rows, 6 px after its right edge: the microphone (Listen) and its Listening note on the
1 2 3 row, Launch Bola Voice (39 px tall) on the 0 / 10 row, 8 px over Calc Index % as before."""

BANDS = (("label2", 94), ("label5", 215), ("label4", 408), ("label3", 542))

COLS = (1005, 1041, 1077)
ROWS = (137, 173, 209, 245)
KEY = 32
KEYPAD = (("but7", "but8", "but9"), ("but4", "but5", "but6"), ("but1", "but2", "but3"))


def layout(h):
    for name, y in BANDS:
        h.set(name, None, y)
    for r, names in enumerate(KEYPAD):
        for c, name in enumerate(names):
            h.set(name, COLS[c], ROWS[r], KEY, KEY)
    h.set("but0", COLS[0], ROWS[3], COLS[2] - 4 - COLS[0], KEY)
    h.set("but10", COLS[2], ROWS[3], KEY, KEY)
    x = COLS[2] + KEY + 6
    mic = h.n("butListen")
    h.set("butListen", x, ROWS[2] + (KEY - mic.nh) // 2)
    note = h.n("labelListening")
    h.set("labelListening", x + mic.nw + 6, ROWS[2] + (KEY - note.nh + 1) // 2)
    bola = h.n("butBolaLaunch")
    h.set("butBolaLaunch", x, h.n("butCalcIndex").ny - 8 - bola.nh)
