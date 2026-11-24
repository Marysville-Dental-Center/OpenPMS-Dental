// UI-PLAIN (docs/port-notes/UI-PLAIN.md): the plain windows (a Form that is not a FormODBase: the plain exes' windows, the graph's dashboard
// dialogs and controls, the CodeBase progress windows and message box) draw in the look of the program's windows: the window background token,
// the inputs' frame and every shared look (UiPlainWindow.Attach in each constructor). The harness builds a plain window from its
// InitializeComponent and calls Attach as its constructor does (WindowBuilder.AttachPlainLookAsTheConstructorDoes), so a window whose
// constructor does not call it is built in Windows' look and fails here.
// Licensed under the GNU General Public License, version 2 or later. See LICENSE in the repository root.
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.Linq;
using System.Windows.Forms;
using CodeBase;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using UiRefreshTests.Scan;

namespace UiRefreshTests {
	[TestClass]
	public class PlainLookTests {
		///<summary>The assemblies whose every plain window calls UiPlainWindow.Attach: the plain exes (but RdlDesign, the report designer's own
		///slice) and the graph library, and CodeBase (its progress windows and message box).</summary>
		public static readonly string[] ArrayPlainAssemblies=new string[] { "CentralManager","CodeBase","CrudGenerator","DatabaseIntegrityCheck",
			"DocumentationBuilder","MySQLManager","OpenPmsDentalDbSettings","OpenPmsDentalGraph","OpenPmsDentalHL7","ServiceManager","UpdateFileCopier" };
		///<summary>Plain forms of those assemblies that are not windows of their own: the progress windows' base class (its two windows call
		///Attach).</summary>
		public static readonly string[] ArrayNotWindows=new string[] { "CodeBase.FormProgressBase" };
		///<summary>The most of a plain window's picture outside its inputs that may be the classic window grey (SystemColors.Control). A read-only
		///or disabled input is that grey in the program's windows too (its area is taken off the count).</summary>
		private const double MAX_SHARE_CLASSIC_GREY=0.10;

		[TestMethod]
		public void PlainWindows_EveryConstructorAttachesTheLook() {
			List<string> listProblems=new List<string>();
			StaRunner.Run(() => {
				NoDatabaseGuard.Install();
				List<ScanType> listScanTypes=WindowBuilder.GetScanTypes().Where(x => x.Kind==BuildKind.PlainWindow && ArrayPlainAssemblies.Contains(x.Assembly)).ToList();
				Assert.IsTrue(listScanTypes.Count>=40,"The plain windows: "+listScanTypes.Count);
				foreach(ScanType scanType in listScanTypes.Where(x => !ArrayNotWindows.Contains(x.Type.FullName))) {
					Type typePlainWindow=scanType.Type.Assembly.GetType(typeof(UiPlainWindow).FullName)??typeof(UiPlainWindow);
					if(!WindowBuilder.ConstructorCalls(scanType.Type,typePlainWindow.GetMethod(nameof(UiPlainWindow.Attach),new Type[] { typeof(Form) }))) {
						listProblems.Add(scanType.WindowKey+": its constructor does not call UiPlainWindow.Attach");
					}
				}
			});
			Assert.AreEqual(0,listProblems.Count,string.Join(Environment.NewLine,listProblems));
		}

		///<summary>Every window of grid-windows/12.tsv (the plain exes' windows and the graph's) built as the program builds it: the background
		///token on the window and on every control that shows the window's background, every framed input and every stock control with a shared
		///look attached, and a picture that is not the classic grey.</summary>
		[TestMethod]
		public void PlainWindows_Slice12_DrawTheNewBackgroundAndFramedInputs() {
			List<GridRulesTests.GridWindow> listWindows=GridRulesTests.ReadGridWindows().Where(x => x.Slice=="12").ToList();
			Assert.IsTrue(listWindows.Count>=40,"grid-windows/12.tsv: "+listWindows.Count);
			List<string> listProblems=new List<string>();
			int countFramed=0;
			StaRunner.Run(() => {
				NoDatabaseGuard.Install();
				List<ScanType> listScanTypes=WindowBuilder.GetScanTypes();
				foreach(GridRulesTests.GridWindow gridWindow in listWindows) {
					ScanType scanType=listScanTypes.FirstOrDefault(x => x.WindowKey==gridWindow.Window);
					if(scanType==null) {
						listProblems.Add(gridWindow.Window+": not a scanned type");
						continue;
					}
					if(!GridRulesTests.IsPlain(scanType)) {
						continue;//FormCentralSecurity: a FormODBase window of the Central Manager
					}
					try {
						using(BuiltWindow builtWindow=WindowBuilder.Build(scanType)) {
							if(!UiPlainWindow.IsAttached(builtWindow.Form)) {
								listProblems.Add(gridWindow.Window+": no UiPlainWindow.Attach");
							}
							countFramed+=CheckTree(gridWindow.Window,builtWindow.Root,listProblems);
							using(Bitmap bitmap=DesignSheetWindows.Capture(builtWindow.Root,Rectangle.Empty)) {
								double share=GetShareOutsideInputs(bitmap,SystemColors.Control,builtWindow.Root);
								if(share>MAX_SHARE_CLASSIC_GREY) {
									listProblems.Add(gridWindow.Window+": "+Math.Round(share*100)+"% of the picture is the classic grey");
								}
							}
						}
					}
					catch(Exception exception) {
						listProblems.Add(gridWindow.Window+": "+TierOneScan.Describe(exception));
					}
				}
			});
			Assert.AreEqual(0,listProblems.Count,string.Join(Environment.NewLine,listProblems));
			Assert.IsTrue(countFramed>=50,"The plain windows' framed inputs: "+countFramed);
		}

