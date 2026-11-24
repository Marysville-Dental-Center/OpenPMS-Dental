"""FormQuickBooksOnlineSetup on the grid (UI-12 slice 11, layout script on the planner's tree). The Status row: its caption stood at the
padding (not on the label column) and the status text right-aligned 340 px away under the buttons' row. The caption right-aligned on the
label column; the status text (the code gives it up to a two-line sentence) at the field column, left-aligned on the caption's line, up to
6 px before Connect, three lines tall inside the group's padding; Connect and Disconnect stay at the row's right. The Relay Port box on its
caption's row. The three list rows 12 px apart, the six lists one width (fix round)."""
import importlib.util
import os

_spec = importlib.util.spec_from_file_location("_s11", os.path.join(os.path.dirname(os.path.abspath(__file__)), "_s11.py"))
s = importlib.util.module_from_spec(_spec)
_spec.loader.exec_module(s)


def layout(h):
    x = h.n("textClientId").nx
    c = h.n("butConnect")
    lw = h.lw("labelStatus")
    h.set("labelStatus", x - 6 - lw, c.ny, lw, h.H)
    v = h.n("labelStatusValue")
    h.text_align("labelStatusValue", "TopLeft")
    h.set("labelStatusValue", x, c.ny + 3, c.nx - 6 - x, 3 * 16 + 2)
    # the Relay Port box on its caption's row (it stood 5 px under it: a 9 px gap under Redirect URI)
    h.set("textRelayPort", None, h.n("labelRelayPort").ny)
    # Fix round: the three list rows (Deposit, Income, Classes) stood 4 px apart with 73 px empty under them: 12 px apart (the section gap),
    # each row's caption, note and arrow buttons moving with its lists. The six lists one width (the planner's 221 for the In OD lists; the
    # Available lists 230 on main), the arrows centred in the 8 px gaps between each pair; the Available notes keep their right edge.
    rows = (("listBoxDepositAccountsInOD", "listBoxDepositAccountsAvailable", "labelDepositAccountsInOD", "labelDepositAccountsAvailable",
             "butAddDepositAccount", "butRemoveDepositAccount"),
            ("listBoxIncomeAccountsInOD", "listBoxIncomeAccountsAvailable", "labelIncomeAccountsInOD", "labelIncomeAccountsAvailable",
             "butAddIncomeAccount", "butRemoveIncomeAccount"),
            ("listBoxClassRefsInOD", "listBoxClassRefsAvailable", "labelClassesInOD", "labelClassesAvailable", "butAddClass",
             "butRemoveClass"))
    y = h.n(rows[0][0]).ny
    for row in rows:
        dy = y - h.n(row[0]).ny
        for name in row:
            h.set(name, None, h.n(name).ny + dy)
        left, right, note = h.n(row[0]), h.n(row[1]), h.n(row[3])
        gap = note.nx - (right.nx + right.nw)
        note_right = note.nx + note.nw
        h.set(row[1], None, None, left.nw, None)
        x = right.nx + right.nw + gap
        h.set(row[3], x, None, note_right - x, None)
        y += left.nh + s.SECTION
