# ForkMeasure

`measure_fork.py` measures how much of the code is still Open Dental's and how much is new or rewritten. The results are
published in [docs/differences.md](../../docs/differences.md#the-numbers).

```
python tools/ForkMeasure/measure_fork.py d804c1954 HEAD
python tools/ForkMeasure/measure_fork.py d804c1954 HEAD --json
```

Run it from the repository root with Python 3 and git on the path (about a minute). It reads the two commits only, never
the working tree. `d804c1954` is `version 24.3.63.0`, the last Open Dental commit; Open Dental's public source stops there.

## Method

- Code files only: `.cs .xaml .sql .ps1 .psm1 .py .js .cpp .h .vb .cshtml .css .html`.
- A substantive line has at least one letter or digit. Whitespace is removed and the product rename is folded
  (`OpenPmsDental*` and `OpenDent*` names become one token), so a renamed but otherwise unchanged line still counts as
  Open Dental's.
- Each file at the head is paired with its Open Dental file: git's rename and copy detection (`-M50%`), the same path
  otherwise. A line counts as unchanged from Open Dental when the paired file holds the same line (a multiset match);
  everything else is new or rewritten. A file without a pair is new.
- The strict share counts only lines of 25 characters or more (after the whitespace is removed), so short lines such as
  `return;` or `}else{`, which match almost anywhere, do not inflate it.
- Generated files (`*.Designer.cs`, `Crud/`, `db/init/new-database.sql`, `AssemblyInfo.cs`) are reported apart from the
  hand-written code.
- Removed files are Open Dental files that no head file pairs with.

The measure is line based: a moved line counts as unchanged, a line with one changed character counts as rewritten.
`--json` prints the raw counts per top folder.
