// UI-13 (docs/port-notes/UI-13.md, step 3): the element inventory (docs/port-notes/UI-13-inventory.tsv, written by
// tools/UiFit/element_inventory.py) holds every element type the program shows, and each is in the new look with its code and its row of
// coverage.tsv, or carries a recorded reason. The counts of the designer files are recounted here with the tool's own rule, so a new control
// type in a designer file, or a type whose uses change, fails until the tool is run again and the new type gets its look or its reason.
// Licensed under the GNU General Public License, version 2 or later. See LICENSE in the repository root.
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using UiRefreshTests.Scan;

namespace UiRefreshTests {
	[TestClass]
	public class ElementInventoryTests {
		///<summary>The projects whose designer files the tool reads (element_inventory.py's ROOTS).</summary>
		private static readonly string[] _arrayRoots=new string[] { "OpenPmsDental","OpenPmsDentalBusiness","CodeBase","SparksToothChart" };
		private const string NO_COVERAGE_PREFIX="(outside coverage.tsv";

		private static List<string[]> ReadInventory() {
			string path=Path.Combine(RepoPaths.Root(),"docs","port-notes","UI-13-inventory.tsv");
			Assert.IsTrue(File.Exists(path),"No inventory: run python tools/UiFit/element_inventory.py");
			return File.ReadAllLines(path).Where(x => x!="" && !x.StartsWith("#") && !x.StartsWith("type\t")).Select(x => x.Split('\t')).ToList();
		}

		[TestMethod]
		public void Inventory_EveryTypeHasItsLookOrAReason() {
			List<string[]> listRows=ReadInventory();
			Assert.IsTrue(listRows.Count>100,"The inventory has "+listRows.Count+" rows.");
			HashSet<string> hashSetCoverage=new HashSet<string>(File.ReadAllLines(RepoPaths.DataFile("coverage.tsv"))
				.Where(x => x!="" && !x.StartsWith("#") && !x.StartsWith("files\t")).Select(x => x.Split('\t')[0]));
			List<string> listProblems=new List<string>();
			foreach(string[] row in listRows) {
				if(row.Length!=8) {
					listProblems.Add(string.Join(" | ",row)+": not 8 columns");
					continue;
				}
				string type=row[0];
				string styled=row[4];
				string by=row[5];
				string coverage=row[6];
				string reason=row[7];
				if(styled=="yes") {
					if(by=="") {
						listProblems.Add(type+": styled with no code named");
					}
					if(coverage=="") {
						listProblems.Add(type+": styled with no coverage row and no reason for none");
					}
				}
				else if(styled=="keeps" || styled=="n/a") {
					if(reason=="") {
						listProblems.Add(type+": "+styled+" with no reason");
					}
				}
				else {
					listProblems.Add(type+": styled is '"+styled+"' (yes, keeps or n/a): "+reason);
				}
				if(coverage!="" && !coverage.StartsWith(NO_COVERAGE_PREFIX) && !hashSetCoverage.Contains(coverage)) {
					listProblems.Add(type+": coverage row '"+coverage+"' is not in coverage.tsv");
				}
			}
			Assert.AreEqual(0,listProblems.Count,"The inventory (docs/port-notes/UI-13-inventory.tsv):\r\n"+string.Join("\r\n",listProblems));
		}

		[TestMethod]
		public void Inventory_DesignerCountsMatchTheDesignerFiles() {
			Dictionary<string,int> dictCounts=new Dictionary<string,int>();
			Regex regex=new Regex(@"this\.\w+ = new ([\w.]+)\(\);");
			foreach(string root in _arrayRoots) {
				foreach(string path in Directory.EnumerateFiles(Path.Combine(RepoPaths.Root(),root),"*.cs",SearchOption.AllDirectories)) {
					string[] arrayParts=path.Substring(RepoPaths.Root().Length).Replace('\\','/').Split('/');
					if(arrayParts.Contains("bin") || arrayParts.Contains("obj") || !path.EndsWith(".designer.cs",StringComparison.OrdinalIgnoreCase)) {
						continue;
					}
					foreach(Match match in regex.Matches(File.ReadAllText(path))) {
						string type=match.Groups[1].Value;
						if(type.StartsWith("UI.")) {
							type="OpenPmsDental."+type;
						}
						dictCounts.TryGetValue(type,out int count);
						dictCounts[type]=count+1;
					}
				}
			}
			Dictionary<string,string[]> dictRows=ReadInventory().Where(x => x[1]=="stock" || x[1]=="own control").ToDictionary(x => x[0]);
			List<string> listProblems=new List<string>();
			foreach(KeyValuePair<string,int> pair in dictCounts.OrderBy(x => x.Key,StringComparer.Ordinal)) {
				if(!dictRows.TryGetValue(pair.Key,out string[] row)) {
					listProblems.Add(pair.Key+": in "+pair.Value+" designer lines, not in the inventory");
					continue;
				}
				if(row[2]!=pair.Value.ToString()) {
					listProblems.Add(pair.Key+": "+pair.Value+" designer lines, the inventory says "+row[2]);
				}
			}
			Assert.AreEqual(0,listProblems.Count,"Run python tools/UiFit/element_inventory.py and give each new type its look or reason:\r\n"
				+string.Join("\r\n",listProblems));
		}
	}
}
