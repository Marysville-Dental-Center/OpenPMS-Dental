"""UI-11 (docs/port-notes/UI-11.md): the chrome colour literals of every window.

Lists every literal BackColor and ForeColor value the designers and the code of the program's projects set (a named colour, a system colour
or Color.FromArgb with numbers), with its file, control, the control's type and the value, and decides each one by role: the literal that
means chrome (a window, panel, group box, tab page, label, splitter or status strip in the classic grey or the old background) goes, so the
control takes its token from the looks; the rest stays, with the reason (content the office sets or the program shows as data, a deliberate
status colour, a system colour the looks read as the token's own value and high-contrast partner).

    python tools/UiFit/colour_literals.py --inventory <file>   writes every literal with its type and the rule's decision (a work file)
    python tools/UiFit/colour_literals.py                      writes the stays table of tests/UiRefreshTests/Data/colour-literals.tsv
    python tools/UiFit/colour_literals.py --check              compares with the committed table (exit 1 when it differs)
    python tools/UiFit/colour_literals.py --apply              removes the designer lines the rules map to a token (prints what it did)

NoHqTests' ChromeColourScanTests reads the same lines from the source and fails on a literal the table does not list (a new chrome
literal: use the token, or list it here with its reason) and on a row the source no longer has. No packages; deterministic (sorted).
"""
import argparse
import collections
import glob
import os
import re
import sys

PLAIN_ROOTS = ["CentralManager", "DatabaseIntegrityCheck", "DocumentationBuilder", "MySQLManager", "OpenPmsDentalDbSettings", "OpenPmsDentalGraph",
               "OpenPmsDentalHL7", "ServiceManager", "UpdateFileCopier", "xCrudGenerator"]
ROOTS = ["OpenPmsDental", "OpenPmsDentalBusiness", "CodeBase", "SparksToothChart", "WpfControlsOD", "RdlDesign", "ODR",
         "ImagingDeviceManager"] + PLAIN_ROOTS
LIST = "tests/UiRefreshTests/Data/colour-literals.tsv"
HEADER = "file\tcontrol\tproperty\tvalue\tcount\twhy"
MARK = "# UI-11: the chrome colour literals"

# A literal value: a named colour, a system colour, Color.FromArgb with numbers only (the designer's casts removed), Color.Empty.
RX_VALUE = r"(?:System\.Drawing\.)?(?:Color|SystemColors)\.(?:FromArgb\((?:[\s\d,()]|\(int\)|\(byte\))*\)|\w+)"
RX_DESIGNER = re.compile(r"^\s*this\.(?:(?P<ctl>[\w.]+)\.)?(?P<prop>BackColor|ForeColor) = (?P<val>" + RX_VALUE + r");\s*$")
RX_CODE = re.compile(r"(?P<lhs>[\w.\[\]\"]*?)\b(?P<prop>BackColor|ForeColor)\s*=(?!=)\s*(?P<val>" + RX_VALUE + r")(?=\s*[;,})])")
RX_FIELD = re.compile(r"^\s*(?:private|protected|internal|public)\s+(?:readonly\s+)?(?P<type>[\w.]+)\s+(?P<name>\w+)\s*;")
RX_NEW = re.compile(r"^\s*this\.(?P<name>\w+) = new (?P<type>[\w.]+)\(")


def repo_root():
    here = os.path.dirname(os.path.abspath(__file__))
    return os.path.normpath(os.path.join(here, "..", ".."))


def is_skipped(rel):
    parts = rel.split("/")
    return "bin" in parts or "obj" in parts


def read(path):
    with open(path, "rb") as f:
        return f.read().decode("utf-8-sig", errors="replace")


def normalise(value):
    """The value as the table writes it: no namespace, no casts, no spaces (Color.FromArgb(252,253,254), SystemColors.Control)."""
    v = value.replace("System.Drawing.", "")
    v = re.sub(r"\(\(int\)\(\(\(byte\)\((\d+)\)\)\)\)", r"\1", v)
    v = re.sub(r"\s+", "", v)
    return v


def strip_comment(line):
    """The line without a // comment (strings in these lines hold no //)."""
    i = line.find("//")
    return line if i < 0 else line[:i]


