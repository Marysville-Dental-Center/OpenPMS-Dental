// Source checks behind two rows of the stage 3b coverage table (docs/port-notes/WINCHK-HEADLESS.md, WINCHK-5 section 3), without the opt-in
// and without starting anything: "ImageLists created in code" (.NET 8 made Depth32Bit the default ColorDepth of an ImageList made without a
// stream; the .NET Framework's was Depth8Bit), and "the WPF Frm calendar pop-up" (D6 shows WindowMonthCalendar below a combo box because no
// production Frm has a DatePicker). Each test reads the product's .cs and .xaml files of the worktree.
// Licensed under the GNU General Public License, version 2 or later. See LICENSE in the repository root.
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace WindowChecksTests {
	[TestClass]
	public class SourceScanTests {
		///<summary>The product's source folders: every top-level folder of the repository but tests, the legacy UnitTests, docs, tools and the
		///hidden ones.</summary>
		private static List<string> ProductFiles(string pattern) {
			string root=WinChkEnv.RepoRoot;
			string[] arrayExcluded=new[] { "tests","UnitTests","docs","tools","packages","db" };
			return Directory.GetDirectories(root).Where(x => !Path.GetFileName(x).StartsWith(".") && !arrayExcluded.Contains(Path.GetFileName(x),StringComparer.OrdinalIgnoreCase))
				.SelectMany(x => Directory.GetFiles(x,pattern,SearchOption.AllDirectories))
				.Where(x => !x.Split(Path.DirectorySeparatorChar).Any(y => y.Equals("bin",StringComparison.OrdinalIgnoreCase) || y.Equals("obj",StringComparison.OrdinalIgnoreCase)))
				.ToList();
		}

		private static bool IsDesignerFile(string path) {
			return Path.GetFileName(path).EndsWith(".designer.cs",StringComparison.OrdinalIgnoreCase);
		}

		///<summary>No ImageList is made outside a designer's InitializeComponent, and every designer ImageList either loads an ImageStream (the
		///stream carries the images at the depth they were stored with) or sets ColorDepth itself. So no ImageList of the program takes the default
		///ColorDepth that .NET 8 changed: the icon edges of the toolbars and trees are as on the .NET Framework (the pairs of the B and C checks show
		///them).</summary>
		[TestMethod]
		public void ImageLists_TakeTheirDepthFromAStreamOrSetIt() {
			Regex regexNew=new Regex(@"new\s+(System\.Windows\.Forms\.)?ImageList\s*\(");
			Regex regexField=new Regex(@"this\.(\w+)\s*=\s*new\s+System\.Windows\.Forms\.ImageList\s*\(");
			List<string> listOutsideDesigners=new List<string>();
			List<string> listWithoutDepth=new List<string>();
			int countStream=0;
			int countDepth=0;
			foreach(string path in ProductFiles("*.cs")) {
				string text=File.ReadAllText(path);
				if(!regexNew.IsMatch(text)) {
					continue;
				}
				string pathShown=path.Substring(WinChkEnv.RepoRoot.Length+1);
				if(!IsDesignerFile(path)) {
					listOutsideDesigners.Add(pathShown);
					continue;
				}
				foreach(Match match in regexField.Matches(text)) {
					string name=match.Groups[1].Value;
					if(text.Contains("this."+name+".ImageStream")) {
						countStream++;
					}
					else if(text.Contains("this."+name+".ColorDepth")) {
						countDepth++;
					}
					else {
						listWithoutDepth.Add(pathShown+": "+name);
					}
				}
			}
			Console.WriteLine("Designer ImageLists: "+countStream+" with an ImageStream, "+countDepth+" with an explicit ColorDepth");
			Assert.AreEqual("",string.Join(", ",listOutsideDesigners),"ImageLists made outside a designer (they take .NET 8's Depth32Bit default)");
			Assert.AreEqual("",string.Join(", ",listWithoutDepth),"Designer ImageLists with neither an ImageStream nor a ColorDepth");
			Assert.IsTrue(countStream+countDepth>0,"The scan found the designer ImageLists");
		}

		///<summary>WpfControls.UI.DatePicker (whose drop-down is WindowMonthCalendar) is used by no production Frm, only by the developer test Frms
		///FrmTestAllControls and FrmTestCombos. When a Frm gets one, D6 should open the calendar from that Frm's DatePicker.</summary>
		[TestMethod]
		public void WpfDatePicker_IsInNoProductionFrm() {
			List<string> listFrms=ProductFiles("*.xaml").Where(x => File.ReadAllText(x).Contains("<ui:DatePicker"))
				.Select(x => Path.GetFileNameWithoutExtension(x)).OrderBy(x => x).ToList();
			CollectionAssert.AreEqual(new[] { "FrmTestAllControls","FrmTestCombos" },listFrms,"Frms with a DatePicker: "+string.Join(", ",listFrms));
		}
	}
}
