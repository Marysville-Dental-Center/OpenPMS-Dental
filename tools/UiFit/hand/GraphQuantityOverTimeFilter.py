"""GraphQuantityOverTimeFilter on the grid (UI-12 slice 12, layout script on the planner's tree). The grouping options control stood 5 px
in, 112 x 60 over its own 108 x 87 (its group box was cut): at the panel's corner at its own size, the options panel as wide as it and the
settings panel as tall (the code sets that height again from the options control's height when it shows a graph). The graph under it is
GraphQuantityOverTime, laid out by its own script."""
import importlib.util
import os

_spec = importlib.util.spec_from_file_location("_s12", os.path.join(os.path.dirname(os.path.abspath(__file__)), "_s12.py"))
s = importlib.util.module_from_spec(_spec)
_spec.loader.exec_module(s)


def layout(h):
    h.set("groupingOptionsCtrl1", 0, 0, 108, 87)
    h.splitter("splitContainerOptions", 108)
    h.splitter("splitContainer", 87)
    h.set("splitContainerOptions", None, None, None, 87)  # docked to fill the first panel: its designer size is that panel
