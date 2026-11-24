#!/usr/bin/env python3
"""UI-12T: tests of the rollout tooling (docs/port-notes/UI-12.md, "Tooling (UI-12T)"). Plain unittest, no packages:

    python -m unittest discover -s tools/UiFit -p "test_*.py"

The per-slice data split (split_grid_data.py, slice_data.py), the grid's numbers (grid-numbers.tsv), the batch plan (plan_batches.py) and the
window-family templates (grid_templates.py).
"""
import os
import shutil
import sys
import tempfile
import unittest

sys.path.insert(0, os.path.dirname(os.path.abspath(__file__)))
import fit_tools  # noqa: E402
import grid_templates  # noqa: E402
import grid_tools  # noqa: E402
import plan_batches  # noqa: E402
import slice_data as sd  # noqa: E402
import split_grid_data  # noqa: E402

ROOT = fit_tools.repo_root()


class SplitTests(unittest.TestCase):
    def make_root(self):
        tmp = tempfile.mkdtemp(prefix="ui12t-")
        self.addCleanup(shutil.rmtree, tmp, True)
        src = sd.data_dir(ROOT)
        dst = sd.data_dir(tmp)
        os.makedirs(dst)
        for name in sd.FOLDERS:
            shutil.copytree(os.path.join(src, name), os.path.join(dst, name))
        shutil.copy(os.path.join(src, "fit-fixes.tsv"), dst)
        return tmp, dst

    def folder_texts(self, data):
        out = {}
        for name in sd.FOLDERS:
            for s, p in sd.slice_files(os.path.join(data, name)):
                with open(p, encoding="utf-8", newline="") as f:
                    out[name + "/" + s] = f.read().replace("\r\n", "\n")
        with open(os.path.join(data, "fit-fixes.tsv"), encoding="utf-8", newline="") as f:
            out["fit-fixes"] = f.read().replace("\r\n", "\n")
        return out

    def test_repository_is_split(self):
        r = split_grid_data.Run(ROOT, True)
        r.run()
        self.assertEqual([], r.changed, "run tools/UiFit/split_grid_data.py")

    def test_rerun_changes_nothing(self):
        tmp, data = self.make_root()
        before = self.folder_texts(data)
        r = split_grid_data.Run(tmp, False)
        r.run()
        self.assertEqual([], r.changed)
        self.assertEqual(before, self.folder_texts(data))

    def test_a_branch_of_single_files_splits_into_the_same_folders(self):
        """A slice branch made before the split (its single files, its marks in fit-fixes.tsv) merged in: the rerun gives the folders."""
        tmp, data = self.make_root()
        before = self.folder_texts(data)
        for name in ("grid-windows", "grid-hand", "grid-known", "grid-runtime-texts", "layout-snapshot"):
            comments, header, rows = sd.read_folder(os.path.join(data, name))
            lines = [ln for _, ln in rows]
            if name == "layout-snapshot":
                comments = comments[:3]
                held = set(ln.split("\t")[0] for s, ln in rows if s != sd.UNSLICED)
                lines = sorted((ln for s, ln in rows if s != sd.UNSLICED or ln.split("\t")[0] not in held), key=lambda x: x.split("\t")[0])
            sd.write_file(os.path.join(data, name + ".tsv"), comments, header, lines, "\r\n")
            shutil.rmtree(os.path.join(data, name))
        # The superseded marks back in the notes of fit-fixes.tsv.
        _, _, sup = sd.read_folder(os.path.join(data, "fit-fixes-superseded"))
        keys = set(ln for _, ln in sup)
        shutil.rmtree(os.path.join(data, "fit-fixes-superseded"))
        path = os.path.join(data, "fit-fixes.tsv")
        with open(path, encoding="utf-8", newline="") as f:
            text = f.read()
        nl = sd.line_end(text)
        lines = text.split(nl)
        for i, ln in enumerate(lines):
            if "\t".join(ln.split("\t")[:3]) in keys:
                lines[i] = ln + sd.SUPERSEDED_MARK
        with open(path, "w", encoding="utf-8", newline="") as f:
            f.write(nl.join(lines))
        split_grid_data.Run(tmp, False).run()
        self.assertEqual(before, self.folder_texts(data))
        for name in sd.FOLDERS:
            self.assertFalse(os.path.exists(os.path.join(data, name + ".tsv")), name)

    def test_unsliced_rows_of_an_owned_file_are_pruned(self):
        tmp, data = self.make_root()
        before = self.folder_texts(data)
        folder = os.path.join(data, "layout-snapshot")
        c, h, d, nl = sd.read_file(os.path.join(folder, "pilot.tsv"))
        c2, h2, d2, nl2 = sd.read_file(os.path.join(folder, "unsliced.tsv"))
        stale = [ln.replace("\t", "\t", 1) for ln in d[:3]]
        sd.write_file(os.path.join(folder, "unsliced.tsv"), c2, h2, sorted(d2 + stale, key=lambda x: x.split("\t")[0]), nl2)
        split_grid_data.Run(tmp, False).run()
        self.assertEqual(before, self.folder_texts(data))

    def test_no_window_in_two_files(self):
        for name, key in sd.FOLDERS.items():
            dup = sd.duplicates(os.path.join(sd.data_dir(ROOT), name), [key] if key else ["file", "control"])
            dup = [x for x in dup if sd.UNSLICED not in x[1]]
            self.assertEqual([], dup, name)

    def test_slice_order(self):
        self.assertEqual(["pilot", "1", "2", "10", "x", "unsliced"], sorted(["unsliced", "10", "2", "pilot", "x", "1"], key=sd.slice_key))

    def test_snapshot_owner_by_the_declared_class(self):
        # UI-12 slice 13: a designer file named otherwise than its class (FormCvxs in FormCvx.designer.cs) belongs to the window's slice
        # without a hand row; the file name and the hand rows still count, and the first slice wins.
        win = {"N.FormCvxs": "11", "N.FormA": "2", "N.FormB": "1"}
        declared = {"P/FormCvx.designer.cs": ["formcvxs"], "P/FormA.Designer.cs": ["forma"], "P/Other.Designer.cs": ["formb", "formd"]}
        hand = [{"designer_file": "P/UserControlC.Designer.cs", "window": "N.FormB"}]
        owners = sd.snapshot_owners(win, hand, declared=lambda rel: declared.get(rel, []))
        self.assertEqual("11", sd.snapshot_owner("P/FormCvx.designer.cs", owners))
        self.assertEqual("2", sd.snapshot_owner("P/FormA.Designer.cs", owners))
        self.assertEqual("1", sd.snapshot_owner("P/Other.Designer.cs", owners))
        self.assertEqual("1", sd.snapshot_owner("P/UserControlC.Designer.cs", owners))
        self.assertEqual(sd.UNSLICED, sd.snapshot_owner("P/FormD.Designer.cs", owners))

    def test_declared_classes_reads_the_file(self):
        tmp = tempfile.mkdtemp(prefix="ui12-13-")
        self.addCleanup(shutil.rmtree, tmp, True)
        path = os.path.join(tmp, "FormCvx.designer.cs")
        with open(path, "wb") as f:
            f.write(b"namespace N {\r\n\tpartial class FormCvxs {\r\n\t\tprivate void InitializeComponent() { }\r\n\t}\r\n}\r\n")
        self.assertEqual(["formcvxs"], sd.declared_classes(path))
        self.assertEqual([], sd.declared_classes(os.path.join(tmp, "missing.cs")))
        owners = sd.snapshot_owners({"N.FormCvxs": "11"}, [], root=tmp)
        self.assertEqual("11", sd.snapshot_owner("FormCvx.designer.cs", owners))


