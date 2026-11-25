"""Measure how much of the current code is unchanged from Open Dental (the fork point) and how much is new or rewritten.

Usage: python tools/ForkMeasure/measure_fork.py <fork commit> [<head commit>] [--json]
       python tools/ForkMeasure/measure_fork.py d804c1954 HEAD

Run it from the repository root. d804c1954 is "version 24.3.63.0", the last Open Dental commit (Open Dental's public
source stops there). Needs Python 3 and git; reads the two commits only, never the working tree.

Method: files at FORK and HEAD with code extensions; pairs from `git diff -M50% -l0` (renames and copies), the same path
otherwise. A substantive line is a line with at least one letter or digit, whitespace removed, the product rename undone
(OpenPmsDental* / OpenDent* names folded to one token). A HEAD line counts as "from Open Dental" when the paired Open
Dental file holds the same normalized line (multiset match). Everything else in HEAD is new or rewritten. The strict
figures count only normalized lines of 25 characters or more, so short lines such as `return;` or `}else{` that match
anywhere do not inflate the share. Generated files (*.Designer.cs, Crud/, the init SQL, AssemblyInfo.cs) are reported
apart from the hand-written code.
"""
import subprocess, re, sys, collections, json

args = [a for a in sys.argv[1:] if not a.startswith('--')]
AS_JSON = '--json' in sys.argv[1:]
if not args:
    sys.exit(__doc__)
FORK = args[0]
HEAD = args[1] if len(args) > 1 else 'HEAD'
EXT = re.compile(r'\.(cs|xaml|sql|ps1|psm1|py|js|cpp|h|vb|cshtml|css|html)$', re.I)
GENERATED = re.compile(r'(^db/init/new-database\.sql$|\.Designer\.cs$|\.designer\.cs$|/Crud/|AssemblyInfo\.cs$)', re.I)
REN = re.compile(r'OpenPmsDental(Business|HL7|Graph|Server|DbSettings|Storage|Helper32)?|OpenDent(al)?(Business|HL7|Graph|Server|Wpf|Cloud)?')
ALNUM = re.compile(r'[A-Za-z0-9]')
STRICT = 25
PROGRAM = ('OpenPmsDental', 'OpenPmsDentalBusiness', 'WpfControlsOD')
GROUPS = [
	('Program (OpenPmsDental, OpenPmsDentalBusiness, WpfControlsOD)', PROGRAM),
	('Tests (tests/)', ('tests',)),
	('Tools (tools/)', ('tools',)),
	('Native imaging (ImagingCapture, ImagingDeviceManager, OpenPmsDentalHelper32)', ('ImagingCapture', 'ImagingDeviceManager', 'OpenPmsDentalHelper32')),
]


def git(*a, inp=None):
	return subprocess.run(['git', *a], capture_output=True, input=inp, check=True).stdout


def files(rev):
	return [p for p in git('ls-tree', '-r', '--name-only', '-z', rev).decode('utf-8').split('\0') if p and EXT.search(p)]


def blobs(rev, paths):
	out = {}
	req = ''.join(f'{rev}:{p}\n' for p in paths).encode('utf-8')
	data = git('cat-file', '--batch', inp=req)
	i = 0
	for p in paths:
		nl = data.index(b'\n', i)
		hdr = data[i:nl].split()
		if hdr[-1] == b'missing':
			i = nl + 1
			out[p] = b''
			continue
		n = int(hdr[2])
		out[p] = data[nl + 1:nl + 1 + n]
		i = nl + 1 + n + 1
	return out


def norm(b):
	t = b.decode('utf-8', 'replace')
	res = []
	for line in t.splitlines():
		s = re.sub(r'\s+', '', line)
		if not ALNUM.search(s):
			continue
		res.append(REN.sub('PRODUCT', s))
	return res


def project(p):
	return p.split('/')[0]


fork_files = files(FORK)
head_files = files(HEAD)
fork_set = set(fork_files)
pairs = {}
toks = git('diff', '--name-status', '-z', '-M50%', '-l0', FORK, HEAD).decode('utf-8').split('\0')
i = 0
while i < len(toks) and toks[i]:
	st = toks[i]
	if st[0] in 'RC':
		src, dst = toks[i + 1], toks[i + 2]
		i += 3
		if EXT.search(dst) and EXT.search(src):
			pairs[dst] = src
	else:
		i += 2
