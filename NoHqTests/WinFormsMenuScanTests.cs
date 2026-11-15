// S3a-M3a (net10 plan, stage 3a): no WinForms ContextMenu, MenuItem, MainMenu, Menu or DataGrid, and no Control.ContextMenu, in the shipped code,
// RdlDesign, CentralManager and UnitTests' UI Testing. On .NET 10 these types are binary-compatibility shims that cannot be instantiated (WFDEV006,
// https://learn.microsoft.com/en-us/dotnet/desktop/winforms/wfdev-diagnostics/wfdev006); S3a-M1, S3a-M2 and S3a-M3a moved every use to
// ContextMenuStrip, ToolStripMenuItem, MenuStrip and DataGridView. A source scan of the C# projects of the solution filter (as PointerSizeScanTests:
// not tests\, tools\, third-party\, UnitTests, UnitTestsCore, xCrudGenerator), plus the folder UnitTests\UI Testing. Comments and string contents
// are not code. Three rules:
// 1. A qualified name: System.Windows.Forms.ContextMenu (and MenuItem, MainMenu, Menu, the DataGrid types, Shortcut, MenuMerge), also through a
//    using alias of System.Windows.Forms, and a using alias of one of those types.
// 2. In a file that imports System.Windows.Forms and none of the WPF namespaces that have types of the same names (System.Windows.Controls,
//    WpfControls.UI, WpfControls): the bare type names, and the WinForms members Control.ContextMenu, Form.Menu, MenuItem.Popup, the DataGrid's
//    TableStyles, GridColumnStyles and SetDataBinding, and the toolbar's old DropDownMenu.
// 3. In the same files: the members only WinForms menus have (MenuItems, GetContextMenu, MergedMenu), except the lines of _listNotWinForms
//    (WebView2's own context menu has MenuItems).
// Known pending, for S3a-M3b (after EHR-7): OpenPmsDental\Forms\FormMedical.cs (its Allergies and Problems grids), and the bridges it needs, in
// regions named "Bridge for S3a-M3b" in GridOD.cs, PopupHelper.cs and Lan.cs. The test fails when one of them no longer has a WinForms menu,
// so that M3b removes it from the list. It needs no build, database or network, and starts nothing.
// Licensed under the GNU General Public License, version 2 or later. See LICENSE in the repository root.
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace NoHqTests {
	[TestClass]
	public class WinFormsMenuScanTests {
		///<summary>The one file that keeps WinForms menus until S3a-M3b (FormMedical is EHR-7's until it merges).</summary>
		public static readonly string[] ArrayFilesPending=new[] { @"OpenPmsDental\Forms\FormMedical.cs" };
		///<summary>The files whose regions named "Bridge for S3a-M3b" keep the WinForms code FormMedical needs: GridOD's ContextMenu path, the links as
		///MenuItems for it, and Lan's translation of a grid's ContextMenu. Removed by S3a-M3b with FormMedical's menus.</summary>
		public static readonly string[] ArrayFilesBridge=new[] { @"OpenPmsDentalBusiness\UI\GridOD.cs",@"OpenPmsDentalBusiness\UI\PopupHelper.cs",
			@"OpenPmsDental\Logic\Lan.cs" };
		public const string BRIDGE_REGION="Bridge for S3a-M3b";

		private const string TYPES=@"ContextMenu|MenuItem|MainMenu|Menu|DataGrid|DataGridTableStyle|DataGridColumnStyle|DataGridTextBoxColumn|DataGridTextBox|DataGridBoolColumn"
			+@"|DataGridCell|DataGridLineStyle|GridColumnStylesCollection|GridTableStylesCollection|Shortcut|MenuMerge";
		private static readonly Regex _regexQualified=new Regex(@"\bSystem\s*\.\s*Windows\s*\.\s*Forms\s*\.\s*(?:"+TYPES+@")\b");
		private static readonly Regex _regexAliasNamespace=new Regex(@"^\s*using\s+(\w+)\s*=\s*(?:global\s*::\s*)?System\s*\.\s*Windows\s*\.\s*Forms\s*;");
		private static readonly Regex _regexUsingForms=new Regex(@"^\s*using\s+System\s*\.\s*Windows\s*\.\s*Forms\s*;");
		private static readonly Regex _regexUsingWpf=new Regex(@"^\s*using\s+(?:System\s*\.\s*Windows\s*\.\s*Controls|WpfControls\s*\.\s*UI|WpfControls)\s*;");
		///<summary>A bare type name: not after a dot or a word character (so not a member and not part of a longer name).</summary>
		private static readonly Regex _regexBareType=new Regex(@"(?<![\.\w])(?:ContextMenu|MenuItem|MainMenu|DataGrid|DataGridTableStyle|DataGridColumnStyle"
			+@"|DataGridTextBoxColumn|DataGridTextBox|DataGridBoolColumn|DataGridCell|GridColumnStylesCollection|GridTableStylesCollection|MenuMerge)\b"
			+@"|(?<![\.\w])Menu\s*(?:\.\s*MenuItemCollection\b|\[\s*\])|(?<![\.\w])Shortcut\s*\.\s*\w");
		///<summary>WinForms members, in a WinForms file: Control.ContextMenu, Form.Menu (not Keys.Menu, the Alt key), MenuItem.Popup, the DataGrid's
		///table styles and SetDataBinding, ODToolBarButton's old DropDownMenu.</summary>
		private static readonly Regex _regexFormsMember=new Regex(@"(?<!\b(?:UI|Controls)\s*)\.\s*ContextMenu\b|(?<!\b(?:Keys|UI|Controls)\s*)\.\s*Menu\b"
			+@"|\.\s*Popup\s*[+-]?=|\.\s*TableStyles\b|\.\s*GridColumnStyles\b|\.\s*SetDataBinding\s*\(|\.\s*DropDownMenu\b");
		///<summary>Members only the WinForms menus have among the types a WinForms file uses: MenuItems, GetContextMenu, MergedMenu (WPF has none of
		///them; WebView2's context menu event has MenuItems of its own, see _listNotWinForms).</summary>
		private static readonly Regex _regexMenuOnlyMember=new Regex(@"\.\s*(?:MenuItems|GetContextMenu|MergedMenu)\b");
		///<summary>Lines that the rules find but that are not WinForms menus: (file, a regex of the line's code, why).</summary>
		private static readonly List<Tuple<string,Regex,string>> _listNotWinForms=new List<Tuple<string,Regex,string>> {
			Tuple.Create(@"CodeBase\Controls\ODWebView2.cs",new Regex(@"\be\s*\.\s*MenuItems\b"),
				"WebView2's CoreWebView2ContextMenuRequestedEventArgs.MenuItems (the browser's own right-click menu), not a WinForms menu."),
		};
		private static readonly Regex _regexRegion=new Regex(@"^\s*#\s*region\b\s*(.*)$");
		private static readonly Regex _regexEndRegion=new Regex(@"^\s*#\s*endregion\b");

		[TestMethod]
		public void ShippedSource_UsesNoWinFormsMenuOrDataGrid() {
			string pathRoot=RemovedAssembliesTests.GetRepositoryRoot();
			List<string> listFolders=GetScannedFolders(pathRoot);
			List<string> listNames=listFolders.Select(x => x.Substring(pathRoot.Length+1)).ToList();
			foreach(string name in new[] { "OpenPmsDentalBusiness","OpenPmsDental","WpfControlsOD","CodeBase","ODR","RdlDesign","CentralManager",
				@"UnitTests\UI Testing" })
			{
				Assert.IsTrue(listNames.Contains(name),"The scan must include "+name+".");
			}
			foreach(string name in new[] { "xCrudGenerator","UnitTests","UnitTestsCore","NoHqTests" }) {
				Assert.IsFalse(listNames.Contains(name),"The scan leaves out "+name+" (UnitTests only but its UI Testing folder).");
			}
			List<SourceFile> listFiles=ReadFiles(pathRoot,listFolders);
			Assert.IsTrue(listFiles.Count>1000,"Expected the shipped source files, found "+listFiles.Count+".");
			foreach(string file in ArrayFilesPending.Concat(ArrayFilesBridge)) {
				Assert.IsTrue(listFiles.Any(x => x.PathRelative.Equals(file,StringComparison.OrdinalIgnoreCase)),"The scan must read "+file+".");
			}
			//Sanity: the scan sees the ToolStrip menus and DataGridViews that replaced them, so a broken reader cannot pass by finding nothing.
			Assert.IsTrue(listFiles.Count(x => x.Code.Contains("ContextMenuStrip"))>=100,"Expected the ContextMenuStrips of the program.");
			Assert.IsTrue(listFiles.Count(x => x.Code.Contains("DataGridView"))>=11,"Expected the DataGridViews of FormQuery and RdlDesign.");
			List<string> listProblems=new List<string>();
			Dictionary<string,int> dictCountsPending=new Dictionary<string,int>(StringComparer.OrdinalIgnoreCase);
			foreach(SourceFile sourceFile in listFiles) {
				List<Finding> listFindings=FindUses(sourceFile);
				if(ArrayFilesPending.Contains(sourceFile.PathRelative,StringComparer.OrdinalIgnoreCase)) {
					dictCountsPending[sourceFile.PathRelative]=listFindings.Count;
					continue;
				}
				if(ArrayFilesBridge.Contains(sourceFile.PathRelative,StringComparer.OrdinalIgnoreCase)) {
					dictCountsPending[sourceFile.PathRelative]=listFindings.Count(x => x.IsInBridgeRegion);
				}
				listProblems.AddRange(listFindings.Where(x => !x.IsInBridgeRegion || !ArrayFilesBridge.Contains(sourceFile.PathRelative,StringComparer.OrdinalIgnoreCase))
					.Select(x => sourceFile.PathRelative+":"+x.Line+": "+x.Rule+": "+x.Text));
			}
			Assert.AreEqual(0,listProblems.Count,"WinForms menus or DataGrids, which .NET 10 cannot instantiate (S3a-M3, docs/port-notes/S3a-M3.md): use "
				+"ContextMenuStrip, ToolStripMenuItem, MenuStrip and DataGridView.\r\n"+string.Join("\r\n",listProblems));
			foreach(string file in ArrayFilesPending.Concat(ArrayFilesBridge)) {
				Assert.IsTrue(dictCountsPending[file]>0,file+" has no WinForms menu left (in its \""+BRIDGE_REGION+"\" regions for a bridge file): S3a-M3b "
					+"removes it from ArrayFilesPending or ArrayFilesBridge, and removes the bridges once FormMedical is converted.");
			}
		}

		#region Self-test of the scanner
		[TestMethod]
		public void Scan_QualifiedTypes_AreFoundEverywhere() {
			AssertFound(new[] { "using System.Windows.Controls;","var menu=new System.Windows.Forms.ContextMenu();" },"2 qualified");
			AssertFound(new[] { "private System.Windows.Forms.MenuItem menuItem1;" },"1 qualified");
			AssertFound(new[] { "this.grid2 = new System.Windows.Forms.DataGrid();","System.Windows.Forms.DataGridTableStyle ts;" },"1 qualified","2 qualified");
			AssertFound(new[] { "this.Y.Shortcut = System.Windows.Forms.Shortcut.CtrlN;" },"1 qualified");
			AssertFound(new[] { "using WinForms=System.Windows.Forms;","WinForms.MainMenu mainMenu=null;" },"2 qualified");
			AssertFound(new[] { "using CM=System.Windows.Forms.ContextMenu;" },"1 qualified");
			//The replacements, comments and strings are not found.
			AssertFound(new[] { "var strip=new System.Windows.Forms.ContextMenuStrip();","System.Windows.Forms.ToolStripMenuItem item;",
				"System.Windows.Forms.DataGridView grid;","System.Windows.Forms.MenuStrip menuStrip;","//new System.Windows.Forms.ContextMenu()",
				"string s=\"System.Windows.Forms.MenuItem\";","/* System.Windows.Forms.DataGrid */" });
		}

		[TestMethod]
		public void Scan_BareTypesAndMembers_AreFoundInWinFormsFiles() {
			AssertFound(new[] { "using System.Windows.Forms;","ContextMenu contextMenu=new ContextMenu();" },"2 bare type");
			AssertFound(new[] { "using System.Windows.Forms;","MenuItem item=(MenuItem)sender;","menu.MenuItems.Add(item);" },"2 bare type","3 menu member");
			AssertFound(new[] { "using System.Windows.Forms;","Menu.MenuItemCollection collection=null;","void C(params Menu[] menus) {}" },"2 bare type","3 bare type");
			AssertFound(new[] { "using System.Windows.Forms;","item.ShortcutKeys=Shortcut.CtrlX;" },"2 bare type");
			AssertFound(new[] { "using System.Windows.Forms;","if(control is DataGridTextBox) {}" },"2 bare type");
			AssertFound(new[] { "using System.Windows.Forms;","DataGridTextBoxColumn column;","grid.TableStyles.Clear();","grid.SetDataBinding(table,\"\");" },
				"2 bare type","3 forms member","4 forms member");
			AssertFound(new[] { "using System.Windows.Forms;","grid.ContextMenu=menu;","this.Menu=mainMenu;","menu.Popup+=Menu_Popup;","button.DropDownMenu=menu;" },
				"2 forms member","3 forms member","4 forms member","5 forms member");
			AssertFound(new[] { "using System.Windows.Forms;","Form form=null;","form.MergedMenu.GetContextMenu();" },"3 menu member");
			//Not WinForms: the replacements, Keys.Menu (the Alt key), longer names, members of other types, comments.
			AssertFound(new[] { "using System.Windows.Forms;","ContextMenuStrip strip=new ContextMenuStrip();","ToolStripMenuItem item=new ToolStripMenuItem();",
				"DataGridView grid=null;","DataGridViewTextBoxEditingControl editingControl=null;","MenuItemOD menuItemOD=null;","if(keyData==Keys.Menu) {}","grid.ContextMenuStrip=strip;","strip.Opening+=Strip_Opening;",
				"button.ContextMenuStripDropDown=strip;","bool show=grid.ShowContextMenu;","//grid.ContextMenu=menu;","x.MenuItemsCount=1;" });
		}

		[TestMethod]
		public void Scan_WpfFiles_OnlyQualifiedAndMenuOnlyMembers() {
			//WPF files have ContextMenu and MenuItem of their own (System.Windows.Controls, WpfControls.UI): bare names and .ContextMenu are theirs.
			AssertFound(new[] { "using System.Windows.Controls;","ContextMenu contextMenu=new ContextMenu();","MenuItem menuItem=new MenuItem();",
				"frameworkElement.ContextMenu=contextMenu;","DataGrid dataGrid=new DataGrid();" });
			AssertFound(new[] { "using System.Windows.Forms;","using WpfControls.UI;","MenuItem menuItem=new MenuItem();" });
			AssertFound(new[] { "using System.Windows.Forms;","using System.Windows.Controls;","var menu=new System.Windows.Forms.ContextMenu();",
				"wpfMenu.Items.Count;" },"3 qualified");
			//A WinForms file may name the WPF types by their namespace (ControlImages' toolbar menus).
			AssertFound(new[] { "using System.Windows.Forms;","WpfControls.UI.ContextMenu contextMenuPrint=new WpfControls.UI.ContextMenu();",
				"System.Windows.Controls.MenuItem menuItem=null;","var menu=new WpfControls.UI.Menu();" });
			//Without using System.Windows.Forms, bare names are someone else's types.
			AssertFound(new[] { "namespace Other {","class ContextMenu {}","class X { ContextMenu c=new ContextMenu(); }","}" });
		}

		[TestMethod]
		public void Scan_BridgeRegion_IsMarked() {
			string[] arrayLines=new[] {
				"using System.Windows.Forms;",
				"class GridOD {",
				"\t#region Bridge for S3a-M3b - WinForms ContextMenu set by a caller",
				"\tprivate List<MenuItem> _listMenuItemsLinks;",
				"\t#region Inner",
				"\tvoid F() { ContextMenu.Popup+=ContextMenu_Popup; }",
				"\t#endregion Inner",
				"\t#endregion Bridge for S3a-M3b - WinForms ContextMenu set by a caller",
				"\tprivate MenuItem _menuItemOutside;",
				"}",
			};
			List<Finding> listFindings=FindUses(new SourceFile(@"Test\Planted.cs",arrayLines));
			CollectionAssert.AreEqual(new[] { 4,6,9 },listFindings.Select(x => x.Line).ToArray(),string.Join(" | ",listFindings.Select(x => x.Line+" "+x.Rule)));
			CollectionAssert.AreEqual(new[] { true,true,false },listFindings.Select(x => x.IsInBridgeRegion).ToArray());
		}

		private static void AssertFound(string[] arrayLines,params string[] arrayExpected) {
			List<Finding> listFindings=FindUses(new SourceFile(@"Test\Planted.cs",arrayLines));
			List<string> listFound=listFindings.Select(x => x.Line+" "+x.Rule).ToList();
			CollectionAssert.AreEquivalent(arrayExpected,listFound,"Found: "+string.Join(" | ",listFindings.Select(x => x.Line+" "+x.Rule+": "+x.Text)));
		}
		#endregion Self-test of the scanner

		#region Rules
		private class Finding {
			public int Line;
			public string Rule;
			public string Text;
			public bool IsInBridgeRegion;
		}

		///<summary>The WinForms menu and DataGrid uses of a file, one per line and rule.</summary>
		private static List<Finding> FindUses(SourceFile sourceFile) {
			List<Finding> listFindings=new List<Finding>();
			bool isFormsFile=sourceFile.ArrayCode.Any(x => _regexUsingForms.IsMatch(x));
			bool isWpfFile=sourceFile.ArrayCode.Any(x => _regexUsingWpf.IsMatch(x));
			List<string> listAliases=sourceFile.ArrayCode.Select(x => _regexAliasNamespace.Match(x)).Where(x => x.Success).Select(x => x.Groups[1].Value).ToList();
			Regex regexAlias=listAliases.Count==0 ? null
				: new Regex(@"(?<![\.\w])(?:"+string.Join("|",listAliases.Select(x => Regex.Escape(x)))+@")\s*\.\s*(?:"+TYPES+@")\b");
			List<string> listRegions=new List<string>();
			for(int i=0;i<sourceFile.ArrayCode.Length;i++) {
				string code=sourceFile.ArrayCode[i];
				Match matchRegion=_regexRegion.Match(code);
				if(matchRegion.Success) {
					listRegions.Add(sourceFile.ArrayRaw[i].Trim());
					continue;
				}
				if(_regexEndRegion.IsMatch(code)) {
					if(listRegions.Count>0) {
						listRegions.RemoveAt(listRegions.Count-1);
					}
					continue;
				}
				string rule=null;
				if(_regexQualified.IsMatch(code) || (regexAlias!=null && regexAlias.IsMatch(code))) {
					rule="qualified";
				}
				else if(isFormsFile && !isWpfFile && _regexBareType.IsMatch(code)) {
					rule="bare type";
				}
				else if(isFormsFile && !isWpfFile && _regexFormsMember.IsMatch(code)) {
					rule="forms member";
				}
				else if(isFormsFile && !isWpfFile && _regexMenuOnlyMember.IsMatch(code)) {
					rule="menu member";
				}
				if(rule!=null && _listNotWinForms.Any(x => x.Item1.Equals(sourceFile.PathRelative,StringComparison.OrdinalIgnoreCase) && x.Item2.IsMatch(code))) {
					rule=null;
				}
				if(rule==null) {
					continue;
				}
				listFindings.Add(new Finding { Line=i+1,Rule=rule,Text=sourceFile.ArrayRaw[i].Trim(),
					IsInBridgeRegion=listRegions.Any(x => x.Contains("#region "+BRIDGE_REGION)) });
			}
			return listFindings;
		}
		#endregion Rules

		#region Helpers
		private class SourceFile {
			public string PathRelative;
			///<summary>The lines as they are in the file.</summary>
			public string[] ArrayRaw;
			///<summary>Per line, comments removed and string and character literals emptied (the quotes stay).</summary>
			public string[] ArrayCode;
			///<summary>ArrayCode joined with \n.</summary>
			public string Code;

			public SourceFile(string pathRelative,string[] arrayLines) {
				PathRelative=pathRelative;
				ArrayRaw=arrayLines;
				ArrayCode=RemoveComments(arrayLines);
				Code=string.Join("\n",ArrayCode);
			}
		}

		private static List<SourceFile> ReadFiles(string pathRoot,List<string> listFolders) {
			List<SourceFile> listFiles=new List<SourceFile>();
			foreach(string folder in listFolders) {
				foreach(string path in Directory.EnumerateFiles(folder,"*.cs",SearchOption.AllDirectories)) {
					if(IsBuildFolder(folder,path)) {
						continue;
					}
					listFiles.Add(new SourceFile(path.Substring(pathRoot.Length+1),File.ReadAllLines(path)));
				}
			}
			return listFiles.GroupBy(x => x.PathRelative,StringComparer.OrdinalIgnoreCase).Select(x => x.First()).ToList();
		}

		///<summary>Each line with comments removed, and with the contents of string and character literals emptied (the quotes stay). Block comments
		///and verbatim strings may span lines: the state carries from one line to the next. The same remover as PointerSizeScanTests (S3a-12).</summary>
		private static string[] RemoveComments(string[] arrayLines) {
			string[] arrayResult=new string[arrayLines.Length];
			bool isInBlockComment=false;
			bool isInVerbatimString=false;
			for(int index=0;index<arrayLines.Length;index++) {
				string line=arrayLines[index];
				StringBuilder stringBuilder=new StringBuilder();
				int i=0;
				while(i<line.Length) {
					if(isInBlockComment) {
						int indexEnd=line.IndexOf("*/",i,StringComparison.Ordinal);
						if(indexEnd<0) {
							i=line.Length;
							continue;
						}
						isInBlockComment=false;
						i=indexEnd+2;
						continue;
					}
					if(isInVerbatimString) {
						if(line[i]=='"') {
							if(i+1<line.Length && line[i+1]=='"') {
								i+=2;
								continue;
							}
							isInVerbatimString=false;
							stringBuilder.Append('"');
						}
						i++;
						continue;
					}
					char c=line[i];
					if(c=='/' && i+1<line.Length && line[i+1]=='/') {
						break;
					}
					if(c=='/' && i+1<line.Length && line[i+1]=='*') {
						isInBlockComment=true;
						i+=2;
						continue;
					}
					if(c=='@' && i+1<line.Length && line[i+1]=='"') {
						stringBuilder.Append("@\"");
						isInVerbatimString=true;
						i+=2;
						continue;
					}
					if((c=='$' && i+2<line.Length && line[i+1]=='@' && line[i+2]=='"') || (c=='@' && i+2<line.Length && line[i+1]=='$' && line[i+2]=='"')) {
						stringBuilder.Append("@\"");
						isInVerbatimString=true;
						i+=3;
						continue;
					}
					if(c=='"' || c=='\'') {
						stringBuilder.Append(c);
						i++;
						while(i<line.Length && line[i]!=c) {
							if(line[i]=='\\') {
								i+=2;
								continue;
							}
							i++;
						}
						if(i<line.Length) {
							stringBuilder.Append(c);
							i++;
						}
						continue;
					}
					stringBuilder.Append(c);
					i++;
				}
				arrayResult[index]=stringBuilder.ToString();
			}
			return arrayResult;
		}

		///<summary>The folders of the C# projects in the solution filter: the shipped ones, RdlDesign, DocumentationBuilder and CentralManager among
		///them (not tests\, tools\, third-party\, UnitTests, UnitTestsCore, xCrudGenerator, or this project), and UnitTests\UI Testing.</summary>
		private static List<string> GetScannedFolders(string pathRoot) {
			string pathSlnf=Directory.GetFiles(pathRoot,"*.slnf").Single();
			List<string> listFolders=new List<string>();
			foreach(Match match in Regex.Matches(File.ReadAllText(pathSlnf),"\"([^\"]+\\.csproj)\"")) {
				string pathProject=match.Groups[1].Value.Replace("\\\\","\\");
				string firstPart=pathProject.Split('\\')[0];
				if(new[] { "tests","NoHqTests","tools","third-party","UnitTests","UnitTestsCore","xCrudGenerator" }.Contains(firstPart,StringComparer.OrdinalIgnoreCase)) {
					continue;
				}
				listFolders.Add(Path.GetDirectoryName(Path.Combine(pathRoot,pathProject)));
			}
			listFolders.Add(Path.Combine(pathRoot,"UnitTests","UI Testing"));
			return listFolders.Distinct(StringComparer.OrdinalIgnoreCase).ToList();
		}

		private static bool IsBuildFolder(string folder,string path) {
			string[] arrayParts=path.Substring(folder.Length+1).Split(Path.DirectorySeparatorChar);
			return arrayParts.Any(x => x.Equals("bin",StringComparison.OrdinalIgnoreCase) || x.Equals("obj",StringComparison.OrdinalIgnoreCase));
		}
		#endregion Helpers
	}
}
