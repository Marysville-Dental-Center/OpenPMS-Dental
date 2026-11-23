"""FormEvaluationReport on the grid (UI-12 slice 4, layout script on the planner's tree). The planner put All Courses at the Instructors
grid's x (it was over the Courses grid on main) and kept the grids at main's 12 px margin with 26 px at the right: the three grids one
width, 8 px apart, from the content edge to the right content edge; All Courses, All Instructors and All Students each at the start of its
grid on the row over them."""


def layout(h):
    R = h.tree.w - 8
    grids = ("gridCourses", "gridInstructors", "gridStudents")
    over = ("checkAllCourses", "checkAllInstructors", "butAllStudents")
    w = (R - 8 - 16) // 3
    x = R - 3 * w - 16
    for g, o in zip(grids, over):
        h.set(g, x, None, w, None)
        if o.startswith("check"):
            h.check_left(o)
            h.set(o, x, None, h.cw(o), None)
        else:
            h.set(o, x, None)
        x += w + 8