class NumbersTests(unittest.TestCase):
    def test_grid_is_the_data_file(self):
        self.assertEqual(grid_tools.load_grid(), grid_tools.GRID)
        self.assertEqual(27, grid_tools.GRID["field_height"] + grid_tools.GRID["row_gap"])
        self.assertEqual(0.5, grid_tools.GRID["label_cap"])

    def test_code_constants_match(self):
        import re
        with open(os.path.join(ROOT, "CodeBase", "UI", "UiGrid.cs"), encoding="utf-8-sig") as f:
            consts = dict((k.lower(), float(v)) for k, v in re.findall(r"public const (?:int|double) (\w+)=([\d.]+);", f.read()))
        self.assertEqual(dict((k, float(v)) for k, v in grid_tools.GRID.items()), consts)

    def test_a_wrong_pitch_is_refused(self):
        tmp = tempfile.mkdtemp(prefix="ui12t-")
        self.addCleanup(shutil.rmtree, tmp, True)
        p = os.path.join(tmp, "n.tsv")
        with open(grid_tools.GRID_NUMBERS, encoding="utf-8") as f:
            text = f.read().replace("pitch\t27\t", "pitch\t29\t")
        with open(p, "w", encoding="utf-8") as f:
            f.write(text)
        with self.assertRaises(ValueError):
            grid_tools.load_grid(p)


