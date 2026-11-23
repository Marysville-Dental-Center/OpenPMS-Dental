"""FormEhrSetup on the grid (UI-12 slice 8, layout script on the planner's tree). The planner put the left button column at x 27 and the
buttons after the Code Systems group at 202 while the group's own buttons stand at 214, and the left column's first button 3 px higher than
the group's: the left column on the padding, the group 16 px after it (the band gap), the buttons after the group on the group's button
column, the left column's first button on the group's first button's row, all on the 28 px pitch; Emergency Now and its status swatch 16
px after the group on the first row. The window keeps main's size (a launcher without a bottom bar; it closes by its title bar)."""
import importlib.util
import os

_spec = importlib.util.spec_from_file_location("_s8", os.path.join(os.path.dirname(os.path.abspath(__file__)), "_s8.py"))
s = importlib.util.module_from_spec(_spec)
_spec.loader.exec_module(s)


def layout(h):
    g = h.n("groupCodeSystems")
    xg = 8 + 128 + 16
    h.set("groupCodeSystems", xg, None)
    inner = h.n("butCodeImport")
    y0 = g.ny + inner.ny
    y = y0
    for name in ("butAllergies", "butVaccineDef", "butDrugManufacturer", "butDrugUnit", "butReminderRules", "butInboundEmail",
                 "butEducationalResources", "butProviderKeys"):
        h.set(name, 8, y)
        y += 28
    xr = xg + inner.nx
    y = g.ny + g.nh + 8
    for name in ("butLoincs", "butEhrTriggers", "butTimeSynch", "butOIDs"):
        h.set(name, xr, y)
        y += 28
    xe = xg + g.nw + 16
    en = h.n("butEmergencyNow")
    h.set("butEmergencyNow", xe, y0)
    h.set("panelEmergencyNow", xe + en.nw + 6, y0)