		///<summary>The window's problems: an old background on a control that shows the window's background, a framed input or a stock control
		///without its look. Returns the count of framed inputs.</summary>
		private static int CheckTree(string window,Control root,List<string> listProblems) {
			int countFramed=0;
			foreach(Control control in Walk(root)) {
				string name=window+" "+(control==root ? "(window)" : control.Name);
				if(UiPlainWindow.IsBackground(control) && UiPlainWindow.IsOldBackground(control.BackColor)) {
					listProblems.Add(name+": background "+control.BackColor.Name);
				}
				if(control!=root && UiInputFrame.IsFramed(control)) {
					countFramed++;
					if(!UiInputFrame.IsAttached(control)) {
						listProblems.Add(name+": an input without the frame");
					}
				}
				if(UiButtonLook.IsDrawn(control) && !UiButtonLook.IsAttached(control)) {
					listProblems.Add(name+": a stock button in Windows' look");
				}
				if(UiCheckLook.IsDrawn(control) && !UiCheckLook.IsAttached(control)) {
					listProblems.Add(name+": a stock check box in Windows' look");
				}
				if(control is RadioButton && !UiRadioLook.IsAttached(control)) {
					listProblems.Add(name+": a stock radio button in Windows' look");
				}
				if(UiGroupBoxLook.IsDrawn(control) && !UiGroupBoxLook.IsAttached(control)) {
					listProblems.Add(name+": a stock group box in Windows' look");
				}
				if(UiTabLook.IsDrawn(control) && !UiTabLook.IsAttached(control)) {
					listProblems.Add(name+": a stock tab control in Windows' look");
				}
				if(control!=root && UiScrollLook.IsScrolled(control) && !UiScrollLook.IsAttached(control)) {
					listProblems.Add(name+": scroll bars in Windows' look");
				}
			}
			return countFramed;
		}

		private static IEnumerable<Control> Walk(Control root) {
			yield return root;
			foreach(Control control in root.Controls) {
				foreach(Control descendant in Walk(control)) {
					yield return descendant;
				}
			}
		}

		///<summary>The share of the picture in the colour, less the visible inputs' area (text boxes, lists, combo boxes, up-down boxes, date
		///pickers: a read-only or disabled one is the classic grey in the program's windows too).</summary>
		private static double GetShareOutsideInputs(Bitmap bitmap,Color color,Control root) {
			int argb=color.ToArgb();
			long count=0;
			for(int y=0;y<bitmap.Height;y++) {
				for(int x=0;x<bitmap.Width;x++) {
					if(bitmap.GetPixel(x,y).ToArgb()==argb) {
						count++;
					}
				}
			}
			long areaInputs=Walk(root).Where(x => x!=root && (x is TextBoxBase || x is ListBox || x is ComboBox || x is UpDownBase
				|| x is DateTimePicker)).Sum(x => (long)x.Width*x.Height);
			count=Math.Max(0,count-areaInputs);
			return bitmap.Width*bitmap.Height==0 ? 0 : (double)count/(bitmap.Width*bitmap.Height);
		}