class BatchTests(unittest.TestCase):
    @classmethod
    def setUpClass(cls):
        cls.batches, cls.rank, cls.controls, cls.group_of = plan_batches.plan(ROOT)

    def test_no_file_in_two_batches(self):
        self.assertEqual([], plan_batches.check_disjoint(ROOT, self.batches))

    def test_no_laid_out_window(self):
        _, _, laid = plan_batches.load(ROOT)
        files = [f for _, _, fs in self.batches for f in fs]
        self.assertEqual(set(), laid & set(files))
        self.assertTrue(any(f.endswith("FormPatientEdit.Designer.cs") for f in laid))
        self.assertEqual(len(files), len(set(files)))

    def test_sizes(self):
        for n, kind, files in self.batches:
            groups = set(self.group_of[f] for f in files)
            for g in groups:
                members = [f for f in files if self.group_of[f] == g]
                total = sum(self.controls[f] for f in members)
                self.assertEqual(kind, "small" if total <= 20 else "large", g)
            # A batch closes when it reaches its size; the last group may pass it.
            size = 120 if kind == "small" else 50
            last = self.group_of[files[-1]]
            self.assertLess(len([f for f in files if self.group_of[f] != last]), size, n)

    def test_a_window_and_its_user_controls_together(self):
        where = dict((f, n) for n, _, fs in self.batches for f in fs)
        for f, g in self.group_of.items():
            self.assertEqual(where[g], where[f], f)

    def test_the_committed_plan_is_current(self):
        rows = sd.read_dicts(os.path.join(ROOT, "docs", "port-notes", "UI-12-batches.tsv"))
        self.assertEqual([f for _, _, fs in self.batches for f in fs], [r["designer_file"] for r in rows],
                         "run python tools/UiFit/plan_batches.py")


def node(name, type_, x, y, w, h, parent=None, text_w=0, anchor=5):
    """A control tree node as GridDump writes it (the columns the templates read)."""
    r = dict((c, "0") for c in ("index", "client_x", "client_y", "client_width", "client_height", "auto_size", "sizes_itself",
                                "layout_panel_child", "multiline", "font_height", "text_width", "text_height", "title_bottom", "preferred_height"))
    r.update({"window": "N.FormX", "path": name, "name": name, "type": type_, "x": str(x), "y": str(y), "width": str(w), "height": str(h),
              "visible": "1", "dock": "None", "anchor": str(anchor), "check_align": "", "text_align": "", "text": name, "text_width": str(text_w)})
    if parent is None:
        r.update({"client_width": str(w), "client_height": str(h)})
    n = grid_tools.Node(r)
    n.editable = True
    if parent is not None:
        n.parent = parent
        parent.children.append(n)
    return n


