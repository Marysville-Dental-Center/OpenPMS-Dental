// The tier 1 builder (docs/specs/ui-refresh-plan.md section 5.4, docs/port-notes/UI-0b.md). For a Form or UserControl type it creates an empty
// object, runs only the base constructor (FormODBase, Form or UserControl), then calls InitializeComponent (each class's own, base classes
// first) and, for a FormODBase whose own constructor calls it (SourceIndex), InitializeLayoutManager. The type's own constructor never runs,
// so constructor arguments and the database calls in it do not matter. A FormODBase window is then laid out the way FormODBase.OnLoad does it,
// without a handle or a Show: SetPanelClient, SetScaleMS, the window's bounds, LayoutFormBoundsAndFonts. A UserControl is hosted the way the
// program adds it: in a bare test FormODBase through LayoutManager.AddUnscaled (the preference pages, the setup wizard's pages, ControlImages)
// or LayoutManager.Add (every other UserControl a program window uses), or in a plain test Form when no program window hosts it (the plain
// exes', the graph's). Nothing is shown and no handle is asked for.
// Licensed under the GNU General Public License, version 2 or later. See LICENSE in the repository root.
using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Windows.Forms;
using OpenPmsDental;
using ProjectFileTests;

namespace UiRefreshTests.Scan {
	///<summary>How a type is built (section 5.4).</summary>
	public enum BuildKind {
		///<summary>A FormODBase window whose constructor calls InitializeLayoutManager.</summary>
		LayoutManagerWindow,
		///<summary>A FormODBase window whose constructor never calls it (the five allocator windows of section 5.1): built like a plain form.</summary>
		WindowWithoutLayoutManager,
		///<summary>A Form that is not a FormODBase (the plain exes', the graph's, a few of the program's).</summary>
		PlainWindow,
		///<summary>A UserControl a program window adds with LayoutManager.Add (or creates in its designer).</summary>
		UserControlAdd,
		///<summary>A UserControl a program window adds with LayoutManager.AddUnscaled.</summary>
		UserControlAddUnscaled,
		///<summary>A UserControl no program window hosts: hosted in a plain test Form.</summary>
		UserControlPlain,
	}

	///<summary>One type the scan builds, with how and why.</summary>
	public class ScanType {
		public Type Type;
		public BuildKind Kind;
		///<summary>The assembly's short name.</summary>
		public string Assembly="";
		///<summary>Why the kind was chosen, for the types list.</summary>
		public string Note="";

		public bool IsUnderLayoutManager {
			get { return Kind==BuildKind.LayoutManagerWindow || Kind==BuildKind.UserControlAdd || Kind==BuildKind.UserControlAddUnscaled; }
		}

		public override string ToString() {
			return Type.FullName;
		}
	}

	///<summary>A built window: the form, the control that was built (the form itself, or the hosted UserControl) and its LayoutManagerForms.</summary>
	public class BuiltWindow : IDisposable {
		public ScanType ScanType;
		public Form Form;
		///<summary>The built control: the form, or the hosted UserControl.</summary>
		public Control Root;
		public LayoutManagerForms LayoutManager;
		///<summary>The scale the window is laid out at now.</summary>
		public float Scale=1f;

		///<summary>Lays a LayoutManager window out at a simulated Windows scale, as FormODBase.OnLoad does after a DPI change. Other windows
		///are not scaled by LayoutManager and stay as built.</summary>
		public void LayOutAt(float scale) {
			Scale=scale;
			if(LayoutManager==null) {
				return;
			}
			LayoutManager.SetScaleMS(scale);
			FormODBase formODBase=(FormODBase)Form;
			int widthBorder=(int)Math.Round(8+(5*(LayoutManager.GetScaleMS()-1)),MidpointRounding.AwayFromZero);
			int width=LayoutManager.Scale(LayoutManager.SizeClientOriginal.Width)+widthBorder*2;
			int height=LayoutManager.Scale(LayoutManager.SizeClientOriginal.Height)+widthBorder+LayoutManager.GetHeightTitleBar();
			formODBase.Bounds=new Rectangle(formODBase.Left,formODBase.Top,width,height);
			LayoutManager.LayoutFormBoundsAndFonts(formODBase);
		}

