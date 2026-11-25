// The tooth chart in both bitnesses (package TC-10, docs/port-notes/TC-10.md; the TC-10 row of docs/specs/wpf-tooth-chart-plan.md). Two checks of
// S3a-15's ToothChartEngineBitnessTests.cs (docs/port-notes/S3a-15.md) that TC-9a deleted with the old engines, brought back for the code that
// stays: the native declarations (P/Invokes) of SparksToothChart itself take their handles as IntPtr, and the Simple 2D chart (and the WPF chart a
// stored DirectX or OpenGL choice starts) starts and draws on a hidden form. The lane runs this assembly as x86 (the test runner) and as x64 (its
// x64 test build, -Projects BINATests -X64Test), so every check holds in the bitness of the test process. No window is shown.
// Licensed under the GNU General Public License, version 2 or later. See LICENSE in the repository root.
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Text.RegularExpressions;
using System.Threading;
using System.Windows.Forms;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Mono.Cecil;
using OpenPmsDentalBusiness;
using SparksToothChart;
using SparksToothChart.Wpf3D;

namespace BINATests {
	[TestClass]
	public class ToothChartBitnessTests {
		///<summary>Every native declaration of SparksToothChart.dll, as "Type.Method module!EntryPoint: Return(Parameters)" (the type names of the
		///metadata). Today the six gdi32 functions of ChartFontMeasure (in Wpf3D\ToothSceneBuilder.cs) that measure the 2D layer's text as the old DirectX chart's GDI fonts
		///did (TEXTMETRIC, SIZE). A new declaration is added here on purpose, after its handles are checked.</summary>
		private static readonly string[] _arrayDeclarationsExpected={
			"ChartFontMeasure.CreateCompatibleDC gdi32.dll!CreateCompatibleDC: IntPtr(IntPtr)",
			"ChartFontMeasure.CreateFont gdi32.dll!CreateFontW: IntPtr(Int32,Int32,Int32,Int32,Int32,UInt32,UInt32,UInt32,UInt32,UInt32,UInt32,UInt32,UInt32,String)",
			"ChartFontMeasure.DeleteObject gdi32.dll!DeleteObject: Boolean(IntPtr)",
			"ChartFontMeasure.GetTextExtentPoint32 gdi32.dll!GetTextExtentPoint32W: Boolean(IntPtr,String,Int32,SIZE&)",
			"ChartFontMeasure.GetTextMetrics gdi32.dll!GetTextMetricsW: Boolean(IntPtr,TEXTMETRIC&)",
			"ChartFontMeasure.SelectObject gdi32.dll!SelectObject: IntPtr(IntPtr,IntPtr)" };
		///<summary>A parameter name that says it holds a handle (a window, device context, GDI object, font, rendering context or module).</summary>
		private static readonly Regex _regexHandleName=new Regex(@"^h[A-Z]|^(?i:hdc|hwnd|hglrc|hgdiobj|hfont|hbitmap|hobject|hmodule|hinstance|hrgn|hpal)$"
			+@"|(?i:handle|context)$");
		///<summary>The functions of _arrayDeclarationsExpected that return a handle (HDC, HFONT, HGDIOBJ): never an Int32 in a 64-bit process.</summary>
		private static readonly string[] _arrayEntryPointsReturningHandles={ "CreateCompatibleDC","CreateFontW","SelectObject" };
		///<summary>The size of a hidden chart, as S3a-15's tests drew it.</summary>
		private static readonly Size _sizeChart=new Size(400,300);
		private static readonly object _lockRecord=new object();

		///<summary>"x64" or "x86": the bitness of this test process.</summary>
		private static string Bitness() {
			return Environment.Is64BitProcess?"x64":"x86";
		}

		///<summary>The file next to the test assembly that records what the hidden-form charts did in this bitness (ToothChartBitness.x86.txt or
		///.x64.txt): the lane's runners print only counts for a passing run, so the port note's evidence is this file, copied out of bin before a clean.
		///</summary>
		private static string RecordPath() {
			return Path.Combine(AppDomain.CurrentDomain.BaseDirectory,"ToothChartBitness."+Bitness()+".txt");
		}