class TemplateTests(unittest.TestCase):
    def test_detects_a_list_window(self):
        root = node("", "N.FormX", 0, 0, 600, 400)
        node("gridMain", "OpenPmsDental.UI.GridOD", 8, 8, 480, 340, root, anchor=15)
        node("butAdd", "OpenPmsDental.UI.Button", 500, 8, 75, 24, root)
        node("butClose", "OpenPmsDental.UI.Button", 500, 360, 75, 24, root)
        self.assertEqual("list", grid_templates.detect(root))

    def test_detects_a_report_filter(self):
        root = node("", "N.FormRpX", 0, 0, 400, 300)
        node("datePickerFrom", "OpenPmsDental.UI.ODDatePicker", 8, 8, 227, 23, root)
        node("comboClinic", "OpenPmsDental.UI.ComboBoxClinicPicker", 8, 40, 200, 21, root)
        node("checkDetail", "OpenPmsDental.UI.CheckBox", 8, 70, 120, 18, root)
        node("butOK", "OpenPmsDental.UI.Button", 300, 260, 75, 24, root)
        self.assertEqual("report", grid_templates.detect(root))

    def test_detects_a_simple_edit_dialog_and_not_a_module(self):
        root = node("", "N.FormXEdit", 0, 0, 400, 200)
        node("label1", "System.Windows.Forms.Label", 8, 8, 100, 18, root)
        node("textName", "System.Windows.Forms.TextBox", 114, 8, 200, 20, root)
        node("butOK", "OpenPmsDental.UI.Button", 300, 160, 75, 24, root)
        self.assertEqual("edit", grid_templates.detect(root))
        node("toothChart", "SparksToothChart.ToothChartWrapper", 8, 40, 200, 100, root)
        self.assertIsNone(grid_templates.detect(root))

    def test_untangle_parts_a_label_from_its_field(self):
        root = node("", "N.FormXEdit", 0, 0, 400, 200)
        lb = node("label1", "System.Windows.Forms.Label", 8, 8, 120, 20, root, text_w=60)
        lb.text_align = "MiddleRight"  # its text drawn at its right end, under the field
        node("textName", "System.Windows.Forms.TextBox", 110, 8, 200, 20, root)
        self.assertEqual(["label1"], grid_templates.untangle(root, "edit"))
        self.assertEqual(110 - grid_tools.GRID["label_gap"] - 8, lb.w)

    def test_untangle_moves_a_radio_button_drawn_into_the_one_above(self):
        root = node("", "N.FormXEdit", 0, 0, 400, 200)
        node("radioA", "System.Windows.Forms.RadioButton", 8, 8, 120, 20, root, text_w=60)
        b = node("radioB", "System.Windows.Forms.RadioButton", 8, 24, 120, 20, root, text_w=60)
        self.assertEqual(["radioB"], grid_templates.untangle(root, "edit"))
        self.assertEqual(28, b.y)

    def test_settled_ignores_the_informational_rows(self):
        w = "N.FormX"
        self.assertTrue(grid_templates.settled([(w, ".", "the code sizes the window at run time (check ...)"), (w, ".", "template list: ...")]))
        self.assertFalse(grid_templates.settled([(w, ".", "overlapping items kept as drawn: a, b")]))


