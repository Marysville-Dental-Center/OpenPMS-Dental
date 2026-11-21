#!/usr/bin/env python3
# UI-10 text fit tools (docs/port-notes/UI-10.md): turn the tier 1 scan's files into tests/UiRefreshTests/Data/fit-scan*.tsv, and a slice's
# rows into designer edits (Width and Height only; AutoSize = false with an explicit Size for a control that sizes itself) with their
# rows of tests/UiRefreshTests/Data/fit-fixes.tsv. Plain Python 3, no packages. Designer files are edited as bytes (latin-1 round trip), so
# every line end and the BOM stay as they are.
#
#   fit_tools.py scan-tsv  --scan <scan folder>/options --out tests/UiRefreshTests/Data
#   fit_tools.py plan      --scan <scan folder>/options --windows OpenPmsDental.FormProvEdit,... [--apply] [--note "slice 1"]
#
# The scan folder is the opt-in run's output (OPMS_UI_SCAN=1, OPMS_UI_SCAN_OUT): overflow-segoe9.tsv, selfsizing-segoe9.tsv,
# bounds-segoe9.tsv, bounds-record-before.tsv, bounds-record-segoe9.tsv. All sizes are at 100% (designer pixels).
import argparse
import collections
import csv
import math
import os
import re
import subprocess
import sys

csv.field_size_limit(1 << 24)
RUN = "segoe9"
TAB = chr(9)
LF = chr(10)
CRLF = chr(13) + chr(10)


def read_tsv(path):
    with open(path, encoding="utf-8", newline="") as f:
        return list(csv.DictReader(f, delimiter="\t", quoting=csv.QUOTE_NONE))


def parse_rect(text):
    m = re.match(r"(-?\d+),(-?\d+),(\d+)x(\d+)$", text)
    return tuple(int(x) for x in m.groups()) if m else None


def repo_root():
    return subprocess.check_output(["git", "rev-parse", "--show-toplevel"], text=True).strip()


def designer_files(root):
    out = subprocess.check_output(["git", "ls-files", "-z"], cwd=root).decode("utf-8").split("\0")
    return [x for x in out if re.search(r"\.designer\.cs$", x, re.I)]


def window_files(root, windows):
    """Window type full name -> designer files whose base name is the type's simple name (in the type's assembly folder when it is ambiguous)."""
    files = designer_files(root)
    by_name = collections.defaultdict(list)
    for f in files:
        base = re.sub(r"\.designer\.cs$", "", os.path.basename(f), flags=re.I).lower()
        by_name[base].append(f)
    result = {}
    for w in windows:
        simple = w.split(".")[-1].lower()
        asm = w.split(".")[0].lower()
        cand = by_name.get(simple, [])
        if not cand and ALLOW_CODE_FILES:
            # UI-2b (--code-files): a window whose designer code sits in its own .cs file (the report designer's dialogs), or whose designer
            # file is named otherwise than the type (FormProcCodes in FormProcCodes2.designer.cs), "(no designer file)" in the ranking. The
            # simple name drops a scan key's "@assembly" (a CodeBase window linked into another exe).
            simple = simple.split("@")[0]
            cand = by_name.get(simple, []) + code_files_with_designer_code(root).get(simple, [])
            if not cand:
                cand = files_declaring(root, files, simple)
        if len(cand) > 1:
            narrowed = [c for c in cand if c.lower().startswith(asm.lower())]
            cand = narrowed or cand
        result[w] = cand
    return result


_code_files_cache = {}


def code_files_with_designer_code(root):
    """Simple type name -> .cs files (not .designer.cs) of that base name that define InitializeComponent, for --code-files only."""
    if not ALLOW_CODE_FILES:
        return {}
    if root not in _code_files_cache:
        out = subprocess.check_output(["git", "ls-files", "-z", "*.cs"], cwd=root).decode("utf-8").split("\0")
        by_name = collections.defaultdict(list)
        for f in out:
            if not f or re.search(r"\.designer\.cs$", f, re.I):
                continue
            with open(os.path.join(root, f), "rb") as fh:
                if re.search(rb"void\s+InitializeComponent\s*\(\s*\)", fh.read()):
                    by_name[os.path.basename(f)[:-3].lower()].append(f)
        _code_files_cache[root] = by_name
    return _code_files_cache[root]