def is_designer(rel, text):
    return rel.lower().endswith(".designer.cs") or "private void InitializeComponent()" in text


def designer_types(lines):
    types = {}
    for line in lines:
        m = RX_FIELD.match(line) or RX_NEW.match(line)
        if m:
            t = m.group("type")
            if t.startswith("UI."):
                t = "OpenPmsDental." + t
            types.setdefault(m.group("name"), t)
    return types


def initialize_component_range(lines):
    """The line numbers (start, end) of InitializeComponent's body, or None."""
    for i, line in enumerate(lines):
        if "void InitializeComponent()" in line:
            depth = 0
            seen = False
            for j in range(i, len(lines)):
                depth += lines[j].count("{") - lines[j].count("}")
                if "{" in lines[j]:
                    seen = True
                if seen and depth <= 0:
                    return (i, j)
            return (i, len(lines) - 1)
    return None


def scan(root):
    """Every literal: dicts with file, line, control, type, property, value, kind (designer/code), window (the class)."""
    found = []
    for top in ROOTS:
        for path in sorted(glob.glob(os.path.join(root, top, "**", "*.cs"), recursive=True)):
            rel = os.path.relpath(path, root).replace(os.sep, "/")
            if is_skipped(rel):
                continue
            text = read(path)
            if "BackColor" not in text and "ForeColor" not in text:
                continue
            lines = text.split("\n")
            types = designer_types(lines)
            rng = initialize_component_range(lines) if is_designer(rel, text) else None
            for n, raw in enumerate(lines):
                line = strip_comment(raw)
                if rng and rng[0] <= n <= rng[1]:
                    m = RX_DESIGNER.match(line)
                    if m:
                        ctl = m.group("ctl") or "."
                        found.append(dict(file=rel, line=n + 1, control=ctl, type=types.get(ctl.split(".")[0], "?") if ctl != "." else "(window)",
                                          property=m.group("prop"), value=normalise(m.group("val")), kind="designer"))
                        continue
                for m in RX_CODE.finditer(line):
                    lhs = m.group("lhs").rstrip(".")
                    if lhs.startswith("this."):
                        lhs = lhs[5:]
                    ctl = lhs if lhs else "(initializer)"
                    t = types.get(ctl.split(".")[0], "(code)")
                    found.append(dict(file=rel, line=n + 1, control=ctl, type=t, property=m.group("prop"), value=normalise(m.group("val")),
                                      kind="code"))
    return found


# ---- the rules ------------------------------------------------------------------------------------------------------------------------------
# The controls that show the window's background (chrome): their old background goes, so they take their parent's colour, which is the
# window's UiTheme.Surface (FormODBase, FormFrame and UiPlainWindow set it) or the token of the container they sit in.
CONTAINERS = {"(window)", "System.Windows.Forms.Panel", "OpenPmsDental.UI.PanelOD", "System.Windows.Forms.SplitContainer",
              "OpenPmsDental.UI.SplitContainer", "OpenPmsDental.UI.SplitterPanel", "System.Windows.Forms.GroupBox", "OpenPmsDental.UI.GroupBox",
              "System.Windows.Forms.TabPage", "OpenPmsDental.UI.TabPage", "System.Windows.Forms.TabControl", "OpenPmsDental.UI.TabControl",
              "System.Windows.Forms.Label", "System.Windows.Forms.CheckBox", "OpenPmsDental.UI.CheckBox", "System.Windows.Forms.RadioButton",
              "System.Windows.Forms.FlowLayoutPanel", "System.Windows.Forms.TableLayoutPanel", "OpenPmsDental.User_Controls.UserControlReportSetup",
              "OpenPmsDental.EmailPreviewControl", "OpenPmsDental.UI.ODDatePicker"}
# The old window backgrounds: Windows' classic grey and its light shades, white (Open Dental's module and setup pages), the old look's
# 252,253,254 (ColorOD.Background before UI-3) and its near-whites.
OLD_BACKGROUNDS = {"SystemColors.Control", "SystemColors.ControlLight", "SystemColors.ControlLightLight", "SystemColors.Window", "Color.White",
                   "Color.WhiteSmoke", "Color.FromArgb(252,253,254)", "Color.FromArgb(249,249,249)", "Color.FromArgb(250,250,250)"}