class BoxLeftTextAlignTests(unittest.TestCase):
    """UI-12 slice 12 fix round: a plain window's stock check box or radio button turned box-left takes its text to the left as well."""

    def node(self, type_, text_align):
        class N:
            pass
        n = N()
        n.type, n.text_align = type_, text_align
        return n

    def setUp(self):
        self.addCleanup(grid_tools.PLAIN_WINDOW.__setitem__, 0, grid_tools.PLAIN_WINDOW[0])

    def test_plain_window_stock_boxes_align_left(self):
        grid_tools.PLAIN_WINDOW[0] = True
        self.assertEqual("MiddleLeft", grid_tools.box_left_text_align(self.node("System.Windows.Forms.CheckBox", "MiddleRight")))
        self.assertEqual("TopLeft", grid_tools.box_left_text_align(self.node("System.Windows.Forms.RadioButton", "TopRight")))
        self.assertIsNone(grid_tools.box_left_text_align(self.node("System.Windows.Forms.CheckBox", "MiddleLeft")))
        self.assertIsNone(grid_tools.box_left_text_align(self.node("OpenPmsDental.UI.CheckBox", "MiddleRight")))

    def test_program_windows_unchanged(self):
        grid_tools.PLAIN_WINDOW[0] = False
        self.assertIsNone(grid_tools.box_left_text_align(self.node("System.Windows.Forms.CheckBox", "MiddleRight")))


class AnchorTests(unittest.TestCase):
    """UI-12 slice 4 fix round: the designer's anchors (the tree has LayoutManager's reset Top|Left) and the bottom-anchor check."""
    DESIGNER = (
        "\t\t\tthis.gridMain = new OpenPmsDental.UI.GridOD();\r\n"
        "\t\t\tthis.butAdd = new OpenPmsDental.UI.Button();\r\n"
        "\t\t\tthis.butDelete = new OpenPmsDental.UI.Button();\r\n"
        "\t\t\tthis.butClose = new OpenPmsDental.UI.Button();\r\n"
        "\t\t\tthis.gridMain.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) \r\n"
        "            | System.Windows.Forms.AnchorStyles.Left) \r\n"
        "            | System.Windows.Forms.AnchorStyles.Right)));\r\n"
        "\t\t\tthis.butAdd.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));\r\n"
        "\t\t\tthis.butDelete.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));\r\n"
        "\t\t\tthis.butClose.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));\r\n")

    def designer(self):
        tmp = tempfile.mkdtemp(prefix="ui12-anchor-")
        self.addCleanup(shutil.rmtree, tmp, True)
        with open(os.path.join(tmp, "FormX.designer.cs"), "wb") as f:
            f.write(self.DESIGNER.encode("latin-1"))
        return grid_tools.Designer(tmp, "FormX.designer.cs")

    def test_reads_the_designer_anchor(self):
        d = self.designer()
        self.assertEqual(15, d.anchor("gridMain"))
        self.assertEqual(10, d.anchor("butAdd"))
        self.assertEqual(5, d.anchor("labelNone"))

    def test_flags_a_bottom_anchored_button_at_the_top(self):
        d = self.designer()
        root = node("", "N.FormX", 0, 0, 600, 400)
        node("gridMain", "OpenPmsDental.UI.GridOD", 8, 8, 480, 100, root)
        node("butAdd", "OpenPmsDental.UI.Button", 500, 8, 75, 24, root)        # bottom-anchored at the top: flagged
        node("butDelete", "OpenPmsDental.UI.Button", 400, 112, 75, 24, root)   # under the grid's stretching bottom edge: not flagged
        node("butClose", "OpenPmsDental.UI.Button", 500, 368, 75, 24, root)    # in the bottom bar: not flagged
        self.assertEqual(["butAdd"], [r[1] for r in grid_tools.bottom_anchored_high(root, d)])