		[ClassInitialize]
		public static void ClassInitialize(TestContext testContext) {
			try {
				File.Delete(RecordPath());
			}
			catch(Exception ex) {
				Trace.WriteLine(ex.Message);
			}
		}

		///<summary>Writes line to the record file and to the test's output.</summary>
		private static void Record(string line) {
			string lineRecord=DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss")+" "+Bitness()+" "+line;
			Console.WriteLine("ToothChartBitness: "+lineRecord);
			lock(_lockRecord) {
				File.AppendAllText(RecordPath(),lineRecord+Environment.NewLine);
			}
		}

		///<summary>Runs action on a new STA thread, which a WinForms control wants, and rethrows what it throws (an assert as itself).</summary>
		private static void RunSta(Action action) {
			Exception exception=null;
			Thread thread=new Thread(() => {
				try {
					action();
				}
				catch(Exception ex) {
					exception=ex;
				}
			});
			thread.SetApartmentState(ApartmentState.STA);
			thread.IsBackground=true;
			thread.Start();
			if(!thread.Join(TimeSpan.FromSeconds(180))) {
				Assert.Fail("The STA thread did not finish in 180 s.");
			}
			if(exception is AssertInconclusiveException || exception is AssertFailedException) {
				throw exception;
			}
			if(exception!=null) {
				throw new AssertFailedException(exception.GetType().Name+": "+exception.Message,exception);
			}
		}

		///<summary>The type name of a parameter or return in the form of _arrayDeclarationsExpected: the metadata name, a by-reference type with
		///its &amp; (Mono.Cecil's Name), a nested type without its declaring type.</summary>
		private static string GetTypeName(TypeReference typeReference) {
			return typeReference.Name;
		}

