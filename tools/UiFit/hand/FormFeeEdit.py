"""FormFeeEdit on the grid (UI-12 slice 8, layout script on the planner's tree). The planner grew the window from main's 374 x 132 to
536 x 336 for three controls hidden in the designer that no code shows (the two "(not visible)" check boxes, whose lines in the
constructor are commented out, and textBox1, which the code never names): main's size, Save on the bottom and right padding, the two
check boxes on the field column under the Fee row (a list); textBox1 keeps main's place under the window's bottom edge
(grid-known/8.tsv: hidden-frame), as FormPopupDisplay's unused group in slice 6."""
import importlib.util
import os

_spec = importlib.util.spec_from_file_location("_s8", os.path.join(os.path.dirname(os.path.abspath(__file__)), "_s8.py"))
s = importlib.util.module_from_spec(_spec)
_spec.loader.exec_module(s)


MAIN_W, MAIN_H = 374, 132


def layout(h):
    s.size(h, MAIN_W, MAIN_H)
    s.bar(h, right=("butSave",))
    tf = h.n("textFee")
    y = tf.ny + tf.nh + 4
    for name in ("checkDefFee", "checkDefCov"):
        h.check_left(name)
        h.set(name, tf.nx, y)
        y += h.n(name).nh + 4
    h.set("textBox1", 16, MAIN_H)
