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
		///<summary>The assemblies whose every plain window calls UiPlainWindow.Attach: the plain exes and the graph library, and CodeBase (its
		///progress windows and message box); UI-PLAIN-2: the report designer (RdlDesigner, its own copy of the looks) and ODR.</summary>
		public static readonly string[] ArrayPlainAssemblies=new string[] { "CentralManager","CodeBase","CrudGenerator","DatabaseIntegrityCheck",
			"DocumentationBuilder","MySQLManager","ODR","OpenPmsDentalDbSettings","OpenPmsDentalGraph","OpenPmsDentalHL7","RdlDesigner","ServiceManager",
			"UpdateFileCopier" };
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
				Assert.IsTrue(listScanTypes.Count>=60,"The plain windows: "+listScanTypes.Count);
				Assert.IsTrue(listScanTypes.Count(x => x.Assembly=="RdlDesigner")>=20,"The report designer's windows");
				Assert.IsTrue(listScanTypes.Any(x => x.Type.FullName=="ODR.FormParameterInput"),"ODR's FormParameterInput");
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
							CheckClassicGrey(gridWindow.Window,builtWindow.Root,listProblems);
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

		///<summary>UI-PLAIN-2: every report designer and ODR window of grid-windows/13.tsv (the windows, the property pages a PropertyDialog hosts,
		///ODR's FormParameterInput) built as the program builds it: the window or its test host attached (with the report designer's own copy of
		///the looks), the background token, every framed input and stock control in its look, and at most 10% of the gallery's picture in the
		///classic grey outside the inputs.</summary>
		[TestMethod]
		public void PlainWindows_Slice13_ReportDesignerAndOdr_DrawTheNewLook() {
			List<GridRulesTests.GridWindow> listWindows=GridRulesTests.ReadGridWindows()
				.Where(x => x.Slice=="13" && (x.Window.StartsWith("fyiReporting.RdlDesign.") || x.Window.StartsWith("ODR."))).ToList();
			Assert.IsTrue(listWindows.Count>=14,"grid-windows/13.tsv, the report designer and ODR: "+listWindows.Count);
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
					System.Reflection.Assembly assemblyLooks=scanType.Type.Assembly;
					try {
						using(BuiltWindow builtWindow=WindowBuilder.Build(scanType)) {
							if(!PlainLookCopy.IsTrue(assemblyLooks,"UiPlainWindow","IsAttached",builtWindow.Form)) {
								listProblems.Add(gridWindow.Window+": no UiPlainWindow.Attach");
							}
							countFramed+=CheckTree(gridWindow.Window,builtWindow.Root,listProblems,assemblyLooks);
							CheckClassicGrey(gridWindow.Window,builtWindow.Root,listProblems);
						}
					}
					catch(Exception exception) {
						listProblems.Add(gridWindow.Window+": "+TierOneScan.Describe(exception));
					}
				}
			});
			Assert.AreEqual(0,listProblems.Count,string.Join(Environment.NewLine,listProblems));
			Assert.IsTrue(countFramed>=30,"The report designer's and ODR's framed inputs: "+countFramed);
		}

		///<summary>UI-PLAIN fix round: the picture as the gallery draws it (each control printed alone, DesignSheetWindows.IsPrintingWithoutChildren:
		///a transparent control such as GraphIncome on the background behind it) has at most MAX_SHARE_CLASSIC_GREY of the classic grey outside
		///its inputs. The test drew with DrawToBitmap before, which is not the gallery's picture.</summary>
		private static void CheckClassicGrey(string window,Control root,List<string> listProblems) {
			bool isPrintingWithoutChildrenBefore=DesignSheetWindows.IsPrintingWithoutChildren;
			try {
				DesignSheetWindows.IsPrintingWithoutChildren=true;
				using(Bitmap bitmap=DesignSheetWindows.Capture(root,Rectangle.Empty)) {
					double share=GetShareOutsideInputs(bitmap,SystemColors.Control,root);
					if(share>MAX_SHARE_CLASSIC_GREY) {
						listProblems.Add(window+": "+Math.Round(share*100)+"% of the picture is the classic grey");
					}
				}
			}
			finally {
				DesignSheetWindows.IsPrintingWithoutChildren=isPrintingWithoutChildrenBefore;
			}
		}

		///<summary>UI-PLAIN fix round: OpenPmsDentalDbSettings' two windows make their controls in their constructors (not in grid-windows/12.tsv):
		///built by their constructor (WindowBuilder.IsBuildingByConstructor), they have the look as the slice 12 windows do. UI-PLAIN-2 fix round:
		///so does the report designer's main window, whose constructor builds its menus, toolbar, status strip and MDI area.</summary>
		[TestMethod]
		public void PlainWindows_BuiltByTheirConstructor_DrawTheNewLook() {
			List<string> listProblems=new List<string>();
			int countFramed=0;
			StaRunner.Run(() => {
				NoDatabaseGuard.Install();
				bool isBuildingBefore=WindowBuilder.IsBuildingByConstructor;
				try {
					WindowBuilder.IsBuildingByConstructor=true;
					List<ScanType> listScanTypes=WindowBuilder.GetScanTypes().Where(x => x.Kind==BuildKind.PlainWindow && WindowBuilder.IsBuiltByItsConstructor(x.Type)).ToList();
					Assert.AreEqual(3,listScanTypes.Count,"FormDbSettings, FormSecretPrompt and the report designer's main window");
					foreach(ScanType scanType in listScanTypes) {
						//UI-PLAIN-2 fix round: RdlDesigner attaches its own copy of the looks.
						System.Reflection.Assembly assemblyLooks=scanType.Type.Assembly;
						using(BuiltWindow builtWindow=WindowBuilder.Build(scanType)) {
							if(!PlainLookCopy.IsTrue(assemblyLooks,"UiPlainWindow","IsAttached",builtWindow.Form)) {
								listProblems.Add(scanType.WindowKey+": no UiPlainWindow.Attach");
							}
							if(builtWindow.Form.Controls.Count==0) {
								listProblems.Add(scanType.WindowKey+": built without its controls");
							}
							countFramed+=CheckTree(scanType.WindowKey,builtWindow.Root,listProblems,assemblyLooks);
							CheckClassicGrey(scanType.WindowKey,builtWindow.Root,listProblems);
							if(builtWindow.Form.IsMdiContainer) {
								MdiClient mdiClient=builtWindow.Form.Controls.OfType<MdiClient>().FirstOrDefault();
								if(mdiClient==null || mdiClient.BackColor.ToArgb()!=UiTheme.Surface.ToArgb()) {
									listProblems.Add(scanType.WindowKey+": the MDI area is not the background token");
								}
							}
						}
					}
				}
				finally {
					WindowBuilder.IsBuildingByConstructor=isBuildingBefore;
				}
			});
			Assert.AreEqual(0,listProblems.Count,string.Join(Environment.NewLine,listProblems));
			Assert.IsTrue(countFramed>=4,"Their framed inputs: "+countFramed);
		}

		///<summary>The window's problems: an old background on a control that shows the window's background, a framed input or a stock control
		///without its look. Returns the count of framed inputs.</summary>
		private static int CheckTree(string window,Control root,List<string> listProblems) {
			return CheckTree(window,root,listProblems,null);
		}

		///<summary>CheckTree with the looks of the assembly's own copy of CodeBase's UI files (UI-PLAIN-2: the report designer's), else
		///CodeBase's (null).</summary>
		private static int CheckTree(string window,Control root,List<string> listProblems,System.Reflection.Assembly assemblyLooks) {
			int countFramed=0;
			foreach(Control control in Walk(root)) {
				string name=window+" "+(control==root ? "(window)" : control.Name);
				string nameIsOld=control is Form ? "IsOldWindowBackground" : "IsOldBackground";
				bool isOld=PlainLookCopy.IsTrue(assemblyLooks,"UiPlainWindow",nameIsOld,control.BackColor);
				if(PlainLookCopy.IsTrue(assemblyLooks,"UiPlainWindow","IsBackground",control) && isOld) {
					listProblems.Add(name+": background "+control.BackColor.Name);
				}
				if(control!=root && PlainLookCopy.IsTrue(assemblyLooks,"UiInputFrame","IsFramed",control)) {
					countFramed++;
					if(!PlainLookCopy.IsTrue(assemblyLooks,"UiInputFrame","IsAttached",control)) {
						listProblems.Add(name+": an input without the frame");
					}
				}
				foreach(string[] arrayLook in new string[][] { new[] { "UiButtonLook","a stock button" },new[] { "UiCheckLook","a stock check box" },
					new[] { "UiGroupBoxLook","a stock group box" },new[] { "UiTabLook","a stock tab control" } })
				{
					if(PlainLookCopy.IsTrue(assemblyLooks,arrayLook[0],"IsDrawn",control) && !PlainLookCopy.IsTrue(assemblyLooks,arrayLook[0],"IsAttached",control)) {
						listProblems.Add(name+": "+arrayLook[1]+" in Windows' look");
					}
				}
				if(control is RadioButton && !PlainLookCopy.IsTrue(assemblyLooks,"UiRadioLook","IsAttached",control)) {
					listProblems.Add(name+": a stock radio button in Windows' look");
				}
				if(control!=root && PlainLookCopy.IsTrue(assemblyLooks,"UiScrollLook","IsScrolled",control)
					&& !PlainLookCopy.IsTrue(assemblyLooks,"UiScrollLook","IsAttached",control))
				{
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
		public void UiPlainWindow_LightGreyWindow_TakesTheToken() {
			StaRunner.Run(() => {
				using(Form form=new Form { BackColor=SystemColors.ControlLight })
				using(Panel panel=new Panel { BackColor=SystemColors.ControlLight }) {
					form.Controls.Add(panel);
					UiPlainWindow.Attach(form);
					Assert.AreEqual(UiTheme.Surface,form.BackColor,"The progress windows' light grey window takes the token.");
					Assert.AreEqual(SystemColors.ControlLight,panel.BackColor,"A light grey panel the window chose inside it stays.");
				}
			});
		}

		[TestMethod]
		public void ButtonLook_ImageAndTextAsOneBlock() {
			Assert.AreEqual(ContentAlignment.MiddleCenter,UiButtonLook.GetBlockAlign(ContentAlignment.MiddleLeft,ContentAlignment.MiddleRight),"Add Row");
			Assert.AreEqual(ContentAlignment.MiddleCenter,UiButtonLook.GetBlockAlign(ContentAlignment.TopCenter,ContentAlignment.BottomCenter),"Add Col");
			Assert.AreEqual(ContentAlignment.MiddleLeft,UiButtonLook.GetBlockAlign(ContentAlignment.MiddleLeft,ContentAlignment.MiddleLeft));
			Assert.AreEqual(ContentAlignment.BottomRight,UiButtonLook.GetBlockAlign(ContentAlignment.BottomRight,ContentAlignment.BottomRight));
			StaRunner.Run(() => {
				using(Bitmap bitmapIcon=new Bitmap(16,16))
				using(System.Windows.Forms.Button button=new System.Windows.Forms.Button { Text="Add Row",Size=new Size(200,30),Image=bitmapIcon,
					ImageAlign=ContentAlignment.MiddleLeft,TextAlign=ContentAlignment.MiddleRight,TextImageRelation=TextImageRelation.TextBeforeImage })
				using(Bitmap bitmap=new Bitmap(200,30,PixelFormat.Format24bppRgb))
				using(Graphics g=Graphics.FromImage(bitmap)) {
					using(Graphics gIcon=Graphics.FromImage(bitmapIcon)) {
						gIcon.Clear(Color.Red);
					}
					UiButtonLook.PaintButton(g,button,EnumUiButtonState.Normal,isFocused:false);
					int xRedFirst=-1;
					int xTextLast=-1;
					for(int x=0;x<200;x++) {
						Color color=bitmap.GetPixel(x,15);
						if(xRedFirst<0 && color.R>200 && color.G<60 && color.B<60) {
							xRedFirst=x;
						}
					}
					for(int x=0;x<200;x++) {
						for(int y=4;y<26;y++) {
							Color color=bitmap.GetPixel(x,y);
							if(color.R+color.G+color.B<300 && !(color.R>200 && color.G<60)) {
								xTextLast=Math.Max(xTextLast,x);
							}
						}
					}
					Assert.IsTrue(xRedFirst>0,"The image is drawn.");
					Assert.IsTrue(xTextLast>0 && xTextLast<xRedFirst,"The text before the image (text ends at "+xTextLast+", image at "+xRedFirst+").");
					Assert.IsTrue(Math.Abs((200-(xRedFirst+16))-(xTextLast>0 ? FirstDark(bitmap) : 0))<=12,"The block centred.");
				}
			});
			StaRunner.Run(() => {
				using(System.Windows.Forms.Button button=new System.Windows.Forms.Button { Text="&OK",UseMnemonic=false }) {
					Assert.AreEqual(TextFormatFlags.NoPrefix,UiLooks.GetPrefixFlags(button,button.UseMnemonic));
				}
			});
		}

		private static int FirstDark(Bitmap bitmap) {
			for(int x=0;x<bitmap.Width;x++) {
				for(int y=4;y<bitmap.Height-4;y++) {
					Color color=bitmap.GetPixel(x,y);
					if(color.R+color.G+color.B<300 && !(color.R>200 && color.G<60)) {
						return x;
					}
				}
			}
			return -1;
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
					tabControl.Alignment=TabAlignment.Left;
					Assert.IsFalse(UiTabLook.IsDrawn(tabControl),"Side tabs keep Windows' drawing.");
					//UI-PLAIN-2 fix round: bottom tabs (the report designer's Designer, RDL Text and Preview) in the look, mirrored.
					tabControl.Alignment=TabAlignment.Bottom;
					Assert.IsTrue(UiTabLook.IsDrawn(tabControl),"Bottom tabs in the look.");
					tabControl.SelectedIndex=0;
					Rectangle rectangleFirstBottom=tabControl.GetTabRect(0);
					using(Bitmap bitmap=new Bitmap(300,180,PixelFormat.Format24bppRgb))
					using(Graphics g=Graphics.FromImage(bitmap)) {
						g.Clear(Color.Magenta);
						UiTabLook.PaintTabControl(g,tabControl,idxHover:-1);
						AssertNear(UiTheme.GetColor(EnumUiColor.SurfaceSubtle),bitmap.GetPixel(290,rectangleFirstBottom.Bottom-3),"the bottom strip beside the tabs");
						AssertNear(UiTheme.GetColor(EnumUiColor.Accent),bitmap.GetPixel(rectangleFirstBottom.Left+rectangleFirstBottom.Width/2,rectangleFirstBottom.Top),
							"the selected tab's line along its top");
						AssertNear(UiTheme.GetColor(EnumUiColor.Stroke),bitmap.GetPixel(150,0),"the page's top border");
						AssertNear(UiTheme.Surface,bitmap.GetPixel(150,60),"the page area: the colour behind");
						Assert.IsTrue(HasDark(bitmap,rectangleFirstBottom),"the first tab's text");
					}
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

		///<summary>UI-PLAIN fix round: a tab's ImageList image (DashboardTabCtrl's add tab and delete X in its setup mode) is drawn where Windows
		///puts it, 6 px in and centred top to bottom, the text after it, painted and printed (WM_PRINT); an ImageList given after the look was
		///attached (the dashboard switches it at run time) too, and a new ImageIndex shows. A tab without an image has none.</summary>
		[TestMethod]
		public void TabLook_TheTabsImages() {
			StaRunner.Run(() => {
				Color colorRed=Color.FromArgb(220,0,0);
				Color colorGreen=Color.FromArgb(0,160,0);
				using(Form form=new Form { BackColor=UiTheme.Surface,ClientSize=new Size(320,200) })
				using(System.Windows.Forms.TabControl tabControl=new System.Windows.Forms.TabControl { Bounds=new Rectangle(10,10,300,180) })
				using(ImageList imageList=new ImageList { ColorDepth=ColorDepth.Depth32Bit,ImageSize=new Size(11,30) })
				using(Bitmap bitmapGreen=new Bitmap(11,30))
				using(Bitmap bitmapRed=new Bitmap(11,30)) {
					using(Graphics gGreen=Graphics.FromImage(bitmapGreen)) {
						gGreen.Clear(colorGreen);
					}
					using(Graphics gRed=Graphics.FromImage(bitmapRed)) {
						gRed.Clear(colorRed);
					}
					imageList.Images.Add(bitmapGreen);
					imageList.Images.Add(bitmapRed);
					form.Controls.Add(tabControl);
					tabControl.TabPages.Add(new System.Windows.Forms.TabPage("Production"));
					tabControl.TabPages.Add(new System.Windows.Forms.TabPage("Appointments"));
					UiTabLook.Attach(tabControl);
					tabControl.ImageList=imageList;//after Attach, as the dashboard's setup mode does
					tabControl.TabPages[0].ImageIndex=1;
					Assert.IsTrue(UiTabLook.IsDrawn(tabControl),"A tab control with images keeps the look.");
					Rectangle rectangleHeader=tabControl.GetTabRect(0);
					Rectangle rectangleImage=UiTabLook.GetImageRectangle(tabControl,0);
					Assert.AreEqual(rectangleHeader.X+6,rectangleImage.X,"6 px in from the header's left");
					Assert.IsTrue(UiTabLook.GetImageRectangle(tabControl,1).IsEmpty,"The second tab has no image.");
					Point pointMiddle=new Point(rectangleImage.X+rectangleImage.Width/2,
						Math.Max(rectangleHeader.Top+2,Math.Min(rectangleHeader.Bottom-3,rectangleImage.Y+rectangleImage.Height/2)));
					using(Bitmap bitmap=new Bitmap(300,180,PixelFormat.Format24bppRgb))
					using(Graphics g=Graphics.FromImage(bitmap)) {
						g.Clear(Color.Magenta);
						UiTabLook.PaintTabControl(g,tabControl,idxHover:-1);
						AssertNear(colorRed,bitmap.GetPixel(pointMiddle.X,pointMiddle.Y),"the first tab's image (ImageIndex 1)");
						Assert.IsTrue(HasDark(bitmap,Rectangle.FromLTRB(rectangleImage.Right,rectangleHeader.Top,rectangleHeader.Right,rectangleHeader.Bottom-3)),
							"the first tab's text after its image");
						Assert.IsFalse(HasDark(bitmap,Rectangle.FromLTRB(rectangleHeader.Left,rectangleHeader.Top,rectangleImage.Left,rectangleHeader.Bottom-3)),
							"no text left of the image");
						//The image follows ImageIndex (the dashboard's X under the mouse).
						tabControl.TabPages[0].ImageIndex=0;
						UiTabLook.PaintTabControl(g,tabControl,idxHover:-1);
						AssertNear(colorGreen,bitmap.GetPixel(pointMiddle.X,pointMiddle.Y),"the first tab's image (ImageIndex 0)");
					}
					//Printed (WM_PRINT, the gallery's way): the same image.
					using(Bitmap bitmapPrinted=new Bitmap(300,180,PixelFormat.Format24bppRgb)) {
						tabControl.DrawToBitmap(bitmapPrinted,new Rectangle(0,0,300,180));
						AssertNear(colorGreen,bitmapPrinted.GetPixel(pointMiddle.X,pointMiddle.Y),"printed: the first tab's image");
					}
				}
			});
		}

		///<summary>UI-PLAIN fix round: an enabled stock DropDownList combo box's face in the input fill (the text boxes' white) with its text in Text,
		///as the program's ComboBox; Windows drew it as a grey read-only button. A disabled, editable or owner-drawn one keeps Windows' face.</summary>
		[TestMethod]
		public void InputFrame_DropDownListFaceInTheInputFill() {
			StaRunner.Run(() => {
				using(Form form=new Form { BackColor=UiTheme.Surface,ClientSize=new Size(200,60) })
				using(System.Windows.Forms.ComboBox comboBox=new System.Windows.Forms.ComboBox { DropDownStyle=ComboBoxStyle.DropDownList,Bounds=new Rectangle(10,10,150,21) }) {
					form.Controls.Add(comboBox);
					comboBox.Items.AddRange(new object[] { "Weekly","Monthly" });
					comboBox.SelectedIndex=0;
					Assert.IsTrue(UiInputFrame.IsFaceDrawn(comboBox));
					Rectangle rectangle=new Rectangle(0,0,comboBox.Width,comboBox.Height);
					Rectangle rectangleInner=UiInputFrame.GetInnerRectangle(rectangle,comboBox);
					using(Bitmap bitmap=new Bitmap(comboBox.Width,comboBox.Height,PixelFormat.Format24bppRgb))
					using(Graphics g=Graphics.FromImage(bitmap)) {
						g.Clear(Color.FromArgb(240,240,240));//Windows' read-only face
						UiInputFrame.PaintFrame(g,rectangle,comboBox,isFocused:false);
						AssertNear(SystemColors.Window,bitmap.GetPixel(rectangleInner.Right-3,rectangleInner.Top+rectangleInner.Height/2),"the face right of the text");
						AssertNear(SystemColors.Window,bitmap.GetPixel(rectangleInner.Left+1,rectangleInner.Top+1),"the face's top left");
						Assert.IsTrue(HasDark(bitmap,rectangleInner),"the selected item's text");
					}
					comboBox.Enabled=false;
					Assert.IsFalse(UiInputFrame.IsFaceDrawn(comboBox),"A disabled combo box keeps Windows' grey.");
					comboBox.Enabled=true;
					comboBox.DrawMode=DrawMode.OwnerDrawFixed;
					Assert.IsFalse(UiInputFrame.IsFaceDrawn(comboBox),"An owner-drawn combo box keeps its owner's face.");
					comboBox.DrawMode=DrawMode.Normal;
					comboBox.DropDownStyle=ComboBoxStyle.DropDown;
					Assert.IsFalse(UiInputFrame.IsFaceDrawn(comboBox),"An editable combo box's face is its white edit.");
				}
			});
		}

		///<summary>UI-PLAIN fix round: a stock button that is its window's AcceptButton draws as the program's default button (filled Accent, as
		///UI.Button.IsPrimary), the other buttons plain.</summary>
		[TestMethod]
		public void ButtonLook_AcceptButtonIsPrimary() {
			StaRunner.Run(() => {
				using(Form form=new Form { BackColor=UiTheme.Surface,ClientSize=new Size(200,60) })
				using(System.Windows.Forms.Button butOK=new System.Windows.Forms.Button { Name="butOK",Text="OK",Bounds=new Rectangle(10,10,75,26) })
				using(System.Windows.Forms.Button butCancel=new System.Windows.Forms.Button { Name="butCancel",Text="Cancel",Bounds=new Rectangle(100,10,75,26) }) {
					form.Controls.Add(butOK);
					form.Controls.Add(butCancel);
					form.AcceptButton=butOK;
					form.CancelButton=butCancel;
					Assert.IsTrue(UiButtonLook.IsPrimary(butOK));
					Assert.IsFalse(UiButtonLook.IsPrimary(butCancel));
					Color colorAccent=UiTheme.GetColor(UiControlPaint.GetButtonFill(EnumUiButtonState.Normal,isPrimary:true));
					Color colorPlain=UiTheme.GetColor(UiControlPaint.GetButtonFill(EnumUiButtonState.Normal,isPrimary:false));
					using(Bitmap bitmap=new Bitmap(75,26,PixelFormat.Format24bppRgb))
					using(Graphics g=Graphics.FromImage(bitmap)) {
						UiButtonLook.PaintButton(g,butOK,EnumUiButtonState.Normal,isFocused:false);
						AssertNear(colorAccent,bitmap.GetPixel(6,6),"the AcceptButton's fill");
						UiButtonLook.PaintButton(g,butCancel,EnumUiButtonState.Normal,isFocused:false);
						AssertNear(colorPlain,bitmap.GetPixel(6,6),"the other button's fill");
					}
				}
			});
		}

		///<summary>UI-PLAIN fix round: inside the program a plain window (MsgBoxCopyPaste, the progress windows, the dashboard's dialogs) is not
		///zoomed by any LayoutManager, so its looks draw at the control's dpi without the program's zoom; a control in a window without the look
		///keeps the zoom, as before.</summary>
		[TestMethod]
		public void PlainWindow_LooksIgnoreTheProgramsZoom() {
			StaRunner.Run(() => {
				Func<float> funcGetZoomBefore=UiLooks.FuncGetZoom;
				try {
					UiLooks.FuncGetZoom=() => 1.25f;
					using(Form formPlain=new Form { ClientSize=new Size(200,60) })
					using(Form formOther=new Form { ClientSize=new Size(200,60) })
					using(CheckBox checkBoxPlain=new CheckBox { Text="Show",Bounds=new Rectangle(10,10,120,20) })
					using(CheckBox checkBoxOther=new CheckBox { Text="Show",Bounds=new Rectangle(10,10,120,20) }) {
						formPlain.Controls.Add(checkBoxPlain);
						formOther.Controls.Add(checkBoxOther);
						UiPlainWindow.Attach(formPlain);
						float scaleDpi=Math.Max(1f,checkBoxPlain.DeviceDpi/96f);
						Assert.AreEqual(scaleDpi,UiLooks.GetScale(checkBoxPlain),0.001f,"a check box in a plain window: the dpi alone");
						Assert.AreEqual(scaleDpi*1.25f,UiLooks.GetScale(checkBoxOther),0.001f,"a check box in a window without the look: the zoom as before");
						Assert.IsTrue(UiCheckLook.IsAttached(checkBoxPlain));
						//Drawn: the same picture as at zoom 1.
						using(Bitmap bitmapZoom=new Bitmap(120,20,PixelFormat.Format24bppRgb))
						using(Bitmap bitmapOne=new Bitmap(120,20,PixelFormat.Format24bppRgb)) {
							checkBoxPlain.DrawToBitmap(bitmapZoom,new Rectangle(0,0,120,20));
							UiLooks.FuncGetZoom=() => 1f;
							checkBoxPlain.DrawToBitmap(bitmapOne,new Rectangle(0,0,120,20));
							for(int y=0;y<20;y++) {
								for(int x=0;x<120;x++) {
									Assert.AreEqual(bitmapOne.GetPixel(x,y).ToArgb(),bitmapZoom.GetPixel(x,y).ToArgb(),"the check box at zoom 1.25 as at 1, pixel "+x+","+y);
								}
							}
						}
					}
				}
				finally {
					UiLooks.FuncGetZoom=funcGetZoomBefore;
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
