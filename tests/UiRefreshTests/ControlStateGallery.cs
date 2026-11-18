// Gallery G-UI1 part 2 for package UI-4a (docs/specs/ui-refresh-plan.md section 8; docs/port-notes/UI-4a.md): the real controls with their state
// strips, WinForms above WPF, at 100, 125, 150, 175 and 200% (WinForms through LayoutManager's scale, WPF through its render scale), off screen.
// Opt-in: OPMS_UI_STATES_GALLERY names the folder; tools/New-UiGallery.ps1 then makes the page from it (the port note has the commands).
// And the AcceptButton list for G-UI3 (Data/accept-buttons.tsv): every form's AcceptButton in the designer code, with its window, its type and
// text, the ones that are not OK or Save marked for the owner. The test fails when the list and the source differ;
// OPMS_UI_ACCEPT_BUTTONS_WRITE=1 writes it again.
// Licensed under the GNU General Public License, version 2 or later. See LICENSE in the repository root.
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using CodeBase;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using ToothChartImageTests;
using UiRefreshTests.Scan;

namespace UiRefreshTests {
	[TestClass]
	public class ControlStateGallery {
		public const string PATH_ACCEPT_BUTTONS=@"tests\UiRefreshTests\Data\accept-buttons.tsv";
		public static readonly float[] ArrayScales=new float[] { 1f,1.25f,1.5f,1.75f,2f };

		[TestMethod]
		public void Gallery_StateStrips_WinFormsBesideWpf() {
			string folder=Environment.GetEnvironmentVariable("OPMS_UI_STATES_GALLERY");
			if(string.IsNullOrEmpty(folder)) {
				Assert.Inconclusive("Opt-in: OPMS_UI_STATES_GALLERY names the folder for the G-UI1 part 2 state strips.");
			}
			bool rendersWpf=false;
			int countStrips=0;
			IconRendererTests.RunSta(() => {
				rendersWpf=ControlPictures.RendersWpf();
				SheetState[] arrayButtonStates=new[] { SheetState.Normal,SheetState.Hover,SheetState.Pressed,SheetState.Focused,SheetState.Disabled };
				foreach(float scale in ArrayScales) {
					string folderScale=Path.Combine(folder,"scale-"+(int)Math.Round(scale*100));
					Directory.CreateDirectory(folderScale);
					foreach(bool isPrimary in new[] { false,true }) {
						List<Bitmap> listWinForms=arrayButtonStates.Select(x => StateControls.RenderWinFormsButton(x,isPrimary,isPrimary ? "Save" : "Cancel",scale)).ToList();
						List<Bitmap> listWpf=rendersWpf ? arrayButtonStates.Select(x => StateControls.RenderWpfButton(x,isPrimary,isPrimary ? "Save" : "Cancel",false,scale)).ToList() : new List<Bitmap>();
						SaveStrip(Path.Combine(folderScale,isPrimary ? "button-default.png" : "button.png"),listWinForms,listWpf,scale);
						countStrips++;
					}
					List<Tuple<SheetState,bool>> listCheckStates=new List<Tuple<SheetState,bool>>();
					foreach(SheetState sheetState in StateControls.ArrayCheckStates) {
						listCheckStates.Add(Tuple.Create(sheetState,false));
					}
					listCheckStates.Add(Tuple.Create(SheetState.Checked,true));
					listCheckStates.Add(Tuple.Create(SheetState.Disabled,true));
					List<Bitmap> listCheckWinForms=listCheckStates.Select(x => StateControls.RenderWinFormsCheckBox(x.Item1,x.Item2,"Active",scale)).ToList();
					Rectangle rectangleBox;
					List<Bitmap> listCheckWpf=rendersWpf ? listCheckStates.Select(x => StateControls.RenderWpfCheckBox(x.Item1,x.Item2,"Active",false,scale,out rectangleBox)).ToList() : new List<Bitmap>();
					SaveStrip(Path.Combine(folderScale,"checkbox.png"),listCheckWinForms,listCheckWpf,scale);
					countStrips++;
					//The radio button: WPF only (the WinForms one is the stock control).
					Rectangle rectangleCircle;
					List<Bitmap> listRadio=rendersWpf ? new[] { SheetState.Normal,SheetState.Hover,SheetState.Disabled,SheetState.Checked }
						.Select(x => StateControls.RenderWpfRadioButton(x,"Daily",false,scale,out rectangleCircle)).ToList() : new List<Bitmap>();
					SaveStrip(Path.Combine(folderScale,"radiobutton-wpf.png"),new List<Bitmap>(),listRadio,scale);
					countStrips++;
					//The group box (6 px corners), rest and disabled, and the tooltip (4 px corners).
					float scaleToolTip;
					List<Bitmap> listOthersWinForms=new List<Bitmap> { StateControls.RenderWinFormsGroupBox("Address",true,scale),
						StateControls.RenderWinFormsGroupBox("Address",false,scale),StateControls.RenderWinFormsToolTip("The patient's city",scale,out scaleToolTip) };
					List<Bitmap> listOthersWpf=rendersWpf ? new List<Bitmap> { StateControls.RenderWpfGroupBox("Address",true,scale),
						StateControls.RenderWpfGroupBox("Address",false,scale),StateControls.RenderWpfToolTip("The patient's city",scale) } : new List<Bitmap>();
					SaveStrip(Path.Combine(folderScale,"groupbox-tooltip.png"),listOthersWinForms,listOthersWpf,scale);
					countStrips++;
					//PanelOD (WinForms, the Stroke border) beside the WPF Label (the Text colour), rest and disabled.
					Rectangle rectangleClient;
					List<Bitmap> listPanel=new List<Bitmap> { StateControls.RenderWinFormsPanelOD(true,scale,out rectangleClient),
						StateControls.RenderWinFormsPanelOD(false,scale,out rectangleClient) };
					List<Bitmap> listLabel=rendersWpf ? new List<Bitmap> { StateControls.RenderWpfLabel("City",false,true,scale),
						StateControls.RenderWpfLabel("City",false,false,scale) } : new List<Bitmap>();
					SaveStrip(Path.Combine(folderScale,"panel-label.png"),listPanel,listLabel,scale);
					countStrips++;
				}
			});
			Assert.IsTrue(countStrips>0);
			if(!rendersWpf) {
				Assert.Inconclusive("Wrote the WinForms rows only: WPF draws nothing in this session.");
			}
		}

