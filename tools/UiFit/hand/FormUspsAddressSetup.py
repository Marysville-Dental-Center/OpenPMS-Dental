"""FormUspsAddressSetup on the grid (UI-12 slice 9, layout script on the planner's tree): the Enabled check box stood on the field column
over the description, which starts at the padding, and the secret status (the code's longest text, grid-runtime-texts/9.tsv, two lines) had
a 30 px label: Enabled at the padding, the status as tall as its longest text under the secret's box, Environment 8 px under it, Test
connection and its result under that."""
import importlib.util
import os

_spec = importlib.util.spec_from_file_location("_s7", os.path.join(os.path.dirname(os.path.abspath(__file__)), "_s7.py"))
s = importlib.util.module_from_spec(_spec)
_spec.loader.exec_module(s)


def layout(h):
    s.move(h, "checkEnabled", x=8)
    h.check_left("checkEnabled")
    st = h.n("labelSecretStatus")
    nh = h.note_h("labelSecretStatus", st.nw)
    h.set("labelSecretStatus", None, None, None, nh)
    y = st.ny + nh + 8
    s.move(h, "comboEnvironment", y=y)
    s.move(h, "labelEnvironment", y=y)
    y += h.H + 4
    s.move(h, "butTest", y=y)
    s.move(h, "labelTestResult", x=8, y=y + h.n("butTest").nh + 4)
