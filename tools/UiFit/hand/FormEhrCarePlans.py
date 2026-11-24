"""FormEhrCarePlans on the grid (UI-12 slice 13, layout script on the planner's tree; the harness draws the grid's columns as FillCarePlans
sizes them, Scan/SampleData.cs). The planner kept the grid at main's x 12, 12 px short of the right padding (a list window it did not take
for one: no template, it had no unplaced row). The grid from the padding to the right padding, down to the section gap over Add."""
import importlib.util
import os

_spec = importlib.util.spec_from_file_location("_s13", os.path.join(os.path.dirname(os.path.abspath(__file__)), "_s13.py"))
s = importlib.util.module_from_spec(_spec)
_spec.loader.exec_module(s)


def layout(h):
    top = s.bar(h, left=("butAdd",))
    h.set("gridCarePlans", 8, 8, s.W(h) - 16, top - s.SECTION - 8)