		[TestMethod]
		public void UiPlainWindow_BackgroundTokenLooksAndAddedControls() {
			StaRunner.Run(() => {
				using(Form form=new Form { ClientSize=new Size(300,200) }) {
					Panel panelGrey=new Panel { Name="panelGrey",Bounds=new Rectangle(8,8,100,60),BackColor=SystemColors.Control };
					Panel panelOld=new Panel { Name="panelOld",Bounds=new Rectangle(8,72,100,60),BackColor=Color.FromArgb(252,253,254) };
					Panel panelWhite=new Panel { Name="panelWhite",Bounds=new Rectangle(112,8,100,60),BackColor=Color.White };
					Label labelClear=new Label { Name="labelClear",Bounds=new Rectangle(112,72,80,20),BackColor=Color.Transparent };
					TextBox textBox=new TextBox { Name="textBox",Bounds=new Rectangle(8,4,80,23) };
					System.Windows.Forms.Button button=new System.Windows.Forms.Button { Name="butOK",Bounds=new Rectangle(216,168,76,24),Text="OK" };
					System.Windows.Forms.GroupBox groupBox=new System.Windows.Forms.GroupBox { Name="groupBox",Bounds=new Rectangle(216,8,80,80),Text="Group" };
					panelGrey.Controls.Add(textBox);
					form.Controls.AddRange(new Control[] { panelGrey,panelOld,panelWhite,labelClear,button,groupBox });
					UiPlainWindow.Attach(form);
					Assert.IsTrue(UiPlainWindow.IsAttached(form));
					Assert.AreEqual(UiTheme.Surface,form.BackColor,"The window background token, as FormODBase's.");
					Assert.AreEqual(UiTheme.Surface,panelGrey.BackColor,"A panel in the classic grey takes the token.");
					Assert.AreEqual(UiTheme.Surface,panelOld.BackColor,"A panel in the old look's background takes the token.");
					Assert.AreEqual(Color.White,panelWhite.BackColor,"A colour the window chose stays.");
					Assert.AreEqual(Color.Transparent,labelClear.BackColor,"A transparent label stays.");
					Assert.IsTrue(UiInputFrame.IsAttached(textBox),"The input frame.");
					Assert.IsTrue(UiButtonLook.IsAttached(button),"The program's button look.");
					Assert.IsTrue(UiGroupBoxLook.IsAttached(groupBox),"The program's group box look.");
					//A control the window's code adds later, inside a panel added later: the same looks.
					Panel panelLater=new Panel { BackColor=SystemColors.Control,Bounds=new Rectangle(8,136,200,60) };
					TextBox textBoxLater=new TextBox { Bounds=new Rectangle(4,4,80,23) };
					panelLater.Controls.Add(textBoxLater);
					form.Controls.Add(panelLater);
					Assert.AreEqual(UiTheme.Surface,panelLater.BackColor);
					Assert.IsTrue(UiInputFrame.IsAttached(textBoxLater),"A text box added with its panel.");
					CheckBox checkBoxLater=new CheckBox { Text="Later",Bounds=new Rectangle(100,4,80,20) };
					panelLater.Controls.Add(checkBoxLater);
					Assert.IsTrue(UiCheckLook.IsAttached(checkBoxLater),"A check box added to a panel added later.");
					//Again: nothing changes.
					UiPlainWindow.Attach(form);
					Assert.AreEqual(UiTheme.Surface,form.BackColor);
				}
			});
		}

		[TestMethod]
		public void UiPlainWindow_OffForTheGalleryBefore() {
			StaRunner.Run(() => {
				bool isOffBefore=UiLooks.IsOffForGallery;
				try {
					UiLooks.IsOffForGallery=true;
					using(Form form=new Form())
					using(TextBox textBox=new TextBox()) {
						form.Controls.Add(textBox);
						UiPlainWindow.Attach(form);
						Assert.IsFalse(UiPlainWindow.IsAttached(form));
						Assert.AreEqual(SystemColors.Control,form.BackColor,"The gallery's before picture is Windows' look.");
						Assert.IsFalse(UiInputFrame.IsAttached(textBox));
					}
				}
				finally {
					UiLooks.IsOffForGallery=isOffBefore;
				}
			});
		}

		[TestMethod]
		public void GroupBoxLook_TheProgramsGroupBox() {
			StaRunner.Run(() => {
				using(Form form=new Form { BackColor=UiTheme.Surface,ClientSize=new Size(200,120) })
				using(System.Windows.Forms.GroupBox groupBox=new System.Windows.Forms.GroupBox { Text="Date Range",Bounds=new Rectangle(10,10,160,90) }) {
					form.Controls.Add(groupBox);
					Assert.IsTrue(UiGroupBoxLook.IsDrawn(groupBox));
					groupBox.FlatStyle=FlatStyle.System;
					Assert.IsFalse(UiGroupBoxLook.IsDrawn(groupBox),"Windows draws a System group box.");
					groupBox.FlatStyle=FlatStyle.Standard;
					using(Bitmap bitmap=new Bitmap(160,90,PixelFormat.Format24bppRgb))
					using(Graphics g=Graphics.FromImage(bitmap)) {
						g.Clear(Color.Magenta);
						UiGroupBoxLook.PaintGroupBox(g,groupBox);
						Color colorStroke=UiTheme.GetColor(EnumUiColor.Stroke);
						//The outline runs along the whole box (the stock box's line starts half a font height down, under its title).
						AssertNear(colorStroke,bitmap.GetPixel(80,0),"the top line");
						AssertNear(colorStroke,bitmap.GetPixel(0,45),"the left line");
						AssertNear(colorStroke,bitmap.GetPixel(80,89),"the bottom line");
						AssertNear(UiTheme.Surface,bitmap.GetPixel(80,60),"inside: the box's (the window's) colour");
						AssertNear(UiTheme.Surface,bitmap.GetPixel(0,0),"the corner: the parent's colour");
						//The title inside the box at its top left, in Text.
						Assert.IsTrue(HasDark(bitmap,new Rectangle(4,2,70,14)),"the title");
						Assert.IsFalse(HasDark(bitmap,new Rectangle(4,30,150,50)),"nothing else inside");
					}
				}
			});
		}