		public void Dispose() {
			try {
				Form?.Dispose();
			}
			catch(Exception) {
				//A window built without its own constructor may fail in its own Dispose; the scan goes on.
			}
			if(Root!=null && Root!=Form) {
				try {
					Root.Dispose();
				}
				catch(Exception) {
				}
			}
		}
	}

	public static class WindowBuilder {
		///<summary>The program's libraries whose windows and UserControls the scan builds, loaded by name from this test's folder (the program
		///references them), else from their own build folder.</summary>
		public static readonly string[] ArrayProgramAssemblies=new string[] { "OpenPmsDental","OpenPmsDentalBusiness","CodeBase","SparksToothChart",
			"OpenPmsDentalGraph","RdlViewer","ODR","WpfControlsOD" };
		///<summary>The plain exes, read from their own build folders: project folder and assembly name. Their UserControls are never under
		///LayoutManager.</summary>
		public static readonly string[][] ArrayPlainExes=new string[][] {
			new string[] { "CentralManager","CentralManager" },
			new string[] { "DatabaseIntegrityCheck","DatabaseIntegrityCheck" },
			new string[] { "DocumentationBuilder","DocumentationBuilder" },
			new string[] { "MySQLManager","MySQLManager" },
			new string[] { "OpenPmsDentalDbSettings","OpenPmsDentalDbSettings" },
			new string[] { "OpenPmsDentalHL7","OpenPmsDentalHL7" },
			new string[] { "RdlDesign","RdlDesigner" },
			new string[] { "ServiceManager","ServiceManager" },
			new string[] { "UpdateFileCopier","UpdateFileCopier" },
			new string[] { "xCrudGenerator","CrudGenerator" },
		};
		///<summary>Libraries whose UserControls are hosted by plain forms only (section 5.1: the 12 graph UserControls).</summary>
		public static readonly string[] ArrayPlainLibraries=new string[] { "OpenPmsDentalGraph" };
		///<summary>Not built: the 32-bit helper (OpenPmsDentalHelper32) cannot load in this 64-bit process.</summary>
		public const string NOTE_HELPER32="OpenPmsDentalHelper32 is a 32-bit process (decision 23); its forms cannot load in the 64-bit test process.";

		#region The types
		///<summary>Every Form and UserControl type of the program's libraries and the plain exes, with how it is built. A plain exe that is not
		///built follows BuildOutput's rule.</summary>
		public static List<ScanType> GetScanTypes() {
			SourceIndex sourceIndex=SourceIndex.Get();
			HashSet<Type> setAddUnscaled=GetAddUnscaledTypes();
			List<ScanType> listScanTypes=new List<ScanType>();
			foreach(string name in ArrayProgramAssemblies) {
				Assembly assembly=LoadProgramAssembly(name);
				bool isPlainLibrary=ArrayPlainLibraries.Contains(name);
				listScanTypes.AddRange(GetScanTypes(assembly,sourceIndex,setAddUnscaled,isProgram:name=="OpenPmsDental",isPlain:isPlainLibrary));
			}
			foreach(string[] arrayExe in ArrayPlainExes) {
				Assembly assembly=LoadPlainExe(arrayExe[0],arrayExe[1]);
				listScanTypes.AddRange(GetScanTypes(assembly,sourceIndex,setAddUnscaled,isProgram:false,isPlain:true));
			}
			return listScanTypes.OrderBy(x => x.Assembly,StringComparer.Ordinal).ThenBy(x => x.Type.FullName,StringComparer.Ordinal).ToList();
		}

		private static IEnumerable<Type> GetLoadableTypes(Assembly assembly) {
			try {
				return assembly.GetTypes();
			}
			catch(ReflectionTypeLoadException reflectionTypeLoadException) {
				return reflectionTypeLoadException.Types.Where(x => x!=null);
			}
		}