# The default text colours: Text's place (the control takes its parent's text colour, Windows' ControlText, as it does without the line).
DEFAULT_TEXT = {"SystemColors.ControlText", "Color.Black"}
INPUTS = {"System.Windows.Forms.TextBox", "TextBox", "OpenPmsDental.ODtextBox", "System.Windows.Forms.RichTextBox", "OpenPmsDental.ValidDouble",
          "OpenPmsDental.ValidNum", "OpenPmsDental.ValidDate", "OpenPmsDental.UI.ComboBox", "System.Windows.Forms.ComboBox", "OpenPmsDental.ODcodeBox",
          "OpenPmsDental.ValidPhone", "OpenPmsDental.ValidDouble", "System.Windows.Forms.MaskedTextBox"}
STATUS_TEXT = {"Color.Red", "Color.DarkRed", "Color.Firebrick", "Color.Maroon", "Color.FromArgb(192,0,0)", "Color.MidnightBlue", "Color.Blue",
               "Color.ForestGreen", "Color.LimeGreen", "Color.DarkGreen", "Color.FromArgb(24,117,133)", "Color.Green"}
# The content controls: what they show is data or a picture, in their own colours (section 1: the tooth chart, the perio chart, signatures,
# sheets, the time bars, the charts, printouts, images).
CONTENT_TYPES = {
    "OpenPmsDental.UserControlScreenTooth": "the screening tooth: content (the tooth chart's drawing keeps Open Dental's look, section 1)",
    "OpenPmsDental.UI.SignatureBoxWrapper": "the signature area: content (the signature as drawn and stored)",
    "OpenPmsDental.TableTimeBar": "the appointment time bar: content (the time pattern's colours)",
    "OpenPmsDental.ToothChartTextBelowStrip": "the tooth chart's text strip: its own look (UQ-6; UI-13's inventory)",
    "OpenPmsDental.ContrPerio": "the perio chart: content (UQ-6, Open Dental's look)",
    "OpenPmsDental.ContrPanelTable": "a drawn table: content",
    "OpenPmsDental.ContrTable": "a drawn table: content",
    "CodeBase.PrintPanel": "a print page: content (printouts keep their look, UQ-5)",
    "OpenPmsDental.ControlImageDock": "the imaging dock's picture area: content",
    "OpenPmsDental.UI.ControlDoubleBuffered": "the sheet's page: content (sheets keep their look, section 1)",
    "System.Windows.Forms.PictureBox": "a picture area: content",
    "System.Windows.Forms.DataVisualization.Charting.Chart": "a chart: content (section 1: reports and graphs)",
    "OpenPmsDentalGraph.GraphQuantityOverTime": "a graph: content (section 1: reports and graphs)",
    "OpenPmsDentalGraph.DashboardCellCtrl": "the graph dashboard's cell: UiPlainWindow gives the classic grey the window background token (IsOldBackground)",
    "OpenPmsDental.UI.ToolBarOD": "the toolbar draws its own SurfaceSubtle strip (UI-6); its designer colour is not used",
    "System.Windows.Forms.DataGridView": "a data grid's cell style: UiLooks.SetDataGridViewLook reads Windows' defaults as the tokens",
    "System.Windows.Forms.DataGridViewCellStyle": "a data grid's cell style: UiLooks.SetDataGridViewLook reads Windows' defaults as the tokens",
    "OpenPmsDental.PanelScrollManual": "the procedure buttons' scroll area: its own look",
}
# Hand decisions: (file, control, property, value) -> why it stays. A row the rules would map but that means something else.
HAND = {
    ("OpenPmsDental/Forms Reports/FormRpLaserLabels.designer.cs", "panLabels", "BackColor", "SystemColors.ControlLightLight"):
        "the label sheet's preview: content (the labels' places on white paper)",
}
# Files whose literals are not chrome as a whole.
FILE_REASONS = [
    ("OpenPmsDental/UI/BordersDpi/FormODBase.cs", "FormODBase's own designer method, never called: the constructor sets UiTheme.Surface"),
    ("WpfControlsOD/Frame/FormFrame.cs", "FormFrame's own designer method, never called: the constructor sets UiTheme.Surface"),
    ("OpenPmsDental/UI/ToolTipOD", "the tooltip's own look (UI-4a): SurfaceRaised's value"),
    ("OpenPmsDental/UI/ComboBoxMulti.cs", "an input's box: SystemColors.Window, SurfaceRaised's value and its high-contrast partner"),
    ("OpenPmsDental/Tables Old Grids/", "a drawn table: content"),
    ("CodeBase/Controls/PrintPanel", "a print page: content (printouts keep their look, UQ-5)"),
    ("OpenPmsDental/SheetFramework/UserControlScreenTooth", "the screening tooth: content (the tooth chart's drawing keeps Open Dental's look, section 1)"),
    ("OpenPmsDental/UI/ClinicSelector/FormComboPicker", "the combo box's drop-down list: SystemColors.Window, SurfaceRaised's value and its high-contrast partner"),
    ("OpenPmsDental/Forms/FormSplash", "the splash screen: the brand picture's colours"),
    ("OpenPmsDental/Forms/FormPopupFade", "the popup's own look (a notice that fades)"),
    ("RdlDesign/", "the report designer: report content (the report canvas, its styles and the expression editor's colours), section 7's UI-11 row"),
    ("ODR/", "the report library: report content"),
    ("OpenPmsDentalBusiness/", "grid and report content drawn by the business layer"),
    ("SparksToothChart/", "the tooth chart: content (section 1)"),
    ("OpenPmsDentalGraph/", "the graph library: chart content; UiPlainWindow gives the classic grey the window background token"),
    ("OpenPmsDental/Reports/", "a report: content"),
    ("OpenPmsDental/ReportingForRdl/Allocators/", "the allocator windows' own colours (their designer colours: UiLooks.IsOwnColours)"),
]


