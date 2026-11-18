// The token renderer of UI-6 (docs/specs/ui-refresh-plan.md, section 3.6, Menus, and section 7's UI-6 row; docs/port-notes/UI-6.md): set in every
// exe's Main (the two named exceptions, UpdateFileCopier and OpenPmsDentalHelper32, reference no CodeBase and have no menu or tool strip; RdlDesign
// compiles the renderer in as linked files), on every other UI thread the program starts, and kept by Windows Forms per thread; and what it draws:
// the menu bar on Surface, drop-downs SurfaceRaised with a Stroke border, hover SubtleHover, Gridline separators, tool strips SurfaceSubtle with
// buttons that show only on hover or checked. Source scans and off-screen drawing; no window is shown, no database.
// Licensed under the GNU General Public License, version 2 or later. See LICENSE in the repository root.
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Reflection.Metadata;
using System.Reflection.PortableExecutable;
using System.Text.RegularExpressions;
using System.Windows.Forms;
using CodeBase;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using UiRefreshTests.Scan;

namespace UiRefreshTests.Toolbars {
	[TestClass]
	public class RendererTests {
		///<summary>Every Windows Forms exe's Main (as NoHqTests' DefaultFontTests lists them, which also checks that the list is complete).</summary>
		public static readonly string[] ArrayMains=new[] {
			@"OpenPmsDental\Main Modules\ProgramEntry.cs",@"OpenPmsDentalDbSettings\Program.cs",@"ServiceManager\Program.cs",
			@"OpenPmsDentalHL7\Program.cs",@"UpdateFileCopier\Program.cs",@"OpenPmsDentalHelper32\Program.cs",@"CentralManager\Program.cs",
			@"DatabaseIntegrityCheck\Program.cs",@"DocumentationBuilder\Program.cs",@"MySQLManager\Program.cs",@"xCrudGenerator\Program.cs",
			@"RdlDesign\RdlDesigner.cs",
		};
		///<summary>The two exes that do not make the call (section 3.6): they reference no CodeBase and have no menu or tool strip.</summary>
		public static readonly string[] ArrayExceptions=new[] { @"UpdateFileCopier\Program.cs",@"OpenPmsDentalHelper32\Program.cs" };
		///<summary>The files that start an STA thread without the call, with the reason. Every other one must call UiTheme.InitWinForms on it.</summary>
		public static readonly Dictionary<string,string> DictThreadsWithout=new Dictionary<string,string> {
			{ @"ImagingDeviceManager\Capture\TwainCaptureSession.cs","the TWAIN worker: its library references no CodeBase, and its two windows are stock Forms that keep the Windows look and have no menu or tool strip (section 1)" },
			{ @"SparksToothChart\ToothChartWrapper.cs","a WPF thread of the tooth chart (no Windows Forms menu or tool strip)" },
			{ @"SparksToothChart\Wpf3D\ToothChartRenderer.cs","the tooth chart's WPF render thread (no Windows Forms menu or tool strip)" },
			{ @"SparksToothChart\Wpf3D\ToothChartWpfView.cs","a WPF thread of the tooth chart (no Windows Forms menu or tool strip)" },
		};