		private static List<ScanType> GetScanTypes(Assembly assembly,SourceIndex sourceIndex,HashSet<Type> setAddUnscaled,bool isProgram,bool isPlain) {
			List<ScanType> listScanTypes=new List<ScanType>();
			string nameAssembly=assembly.GetName().Name;
			foreach(Type type in GetLoadableTypes(assembly)) {
				if(type.IsAbstract || type.ContainsGenericParameters || type.IsNested && type.Name.Contains("<")) {
					continue;
				}
				if(type==typeof(FormODBase)) {
					continue;
				}
				bool isForm=typeof(Form).IsAssignableFrom(type);
				bool isUserControl=typeof(UserControl).IsAssignableFrom(type);
				if(!isForm && !isUserControl) {
					continue;
				}
				ScanType scanType=new ScanType { Type=type,Assembly=nameAssembly };
				if(typeof(FormODBase).IsAssignableFrom(type)) {
					bool? calls=CallsInitializeLayoutManager(type,sourceIndex);
					if(calls==false) {
						scanType.Kind=BuildKind.WindowWithoutLayoutManager;
						scanType.Note="its constructor does not call InitializeLayoutManager";
					}
					else {
						scanType.Kind=BuildKind.LayoutManagerWindow;
						scanType.Note=calls==true ? "" : "no constructor in the source; built with InitializeLayoutManager";
					}
				}
				else if(isForm) {
					scanType.Kind=BuildKind.PlainWindow;
				}
				else if(setAddUnscaled.Contains(type)) {
					scanType.Kind=BuildKind.UserControlAddUnscaled;
				}
				else if(isPlain) {
					scanType.Kind=BuildKind.UserControlPlain;
					scanType.Note="not hosted by a program window";
				}
				else if(isProgram || sourceIndex.SetNewInProgram.Contains(type.Name)) {
					scanType.Kind=BuildKind.UserControlAdd;
				}
				else {
					scanType.Kind=BuildKind.UserControlPlain;
					scanType.Note="no program window creates it";
				}
				listScanTypes.Add(scanType);
			}
			return listScanTypes;
		}

		///<summary>True, false or null (no constructor in the source) for the type, else for its nearest base class with a constructor in the
		///source, up to FormODBase: an implicit constructor runs its base class's.</summary>
		public static bool? CallsInitializeLayoutManager(Type type,SourceIndex sourceIndex) {
			for(Type t=type;t!=null && t!=typeof(FormODBase);t=t.BaseType) {
				bool? calls=sourceIndex.CallsInitializeLayoutManager(t.FullName);
				if(calls.HasValue) {
					return calls;
				}
			}
			return null;
		}

		///<summary>The UserControls the program adds with LayoutManager.AddUnscaled (a grep of AddUnscaled on main): the preference pages
		///(FormPreferences' UserControl fields), the setup wizard's pages (SetupWizControl and its subclasses) and ControlImages.</summary>
		public static HashSet<Type> GetAddUnscaledTypes() {
			HashSet<Type> setTypes=new HashSet<Type>();
			Assembly assembly=typeof(FormODBase).Assembly;
			foreach(FieldInfo fieldInfo in typeof(FormPreferences).GetFields(BindingFlags.Instance|BindingFlags.NonPublic|BindingFlags.Public|BindingFlags.DeclaredOnly)) {
				if(typeof(UserControl).IsAssignableFrom(fieldInfo.FieldType)) {
					setTypes.Add(fieldInfo.FieldType);
				}
			}
			Type typeSetupWiz=typeof(OpenPmsDental.User_Controls.SetupWizard.SetupWizControl);
			foreach(Type type in GetLoadableTypes(assembly)) {
				if(typeSetupWiz.IsAssignableFrom(type)) {
					setTypes.Add(type);
				}
			}
			setTypes.Add(typeof(ControlImages));
			return setTypes;
		}

		///<summary>A program library by name (this test's folder), else from its project's build folder.</summary>
		public static Assembly LoadProgramAssembly(string name) {
			try {
				return Assembly.Load(new AssemblyName(name));
			}
			catch(FileNotFoundException) {
			}
			string folderProject=name=="ODR" ? "ODR" : name;
			return LoadFromBuildFolder(folderProject,name);
		}

		public static Assembly LoadPlainExe(string folderProject,string nameAssembly) {
			return LoadFromBuildFolder(folderProject,nameAssembly);
		}