		[TestMethod]
		public void TabLook_TheProgramsTabs() {
			StaRunner.Run(() => {
				using(Form form=new Form { BackColor=UiTheme.Surface,ClientSize=new Size(320,200) })
				using(System.Windows.Forms.TabControl tabControl=new System.Windows.Forms.TabControl { Bounds=new Rectangle(10,10,300,180) }) {
					form.Controls.Add(tabControl);
					tabControl.TabPages.Add(new System.Windows.Forms.TabPage("First"));
					Assert.IsTrue(UiTabLook.IsDrawn(tabControl));
					UiTabLook.Attach(tabControl);
					System.Windows.Forms.TabPage tabPageLater=new System.Windows.Forms.TabPage("Second");
					tabControl.TabPages.Add(tabPageLater);
					Assert.AreEqual(UiTheme.Surface,tabControl.TabPages[0].BackColor,"A page shows the colour behind the tab control.");
					Assert.AreEqual(UiTheme.Surface,tabPageLater.BackColor,"A page added later too.");
					tabControl.Alignment=TabAlignment.Bottom;
					Assert.IsFalse(UiTabLook.IsDrawn(tabControl),"Bottom tabs keep Windows' drawing.");
					tabControl.Alignment=TabAlignment.Top;
					tabControl.SelectedIndex=0;
					Rectangle rectangleFirst=tabControl.GetTabRect(0);
					Rectangle rectangleSecond=tabControl.GetTabRect(1);
					using(Bitmap bitmap=new Bitmap(300,180,PixelFormat.Format24bppRgb))
					using(Graphics g=Graphics.FromImage(bitmap)) {
						g.Clear(Color.Magenta);
						UiTabLook.PaintTabControl(g,tabControl,idxHover:-1);
						int yStrip=rectangleFirst.Top+2;
						AssertNear(UiTheme.GetColor(EnumUiColor.SurfaceSubtle),bitmap.GetPixel(290,yStrip),"the strip beside the tabs");
						AssertNear(UiTheme.GetColor(EnumUiColor.Accent),bitmap.GetPixel(rectangleFirst.Left+rectangleFirst.Width/2,rectangleFirst.Bottom-1),"the selected tab's line");
						AssertNear(UiTheme.GetColor(EnumUiColor.SurfaceSubtle),bitmap.GetPixel(rectangleSecond.Left+2,rectangleSecond.Bottom-2),"no line under the other tab");
						AssertNear(UiTheme.GetColor(EnumUiColor.Stroke),bitmap.GetPixel(0,120),"the page's left border");
						AssertNear(UiTheme.GetColor(EnumUiColor.Stroke),bitmap.GetPixel(150,179),"the page's bottom border");
						AssertNear(UiTheme.Surface,bitmap.GetPixel(150,120),"the page area: the colour behind");
						Assert.IsTrue(HasDark(bitmap,rectangleFirst),"the first tab's text");
					}
				}
			});
		}

		private static void AssertNear(Color colorExpected,Color colorActual,string what) {
			Assert.IsTrue(Math.Abs(colorExpected.R-colorActual.R)<=12 && Math.Abs(colorExpected.G-colorActual.G)<=12 && Math.Abs(colorExpected.B-colorActual.B)<=12,
				what+": expected "+colorExpected+", got "+colorActual);
		}

		private static bool HasDark(Bitmap bitmap,Rectangle rectangle) {
			for(int y=rectangle.Top;y<rectangle.Bottom;y++) {
				for(int x=rectangle.Left;x<rectangle.Right;x++) {
					Color color=bitmap.GetPixel(x,y);
					if(color.R+color.G+color.B<300) {
						return true;
					}
				}
			}
			return false;
		}
	}
}