def file_reason(rel):
    for prefix, why in FILE_REASONS:
        if rel.startswith(prefix):
            return why
    return None


def decide(r, parents_stay):
    """'map' (the line goes, the control takes the token from its parent and the looks) or the reason it stays."""
    key = (r["file"], r["control"], r["property"], r["value"])
    if key in HAND:
        return HAND[key]
    t, prop, value = r["type"], r["property"], r["value"]
    why = file_reason(r["file"])
    if why:
        return why
    if value in ("Color.Transparent", "Color.Empty"):
        return "transparent: shows the colour behind it (its parent's token)"
    if "CellStyle" in r["control"] or "cellStyle" in r["control"]:
        return CONTENT_TYPES["System.Windows.Forms.DataGridViewCellStyle"]
    if r["kind"] == "code":
        return decide_code(r)
    if t in CONTENT_TYPES:
        return CONTENT_TYPES[t]
    if t in CONTAINERS:
        if r["control"] in parents_stay:
            return "on a coloured parent that stays"
        if prop == "BackColor" and value in OLD_BACKGROUNDS:
            return "map"
        if prop == "ForeColor" and value in DEFAULT_TEXT:
            return "map"
        if prop == "ForeColor" and value in STATUS_TEXT:
            return "a deliberate status colour: a warning, an error or a note in colour"
        if value in ("Color.LightGray",):
            return "a grey block label: UiBlockLook draws it in the program's button look"
        if value in ("SystemColors.ActiveCaption",):
            return "a grid-title label: UiCaptionLook draws it in Accent"
        if value in ("SystemColors.ActiveCaptionText",) or (prop == "ForeColor" and value == "SystemColors.Window"):
            return "text on a caption or a dark block: UiCaptionLook and UiBlockLook read it as TextOnAccent"
        if prop == "BackColor" and value in ("Color.Black", "SystemColors.ControlDark", "SystemColors.ControlText", "Color.FromArgb(64,64,64)"):
            return "a dark line or frame: UiSeparatorLook draws a thin one as one Stroke line, UiBorderLook a frame"
        return "a deliberate colour: a status, a legend or a highlight the window means"
    if t in INPUTS:
        if prop == "BackColor" and value in ("SystemColors.Window", "Color.White"):
            return "an input's box: SurfaceRaised's value (SystemColors.Window is also its high-contrast partner); the input frame draws the box in it"
        if prop == "BackColor" and value in ("SystemColors.Control", "SystemColors.ControlLight", "SystemColors.ControlLightLight"):
            return "a read-only or locked box's grey: the state, as Windows shows a read-only box"
        if prop == "BackColor" and value in OLD_BACKGROUNDS:
            return "a borderless caption box: UiLooks.SetCaptionBoxLook gives it its parent's colour"
        if prop == "ForeColor" and value in ("SystemColors.WindowText", "SystemColors.ControlText", "Color.Black"):
            return "an input's text: Windows' input text colour and its high-contrast partner"
        return "a deliberate status colour: a warning, an error or a note in colour"
    if t in ("System.Windows.Forms.Button", "OpenPmsDental.UI.Button"):
        if value in ("SystemColors.Control", "SystemColors.ControlText", "Color.Black"):
            return "the button's own face and text: UiButtonLook reads Windows' button colours as the program's button"
        return "a coloured button or swatch the window means: UiButtonLook draws its colour (IsSwatch, IsOwnColours)"
    return "?"