		#region Per thread
		[TestMethod]
		public void ToolStripManager_KeepsTheRendererPerThread() {
			//.NET 10's source: ToolStripManager keeps its default renderer in a [ThreadStatic] field, so each UI thread needs its own call.
			FieldInfo fieldInfo=typeof(ToolStripManager).GetFields(BindingFlags.Static|BindingFlags.NonPublic|BindingFlags.Public)
				.FirstOrDefault(x => typeof(ToolStripRenderer).IsAssignableFrom(x.FieldType));
			Assert.IsNotNull(fieldInfo,"ToolStripManager has a static renderer field.");
			Assert.IsNotNull(fieldInfo.GetCustomAttribute<ThreadStaticAttribute>(),"ToolStripManager."+fieldInfo.Name+" is [ThreadStatic].");
			//And so it behaves: a thread that called InitWinForms has the token renderer, a thread started afterwards does not until it calls it.
			ToolStripRenderer rendererFirst=null;
			ToolStripRenderer rendererSecondBefore=null;
			ToolStripRenderer rendererSecondAfter=null;
			StaRunnerHelpers.RunSta(() => {
				UiTheme.InitWinForms();
				rendererFirst=ToolStripManager.Renderer;
			});
			StaRunnerHelpers.RunSta(() => {
				rendererSecondBefore=ToolStripManager.Renderer;
				UiTheme.InitWinForms();
				rendererSecondAfter=ToolStripManager.Renderer;
			});
			Assert.IsInstanceOfType(rendererFirst,typeof(UiToolStripRenderer));
			Assert.IsNotInstanceOfType(rendererSecondBefore,typeof(UiToolStripRenderer),"A new thread starts with Windows Forms' own renderer.");
			Assert.IsInstanceOfType(rendererSecondAfter,typeof(UiToolStripRenderer));
			Assert.AreNotSame(rendererFirst,rendererSecondAfter,"Each thread has its own renderer (UiTheme's brushes are per thread).");
		}

		[TestMethod]
		public void SecondUiThread_ItsContextMenusAndMenusDrawWithTheTokens() {
			StaRunnerHelpers.RunSta(() => {
				UiTheme.InitWinForms();
				using(ContextMenuStrip contextMenuStrip=new ContextMenuStrip()) {
					Assert.IsInstanceOfType(contextMenuStrip.Renderer,typeof(UiToolStripRenderer),"A stock context menu takes the thread's renderer.");
				}
				using(OpenPmsDental.UI.MenuOD menuOD=new OpenPmsDental.UI.MenuOD()) {
					ToolStrip toolStrip=(ToolStrip)menuOD.Controls[0];
					Assert.IsInstanceOfType(toolStrip.Renderer,typeof(UiToolStripRenderer),"MenuOD's own MenuRenderer is the token renderer.");
				}
			});
		}
		#endregion Per thread