def files_declaring(root, files, simple):
    """The designer files (and, with --code-files, the code files with designer code) that declare the class simple (any case)."""
    pattern = re.compile(rb"\bclass\s+" + re.escape(simple.encode("utf-8")) + rb"\b", re.I)
    pool = list(files) + [f for fs in code_files_with_designer_code(root).values() for f in fs]
    found = []
    for f in pool:
        with open(os.path.join(root, f), "rb") as fh:
            if pattern.search(fh.read()):
                found.append(f)
    return found


ALLOW_CODE_FILES = False


def load_scan(folder):
    d = {}
    all_ov = read_tsv(os.path.join(folder, "overflow-%s.tsv" % RUN))
    d["ov"] = [r for r in all_ov if r["scale"] == "1"]
    d["ov_scales"] = [r for r in all_ov if r["scale"] != "1" and r["scale"] != "1.2" and r["ink"] != "inside"]
    d["ss"] = [r for r in read_tsv(os.path.join(folder, "selfsizing-%s.tsv" % RUN)) if r["scale"] == "1"]
    d["ss_scales"] = [r for r in read_tsv(os.path.join(folder, "selfsizing-%s.tsv" % RUN)) if r["scale"] not in ("1", "1.2")]
    d["bd"] = read_tsv(os.path.join(folder, "bounds-%s.tsv" % RUN))
    return d


def cmd_scan_tsv(args):
    root = repo_root()
    scan = load_scan(args.scan)
    windows = sorted({r["window"] for r in scan["ov"] + scan["ss"] + scan["bd"]})
    wf = window_files(root, windows)
    items = []
    for r in scan["ov"]:
        items.append((r["window"], "overflow", r["control"], r["type"], r["kind"], r["need_width"], r["need_height"], r["ink"], r["free_right"],
                      r["free_below"], r["verdict"]))
    for r in scan["ss"]:
        items.append((r["window"], "selfsizing-" + r["what"], r["control"], r["type"], "", "", "", "", r["bounds_before"], r["bounds_after"], r["verdict"]))
    for r in scan["bd"]:
        items.append((r["window"], "bounds-" + r["kind"], r["control"], r["type"], "", "", "", "", r["before"], r["after"], ""))
    per_file = collections.defaultdict(lambda: [0, 0, 0, 0])
    for it in items:
        files = wf.get(it[0]) or ["(no designer file)"]
        for f in files[:1]:
            c = per_file[f]
            if it[1] == "overflow":
                c[0] += 1
                if it[10] != "fixable":
                    c[3] += 1
            elif it[1].startswith("selfsizing"):
                c[1] += 1
                if it[10] != "fixable":
                    c[3] += 1
            else:
                c[2] += 1
    ranked = sorted(per_file.items(), key=lambda kv: (kv[0].startswith("("), -(kv[1][0] + kv[1][1] + kv[1][2]), kv[0]))
    os.makedirs(args.out, exist_ok=True)
    with open(os.path.join(args.out, "fit-scan-files.tsv"), "w", encoding="utf-8", newline="\n") as f:
        f.write("# The tier 1 text-fit scan at Segoe UI 9 pt, 100%, by designer file (UI-10, docs/port-notes/UI-10.md): the order slices take their files in.\n")
        f.write("# Made by tools/UiFit/fit_tools.py scan-tsv from the opt-in scan of the first UI-10 slice (before any fix). A later slice reruns the scan and the tool.\n")
        f.write("rank\tdesigner_file\tnew_overflows\tnew_selfsizing\tbounds_differences\tneeds_the_owner\n")
        for i, (name, c) in enumerate(ranked, 1):
            f.write("%d\t%s\t%d\t%d\t%d\t%d\n" % (i, name, c[0], c[1], c[2], c[3]))
    rank = {name: i for i, (name, c) in enumerate(ranked, 1)}
    items.sort(key=lambda it: (rank.get((wf.get(it[0]) or ["(no designer file)"])[0], 99999), it[0], it[1], it[2]))
    with open(os.path.join(args.out, "fit-scan.tsv"), "w", encoding="utf-8", newline="\n") as f:
        f.write("# The tier 1 text-fit scan at Segoe UI 9 pt, 100%: every new overflow, new overlap or parent-edge pass of a control that sizes itself, and bounds difference,\n")
        f.write("# sorted by the file's rank in fit-scan-files.tsv. kind: overflow = a control whose text no longer fits (need_width, need_height: pixels missing); selfsizing-* = a control that\n")
        f.write("# sizes itself grows into a neighbour (before and after: its bounds); bounds-* = the bounds differ from today's font. verdict: fixable (free space) or needs the owner.\n")
        f.write("designer_file\twindow\tkind\tcontrol\ttype\toverflow_kind\tneed_width\tneed_height\tink\tfree_right_or_before\tfree_below_or_after\tverdict\n")
        for it in items:
            files = wf.get(it[0]) or ["(no designer file)"]
            f.write("\t".join([files[0]] + list(it[:1]) + list(it[1:4]) + list(it[4:11])).replace("\r", "") + "\n")
    print("files:", len(ranked), "items:", len(items))