		///<summary>assembly.dll from folderProject\bin\{this configuration}, else any configuration; BuildOutput's rule when it is not built.</summary>
		public static Assembly LoadFromBuildFolder(string folderProject,string nameAssembly) {
			string folderBin=Path.Combine(RepoPaths.Root(),folderProject,"bin");
			string configuration=new DirectoryInfo(AppContext.BaseDirectory.TrimEnd(Path.DirectorySeparatorChar)).Name;
			string path=Path.Combine(folderBin,configuration,nameAssembly+".dll");
			if(!File.Exists(path) && Directory.Exists(folderBin)) {
				path=Directory.EnumerateFiles(folderBin,nameAssembly+".dll",SearchOption.AllDirectories).FirstOrDefault()??path;
			}
			if(!File.Exists(path)) {
				BuildOutput.NotBuilt(nameAssembly+".dll is not built ("+path+"). Build the whole solution filter (x64) first.");
			}
			return Assembly.LoadFrom(path);
		}
		#endregion The types

		#region Build
		///<summary>Builds the type as section 5.4 says and lays it out at 100%. Throws what building throws (TargetInvocationException unwrapped).</summary>
		public static BuiltWindow Build(ScanType scanType) {
			BuiltWindow builtWindow=new BuiltWindow();
			builtWindow.ScanType=scanType;
			try {
				switch(scanType.Kind) {
					case BuildKind.LayoutManagerWindow:
						FormODBase formODBase=(FormODBase)CreateBare(scanType.Type,typeof(FormODBase));
						builtWindow.Form=formODBase;
						builtWindow.Root=formODBase;
						InvokeNonPublic(formODBase,typeof(FormODBase),"InitializeLayoutManager",new object[] { false,false });
						builtWindow.LayoutManager=GetLayoutManager(formODBase);
						SetPanelClientAsOnLoad(formODBase,builtWindow.LayoutManager);
						builtWindow.LayOutAt(1f);
						break;
					case BuildKind.WindowWithoutLayoutManager:
						builtWindow.Form=(Form)CreateBare(scanType.Type,typeof(FormODBase));
						builtWindow.Root=builtWindow.Form;
						break;
					case BuildKind.PlainWindow:
						builtWindow.Form=(Form)CreateBare(scanType.Type,typeof(Form));
						builtWindow.Root=builtWindow.Form;
						break;
					case BuildKind.UserControlAdd:
					case BuildKind.UserControlAddUnscaled:
						UserControl userControl=(UserControl)CreateBare(scanType.Type,typeof(UserControl));
						builtWindow.Root=userControl;
						HostInFormODBase(builtWindow,userControl,isUnscaled:scanType.Kind==BuildKind.UserControlAddUnscaled);
						break;
					case BuildKind.UserControlPlain:
						UserControl userControlPlain=(UserControl)CreateBare(scanType.Type,typeof(UserControl));
						builtWindow.Root=userControlPlain;
						Form formPlain=new Form();
						formPlain.ClientSize=userControlPlain.Size;
						builtWindow.Form=formPlain;
						formPlain.Controls.Add(userControlPlain);
						break;
				}
			}
			catch(Exception) {
				builtWindow.Dispose();
				throw;
			}
			return builtWindow;
		}

		///<summary>Hosts a UserControl in a bare test FormODBase the way the program adds it, and lays it out at 100%.</summary>
		public static void HostInFormODBase(BuiltWindow builtWindow,UserControl userControl,bool isUnscaled) {
			HostFormOD hostFormOD=new HostFormOD(userControl.Size);
			builtWindow.Form=hostFormOD;
			LayoutManagerForms layoutManager=hostFormOD.GetLayoutManager();
			builtWindow.LayoutManager=layoutManager;
			SetPanelClientAsOnLoad(hostFormOD,layoutManager);
			userControl.Location=new Point(0,0);
			if(isUnscaled) {
				//FormPreferences.FillUserControls and FormSetupWizardProgress dock each page to fill before AddUnscaled.
				userControl.Dock=DockStyle.Fill;
				layoutManager.AddUnscaled(userControl,hostFormOD.PanelClient);
			}
			else {
				layoutManager.Add(userControl,hostFormOD.PanelClient);
			}
			layoutManager.LayoutControlBoundsAndFonts(hostFormOD.PanelClient);
			builtWindow.LayOutAt(1f);
		}

