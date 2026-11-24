"""FormSigElementDefEdit on the grid (UI-12 slice 10, layout script on the planner's tree). The Type list (106) and the Text box (104) under it
ended 2 px apart, and Record New (105) 3 px short of Windows Recorder (108) over it: the list as wide as the box, Record New as wide as
Windows Recorder."""
import importlib.util
import os

_spec = importlib.util.spec_from_file_location("_s10", os.path.join(os.path.dirname(os.path.abspath(__file__)), "_s10.py"))
s = importlib.util.module_from_spec(_spec)
_spec.loader.exec_module(s)


def layout(h):
    h.set("listType", None, None, h.n("textSigText").nw, None)
    h.set("butRecordNew", None, None, h.n("butRecord").nw, None)