def load_record(path):
    rows = {}
    for r in read_tsv(path):
        rows[(r["window"], r["path"])] = r
    return rows


def rect_of(r):
    return (int(r["x"]), int(r["y"]), int(r["width"]), int(r["height"]))


def free_space(rect, siblings, parent_client):
    x, y, w, h = rect
    free_right = parent_client[0] - (x + w)
    free_below = parent_client[1] - (y + h)
    for sx, sy, sw, sh in siblings:
        if sx < x + w and sx + sw > x and sy < y + h and sy + sh > y:
            continue
        beside = sy < y + h and sy + sh > y
        if beside and sx >= x + w:
            free_right = min(free_right, sx - (x + w))
        under = sx < x + w and sx + sw > x
        if under and sy >= y + h:
            free_below = min(free_below, sy - (y + h))
    return max(0, free_right), max(0, free_below)


TEXT_BOX_TYPES = ("System.Windows.Forms.TextBox", "OpenPmsDental.ValidNum", "OpenPmsDental.ValidDouble", "OpenPmsDental.ValidDate", "OpenPmsDental.ValidPhone",
                  "OpenPmsDental.ValidNumber", "ODR.ValidDate", "ODR.ValidDouble")
FONT_FIXED_TYPES = ("System.Windows.Forms.ComboBox", "System.Windows.Forms.DateTimePicker", "System.Windows.Forms.NumericUpDown",
                    "System.Windows.Forms.DomainUpDown", "OpenPmsDental.UI.ComboBox", "OpenPmsDental.ComboBoxPlus")