		///<summary>A FormODBase window made by its own constructor (a test window), laid out at 100% the way FormODBase.OnLoad does it, without
		///a Show.</summary>
		public static BuiltWindow LoadWithoutShow(FormODBase formODBase,LayoutManagerForms layoutManager) {
			BuiltWindow builtWindow=new BuiltWindow();
			builtWindow.ScanType=new ScanType { Type=formODBase.GetType(),Kind=BuildKind.LayoutManagerWindow,Assembly=formODBase.GetType().Assembly.GetName().Name };
			builtWindow.Form=formODBase;
			builtWindow.Root=formODBase;
			builtWindow.LayoutManager=layoutManager;
			SetPanelClientAsOnLoad(formODBase,layoutManager);
			builtWindow.LayOutAt(1f);
			return builtWindow;
		}

		///<summary>FormODBase.OnLoad's SetPanelClient for a window that is not maximized (a maximized one got it in InitializeLayoutManager).</summary>
		private static void SetPanelClientAsOnLoad(FormODBase formODBase,LayoutManagerForms layoutManager) {
			if(formODBase.PanelClient==null || formODBase.PanelBorders==null) {
				layoutManager.SetPanelClient(formODBase);
			}
		}

		public static LayoutManagerForms GetLayoutManager(FormODBase formODBase) {
			FieldInfo fieldInfo=typeof(FormODBase).GetField("LayoutManager",BindingFlags.Instance|BindingFlags.NonPublic);
			return (LayoutManagerForms)fieldInfo.GetValue(formODBase);
		}

		///<summary>An empty object of type, with only typeBase's constructor run (and the ones it chains to), then each class's own
		///InitializeComponent from typeBase down to type.</summary>
		public static object CreateBare(Type type,Type typeBase) {
			object obj=RuntimeHelpers.GetUninitializedObject(type);
			ConstructorInfo constructorInfo=typeBase.GetConstructor(BindingFlags.Instance|BindingFlags.Public|BindingFlags.NonPublic,null,Type.EmptyTypes,null);
			try {
				constructorInfo.Invoke(obj,null);
				List<Type> listChain=new List<Type>();
				for(Type t=type;t!=null && t!=typeBase;t=t.BaseType) {
					listChain.Add(t);
				}
				listChain.Reverse();
				foreach(Type t in listChain) {
					MethodInfo methodInfo=t.GetMethod("InitializeComponent",BindingFlags.Instance|BindingFlags.NonPublic|BindingFlags.Public|BindingFlags.DeclaredOnly,
						null,Type.EmptyTypes,null);
					methodInfo?.Invoke(obj,null);
				}
			}
			catch(TargetInvocationException targetInvocationException) {
				(obj as IDisposable)?.Dispose();
				throw Unwrap(targetInvocationException);
			}
			catch(Exception) {
				(obj as IDisposable)?.Dispose();
				throw;
			}
			return obj;
		}

		private static void InvokeNonPublic(object obj,Type type,string name,object[] arrayArgs) {
			MethodInfo methodInfo=type.GetMethod(name,BindingFlags.Instance|BindingFlags.NonPublic|BindingFlags.Public);
			try {
				methodInfo.Invoke(obj,arrayArgs);
			}
			catch(TargetInvocationException targetInvocationException) {
				throw Unwrap(targetInvocationException);
			}
		}

		private static Exception Unwrap(TargetInvocationException targetInvocationException) {
			Exception exception=targetInvocationException;
			while(exception is TargetInvocationException && exception.InnerException!=null) {
				exception=exception.InnerException;
			}
			return exception;
		}

		///<summary>True when the type has an InitializeComponent of its own or in a base class below typeBase (designer code to scan).</summary>
		public static bool HasDesignerCode(Type type) {
			for(Type t=type;t!=null && t!=typeof(FormODBase) && t!=typeof(Form) && t!=typeof(UserControl);t=t.BaseType) {
				if(t.GetMethod("InitializeComponent",BindingFlags.Instance|BindingFlags.NonPublic|BindingFlags.Public|BindingFlags.DeclaredOnly,null,Type.EmptyTypes,null)!=null) {
					return true;
				}
			}
			return false;
		}
		#endregion Build
	}

	///<summary>The bare test FormODBase that hosts a UserControl (section 5.4): its client size is the control's, and it calls
	///InitializeLayoutManager in its constructor like every program window.</summary>
	public class HostFormOD : FormODBase {
		public HostFormOD(Size sizeClient) {
			Name="HostFormOD";
			ClientSize=sizeClient;
			InitializeLayoutManager();
		}

		public LayoutManagerForms GetLayoutManager() {
			return LayoutManager;
		}
	}
}