# The code's literals that stay, by file (the end of its path) and control (the expression before .BackColor or .ForeColor).
CODE_HAND = {
    ("FormApptEdit.cs", "butColor"): "a colour swatch: the office's colour (content)",
    ("FormApptTypeEdit.cs", "butColor"): "a colour swatch: the office's colour (content)",
    ("FormProcCodeEdit.cs", "butColor"): "a colour swatch: the office's colour (content)",
    ("FormProcApptColorEdit.cs", "panelColor"): "a colour swatch: the office's colour (content)",
    ("ControlChart.cs", "panelDrawColor"): "a colour swatch: the drawing colour (content)",
    ("FormApptsOther.cs", "listViewItem"): "a list item's state: the appointment's status in colour (data)",
    ("FormAutoNotes.cs", "_treeNodeGray"): "a tree item's state: the drag target's highlight and its reset",
    ("FormAutoNotes.cs", "treeNodeSelected"): "a tree item's state: the drag target's highlight and its reset",
    ("FormClaimEdit.cs", "contrPerioGrid"): "the perio grid: content (UQ-6)",
    ("ContrPerio.cs", "this"): "the perio chart: content (UQ-6)",
    ("FormFeeScheds.cs", "butSetOrder"): "a button's state: red while the order is unsaved, then Windows' button face (UiButtonLook reads it as the program's button)",
    ("FormSheetDefEdit.cs", "textBoxControlTreatNotes"): "a sheet field: content (sheets keep their look, section 1)",
    ("FormSheetFillEdit.cs", "textBox"): "a sheet field: content (sheets keep their look, section 1)",
    ("ControlImages.cs", "form"): "the full-screen picture's black: content",
    ("ControlVideoPlayer.cs", "(initializer)"): "the video's black: content",
    ("ControlCameraSession.cs", "(initializer)"): "the camera view's dark surface and its captions over the live picture: content (the imaging design)",
    ("ControlImageDisplay.cs", "labelHover"): "the caption over a picture: content",
    ("FormOpenDentalBShell.cs", "(initializer)"): "the module switch's white fade when no blurred picture is at hand (ModuleSwitchBlur)",
    ("FormProgressStatus.cs", "labelMinimize"): "the minimize label's hover state",
    ("UserControlSecurityTree.cs", "treePermissions"): "a read-only tree's grey: the state",
    ("FormDbSettings.cs", "(initializer)"): "a hint in Windows' grey text (GrayText, TextDisabled's high-contrast partner)",
    ("FormEligibilityResponseDisplay.cs", "dataGridView1.ColumnHeadersDefaultCellStyle"): "a data grid's own header and row colours (data)",
    ("FormEligibilityResponseDisplay.cs", "dataGridView1.AlternatingRowsDefaultCellStyle"): "a data grid's own header and row colours (data)",
}
INPUT_NAMES = re.compile(r"^(?:base|text\w*|textBox\w*|listControls\[i\])$")
SURFACE_BUTTONS = re.compile(r"^but(?:M|OI|L|V|BF|D|B|F|I|O)$")