		#region Source
		[TestMethod]
		public void EveryExesMain_SetsTheRenderer_BeforeAnyWindow_ButTheTwoExceptions() {
			string root=RepoPaths.Root();
			List<string> listProblems=new List<string>();
			foreach(string relative in ArrayMains) {
				string text=ToolBarSources.RemoveComments(File.ReadAllText(Path.Combine(root,relative)).Replace("\r\n","\n"));
				Match matchMain=Regex.Match(text,@"static (void|int) Main\([^)]*\)\s*\{");
				Assert.IsTrue(matchMain.Success,relative+": no Main.");
				string body=text.Substring(matchMain.Index+matchMain.Length);
				int indexCall=Regex.Match(body,@"(CodeBase\.)?UiTheme\.InitWinForms\(\);").Index;
				bool hasCall=Regex.IsMatch(body,@"(CodeBase\.)?UiTheme\.InitWinForms\(\);");
				if(ArrayExceptions.Contains(relative)) {
					if(hasCall) {
						listProblems.Add(relative+": one of the two exceptions calls InitWinForms.");
					}
					string folder=Path.GetDirectoryName(Path.Combine(root,relative));
					string csproj=File.ReadAllText(Directory.GetFiles(folder,"*.csproj").Single());
					if(csproj.Contains("xCodeBase.csproj")) {
						listProblems.Add(relative+": the exception references CodeBase now; then it makes the call like the others.");
					}
					continue;
				}
				if(!hasCall) {
					listProblems.Add(relative+": Main does not call UiTheme.InitWinForms().");
					continue;
				}
				int indexDpi=body.IndexOf("Application.SetHighDpiMode(",StringComparison.Ordinal);
				int indexVisualStyles=body.IndexOf("Application.EnableVisualStyles(",StringComparison.Ordinal);
				int indexRun=Regex.Match(body,@"(?<![\w.])Application\.Run\(").Index;
				if(indexDpi<0 || indexCall<indexDpi) {
					listProblems.Add(relative+": InitWinForms comes before SetHighDpiMode.");
				}
				if((indexVisualStyles>=0 && indexCall>indexVisualStyles) || indexCall>indexRun) {
					listProblems.Add(relative+": InitWinForms comes after EnableVisualStyles or Application.Run (it belongs with the start-up calls).");
				}
			}
			//RdlDesign references no CodeBase: it compiles the renderer and what it needs in as linked files.
			string csprojRdl=File.ReadAllText(Path.Combine(root,@"RdlDesign\ReportDesigner.csproj"));
			foreach(string file in new[] { @"UI\UiTheme.cs",@"UI\UiTheme.Colors.cs",@"UI\UiTheme.Gdi.cs",@"UI\UiTheme.Metrics.cs",@"UI\UiTheme.Renderer.cs",
				@"UI\Icons\UiIcons.cs",@"UI\Icons\UiIcons.Gdi.cs",@"UI\Icons\IconData.g.cs" })
			{
				if(!csprojRdl.Contains(@"<Compile Include=""..\CodeBase\"+file+@"""")) {
					listProblems.Add(@"RdlDesign\ReportDesigner.csproj does not link CodeBase\"+file+".");
				}
			}
			if(csprojRdl.Contains("xCodeBase.csproj")) {
				listProblems.Add(@"RdlDesign\ReportDesigner.csproj references CodeBase: the linked files would then clash with it.");
			}
			Assert.AreEqual(0,listProblems.Count,string.Join("\r\n",listProblems));
		}

		[TestMethod]
		public void EveryExtraUiThread_SetsTheRenderer_OrSaysWhyNot() {
			string root=RepoPaths.Root();
			List<string> listProblems=new List<string>();
			List<string> listFound=new List<string>();
			foreach(string path in Directory.EnumerateFiles(root,"*.cs",SearchOption.AllDirectories)) {
				string relative=path.Substring(root.Length+1);
				if(Regex.IsMatch(relative,@"^(tests|UnitTests|UnitTestsCore|NoHqTests|third-party|spike|tools|\.claude|\.git)\\",RegexOptions.IgnoreCase)
					|| relative.Contains(@"\bin\") || relative.Contains(@"\obj\"))
				{
					continue;
				}
				string text=File.ReadAllText(path);
				if(!text.Contains("ApartmentState.STA")) {
					continue;
				}
				string code=ToolBarSources.RemoveComments(text);
				if(!Regex.IsMatch(code,@"SetApartmentState\(\s*(System\.Threading\.)?ApartmentState\.STA\s*\)")) {
					continue;
				}
				listFound.Add(relative);
				bool hasCall=Regex.IsMatch(code,@"(CodeBase\.)?UiTheme\.InitWinForms\(\);");
				if(!hasCall && !DictThreadsWithout.ContainsKey(relative)) {
					listProblems.Add(relative+" starts an STA thread without UiTheme.InitWinForms() (call it at the thread's start, or list it here with the reason).");
				}
				if(hasCall && DictThreadsWithout.ContainsKey(relative)) {
					listProblems.Add(relative+" calls InitWinForms now: take it off the list.");
				}
			}
			Assert.IsTrue(listFound.Contains(@"CodeBase\Progress\ODProgress.cs"),"The progress windows' thread (ODProgress.ShowProgressBase) is found: "+string.Join(", ",listFound));
			Assert.AreEqual(0,listProblems.Count,string.Join("\r\n",listProblems));
		}

		[TestMethod]
		public void RdlDesign_Built_HasTheLinkedRenderer() {
			string folder=Path.Combine(RepoPaths.Root(),"RdlDesign","bin");
			string pathDll=Directory.Exists(folder) ? Directory.GetFiles(folder,"RdlDesigner.dll",SearchOption.AllDirectories)
				.OrderByDescending(x => File.GetLastWriteTimeUtc(x)).FirstOrDefault() : null;
			if(pathDll==null) {
				ProjectFileTests.BuildOutput.NotBuilt(@"RdlDesign\bin\...\RdlDesigner.dll is not built (the full build builds it).");
				return;
			}
			List<string> listTypes=new List<string>();
			using(FileStream fileStream=File.OpenRead(pathDll))
			using(PEReader peReader=new PEReader(fileStream)) {
				MetadataReader metadataReader=peReader.GetMetadataReader();
				foreach(TypeDefinitionHandle typeDefinitionHandle in metadataReader.TypeDefinitions) {
					TypeDefinition typeDefinition=metadataReader.GetTypeDefinition(typeDefinitionHandle);
					listTypes.Add(metadataReader.GetString(typeDefinition.Namespace)+"."+metadataReader.GetString(typeDefinition.Name));
				}
			}
			CollectionAssert.IsSubsetOf(new[] { "CodeBase.UiToolStripRenderer","CodeBase.UiToolStripColorTable","CodeBase.UiTheme","CodeBase.UiIcons" },listTypes,
				pathDll+" compiles the token renderer in.");
		}
		#endregion Source

		#region Drawing
		[TestMethod]
		public void ContextMenu_DrawsInTheTokens() {
			StaRunnerHelpers.RunSta(() => {
				UiTheme.InitWinForms();
				using(ContextMenuStrip contextMenuStrip=new ContextMenuStrip()) {
					contextMenuStrip.Items.Add(new ToolStripMenuItem("Standard"));
					ToolStripMenuItem toolStripMenuItemHover=new ToolStripMenuItem("Custom");
					contextMenuStrip.Items.Add(toolStripMenuItemHover);
					ToolStripSeparator toolStripSeparator=new ToolStripSeparator();
					contextMenuStrip.Items.Add(toolStripSeparator);
					ToolStripMenuItem toolStripMenuItemChecked=new ToolStripMenuItem("Show inactive") { Checked=true };
					contextMenuStrip.Items.Add(toolStripMenuItemChecked);
					contextMenuStrip.PerformLayout();
					Size size=contextMenuStrip.GetPreferredSize(Size.Empty);
					contextMenuStrip.Size=size;
					contextMenuStrip.CreateControl();
					toolStripMenuItemHover.Select();
					using(Bitmap bitmap=new Bitmap(size.Width,size.Height,PixelFormat.Format24bppRgb)) {
						contextMenuStrip.DrawToBitmap(bitmap,new Rectangle(0,0,size.Width,size.Height));
						Assert.AreEqual(UiTheme.Stroke.ToArgb(),bitmap.GetPixel(0,size.Height/2).ToArgb(),"The drop-down's border is Stroke.");
						Rectangle rectangleStandard=contextMenuStrip.Items[0].Bounds;
						Assert.AreEqual(UiTheme.SurfaceRaised.ToArgb(),bitmap.GetPixel(rectangleStandard.Right-4,rectangleStandard.Top+2).ToArgb(),"Items rest on SurfaceRaised.");
						Rectangle rectangleHover=toolStripMenuItemHover.Bounds;
						Assert.AreEqual(UiTheme.SubtleHover.ToArgb(),bitmap.GetPixel(rectangleHover.Right-8,rectangleHover.Top+rectangleHover.Height/2).ToArgb(),
							"The selected item is SubtleHover.");
						int countDarkHover=0;
						for(int x=rectangleHover.Left;x<rectangleHover.Right;x++) {
							for(int y=rectangleHover.Top;y<rectangleHover.Bottom;y++) {
								Color color=bitmap.GetPixel(x,y);
								if(color.R<90 && color.G<90 && color.B<90) {
									countDarkHover++;
								}
							}
						}
						Assert.IsTrue(countDarkHover>10,"The selected item's text is dark (Text), not the professional look's HighlightText ("+countDarkHover+" dark pixels).");
						Rectangle rectangleSeparator=toolStripSeparator.Bounds;
						Assert.IsTrue(Enumerable.Range(rectangleSeparator.Top,rectangleSeparator.Height)
							.Any(y => bitmap.GetPixel(rectangleSeparator.Left+rectangleSeparator.Width/2,y).ToArgb()==UiTheme.Gridline.ToArgb()),"The separator is Gridline.");
						Rectangle rectangleChecked=toolStripMenuItemChecked.Bounds;
						int countDark=0;
						for(int x=rectangleChecked.Left;x<rectangleChecked.Left+28 && x<size.Width;x++) {
							for(int y=rectangleChecked.Top;y<rectangleChecked.Bottom;y++) {
								Color color=bitmap.GetPixel(x,y);
								if(color.R<120 && color.G<120 && color.B<120) {
									countDark++;
								}
							}
						}
						Assert.IsTrue(countDark>4,"The check mark is drawn ("+countDark+" dark pixels).");
					}
				}
			});
		}

		[TestMethod]
		public void MenuStripAndToolStrip_DrawInTheTokens() {
			StaRunnerHelpers.RunSta(() => {
				UiTheme.InitWinForms();
				using(Form form=new Form())
				using(MenuStrip menuStrip=new MenuStrip())
				using(ToolStrip toolStrip=new ToolStrip()) {
					menuStrip.Items.Add(new ToolStripMenuItem("File"));
					menuStrip.Size=new Size(300,24);
					form.Controls.Add(menuStrip);
					ToolStripButton toolStripButtonRest=new ToolStripButton("Rest");
					ToolStripButton toolStripButtonHover=new ToolStripButton("Hover");
					ToolStripButton toolStripButtonChecked=new ToolStripButton("Checked") { Checked=true };
					toolStrip.Items.AddRange(new ToolStripItem[] { toolStripButtonRest,toolStripButtonHover,toolStripButtonChecked });
					toolStrip.GripStyle=ToolStripGripStyle.Hidden;
					toolStrip.Size=new Size(300,25);
					form.Controls.Add(toolStrip);
					form.CreateControl();
					//Both strips dock to the top of the form: their own sizes.
					using(Bitmap bitmap=new Bitmap(menuStrip.Width,menuStrip.Height,PixelFormat.Format24bppRgb)) {
						menuStrip.DrawToBitmap(bitmap,new Rectangle(0,0,menuStrip.Width,menuStrip.Height));
						Assert.AreEqual(UiTheme.Surface.ToArgb(),bitmap.GetPixel(menuStrip.Width-10,menuStrip.Height/2).ToArgb(),"The menu bar is Surface.");
					}
					toolStripButtonHover.Select();
					using(Bitmap bitmap=new Bitmap(toolStrip.Width,toolStrip.Height,PixelFormat.Format24bppRgb)) {
						toolStrip.DrawToBitmap(bitmap,new Rectangle(0,0,toolStrip.Width,toolStrip.Height));
						Assert.AreEqual(UiTheme.SurfaceSubtle.ToArgb(),bitmap.GetPixel(toolStrip.Width-10,toolStrip.Height/2).ToArgb(),"The tool strip is SurfaceSubtle.");
						Assert.AreEqual(UiTheme.Gridline.ToArgb(),bitmap.GetPixel(toolStrip.Width/2,toolStrip.Height-1).ToArgb(),"Its bottom line is Gridline.");
						Rectangle rectangleRest=toolStripButtonRest.Bounds;
						Assert.AreEqual(UiTheme.SurfaceSubtle.ToArgb(),bitmap.GetPixel(rectangleRest.Left+3,rectangleRest.Top+4).ToArgb(),"A button at rest shows no shape.");
						Rectangle rectangleHover=toolStripButtonHover.Bounds;
						Assert.AreEqual(UiTheme.SubtleHover.ToArgb(),bitmap.GetPixel(rectangleHover.Left+3,rectangleHover.Top+rectangleHover.Height/2).ToArgb(),
							"The hovered button is SubtleHover.");
						Rectangle rectangleChecked=toolStripButtonChecked.Bounds;
						Assert.AreEqual(UiTheme.AccentSubtle.ToArgb(),bitmap.GetPixel(rectangleChecked.Left+4,rectangleChecked.Top+rectangleChecked.Height/2).ToArgb(),
							"The checked button is AccentSubtle.");
						Assert.AreEqual(UiTheme.Accent.ToArgb(),bitmap.GetPixel(rectangleChecked.Left+rectangleChecked.Width/2,rectangleChecked.Top+1).ToArgb(),
							"with a 1 px Accent border.");
					}
				}
			});
		}
		#endregion Drawing
	}
}
