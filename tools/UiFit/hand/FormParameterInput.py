"""ODR's FormParameterInput on the grid (UI-12 slice 13, layout script on the planner's tree; its designer code is in FormParameterInput.cs,
read and edited in InitializeComponent's body). The code fills panelMain with a prompt and an input per report parameter and sets the
window's height to the panel's content plus the window's height outside the panel (Height-panelMain.Height). The planner kept the panel at
main's x 20, 44 px short of the right padding and 63 px above the bar. The panel from the padding to the right padding and down to the
section gap over OK and Cancel."""
import importlib.util
import os

_spec = importlib.util.spec_from_file_location("_s13", os.path.join(os.path.dirname(os.path.abspath(__file__)), "_s13.py"))
s = importlib.util.module_from_spec(_spec)
_spec.loader.exec_module(s)


def layout(h):
    top = s.bar(h, right=("butOK", "butCancel"))
    h.set("panelMain", 8, 8, s.W(h) - 16, top - s.SECTION - 8)