def decide_code(r):
    prop, value, ctl = r["property"], r["value"], r["control"]
    name = os.path.basename(r["file"])
    if (name, ctl) in CODE_HAND:
        return CODE_HAND[(name, ctl)]
    if prop == "ForeColor" and value in STATUS_TEXT:
        return "a deliberate status colour: a warning, an error or a note in colour"
    if prop == "ForeColor" and value in DEFAULT_TEXT | {"SystemColors.WindowText"}:
        return "a status colour's reset to the default text colour (Windows' ControlText, which every label and box draws in)"
    if prop == "BackColor" and SURFACE_BUTTONS.match(ctl) and value in ("SystemColors.Control", "Color.White"):
        return "a surface button's state: white while its surface is chosen, else Windows' button face (UiButtonLook)"
    if prop == "BackColor" and INPUT_NAMES.match(ctl) and value in ("SystemColors.Control", "SystemColors.Window", "Color.White"):
        return "an input's state: the read-only or locked grey, or the editable box (SystemColors.Window, SurfaceRaised's high-contrast partner)"
    if prop == "BackColor" and value in ("Color.Red", "Color.FromArgb(255,255,192)"):
        return "a deliberate status colour: an alert or a search hit in colour"
    return "?"


def parent_map(lines):
    """child -> parent from the designer's Controls.Add lines."""
    parents = {}
    for line in lines:
        m = re.match(r"^\s*this\.(?:(?P<p>[\w.]+)\.)?Controls\.Add\(this\.(?P<c>\w+)\);", line)
        if m:
            parents[m.group("c")] = m.group("p") or "."
    return parents


def decide_all(root, found):
    """Each literal with its decision ('map' or the reason it stays). A child of a container whose literal stays keeps its own."""
    by_file = collections.defaultdict(list)
    for r in found:
        by_file[r["file"]].append(r)
    for rel, rows in by_file.items():
        lines = read(os.path.join(root, rel)).split("\n")
        parents = parent_map(lines)
        first = {}
        for r in rows:
            first[r["control"]] = None
        # the containers whose BackColor stays (a colour other than an old background)
        stay_back = set()
        for r in rows:
            if r["kind"] == "designer" and r["property"] == "BackColor":
                d = decide(r, set())
                if d != "map" and not d.startswith("transparent") and r["type"] in CONTAINERS:
                    stay_back.add(r["control"])
        parents_stay = set()
        for r in rows:
            p = parents.get(r["control"])
            while p:
                if p in stay_back:
                    parents_stay.add(r["control"])
                    break
                if p == ".":
                    break
                p = parents.get(p)
        for r in rows:
            r["decision"] = decide(r, parents_stay if r["property"] == "BackColor" else set())
    return found


SECTION_COMMENTS = [
    MARK + " (docs/port-notes/UI-11.md): every literal BackColor and ForeColor value (a named colour, a system colour, Color.FromArgb",
    "# with numbers) that the designers and the code of the program's projects set and that stays, with the reason. A chrome literal (a window,",
    "# panel, group box, tab page, label or check box in an old background: Windows' grey, white, 252,253,254) is not here: its line went, so the",
    "# control takes the token from its parent and the looks. NoHqTests' ChromeColourScanTests fails on a literal this table does not list and on",
    "# a row the source no longer has as often. python tools/UiFit/colour_literals.py writes it (--check compares), with the reasons of its rules.",
    "# file: relative, forward slashes. control: the designer's field (\".\" the window), or the code's expression. count: how often.",
]


def table_rows(found):
    counts = collections.Counter()
    whys = {}
    for r in found:
        if r["decision"] == "map":
            continue
        key = (r["file"], r["control"], r["property"], r["value"])
        counts[key] += 1
        whys.setdefault(key, r["decision"])
    return [list(k) + [str(counts[k]), whys[k]] for k in sorted(counts)]


def render_section(found):
    lines = SECTION_COMMENTS + [HEADER] + ["\t".join(row) for row in table_rows(found)]
    return "\n".join(lines) + "\n"


