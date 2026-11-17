// The places that write a double as .NET writes it on purpose (allowed-sites.txt), each with its reason. S3b-FMT, docs/port-notes/S3b-FMT.md.
// Licensed under the GNU General Public License, version 2 or later. See LICENSE in the repository root.
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace FormatSweep {
	public class AllowedSite {
		///<summary>The source file, relative to the repository root.</summary>
		public string RelativePath;
		///<summary>The category (Categories), or * for every category.</summary>
		public string Category;
		///<summary>The code of the place as the scan writes it (Site.Code), or * for every place of the file and category.</summary>
		public string Code;
		public string Reason;
		public int CountMatched;

		public bool IsMatch(Site site) {
			return site.RelativePath.Equals(RelativePath,StringComparison.OrdinalIgnoreCase)
				&& (Category=="*" || Category==site.Category)
				&& (Code=="*" || Code==site.Code);
		}
	}

	public static class AllowList {
		///<summary>Lines "path | category | code | reason" (the code may hold " | " only as part of the code; the reason is the last part).
		///Lines starting with # are comments.</summary>
		public static List<AllowedSite> Read(string path) {
			List<AllowedSite> listAllowed=new List<AllowedSite>();
			foreach(string line in File.ReadAllLines(path)) {
				if(line.Trim()=="" || line.TrimStart().StartsWith("#")) {
					continue;
				}
				string[] arrayParts=line.Split(new[] { " | " },StringSplitOptions.None);
				if(arrayParts.Length<4) {
					throw new FormatException("allowed-sites.txt: a line needs path | category | code | reason: "+line);
				}
				AllowedSite allowedSite=new AllowedSite();
				allowedSite.RelativePath=arrayParts[0].Trim();
				allowedSite.Category=arrayParts[1].Trim();
				allowedSite.Code=string.Join(" | ",arrayParts.Skip(2).Take(arrayParts.Length-3)).Trim();
				allowedSite.Reason=arrayParts[arrayParts.Length-1].Trim();
				if(allowedSite.Reason=="") {
					throw new FormatException("allowed-sites.txt: every allowed place needs its reason: "+line);
				}
				listAllowed.Add(allowedSite);
			}
			return listAllowed;
		}

		///<summary>The guarded sites that no allowed line matches.</summary>
		public static List<Site> GetUnrouted(IEnumerable<Site> sites,List<AllowedSite> listAllowed) {
			List<Site> listUnrouted=new List<Site>();
			foreach(Site site in sites.Where(x => Categories.IsGuarded(x.Category))) {
				AllowedSite allowedSite=listAllowed.FirstOrDefault(x => x.IsMatch(site));
				if(allowedSite==null) {
					listUnrouted.Add(site);
				}
				else {
					allowedSite.CountMatched++;
				}
			}
			return listUnrouted;
		}
	}
}
