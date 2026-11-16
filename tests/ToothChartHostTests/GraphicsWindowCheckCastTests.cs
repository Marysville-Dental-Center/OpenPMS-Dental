// The window checks (tests\WindowChecksTests, run by the lead at the gate, never here) reach File > Graphics' controls by name and cast each one
// to a type: ((T)FindControl(ui,hwnd,"name",true)). Those files only build here, so a cast that does not match the field's type compiles and
// throws InvalidCastException only when the window check runs. This source pin reads every such cast of a FormGraphics control and checks
// that the field FormGraphics declares can be cast to the type the check names, as the C# compiler resolves that name in those files.
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
				//The files resolve a short name through these using directives only (no using OpenPmsDental.UI, no alias); the pin relies on that.
				StringAssert.Contains(text,"using System.Windows.Forms;",fileRelative);
				Assert.IsFalse(Regex.IsMatch(text,@"^\s*using\s+(\w+\s*=\s*)?OpenPmsDental\.UI\s*;",RegexOptions.Multiline),
					fileRelative+" has a using for OpenPmsDental.UI; this pin's name resolution no longer matches the compiler's.");
				foreach(Match match in _regexCast.Matches(text)) {
					string nameType=match.Groups[1].Value;
					string nameField=match.Groups[2].Value;
					FieldInfo fieldInfo=typeof(FormGraphics).GetField(nameField,BindingFlags.NonPublic|BindingFlags.Instance);
					if(fieldInfo==null) {
						continue;//a control of another window
					}
					countChecked++;
					Type typeCast=ResolveCastType(nameType);
					Assert.IsNotNull(typeCast,fileRelative+": the cast type "+nameType+" of "+nameField+" was not found.");
					if(!typeCast.IsAssignableFrom(fieldInfo.FieldType)) {
						listWrong.Add(fileRelative+": ("+nameType+") means "+typeCast.FullName+", but FormGraphics."+nameField+" is "
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

		///<summary>A dotted name is a full name; a short name resolves as in the window check files (namespace WindowChecksTests.InProc, with
		///using System.Windows.Forms): a type of WindowChecksTests or its InProc namespace would come first, but the casts here name controls.</summary>
		private static Type ResolveCastType(string nameType) {
			if(nameType.Contains(".")) {
				return FindType(nameType);
			}
			return FindType("System.Windows.Forms."+nameType);
		}

		private static Type FindType(string fullName) {
			foreach(Assembly assembly in new[] { typeof(System.Windows.Forms.Control).Assembly,typeof(FormGraphics).Assembly }) {
				Type type=assembly.GetType(fullName);
				if(type!=null) {
					return type;
				}
			}
			return AppDomain.CurrentDomain.GetAssemblies().Select(x => x.GetType(fullName)).FirstOrDefault(x => x!=null);
		}
	}
}