		///<summary>One strip: the WinForms cells in the first row, the WPF cells in the second, on Surface with a gap between cells. Disposes the cells.</summary>
		internal static void SaveStrip(string path,List<Bitmap> listWinForms,List<Bitmap> listWpf,float scale) {
			int gap=UiTheme.ScaleI(8,scale);
			List<List<Bitmap>> listRows=new List<List<Bitmap>> { listWinForms,listWpf }.Where(x => x.Count>0).ToList();
			if(listRows.Count==0) {
				return;
			}
			int width=listRows.Max(x => x.Sum(y => y.Width)+gap*(x.Count+1));
			int height=listRows.Sum(x => x.Max(y => y.Height))+gap*(listRows.Count+1);
			using(Bitmap bitmap=new Bitmap(width,height,PixelFormat.Format32bppArgb))
			using(Graphics g=Graphics.FromImage(bitmap)) {
				g.Clear(SheetPainter.Light(EnumUiColor.Surface));
				int y=gap;
				foreach(List<Bitmap> listRow in listRows) {
					int x=gap;
					foreach(Bitmap bitmapCell in listRow) {
						g.DrawImageUnscaled(bitmapCell,x,y);
						x+=bitmapCell.Width+gap;
					}
					y+=listRow.Max(b => b.Height)+gap;
				}
				Directory.CreateDirectory(Path.GetDirectoryName(path));
				PngWriter.Save(bitmap,path);
			}
			foreach(Bitmap bitmapCell in listRows.SelectMany(x => x)) {
				bitmapCell.Dispose();
			}
		}