def plan_window(window, scan, rec_before, rec_after, designer_text):
    """Returns (fixes, owner): fixes is name -> dict(width, height, autosize_off, note); owner is a list of (control, reason)."""
    controls = collections.OrderedDict()

    def touch(path, kind):
        controls.setdefault(path, {"ov": [], "ss": [], "bd": []})[kind].append(1)

    ov = {}
    ss = {}
    for r in scan["ov"]:
        if r["window"] == window:
            ov[r["control"]] = r
            touch(r["control"], "ov")
    ov_scales = collections.defaultdict(list)
    for r in scan["ov_scales"]:
        if r["window"] == window:
            ov_scales[r["control"]].append(r)
            touch(r["control"], "ov")
    for r in scan["ss"]:
        if r["window"] == window:
            ss.setdefault(r["control"], []).append(r)
            touch(r["control"], "ss")
            other = rec_after.get((window, r["other"]))
            if r["other"] and other is not None and other["sizes_itself"] == "1":
                touch(r["other"], "ss")
    for r in scan["bd"]:
        if r["window"] == window:
            touch(r["control"], "bd")
    ss_scale_names = set()
    for r in scan["ss_scales"]:
        if r["window"] == window and r["type"] in TEXT_BOX_TYPES:
            # A text box that grows into a neighbour only at a scale (modelled): AutoSize off keeps today's height at every scale.
            ss_scale_names.add(r["control"])
            touch(r["control"], "ss")
            other = rec_after.get((window, r["other"]))
            if r["other"] and other is not None and other["sizes_itself"] == "1" and other["type"] in TEXT_BOX_TYPES:
                ss_scale_names.add(r["other"])
                touch(r["other"], "ss")
    fixes = collections.OrderedDict()
    owner = []
    for path in controls:
        name = path.split("/")[-1]
        if "#" in name:
            owner.append((path, "an unnamed control inside another control: that control's own layout"))
            continue
        if not re.search(r"Controls\.Add\((?:this\.)?%s\)" % re.escape(name), designer_text):
            owner.append((path, "a control of another designer (a control's own children): not this window's file"))
            continue
        b = rec_before.get((window, path))
        a = rec_after.get((window, path))
        if b is None or a is None:
            owner.append((path, "not in the records"))
            continue
        B = rect_of(b)
        A = rect_of(a)
        ctype = a["type"]
        tw, th = B[2], B[3]
        autosize_off = False
        why = []
        reason_owner = None
        sizing = a["sizes_itself"] == "1"
        parent = rec_after.get((window, a["path"].rsplit("/", 1)[0] if "/" in a["path"] else "."))
        pc = (int(parent["client_width"]), int(parent["client_height"])) if parent else (10 ** 6, 10 ** 6)
        sibs = [rect_of(x) for (w, p), x in rec_after.items() if w == window and p != path and x["visible"] == "1" and x["dock"] == "None"
                and (p.rsplit("/", 1)[0] if "/" in p else ".") == (path.rsplit("/", 1)[0] if "/" in path else ".")]
        fr, fb = free_space(B, sibs, pc)
        if path in ov and not sizing:
            r = ov[path]
            nw, nh = int(r["need_width"]), int(r["need_height"])
            gw, gh = min(nw, fr), min(nh, fb)
            tw, th = B[2] + gw, B[3] + gh
            why.append("text needs %+d,%+d px" % (nw, nh))
            if gw < nw or gh < nh:
                reason_owner = "no free space for the whole text (missing %d,%d px after growing into the free space)" % (nw - gw, nh - gh)
        if not sizing and path in ov_scales:
            # The other scales (1.25 to 2): LayoutManager multiplies the designer size, so the pixels missing at a scale are the designer pixels
            # divided by it, rounded up; only into the free space (at that scale), and only what is visibly cut.
            for r in ov_scales[path]:
                sc = float(r["scale"])
                gw = min(int(math.ceil(int(r["need_width"]) / sc)), int(int(r["free_right"]) / sc))
                gh = min(int(math.ceil(int(r["need_height"]) / sc)), int(int(r["free_below"]) / sc))
                tw = max(tw, B[2] + min(gw, fr))
                th = max(th, B[3] + min(gh, fb))
            why.append("also the pixels missing at the scales 1.25 to 2")
        if sizing or path in ss or path in ss_scale_names or (window, path) in {(window, r["control"]) for r in scan["bd"] if r["window"] == window}:
            if ctype in FONT_FIXED_TYPES or (ctype not in TEXT_BOX_TYPES and ctype.endswith(("ComboBox", "DateTimePicker", "UpDown"))):
                owner.append((path, "its height comes from the font (%s); no size edit can keep it; it grows into its neighbour" % ctype.split(".")[-1]))
                continue
            autosize_off = True
            if ctype in TEXT_BOX_TYPES:
                tw, th = B[2], B[3]
                why.append("single-line text box keeps today's height; the new font fits it")
            else:
                gw = min(max(0, A[2] - B[2]), fr)
                gh = min(max(0, A[3] - B[3]), fb)
                if ctype.endswith(("RadioButton", "CheckBox")):
                    gh = 0
                tw = max(tw, B[2] + gw)
                th = max(th, B[3] + gh)
                why.append("AutoSize off, size fixed at what the new font needs inside the free space")
                if A[2] - B[2] > fr or (A[3] - B[3] > fb and not ctype.endswith(("RadioButton", "CheckBox"))):
                    reason_owner = "the grown size does not fit the free space"
        if reason_owner:
            owner.append((path, reason_owner))
        if (tw, th) != (B[2], B[3]) or autosize_off:
            fixes[name] = {"width": tw, "height": th, "autosize_off": autosize_off, "note": "; ".join(why), "path": path, "before": B[2:], "owner": reason_owner}
    return fixes, owner


def apply_fix_to_text(text, name, fix):
    """Returns (new text, error or None). Byte-exact: only digits of the Size line and the AutoSize word change, or one line is inserted."""
    pat = re.compile(r"((?:this\.)?%s\.Size ?= ?new System\.Drawing\.Size\()(\d+)(, ?)(\d+)(\))" % re.escape(name))
    matches = list(pat.finditer(text))
    if len(matches) != 1:
        return text, "Size line found %d times" % len(matches)
    m = matches[0]
    text = text[:m.start(2)] + str(fix["width"]) + m.group(3) + str(fix["height"]) + text[m.end(4):]
    if fix["autosize_off"]:
        pat_a = re.compile(r"((?:this\.)?%s\.AutoSize ?= ?)(true|false)(;)" % re.escape(name))
        ma = pat_a.search(text)
        if ma:
            text = text[:ma.start(2)] + "false" + text[ma.end(2):]
        else:
            pat_l = re.compile(r"^([ \t]*)((?:this\.)?)%s\.Location( ?)= ?[^\r\n]*?(\r\n|\n)" % re.escape(name), re.M)
            ml = pat_l.search(text)
            if not ml:
                return text, "no Location line to put AutoSize before"
            line = ml.group(1) + ml.group(2) + name + (".AutoSize = false;" if ml.group(3) else ".AutoSize=false;") + ml.group(4)
            text = text[:ml.start()] + line + text[ml.start():]
    return text, None


