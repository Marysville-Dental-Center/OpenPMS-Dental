// FormatSweep.exe (S3b-FMT, docs/port-notes/S3b-FMT.md). After a full lane build of OpenDental.Local.slnf:
//   FormatSweep.exe --root <repository> [--report <file>] [--formats <file>] [--rewrite] [--projects <a.csproj,b.csproj>]
// --report writes every place found (tab separated: category, project, file, line, code, format, target), --formats the constant formats
// used with doubles and floats (for tests\FormatParity\formats.txt), --rewrite routes the places that have a mechanical rewrite through the
// helpers (review the diff, then build). Exit code: 0 when no guarded place is unrouted (allowed-sites.txt), 1 otherwise, 2 for a bad call.
// Licensed under the GNU General Public License, version 2 or later. See LICENSE in the repository root.
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace FormatSweep {
	public static class Program {
		public static int Main(string[] args) {
			string root=null;
			string pathReport=null;
			string pathFormats=null;
			bool doRewrite=false;
			string projects=null;
			for(int i=0;i<args.Length;i++) {
				switch(args[i]) {
					case "--root":
						root=args[++i];
						break;
					case "--report":
						pathReport=args[++i];
						break;
					case "--formats":
						pathFormats=args[++i];
						break;
					case "--rewrite":
						doRewrite=true;
						break;
					case "--projects":
						projects=args[++i];
						break;
					default:
						Console.Error.WriteLine("Unknown argument: "+args[i]);
						return 2;
				}
			}
			if(root==null) {
				Console.Error.WriteLine("Usage: FormatSweep --root <repository> [--report <file>] [--formats <file>] [--rewrite] [--projects <list>]");
				return 2;
			}
			root=Path.GetFullPath(root);
			string folderTool=Path.Combine(root,"tools","FormatSweep");
			List<string> listProjects=projects==null
				? Scanner.ReadProjectList(Path.Combine(folderTool,"shipped-projects.txt"))
				: projects.Split(',').Select(x => x.Trim()).Where(x => x!="").ToList();
			ScanResult scanResult=Scanner.Scan(root,listProjects,x => Console.WriteLine(x));
			foreach(string project in scanResult.ListProjectsMissing) {
				Console.WriteLine("NOT BUILT (no compile inputs): "+project);
			}
			List<AllowedSite> listAllowed=AllowList.Read(Path.Combine(folderTool,"allowed-sites.txt"));
			List<Site> listUnrouted=AllowList.GetUnrouted(scanResult.ListSites,listAllowed);
			foreach(IGrouping<string,Site> group in scanResult.ListSites.GroupBy(x => x.Category).OrderBy(x => x.Key)) {
				Console.WriteLine(group.Key+": "+group.Count()+" (rewritable "+group.Count(x => x.ListEdits.Count>0)+")");
			}
			Console.WriteLine("Guarded places not routed and not allowed: "+listUnrouted.Count);
			foreach(AllowedSite allowedSite in listAllowed.Where(x => x.CountMatched==0)) {
				Console.WriteLine("Allowed line matches nothing: "+allowedSite.RelativePath+" | "+allowedSite.Category+" | "+allowedSite.Code);
			}
			if(pathReport!=null) {
				File.WriteAllLines(pathReport,scanResult.ListSites.OrderBy(x => x.Category).ThenBy(x => x.RelativePath).ThenBy(x => x.Line)
					.Select(x => x.ToString()));
			}
			if(pathFormats!=null) {
				List<string> listFormats=scanResult.ListSites.Where(x => !string.IsNullOrEmpty(x.Format))
					.SelectMany(x => x.Category==Categories.COMPOSITE ? x.Format.Split(',') : new[] { x.Format })
					.Distinct(StringComparer.Ordinal).OrderBy(x => x,StringComparer.Ordinal).ToList();
				File.WriteAllLines(pathFormats,listFormats);
			}
			if(doRewrite) {
				List<Site> listToRewrite=listUnrouted.Where(x => x.ListEdits.Count>0).ToList();
				foreach(string line in Rewriter.Apply(listToRewrite)) {
					Console.WriteLine(line);
				}
				Console.WriteLine("Rewritten places: "+listToRewrite.Count+"; left for a rewrite by hand: "+listUnrouted.Count(x => x.ListEdits.Count==0));
			}
			return listUnrouted.Count==0 ? 0 : 1;
		}
	}
}
