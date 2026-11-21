// UI-2a-2 (docs/specs/ui-refresh-plan.md, section 5.1 "No container scales by font"; docs/port-notes/UI-2a.md):
// 1. The ban: no AutoScaleMode.Font assignment anywhere in product code (UnitTests and PluginExample excepted). Only the part of a line before
//    a // comment counts, so the comments in FormODBase and FormFrame and ODWebView2's commented-out line do not trip it.
// 2. The 12 graph UserControls take the mode the package chose: Inherit (the plain form that hosts them scales each nested control once).
// 3. The five UserControls that LayoutManager scales are None; the plain exes' forms are Dpi with 96F,96F.
// It reads source only: no build, database or network, and starts nothing.
// Licensed under the GNU General Public License, version 2 or later. See LICENSE in the repository root.
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace NoHqTests {
	[TestClass]
	public class AutoScaleModeScanTests {
		private static readonly Regex _regexFont=new Regex(@"AutoScaleMode\s*=\s*(System\.Windows\.Forms\.)?AutoScaleMode\.Font\b");
		private static readonly Regex _regexMode=new Regex(@"AutoScaleMode\s*=\s*(?:System\.Windows\.Forms\.)?AutoScaleMode\.(\w+)\s*;");
		private static readonly string[] _arrayExcludedFolders=new string[] { "UnitTests","PluginExample","packages","obj","bin","tests",".git",".claude","node_modules" };
		///<summary>The 12 OpenPmsDentalGraph UserControls (section 5.1).</summary>
		public static readonly string[] ArrayGraphControls=new string[] {
			@"OpenPmsDentalGraph\Dashboard\DashboardTabCtrl.Designer.cs",@"OpenPmsDentalGraph\Dashboard\DashboardPanelCtrl.Designer.cs",
			@"OpenPmsDentalGraph\Dashboard\DashboardCellCtrl.Designer.cs",@"OpenPmsDentalGraph\Base\GraphIncome.Designer.cs",
			@"OpenPmsDentalGraph\Base\GraphQuantityOverTime.Designer.cs",@"OpenPmsDentalGraph\Concrete\GraphQuantityOverTimeFilter.Designer.cs",
			@"OpenPmsDentalGraph\Base\GroupingOptionsCtrl.Designer.cs",@"OpenPmsDentalGraph\Base\BaseGraphOptionsCtrl.Designer.cs",
			@"OpenPmsDentalGraph\Concrete\BrokenApptGraphOptionsCtrl.Designer.cs",@"OpenPmsDentalGraph\Concrete\HqMessagesRealTimeOptionsCtrl.Designer.cs",
			@"OpenPmsDentalGraph\Concrete\IncomeGraphOptionsCtrl.Designer.cs",@"OpenPmsDentalGraph\Concrete\ProductionGraphOptionsCtrl.Designer.cs" };
		///<summary>UserControls that a FormODBase window hosts through LayoutManager, which scales them already.</summary>
		public static readonly string[] ArrayLayoutManagerControls=new string[] {
			@"OpenPmsDental\User Controls\ControlImageDisplay.Designer.cs",@"OpenPmsDental\User Controls\ControlImageDock.Designer.cs",
			@"CodeBase\Controls\PrintPanel.Designer.cs",@"SparksToothChart\ToothChart2D.Designer.cs",@"SparksToothChart\ToothChartWrapper.Designer.cs" };
		///<summary>Plain forms that stand for each plain exe and the graph's forms.</summary>
		public static readonly string[] ArrayPlainForms=new string[] {
			@"CentralManager\FormCentralManager.Designer.cs",@"CentralManager\FormCentralConnections.Designer.cs",@"ServiceManager\FormMain.Designer.cs",
			@"MySQLManager\FormMySQLManager.Designer.cs",@"OpenPmsDentalHL7\FormDebug.Designer.cs",@"UpdateFileCopier\FormMain.Designer.cs",
			@"DatabaseIntegrityCheck\FormDatabaseCheck.Designer.cs",@"DocumentationBuilder\Form1.Designer.cs",@"xCrudGenerator\Form1.Designer.cs",
			@"OpenPmsDentalGraph\Dashboard\FormDashboardEditTab.Designer.cs",@"OpenPmsDentalDbSettings\FormDbSettings.cs",@"OpenPmsDentalDbSettings\FormSecretPrompt.cs" };

		[TestMethod]
		public void Ban_NoAutoScaleModeFontInProductCode() {
			string pathRoot=RemovedAssembliesTests.GetRepositoryRoot();
			List<string> listFiles=GetSourceFiles(pathRoot);
			Assert.IsTrue(listFiles.Count>2000,"Expected the product source files, found "+listFiles.Count+".");
			List<string> listHits=new List<string>();
			foreach(string path in listFiles) {
				string[] arrayLines=File.ReadAllLines(path);
				for(int i=0;i<arrayLines.Length;i++) {
					if(IsFontAssignment(arrayLines[i])) {
						listHits.Add(path.Substring(pathRoot.Length+1)+":"+(i+1));
					}
				}
			}
			Assert.AreEqual(0,listHits.Count,"AutoScaleMode.Font is banned (UI-2a-2, section 5.1): use Dpi with AutoScaleDimensions 96F,96F, Inherit or None.\r\n"
				+string.Join("\r\n",listHits));
		}

		[TestMethod]
		public void Ban_Rule_SkipsCommentsAndMatchesAssignments() {
			Assert.IsTrue(IsFontAssignment("\t\t\tthis.AutoScaleMode = System.Windows.Forms.AutoScaleMode."+"Font;"));
			Assert.IsTrue(IsFontAssignment("AutoScaleMode=AutoScaleMode."+"Font;"));
			Assert.IsFalse(IsFontAssignment("\t\t\t//this.AutoScaleMode = System.Windows.Forms.AutoScaleMode."+"Font;"));
			Assert.IsFalse(IsFontAssignment("this.AutoScaleMode=AutoScaleMode.None;//AutoScaleMode.Font is a huge problem."));
			Assert.IsFalse(IsFontAssignment("this.AutoScaleMode=AutoScaleMode.Dpi;"));
		}

		[TestMethod]
		public void GraphUserControls_AreInherit() {
			string pathRoot=RemovedAssembliesTests.GetRepositoryRoot();
			Assert.AreEqual(12,ArrayGraphControls.Length);
			foreach(string file in ArrayGraphControls) {
				Assert.AreEqual("Inherit",GetMode(Path.Combine(pathRoot,file)),file+": a graph UserControl is hosted by a plain form, so it inherits that form's mode.");
			}
		}

		[TestMethod]
		public void LayoutManagerUserControls_AreNone() {
			string pathRoot=RemovedAssembliesTests.GetRepositoryRoot();
			foreach(string file in ArrayLayoutManagerControls) {
				Assert.AreEqual("None",GetMode(Path.Combine(pathRoot,file)),file+": LayoutManager scales this control.");
			}
		}

		[TestMethod]
		public void PlainForms_AreDpiWith96() {
			string pathRoot=RemovedAssembliesTests.GetRepositoryRoot();
			foreach(string file in ArrayPlainForms) {
				string path=Path.Combine(pathRoot,file);
				Assert.AreEqual("Dpi",GetMode(path),file+": a plain exe form scales by the DPI ratio.");
				Assert.IsTrue(Regex.IsMatch(File.ReadAllText(path),@"AutoScaleDimensions\s*=\s*new (System\.Drawing\.)?SizeF\(96F, ?96F\)"),file+": AutoScaleDimensions must be 96F,96F.");
			}
		}

		private static bool IsFontAssignment(string line) {
			int idx=line.IndexOf("//",StringComparison.Ordinal);
			if(idx>=0) {
				line=line.Substring(0,idx);
			}
			return _regexFont.IsMatch(line);
		}

		private static string GetMode(string path) {
			Assert.IsTrue(File.Exists(path),"No file "+path);
			MatchCollection matches=_regexMode.Matches(string.Join("\n",File.ReadAllLines(path).Select(x => x.Contains("//") ? x.Substring(0,x.IndexOf("//",StringComparison.Ordinal)) : x)));
			Assert.AreEqual(1,matches.Count,path+": expected exactly one AutoScaleMode assignment.");
			return matches[0].Groups[1].Value;
		}

		private static List<string> GetSourceFiles(string pathRoot) {
			List<string> listFiles=new List<string>();
			foreach(string dir in Directory.EnumerateDirectories(pathRoot)) {
				string name=Path.GetFileName(dir);
				if(_arrayExcludedFolders.Contains(name,StringComparer.OrdinalIgnoreCase)) {
					continue;
				}
				listFiles.AddRange(Directory.EnumerateFiles(dir,"*.cs",SearchOption.AllDirectories)
					.Where(x => !x.Contains(@"\obj\") && !x.Contains(@"\bin\") && !x.Contains(@"\PluginExample\") && !x.Contains(@"\UnitTests\")));
			}
			return listFiles;
		}
	}
}