def cmd_plan(args):
    root = repo_root()
    scan = load_scan(args.scan)
    rec_before = load_record(os.path.join(args.scan, "bounds-record-before.tsv"))
    rec_after = load_record(os.path.join(args.scan, "bounds-record-%s.tsv" % RUN))
    windows = args.windows.split(",")
    wf = window_files(root, windows)
    rows_fixes = []
    owner_all = []
    changed_files = set()
    for w in windows:
        files = wf[w]
        if len(files) != 1:
            print("WINDOW %s: designer files %s: skipped" % (w, files))
            continue
        path_file = os.path.join(root, files[0])
        with open(path_file, "rb") as f:
            raw = f.read()
        text = raw.decode("latin-1")
        fixes, owner = plan_window(w, scan, rec_before, rec_after, text)
        for path, reason in owner:
            owner_all.append((files[0], w, path, reason))
        text_new = text
        count = 0
        for name, fix in fixes.items():
            text_new, err = apply_fix_to_text(text_new, name, fix)
            if err:
                owner_all.append((files[0], w, fix["path"], "designer edit failed: " + err))
                continue
            count += 1
            rows_fixes.append((files[0], w, name, "autosize-off" if fix["autosize_off"] else "size", fix["note"] or "size to fit the text",
                               "%dx%d->%dx%d" % (fix["before"][0], fix["before"][1], fix["width"], fix["height"]), fix["owner"] or ""))
        print("%-45s %-60s fixes %3d owner %3d" % (w, files[0], count, len([o for o in owner if True])))
        if args.apply and text_new != text:
            with open(path_file, "wb") as f:
                f.write(text_new.encode("latin-1"))
            changed_files.add(files[0])
    out_owner = args.owner_out
    if out_owner:
        with open(out_owner, "w", encoding="utf-8", newline="\n") as f:
            f.write("designer_file\twindow\tcontrol\treason\n")
            for o in owner_all:
                f.write("\t".join(o) + "\n")
    out_fixes = args.fixes_out
    if args.apply and out_fixes:
        with open(out_fixes, "rb") as f:
            raw = f.read().decode("utf-8")
        eol = CRLF if CRLF in raw else LF
        existing = collections.OrderedDict()
        head = []
        for line in raw.splitlines():
            if line == "":
                continue
            fields = line.split(TAB)
            if line.startswith("#") or fields[0] == "designer_file":
                head.append(line)
            else:
                existing[(fields[0], fields[2])] = fields
        for r in rows_fixes:
            key = (r[0], r[2])
            fix = r[3]
            if key in existing and existing[key][3] == "autosize-off":
                fix = "autosize-off"
            existing[key] = [r[0], r[1], r[2], fix, r[4] + " (" + r[5] + ")"]
        with open(out_fixes, "wb") as f:
            for line in head:
                f.write((line + eol).encode("utf-8"))
            for fields in existing.values():
                f.write((TAB.join(fields) + eol).encode("utf-8"))
    print("rows", len(rows_fixes), "owner", len(owner_all), "files changed", len(changed_files))


def main():
    p = argparse.ArgumentParser()
    sub = p.add_subparsers(dest="cmd", required=True)
    a = sub.add_parser("scan-tsv")
    a.add_argument("--scan", required=True)
    a.add_argument("--out", required=True)
    b = sub.add_parser("plan")
    b.add_argument("--scan", required=True)
    b.add_argument("--windows", required=True)
    b.add_argument("--apply", action="store_true")
    b.add_argument("--owner-out")
    b.add_argument("--fixes-out")
    b.add_argument("--code-files", action="store_true", help="UI-2b: also a window whose designer code is in its own .cs file")
    args = p.parse_args()
    global ALLOW_CODE_FILES
    ALLOW_CODE_FILES = getattr(args, "code_files", False)
    if args.cmd == "scan-tsv":
        cmd_scan_tsv(args)
    else:
        cmd_plan(args)


if __name__ == "__main__":
    main()
