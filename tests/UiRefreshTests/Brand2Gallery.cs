// UI-BRAND-2 (docs/port-notes/UI-BRAND.md section 10): the default A to Z folder name of new installs, and the gallery of the windows whose
// visible text the owner's answers changed (the QuickBooks setup window, the Paths and Backup windows' example folders, ServiceManager's help
// text and the Create AtoZ Folder window's default name), before and after, at 100%.
// Licensed under the GNU General Public License, version 2 or later. See LICENSE in the repository root.
using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Windows.Forms;
using CodeBase;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using UiRefreshTests.Scan;

namespace UiRefreshTests {
	///<summary>The gallery's preparation of a built window for the UI-BRAND-2 before pictures (FitGallery, after its state).</summary>
	public static class Brand2Gallery {
		///<summary>Set to 1: the before pictures. Every control text gets the words it had before UI-BRAND-2 back.</summary>
		public const string VARIABLE_BEFORE="OPMS_UI_BRAND2_GALLERY_BEFORE";
		///<summary>The WinForms windows whose visible text UI-BRAND-2 changed. The WPF window FrmAtoZFoldersCreate is drawn on its own.</summary>
		public static readonly string[] ArrayWindows=new string[] {
			"OpenPmsDental.FormQuickBooksSetup",
			"OpenPmsDental.FormPath",
			"OpenPmsDental.FormBackup",
			"ServiceManager.FormMain",
		};
		///<summary>The A to Z folder name before UI-BRAND-2 (Open Dental's).</summary>
		public const string AtoZFolderNameBefore="OpenDentImages";
		///<summary>Each text as UI-BRAND-2 wrote it, and as it was before.</summary>
		private static readonly string[][] _arrayTextsAfterBefore=new string[][] {
			new string[] { "Push Connect with QuickBooks running in the background the first time you use QuickBooks with Open PMS - Dental (only once). "
				+"QuickBooks' administrator then allows it, even where the old program was allowed.",
				"Push the connect button with QuickBooks running in the background if this is your first time using QuickBooks with Open PMS - Dental. "
				+"(Only need to do this once)" },
			new string[] { @"C:\"+ProgramNames.DefaultAtoZFolderName,@"C:\"+AtoZFolderNameBefore },
			new string[] { @"\\SERVER\"+ProgramNames.DefaultAtoZFolderName,@"\\SERVER\"+AtoZFolderNameBefore },
			new string[] { "begin with \"OpenPmsDental...\" or \"OpenDent...\" (Open Dental's, marked so) or use \"mysqld.exe\".",
				"begin with \"OpenDent...\" or are associated with \"mysqld.exe\"." },
			new string[] { "Most HL7 users will only add one service to this list. If several customer databases are hosted on one server, each must have "
				+"its own uniquely named service, which typically points to an exe in its own folder.",
				"Most HL7 users will only add one service to this list.\r\nBut if multiple customer databases are hosted on one server,\r\nthen each "
				+"database must have its own uniquely named service,\r\nand each such service will typically point to a different exe\r\nin it's own "
				+"separate folder." },
		};

		public static void Prepare(Control root) {
			if(Environment.GetEnvironmentVariable(VARIABLE_BEFORE)!="1") {
				return;
			}
			foreach(Control control in FinalSweep.Walk(root).ToList()) {
				string text=control.Text??"";
				string textBefore=text;
				for(int i=0;i<_arrayTextsAfterBefore.Length;i++) {
					textBefore=textBefore.Replace(_arrayTextsAfterBefore[i][0],_arrayTextsAfterBefore[i][1]);
				}
				if(textBefore!=text) {
					control.Text=textBefore;
				}
			}
		}
	}

	[TestClass]
	public class Brand2Tests {
		///<summary>New installs name the A to Z folder OpenPmsImages: the Create AtoZ Folder window's default (also the share it creates), the
		///examples of the Paths and Backup windows and UpdateFileCopier's debugging default. An office's configured folder is read as it is.</summary>
		[TestMethod]
		public void AtoZFolder_NewInstallsDefaultToOpenPmsImages() {
			Assert.AreEqual("OpenPmsImages",ProgramNames.DefaultAtoZFolderName);
			string textDefault=null;
			StaRunner.Run(() => {
				OpenPmsDental.FrmAtoZFoldersCreate frm=new OpenPmsDental.FrmAtoZFoldersCreate();
				object textBox=typeof(OpenPmsDental.FrmAtoZFoldersCreate).GetField("textName",System.Reflection.BindingFlags.Instance
					|System.Reflection.BindingFlags.NonPublic|System.Reflection.BindingFlags.Public).GetValue(frm);
				textDefault=(string)textBox.GetType().GetProperty("Text").GetValue(textBox);
			});
			Assert.AreEqual(ProgramNames.DefaultAtoZFolderName,textDefault,"The Create AtoZ Folder window's default folder name.");
			string root=RepoRoot();
			string code=File.ReadAllText(Path.Combine(root,"WpfControlsOD","Frms","FrmAtoZFoldersCreate.xaml.cs"));
			StringAssert.Contains(code,"\"SHARE \"+ProgramNames.DefaultAtoZFolderName+\"=");
			StringAssert.Contains(File.ReadAllText(Path.Combine(root,"OpenPmsDental","Forms","FormPath.resx")),@"(like C:\OpenPmsImages ), otherwise");
			StringAssert.Contains(File.ReadAllText(Path.Combine(root,"OpenPmsDental","Forms","FormBackup.designer.cs")),@"(example:)\r\nC:\\OpenPmsImages\\");
			StringAssert.Contains(File.ReadAllText(Path.Combine(root,"UpdateFileCopier","Program.cs")),
				"@\"C:\\\"+CodeBase.ProgramNames.DefaultAtoZFolderName+@\"\\UpdateFiles\"");
			foreach(string file in new[] { @"WpfControlsOD\Frms\FrmAtoZFoldersCreate.xaml",@"WpfControlsOD\Frms\FrmAtoZFoldersCreate.xaml.cs",
				@"OpenPmsDental\Forms\FormPath.resx",@"OpenPmsDental\Forms\FormBackup.designer.cs",@"UpdateFileCopier\Program.cs" })
			{
				Assert.IsFalse(File.ReadAllText(Path.Combine(root,file)).Contains(Brand2Gallery.AtoZFolderNameBefore),file+" still names Open Dental's folder.");
			}
		}

		///<summary>Opt-in: OPMS_UI_BRAND2_GALLERY names the folder of the G-UI3-uibrand2 pictures. Draws each window of Brand2Gallery.ArrayWindows
		///through FitGallery in the program's font at 100%, before (Brand2Gallery.VARIABLE_BEFORE) and after, into before\windows-100 and
		///after\windows-100 (New-UiGallery.ps1's -Before and -After), and the WPF window FrmAtoZFoldersCreate beside them.</summary>
		[TestMethod]
		public void Gallery_UiBrand2_BeforeAndAfter() {
			string folder=Environment.GetEnvironmentVariable("OPMS_UI_BRAND2_GALLERY");
			if(string.IsNullOrEmpty(folder)) {
				Assert.Inconclusive("Opt-in: OPMS_UI_BRAND2_GALLERY names the folder for the UI-BRAND-2 gallery pictures.");
			}
			string[] arrayVariables=new string[] { FitGallery.VARIABLE_FOLDER,FitGallery.VARIABLE_WINDOWS,FitGallery.VARIABLE_FONT,FitGallery.VARIABLE_SCALE,
				Brand2Gallery.VARIABLE_BEFORE };
			Dictionary<string,string> dictBefore=arrayVariables.ToDictionary(x => x,x => Environment.GetEnvironmentVariable(x));
			List<string> listNotes=new List<string>();
			try {
				foreach(bool isBefore in new[] { true,false }) {
					string folderSet=Path.Combine(folder,isBefore ? "before" : "after","windows-100");
					Directory.CreateDirectory(folderSet);
					Environment.SetEnvironmentVariable(FitGallery.VARIABLE_FOLDER,folderSet);
					Environment.SetEnvironmentVariable(FitGallery.VARIABLE_WINDOWS,string.Join(",",Brand2Gallery.ArrayWindows));
					Environment.SetEnvironmentVariable(FitGallery.VARIABLE_FONT,"program");
					Environment.SetEnvironmentVariable(FitGallery.VARIABLE_SCALE,null);
					Environment.SetEnvironmentVariable(Brand2Gallery.VARIABLE_BEFORE,isBefore ? "1" : null);
					new FitGallery().Gallery_SliceWindows_InTheNewFont();
					string[] arrayFailed=File.ReadAllLines(Path.Combine(folderSet,"gallery-failed.txt"));
					Assert.AreEqual(0,arrayFailed.Length,folderSet+": "+string.Join("; ",arrayFailed));
					StaRunner.Run(() => RenderAtoZFoldersCreate(Path.Combine(folderSet,"FrmAtoZFoldersCreate.png"),isBefore,listNotes));
				}
			}
			finally {
				foreach(KeyValuePair<string,string> pair in dictBefore) {
					Environment.SetEnvironmentVariable(pair.Key,pair.Value);
				}
			}
			if(listNotes.Count>0) {
				File.WriteAllLines(Path.Combine(folder,"gallery-notes.txt"),listNotes);
			}
		}

		///<summary>Draws FrmAtoZFoldersCreate without a window (a hidden HwndSource, Measure, Arrange, RenderTargetBitmap), with the old default name
		///for the before picture. Adds a note instead when this session cannot render WPF (disconnected or locked).</summary>
		private static void RenderAtoZFoldersCreate(string path,bool isBefore,List<string> listNotes) {
			OpenPmsDental.FrmAtoZFoldersCreate frm=new OpenPmsDental.FrmAtoZFoldersCreate();
			if(isBefore) {
				object textBox=typeof(OpenPmsDental.FrmAtoZFoldersCreate).GetField("textName",System.Reflection.BindingFlags.Instance
					|System.Reflection.BindingFlags.NonPublic|System.Reflection.BindingFlags.Public).GetValue(frm);
				textBox.GetType().GetProperty("Text").SetValue(textBox,Brand2Gallery.AtoZFolderNameBefore);
			}
			double width=frm.Width;
			double height=frm.Height;
			System.Windows.Interop.HwndSourceParameters hwndSourceParameters=new System.Windows.Interop.HwndSourceParameters("Brand2Frm",
				(int)Math.Ceiling(width),(int)Math.Ceiling(height));
			hwndSourceParameters.WindowStyle=unchecked((int)0x80000000);//WS_POPUP without WS_VISIBLE: never shown
			System.Windows.Interop.HwndSource hwndSource=new System.Windows.Interop.HwndSource(hwndSourceParameters);
			try {
				hwndSource.RootVisual=frm;
				frm.Measure(new System.Windows.Size(width,height));
				frm.Arrange(new System.Windows.Rect(0,0,width,height));
				frm.UpdateLayout();
				System.Windows.Threading.Dispatcher.CurrentDispatcher.Invoke(new Action(() => { }),System.Windows.Threading.DispatcherPriority.ContextIdle);
				frm.UpdateLayout();
				System.Windows.Media.Imaging.RenderTargetBitmap renderTargetBitmap=new System.Windows.Media.Imaging.RenderTargetBitmap((int)Math.Ceiling(width),
					(int)Math.Ceiling(height),96,96,System.Windows.Media.PixelFormats.Pbgra32);
				System.Windows.Media.DrawingVisual drawingVisual=new System.Windows.Media.DrawingVisual();
				using(System.Windows.Media.DrawingContext drawingContext=drawingVisual.RenderOpen()) {
					drawingContext.DrawRectangle(System.Windows.Media.Brushes.White,null,new System.Windows.Rect(0,0,width,height));
				}
				renderTargetBitmap.Render(drawingVisual);
				renderTargetBitmap.Render(frm);
				System.Windows.Media.Imaging.PngBitmapEncoder pngBitmapEncoder=new System.Windows.Media.Imaging.PngBitmapEncoder();
				pngBitmapEncoder.Frames.Add(System.Windows.Media.Imaging.BitmapFrame.Create(renderTargetBitmap));
				using(FileStream fileStream=File.Create(path)) {
					pngBitmapEncoder.Save(fileStream);
				}
			}
			finally {
				hwndSource.RootVisual=null;
				hwndSource.Dispose();
			}
			int countDark=0;
			using(Bitmap bitmap=new Bitmap(path)) {
				for(int x=0;x<bitmap.Width;x+=2) {
					for(int y=0;y<bitmap.Height;y+=2) {
						Color color=bitmap.GetPixel(x,y);
						if(color.A>200 && color.GetBrightness()<0.5f) {//A session that draws nothing leaves even the white background transparent.
							countDark++;
						}
					}
				}
			}
			if(countDark<50) {
				File.Delete(path);
				listNotes.Add("FrmAtoZFoldersCreate came out blank ("+(isBefore ? "before" : "after")+"): WPF draws nothing in this session (disconnected or "
					+"locked), not even off screen. Run the gallery again in a connected session for its picture.");
			}
		}

		private static string RepoRoot() {
			string dir=AppContext.BaseDirectory;
			while(dir!=null && !File.Exists(Path.Combine(dir,"OpenDental.sln"))) {
				dir=Path.GetDirectoryName(dir.TrimEnd(Path.DirectorySeparatorChar));
			}
			Assert.IsNotNull(dir,"the repository root above "+AppContext.BaseDirectory);
			return dir;
		}
	}
}