def split_list(text):
    """The allow list's text before the UI-11 section, and the section."""
    i = text.find(MARK)
    return (text, "") if i < 0 else (text[:i], text[i:])


def window_types(root, files):
    """namespace.class of each file's class (the designer's partial class), for the gallery's window list."""
    out = set()
    for rel in files:
        text = read(os.path.join(root, rel))
        ns = re.search(r"^\s*namespace\s+([\w.]+)", text, re.M)
        cls = re.search(r"\b(?:partial\s+)?class\s+(\w+)", text)
        if ns and cls:
            out.add(ns.group(1) + "." + cls.group(1))
    return sorted(out)


def apply(root, found):
    """Removes the designer lines the rules map; returns the files changed."""
    by_file = collections.defaultdict(set)
    for r in found:
        if r["decision"] == "map" and r["kind"] == "designer":
            by_file[r["file"]].add(r["line"])
    for rel, numbers in sorted(by_file.items()):
        path = os.path.join(root, rel)
        with open(path, "rb") as f:
            data = f.read()
        bom = data.startswith(b"\xef\xbb\xbf")
        text = data.decode("utf-8-sig")
        newline = "\r\n" if "\r\n" in text else "\n"
        lines = text.split("\n")
        kept = [line for i, line in enumerate(lines) if (i + 1) not in numbers]
        out = "\n".join(kept)
        with open(path, "wb") as f:
            f.write((b"\xef\xbb\xbf" if bom else b"") + out.encode("utf-8"))
        print("%s: %d line(s)" % (rel, len(numbers)))
    return sorted(by_file)


def main():
    ap = argparse.ArgumentParser()
    ap.add_argument("--inventory")
    ap.add_argument("--check", action="store_true")
    ap.add_argument("--apply", action="store_true")
    ap.add_argument("--windows", help="writes the window types whose designer has a mapped line (comma separated) to this file")
    args = ap.parse_args()
    root = repo_root()
    found = decide_all(root, scan(root))
    unknown = [r for r in found if r["decision"] in (None, "?")]
    if args.windows:
        files = sorted({r["file"] for r in found if r["decision"] == "map"})
        with open(args.windows, "w", encoding="utf-8", newline="\n") as f:
            f.write(",".join(window_types(root, files)))
        print(len(files), "files")
        return 0
    if args.apply:
        apply(root, found)
        return 0
    if args.inventory:
        with open(args.inventory, "w", encoding="utf-8", newline="\n") as f:
            f.write("file\tline\tkind\tcontrol\ttype\tproperty\tvalue\tdecision\n")
            for r in found:
                f.write("\t".join(str(r[k]) for k in ["file", "line", "kind", "control", "type", "property", "value", "decision"]) + "\n")
        print(len(found), "literals;", collections.Counter("map" if r["decision"] == "map" else ("?" if r["decision"] in (None, "?") else "stays")
                                                           for r in found))
        return 0
    if unknown:
        for r in unknown:
            print("no rule: %s:%d %s.%s = %s" % (r["file"], r["line"], r["control"], r["property"], r["value"]))
        return 1
    mapped = [r for r in found if r["decision"] == "map"]
    if mapped:
        for r in mapped:
            print("chrome literal (use the token: remove the designer line, or UiTheme in code): %s:%d %s.%s = %s"
                  % (r["file"], r["line"], r["control"], r["property"], r["value"]))
        return 1
    path = os.path.join(root, LIST)
    with open(path, "rb") as f:
        text = f.read().decode("utf-8")
    newline = "\r\n" if "\r\n" in text else "\n"
    head, section = split_list(text)
    want = render_section(found).replace("\n", newline)
    if args.check:
        if section != want:
            print("colour-literals.tsv's UI-11 section differs from the source; run python tools/UiFit/colour_literals.py")
            return 1
        print("colour-literals.tsv's UI-11 section matches (%d rows)" % len(table_rows(found)))
        return 0
    if head and not head.endswith(newline):
        head += newline
    with open(path, "wb") as f:
        f.write((head + want).encode("utf-8"))
    print("wrote %d rows" % len(table_rows(found)))
    return 0


if __name__ == "__main__":
    sys.exit(main())