class CodeFileTests(unittest.TestCase):
    """UI-12 slice 13: windows whose designer code is in their own .cs file (the report designer's dialogs, ODR's controls): the planner
    reads InitializeComponent's body there and the applier edits only that body, byte for byte."""
    CODE = (
        "﻿/* licence header: { not code } */\r\n"
        "namespace N {\r\n"
        "\tinternal class DialogX : System.Windows.Forms.Form {\r\n"
        "\t\tprivate System.Windows.Forms.Label label1;\r\n"
        "\t\tpublic DialogX() {\r\n"
        "\t\t\tInitializeComponent();\r\n"
        "\t\t\tthis.label1.Location = new System.Drawing.Point(3, 3);// code after InitializeComponent: not designer code\r\n"
        "\t\t\tthis.Size = new System.Drawing.Size(300, 200);\r\n"
        "\t\t}\r\n"
        "\t\tprivate void InitializeComponent() {\r\n"
        "\t\t\tthis.label1 = new System.Windows.Forms.Label();\r\n"
        "\t\t\t// a comment with a brace }\r\n"
        "\t\t\tthis.label1.Location = new System.Drawing.Point(16, 16);\r\n"
        "\t\t\tthis.label1.Name = \"label1\";\r\n"
        "\t\t\tthis.label1.Size = new System.Drawing.Size(48, 23);\r\n"
        "\t\t\tthis.label1.Text = \"Braces { in a string }\";\r\n"
        "\t\t\tthis.ClientSize = new System.Drawing.Size(456, 342);\r\n"
        "\t\t\tthis.Controls.Add(this.label1);\r\n"
        "\t\t}\r\n"
        "\t\tprivate void Other() { this.label1.Location = new System.Drawing.Point(1, 1); }\r\n"
        "\t}\r\n"
        "}\n")

    def write(self):
        tmp = tempfile.mkdtemp(prefix="ui12-13-code-")
        self.addCleanup(shutil.rmtree, tmp, True)
        with open(os.path.join(tmp, "DialogX.cs"), "wb") as f:
            f.write(self.CODE.encode("utf-8"))
        return tmp

    def test_the_body_of_initialize_component(self):
        start, end = fit_tools.initialize_component_span(self.CODE)
        body = self.CODE[start:end]
        self.assertIn("Point(16, 16)", body)
        self.assertIn("ClientSize", body)
        self.assertNotIn("Other", body)
        self.assertNotIn("Point(3, 3)", body)
        self.assertEqual((0, len("x")), fit_tools.designer_code("a/FormX.Designer.cs", "x"))
        self.assertIsNone(fit_tools.initialize_component_span("class N { }"))

    def test_the_designer_reads_only_initialize_component(self):
        tmp = self.write()
        d = grid_tools.Designer(tmp, "DialogX.cs")
        self.assertEqual((16, 16), d.point("label1"))
        self.assertEqual((456, 342), d.size(".", "ClientSize"))
        self.assertEqual("System.Windows.Forms.Label", d.declared["label1"])

    def test_the_applier_edits_only_the_body(self):
        tmp = self.write()
        plan = os.path.join(tmp, "plan.tsv")
        with open(plan, "w", encoding="utf-8", newline="\n") as f:
            f.write("designer_file\twindow\tcontrol\tproperty\told\tnew\trule\n")
            f.write("DialogX.cs\tN.DialogX\tlabel1\tLocation\t16,16\t8,12\tt\n")
            f.write("DialogX.cs\tN.DialogX\tlabel1\tSize\t48,23\t52,23\tt\n")
            f.write("DialogX.cs\tN.DialogX\t.\tClientSize\t456,342\t460,350\tt\n")
            f.write("DialogX.cs\tN.DialogX\tlabel1\tTextAlign\t\tMiddleRight\tt\n")
        old_root = fit_tools.repo_root
        fit_tools.repo_root = lambda: tmp
        try:
            args = type("A", (), {"plan": plan})()
            grid_tools.cmd_apply(args)
            with open(os.path.join(tmp, "DialogX.cs"), "rb") as f:
                after = f.read()
            grid_tools.cmd_apply(args)  # a second run changes nothing
            with open(os.path.join(tmp, "DialogX.cs"), "rb") as f:
                self.assertEqual(after, f.read())
        finally:
            fit_tools.repo_root = old_root
        expected = (self.CODE.replace("Point(16, 16)", "Point(8, 12)").replace("Size(48, 23)", "Size(52, 23)")
                    .replace("Size(456, 342)", "Size(460, 350)")
                    .replace("\t\t\tthis.label1.Name = \"label1\";\r\n",
                             "\t\t\tthis.label1.TextAlign = System.Drawing.ContentAlignment.MiddleRight;\r\n\t\t\tthis.label1.Name = \"label1\";\r\n"))
        self.assertEqual(expected.encode("utf-8"), after)

    def test_the_applier_writes_integral_height(self):
        """UI-12 slice 15 fix round: a list box's IntegralHeight false is written before its Name line, a second run changes nothing, and
        true (the default) writes no line."""
        text = "\t\t\tthis.lb.Location = new System.Drawing.Point(8, 8);\r\n\t\t\tthis.lb.Name = \"lb\";\r\n"
        once, err = grid_tools.apply_row(text, "lb", "IntegralHeight", "false")
        self.assertIsNone(err)
        self.assertEqual(text.replace("\t\t\tthis.lb.Name", "\t\t\tthis.lb.IntegralHeight = false;\r\n\t\t\tthis.lb.Name"), once)
        self.assertEqual((once, None), grid_tools.apply_row(once, "lb", "IntegralHeight", "false"))
        self.assertEqual((text, None), grid_tools.apply_row(text, "lb", "IntegralHeight", "true"))

    def test_runtime_code_leaves_out_initialize_component(self):
        code = fit_tools.runtime_code("DialogX.cs", self.CODE)
        self.assertIn("Point(3, 3)", code)
        self.assertIn("Point(1, 1)", code)
        self.assertNotIn("Point(16, 16)", code)

    def test_a_plain_form_is_laid_out_in_its_client_area(self):
        # UI-12 slice 13 (the plain-Form rule): a plain Form's root is the window with its frame; a UserControl's root is its client area.
        cols = ["window", "path", "name", "type", "index", "x", "y", "width", "height", "client_x", "client_y", "client_width", "client_height",
                "visible", "dock", "anchor", "auto_size", "sizes_itself", "layout_panel_child", "check_align", "text_align", "multiline",
                "font_height", "text_width", "text_height", "title_bottom", "preferred_height", "text"]
        rows = [("N.DialogX", ".", "DialogX", "N.DialogX", 0, 0, 0, 482, 343, 0, 0, 466, 304),
                ("N.CtlY", ".", "CtlY", "N.CtlY", 0, 0, 0, 472, 288, 0, 0, 472, 288)]
        tmp = tempfile.mkdtemp(prefix="ui12-13-tree-")
        self.addCleanup(shutil.rmtree, tmp, True)
        path = os.path.join(tmp, "tree.tsv")
        with open(path, "w", encoding="utf-8", newline="\n") as f:
            f.write("\t".join(cols) + "\n")
            for r in rows:
                f.write("\t".join(str(x) for x in r) + "\t1\tNone\t5\t0\t0\t0\t\t\t0\t16\t0\t0\t0\t0\t\n")
        trees = grid_tools.load_tree(path)
        self.assertEqual((466, 304), (trees["N.DialogX"].nw, trees["N.DialogX"].nh))
        self.assertEqual((472, 288), (trees["N.CtlY"].nw, trees["N.CtlY"].nh))

    def test_a_report_designer_window_finds_its_code_file(self):
        wf = fit_tools.window_files(ROOT, ["fyiReporting.RdlDesign.DialogNewTable", "ODR.FormParameterInput", "OpenPmsDental.FormCvxs"])
        self.assertEqual(["RdlDesign/DialogNewTable.cs"], wf["fyiReporting.RdlDesign.DialogNewTable"])
        self.assertEqual(["ODR/FormParameterInput.cs"], wf["ODR.FormParameterInput"])
        self.assertEqual(["OpenPmsDental/Forms/FormCvx.designer.cs"], wf["OpenPmsDental.FormCvxs"])


if __name__ == "__main__":
    unittest.main()
