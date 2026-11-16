// The window checks (tests\WindowChecksTests, run by the lead at the gate, never here) reach File > Graphics' controls by name and cast each one
// to a type: ((T)FindControl(ui,hwnd,"name",true)). Those files only build here, so a cast that does not match the field's type compiles and
// throws InvalidCastException only when the window check runs (TC-9a's review: I7 cast checkDrawInSoftware, an OpenPmsDental.UI.CheckBox, to
// the WinForms CheckBox). This source pin reads every such cast of a FormGraphics control and checks that the field FormGraphics declares can be
// cast to the type the check names, with the name resolved through that file's own using directives as the C# compiler resolves it.
// Licensed under the GNU General Public License, version 2 or later. See LICENSE in the repository root.
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text.RegularExpressions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using OpenPmsDental;

namespace ToothChartHostTests {
	[TestClass]
	public class GraphicsWindowCheckCastTests {
		///<summary>A cast of a control the window checks find by name: ((Type)FindControl(ui,hwnd,"name"...</summary>
		private static readonly Regex _regexCast=new Regex(@"\(\(([A-Za-z_][\w.]*)\)FindControl\(ui,\w+,""(\w+)""");
		///<summary>A plain using directive of a namespace (not an alias, not using static).</summary>
		private static readonly Regex _regexUsing=new Regex(@"^\s*using\s+([A-Za-z_][\w.]*)\s*;",RegexOptions.Multiline);

		///<summary>Each window check file that opens File > Graphics, relative to the repository root.</summary>
		private static readonly string[] _arrayFiles=new[] {
			@"tests\WindowChecksTests\InProc\InProcChecks.I.cs",
			@"tests\WindowChecksTests\InProc\InProcChecks.IJ.cs",
		};

		[TestMethod]
		public void WindowChecks_CastEachGraphicsControlToItsFieldType() {
			int countChecked=0;
			List<string> listWrong=new List<string>();
			foreach(string fileRelative in _arrayFiles) {
				string text=File.ReadAllText(Path.Combine(HostPaths.RepoRoot,fileRelative));
				List<string> listNamespaces=_regexUsing.Matches(text).Cast<Match>().Select(x => x.Groups[1].Value).ToList();
				foreach(Match match in _regexCast.Matches(text)) {
					string nameType=match.Groups[1].Value;
					string nameField=match.Groups[2].Value;
					FieldInfo fieldInfo=typeof(FormGraphics).GetField(nameField,BindingFlags.NonPublic|BindingFlags.Instance);
					if(fieldInfo==null) {
						continue;//not a name of File > Graphics' controls
					}
					countChecked++;
					List<Type> listTypes=ResolveCastType(nameType,listNamespaces);
					if(listTypes.Count!=1) {
						listWrong.Add(fileRelative+": ("+nameType+") of "+nameField+" resolves to "+listTypes.Count+" types: "
							+string.Join(", ",listTypes.Select(x => x.FullName)));
						continue;
					}
					if(!listTypes[0].IsAssignableFrom(fieldInfo.FieldType)) {
						listWrong.Add(fileRelative+": ("+nameType+") means "+listTypes[0].FullName+", but FormGraphics."+nameField+" is "
							+fieldInfo.FieldType.FullName);
					}
				}
			}
			//I.cs: radioSimpleChart, butSave. IJ.cs: radio3DChart twice, checkDrawInSoftware, butSave three times (a name match, so Show Features'
			//butSave is checked against FormGraphics' too; both are Buttons).
			Assert.IsTrue(countChecked>=8,"Only "+countChecked+" casts of File > Graphics' controls were found; the pattern no longer matches the checks.");
			StringAssert.Contains(File.ReadAllText(Path.Combine(HostPaths.RepoRoot,_arrayFiles[1])),"FindControl(ui,hwndGraphics,\"checkDrawInSoftware\",true)",
				"I7's Draw in software step");
			Assert.AreEqual(0,listWrong.Count,string.Join(Environment.NewLine,listWrong));
		}

		///<summary>A dotted name is a full name. A short name is looked up in each namespace the file's using directives name; more than one match
		///would not compile. The files' own namespaces (WindowChecksTests, WindowChecksTests.InProc) declare no control types, so they are left out.</summary>
		private static List<Type> ResolveCastType(string nameType,List<string> listNamespaces) {
			if(nameType.Contains(".")) {
				return FindTypes(new[] { nameType });
			}
			return FindTypes(listNamespaces.Select(x => x+"."+nameType));
		}

		private static List<Type> FindTypes(IEnumerable<string> fullNames) {
			List<Assembly> listAssemblies=new List<Assembly> { typeof(System.Windows.Forms.Control).Assembly,typeof(FormGraphics).Assembly };
			listAssemblies.AddRange(AppDomain.CurrentDomain.GetAssemblies().Where(x => !x.IsDynamic && !listAssemblies.Contains(x)));
			List<Type> listTypes=new List<Type>();
			foreach(string fullName in fullNames) {
				Type type=listAssemblies.Select(x => x.GetType(fullName,throwOnError:false)).FirstOrDefault(x => x!=null);
				if(type!=null && type.IsPublic && !listTypes.Contains(type)) {
					listTypes.Add(type);
				}
			}
			return listTypes;
		}
	}
}