		#region AcceptButtons for G-UI3
		[TestMethod]
		public void AcceptButtons_ListMatchesTheSource() {
			string pathRoot=RepoPaths.Root();
			List<string> listLines=GetAcceptButtonLines(pathRoot);
			string pathList=Path.Combine(pathRoot,PATH_ACCEPT_BUTTONS);
			string header="# The forms' AcceptButtons (docs/specs/ui-refresh-plan.md section 3.6; package UI-4a, docs/port-notes/UI-4a.md): a UI.Button set as\r\n"
				+"# its form's AcceptButton draws as the default button (filled Accent). For gallery G-UI3 the owner confirms the ones whose name is not\r\n"
				+"# OK or Save (column check=owner); a rejected one goes on Button.cs's plain list. Stock buttons (RdlDesign, RdlViewer and others) keep\r\n"
				+"# the Windows look. ControlStateGallery.AcceptButtons_ListMatchesTheSource checks it; OPMS_UI_ACCEPT_BUTTONS_WRITE=1 writes it again.\r\n"
				+"file\tform\tbutton\ttype\ttext\tcheck\r\n";
			string textNew=header+string.Join("",listLines.Select(x => x+"\r\n"));
			if(Environment.GetEnvironmentVariable("OPMS_UI_ACCEPT_BUTTONS_WRITE")=="1") {
				File.WriteAllText(pathList,textNew,new UTF8Encoding(false));
			}
			Assert.IsTrue(File.Exists(pathList),pathList+" is missing (OPMS_UI_ACCEPT_BUTTONS_WRITE=1 writes it).");
			List<string> listOld=File.ReadAllLines(pathList).Where(x => !x.StartsWith("#") && !x.StartsWith("file\t")).ToList();
			List<string> listMissing=listLines.Except(listOld).ToList();
			List<string> listExtra=listOld.Except(listLines).ToList();
			Assert.IsTrue(listMissing.Count==0 && listExtra.Count==0,"accept-buttons.tsv differs from the source. Not in the list:\r\n"
				+string.Join("\r\n",listMissing)+"\r\nNo longer in the source:\r\n"+string.Join("\r\n",listExtra));
			int countUiButtons=listLines.Count(x => x.Split('\t')[3]=="OpenPmsDental.UI.Button");
			Assert.IsTrue(countUiButtons>=80,"Only "+countUiButtons+" UI.Button AcceptButtons found: the scan is broken.");
		}

		private static readonly Regex _regexAccept=new Regex(@"this\.AcceptButton\s*=\s*this\.(?<name>\w+)\s*;");
		private static readonly Regex _regexClass=new Regex(@"partial\s+class\s+(?<name>\w+)");

		///<summary>file, form, button, type, text, check ("owner" when the button is not an OK or Save button), sorted.</summary>
		public static List<string> GetAcceptButtonLines(string pathRoot) {
			List<string> listLines=new List<string>();
			foreach(string path in Directory.EnumerateFiles(pathRoot,"*.cs",SearchOption.AllDirectories)) {
				string pathRelative=path.Substring(pathRoot.Length).TrimStart('\\','/').Replace('\\','/');
				if(pathRelative.StartsWith(".") || pathRelative.Contains("/obj/") || pathRelative.Contains("/bin/") || pathRelative.StartsWith("third-party/")
					|| pathRelative.StartsWith("tests/")) {
					continue;
				}
				string code=File.ReadAllText(path);
				Match matchAccept=_regexAccept.Match(code);
				if(!matchAccept.Success) {
					continue;
				}
				string nameButton=matchAccept.Groups["name"].Value;
				Match matchClass=_regexClass.Match(code);
				string nameForm=matchClass.Success ? matchClass.Groups["name"].Value : Path.GetFileNameWithoutExtension(path);
				Match matchType=Regex.Match(code,@"this\."+nameButton+@"\s*=\s*new\s+(?<type>[\w\.]+)\s*\(");
				string type=matchType.Success ? matchType.Groups["type"].Value : "?";
				if(type=="UI.Button") {
					type="OpenPmsDental.UI.Button";
				}
				Match matchText=Regex.Match(code,@"this\."+nameButton+@"\.Text\s*=\s*""(?<text>(?:[^""\\]|\\.)*)""");
				string text=matchText.Success ? matchText.Groups["text"].Value : "";
				string textPlain=text.Replace("&","").Trim().ToLowerInvariant();
				bool isOkOrSave=textPlain=="ok" || textPlain=="save" || Regex.IsMatch(nameButton,@"^(but|b|btn)(OK|Ok|Save)$");
				listLines.Add(pathRelative+"\t"+nameForm+"\t"+nameButton+"\t"+type+"\t"+text+"\t"+(isOkOrSave ? "" : "owner"));
			}
			listLines.Sort(StringComparer.Ordinal);
			return listLines;
		}
		#endregion AcceptButtons for G-UI3
	}
}