		[TestMethod]
		public void ChartPInvokes_PassHandlesAsIntPtr() {
			//S3a-15's scan (Handles_ArePassedAsIntPtr) for SparksToothChart's own declarations: read with Mono.Cecil from a copy of the bytes of the
			//assembly this process loaded (the x86 or the x64 build), every method that is a P/Invoke (IsPInvokeImpl).
			string pathChart=typeof(ToothChartWrapper).Assembly.Location;
			List<string> listDeclarations=new List<string>();
			List<string> listOffenders=new List<string>();
			using(MemoryStream memoryStream=new MemoryStream(File.ReadAllBytes(pathChart)))
			using(ModuleDefinition moduleDefinition=ModuleDefinition.ReadModule(memoryStream)) {
				foreach(TypeDefinition typeDefinition in moduleDefinition.GetTypes()) {
					foreach(MethodDefinition methodDefinition in typeDefinition.Methods.Where(x => x.IsPInvokeImpl)) {
						string entryPoint=methodDefinition.PInvokeInfo.EntryPoint??methodDefinition.Name;
						string declaration=typeDefinition.Name+"."+methodDefinition.Name+" "+methodDefinition.PInvokeInfo.Module.Name+"!"+entryPoint+": "
							+GetTypeName(methodDefinition.ReturnType)+"("+string.Join(",",methodDefinition.Parameters.Select(x => GetTypeName(x.ParameterType)))+")";
						listDeclarations.Add(declaration);
						foreach(ParameterDefinition parameterDefinition in methodDefinition.Parameters) {
							string typeName=parameterDefinition.ParameterType.FullName;
							if(_regexHandleName.IsMatch(parameterDefinition.Name) && typeName!="System.IntPtr" && typeName!="System.UIntPtr") {
								listOffenders.Add(declaration+": the handle "+parameterDefinition.Name+" is "+typeName);
							}
						}
						string typeReturn=methodDefinition.ReturnType.FullName;
						if(_arrayEntryPointsReturningHandles.Contains(entryPoint) && typeReturn!="System.IntPtr") {
							listOffenders.Add(declaration+": returns a handle as "+typeReturn);
						}
						if(typeReturn=="System.Int32" || typeReturn=="System.UInt32") {
							//No function SparksToothChart calls returns a number today; a handle read as an int loses its upper half in a 64-bit process.
							listOffenders.Add(declaration+": returns "+typeReturn+", which a handle must never be");
						}
					}
				}
			}
			listDeclarations.Sort(StringComparer.Ordinal);
			Record("SparksToothChart.dll ("+Path.GetFileName(Path.GetDirectoryName(pathChart))+"): "+listDeclarations.Count+" P/Invokes: "
				+string.Join("; ",listDeclarations)+(listOffenders.Count==0?"":"; offenders: "+string.Join("; ",listOffenders)));
			Assert.AreEqual(0,listOffenders.Count,"Handles that are not IntPtr: "+string.Join("; ",listOffenders));
			CollectionAssert.AreEqual(_arrayDeclarationsExpected,listDeclarations,"The native declarations of SparksToothChart.dll: "+string.Join("; ",listDeclarations));
			//The two structures have no pointer, so each has the size Windows gives it in both bitnesses: TEXTMETRICW 60 bytes, SIZE 8.
			Type typeTextMetric=typeof(ChartFontMeasure).GetNestedType("TEXTMETRIC",BindingFlags.NonPublic);
			Type typeSize=typeof(ChartFontMeasure).GetNestedType("SIZE",BindingFlags.NonPublic);
			Assert.IsNotNull(typeTextMetric,"ChartFontMeasure.TEXTMETRIC");
			Assert.IsNotNull(typeSize,"ChartFontMeasure.SIZE");
			Assert.IsFalse(typeTextMetric.GetFields(BindingFlags.Instance|BindingFlags.Public|BindingFlags.NonPublic).Concat(
				typeSize.GetFields(BindingFlags.Instance|BindingFlags.Public|BindingFlags.NonPublic)).Any(x => x.FieldType==typeof(IntPtr)),"No pointer field.");
			Assert.AreEqual(60,Marshal.SizeOf(typeTextMetric),"TEXTMETRICW");
			Assert.AreEqual(8,Marshal.SizeOf(typeSize),"SIZE");
			//The memory device context is kept as IntPtr, and no file of SparksToothChart cuts a handle to a number.
			Assert.AreEqual(typeof(IntPtr),typeof(ChartFontMeasure).GetField("_hdc",BindingFlags.Static|BindingFlags.NonPublic).FieldType,"ChartFontMeasure._hdc");
			Regex regexCut=new Regex(@"\(\s*(int|uint|Int32|UInt32|long)\s*\)\s*[\w.]*(Handle|_hdc|hdc|hFont\w*)\b|Handle\s*\.\s*ToInt(32|64)\s*\("
				+@"|\b_?(hdc|hwnd|hglrc|hFont\w*|hgdiobj|hObject)\s*\.\s*ToInt(32|64)\s*\(");
			List<string> listCuts=new List<string>();
			string root=RepoFiles.Root();
			List<string> listSources=Directory.GetFiles(Path.Combine(root,"SparksToothChart"),"*.cs",SearchOption.AllDirectories)
				.Select(x => x.Substring(root.Length).TrimStart('\\')).Where(x => !x.Contains(@"\obj\") && !x.Contains(@"\bin\")).ToList();
			Assert.IsTrue(listSources.Contains(@"SparksToothChart\Wpf3D\ToothSceneBuilder.cs"),"The scan reads the chart's sources.");
			foreach(string relative in listSources) {
				foreach(Match match in regexCut.Matches(RepoFiles.Read(relative))) {
					listCuts.Add(relative+": "+match.Value);
				}
			}
			Assert.AreEqual(0,listCuts.Count,"A handle cut to a number: "+string.Join(", ",listCuts));
		}

		///<summary>S3a-15's hidden-form start for the charts that stay: on an STA thread, a borderless form whose window handle is created and which
		///is never shown (no Show, ShowDialog or Visible; asserted not visible), a chart of 400x300 on it, started by the stored choice through DrawMode
		///as the program's settings do (ToothChartRelay.ApplyComputerSettings), and its picture (GetBitmap). The chart must come up as expected,
		///draw a picture of its size that is not one colour, and log no fallback. Recorded in ToothChartBitness.&lt;bitness&gt;.txt.</summary>
		private static void StartOnHiddenForm(DrawingMode drawingModeStored,ToothChartEngine engineExpected) {
			RunSta(() => {
				using(Form form=new Form()) {
					form.ShowInTaskbar=false;
					form.FormBorderStyle=FormBorderStyle.None;
					form.StartPosition=FormStartPosition.Manual;
					form.ClientSize=_sizeChart;
					IntPtr intPtrForm=form.Handle;//creates the form's window; the form is never shown
					Assert.AreNotEqual(IntPtr.Zero,intPtrForm);
					Stopwatch stopwatch=Stopwatch.StartNew();
					int countFallbacksBefore=ToothChartWrapper.GetCountFallbackMessages();
					string line;
					using(ToothChartWrapper wrapper=new ToothChartWrapper()) {
						wrapper.Location=new Point(0,0);
						wrapper.Size=_sizeChart;
						form.Controls.Add(wrapper);
						wrapper.DrawMode=drawingModeStored;
						ToothChartEngine engineStarted=wrapper.Engine;
						Assert.IsFalse(form.Visible,"The form was shown.");
						int countColours;
						using(Bitmap bitmap=wrapper.GetBitmap()) {
							Assert.AreEqual(_sizeChart,bitmap.Size);
							HashSet<int> hashSetColours=new HashSet<int>();
							for(int y=0;y<bitmap.Height;y+=5) {
								for(int x=0;x<bitmap.Width;x+=5) {
									hashSetColours.Add(bitmap.GetPixel(x,y).ToArgb());
								}
							}
							countColours=hashSetColours.Count;
						}
						List<string> listFallbacks=ToothChartWrapper.GetFallbackMessagesSince(countFallbacksBefore);
						line="stored "+drawingModeStored+" ("+(int)drawingModeStored+"), came up "+engineStarted+", handle "+(wrapper.IsHandleCreated?"created":"not created")
							+", drew a "+_sizeChart.Width+"x"+_sizeChart.Height+" picture of "+countColours+" sampled colours, "+stopwatch.ElapsedMilliseconds+" ms"
							+(listFallbacks.Count==0?", no fallback":"; fallback: "+string.Join(" | ",listFallbacks));
						Record(line);
						form.Controls.Remove(wrapper);
						Assert.AreEqual(engineExpected,engineStarted,line);
						Assert.AreEqual(0,listFallbacks.Count,line);
						if(engineStarted==ToothChartEngine.Wpf3D && countColours<=1) {
							//REL-0.1-FIX: where WPF draws nothing (a disconnected session) the WPF chart's picture is one colour.
							ToothChartImageTests.WpfSession.RequireRendering("The engine and fallback checks ran: "+line);
						}
						Assert.IsTrue(countColours>1,"The picture is one colour: "+line);
					}
					Assert.IsFalse(form.Visible,"The form was shown.");
				}
			});
		}

		[TestMethod]
		public void Simple2D_StartsOnAHiddenForm() {
			//The stored Simple 2D choice (1) stays the 2D chart (O-30).
			StartOnHiddenForm(DrawingMode.Simple2D,ToothChartEngine.Simple2D);
		}

		[TestMethod]
		public void WpfChart_StartsOnAHiddenForm() {
			//A stored DirectX (0) or OpenGL (2) choice starts the WPF chart (TC-8); its picture comes from the chart's render thread.
			StartOnHiddenForm(DrawingMode.DirectX,ToothChartEngine.Wpf3D);
			StartOnHiddenForm(DrawingMode.OpenGL,ToothChartEngine.Wpf3D);
		}
	}
}