for p in head_files:
	if p not in pairs and p in fork_set:
		pairs[p] = p

fb = blobs(FORK, fork_files)
hb = blobs(HEAD, head_files)
used_src = set(pairs.values())

agg = collections.defaultdict(lambda: collections.Counter())
for p in head_files:
	gen = bool(GENERATED.search(p))
	key = (project(p), gen)
	h = norm(hb[p])
	hs = [x for x in h if len(x) >= STRICT]
	src = pairs.get(p)
	if src:
		oldlines = norm(fb[src])
		same = sum((collections.Counter(h) & collections.Counter(oldlines)).values())
		same_s = sum((collections.Counter(hs) & collections.Counter(x for x in oldlines if len(x) >= STRICT)).values())
		agg[key]['files_from_od'] += 1
	else:
		same = 0
		same_s = 0
		agg[key]['files_new'] += 1
	agg[key]['lines'] += len(h)
	agg[key]['lines_from_od'] += same
	agg[key]['lines_strict'] += len(hs)
	agg[key]['lines_strict_from_od'] += same_s
removed = [p for p in fork_files if p not in used_src]
rem = collections.Counter()
for p in removed:
	rem[(project(p), bool(GENERATED.search(p)))] += len(norm(fb[p]))
fork_total = collections.Counter()
for p in fork_files:
	fork_total[bool(GENERATED.search(p))] += len(norm(fb[p]))
head_rev = git('rev-parse', '--short=9', HEAD).decode().strip()

if AS_JSON:
	rows = [{'project': proj, 'generated': gen, **c} for (proj, gen), c in sorted(agg.items())]
	json.dump({'fork': FORK, 'head': head_rev, 'rows': rows,
		'removed_files': len(removed), 'removed_lines': {f'{k[0]}|{k[1]}': v for k, v in rem.items()},
		'fork_lines': {str(k): v for k, v in fork_total.items()}, 'fork_files': len(fork_files), 'head_files': len(head_files)},
		sys.stdout, indent=1)
	sys.exit(0)


def total(pred):
	c = collections.Counter()
	for (proj, gen), v in agg.items():
		if pred(proj, gen):
			c.update(v)
	return c


def pct(a, b):
	return f'{100.0 * a / b:.1f}%' if b else '-'


def line(label, c):
	new = c['lines'] - c['lines_from_od']
	print(f'{label}\n  {c["lines"]:,} lines, {pct(c["lines_from_od"], c["lines"])} unchanged from Open Dental'
		f' ({pct(c["lines_strict_from_od"], c["lines_strict"])} strict), {new:,} new or rewritten;'
		f' files: {c["files_new"]:,} new, {c["files_from_od"]:,} from Open Dental')


print(f'Fork point {FORK}, head {head_rev}. Substantive lines of code files (see the method at the top of this script).')
print()
line('Hand-written code (all projects, generated files left out)', total(lambda p, g: not g))
line('Generated files (*.Designer.cs, Crud/, init SQL, AssemblyInfo.cs)', total(lambda p, g: g))
for label, projs in GROUPS:
	line(label + ', hand-written', total(lambda p, g, projs=projs: not g and p in projs))
allc = total(lambda p, g: True)
print(f'New files (all code files): {allc["files_new"]:,}')
print(f'Removed Open Dental files: {len(removed):,}, {sum(rem.values()):,} lines'
	f' (hand-written {sum(v for k, v in rem.items() if not k[1]):,})')
print(f'Open Dental at the fork point: {len(fork_files):,} code files, {sum(fork_total.values()):,} lines'
	f' (hand-written {fork_total[False]:,})')
print()
print('Per project, hand-written: lines, unchanged from Open Dental, strict, new files')
for (proj, gen), c in sorted(agg.items(), key=lambda kv: -kv[1]['lines']):
	if gen or c['lines'] < 1000:
		continue
	print(f'  {proj:<32} {c["lines"]:>9,} {pct(c["lines_from_od"], c["lines"]):>7} {pct(c["lines_strict_from_od"], c["lines_strict"]):>7} {c["files_new"]:>6,}')
