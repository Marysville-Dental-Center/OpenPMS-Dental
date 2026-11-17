// The layout snapshot test (package UI-0b, docs/specs/ui-refresh-plan.md section 3.7, docs/port-notes/UI-0b.md): every designer control's
// Location, Size and ClientSize equal Data\layout-snapshot.tsv, except the controls Data\fit-fixes.tsv lists. A change that is not a fit fix
// fails here; a deliberate one (a new window, the Open Dental 26.2 port) writes the snapshot again with OPMS_UI_LAYOUT_SNAPSHOT_WRITE=1 and
// commits it with the change.
// Licensed under the GNU General Public License, version 2 or later. See LICENSE in the repository root.
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using ProjectFileTests;
using UiRefreshTests.LayoutSnapshot;
using UiRefreshTests.Scan;

namespace UiRefreshTests {
	[TestClass]
	public class LayoutSnapshotTests {
		[TestMethod]
		public void Snapshot_MatchesTheDesignerFiles() {
			string root=RepoPaths.Root();
			string path=RepoPaths.DataFile("layout-snapshot.tsv");
			List<LayoutSnapshotRow> listNow=LayoutSnapshotBuilder.Build(root);
			if(BuildOutput.IsRequiredValue(Environment.GetEnvironmentVariable(LayoutSnapshotBuilder.VARIABLE_WRITE))) {
				LayoutSnapshotBuilder.Write(path,listNow);
			}
			Assert.IsTrue(listNow.Count>10000,"Designer controls found: "+listNow.Count);
			List<LayoutSnapshotRow> listRecord=LayoutSnapshotBuilder.Read(File.ReadAllLines(path));
			HashSet<string> setFitFixes=new HashSet<string>(FitFixRow.ReadAll().Select(x => x.DesignerFile+"\t"+x.Control));
			List<string> listChanges=LayoutSnapshotBuilder.Compare(listRecord,listNow,setFitFixes);
			Assert.AreEqual(0,listChanges.Count,listChanges.Count+" designer layout changes not in fit-fixes.tsv (a deliberate change: run with "
				+LayoutSnapshotBuilder.VARIABLE_WRITE+"=1 and commit the snapshot): "+string.Join(" | ",listChanges.Take(20)));
		}

		[TestMethod]
		public void Parse_ReadsInitializeComponentOnly() {
			string text=string.Join("\n",new string[] {
				"namespace N {",
				"	partial class FormX {",
				"		private void InitializeComponent() {",
				"			this.label1 = new System.Windows.Forms.Label();",
				"			// label1 {",
				"			this.label1.Location = new System.Drawing.Point(12, 34);",
				"			this.label1.Size = new System.Drawing.Size(56, 13);",
				"			this.label1.Text = \"Has a brace { in it\";",
				"			this.button1.Location = new System.Drawing.Point(-1, 2);",
				"			this.ClientSize = new System.Drawing.Size(300, 200);",
				"		}",
				"		private void Other() {",
				"			this.label1.Location = new System.Drawing.Point(99, 99);",
				"		}",
				"	}",
				"}" });
			List<LayoutSnapshotRow> listRows=LayoutSnapshotBuilder.Parse("A/FormX.Designer.cs",text);
			Assert.AreEqual(3,listRows.Count);
			Assert.AreEqual("A/FormX.Designer.cs\tlabel1\t12,34\t56,13\t",listRows[0].ToLine());
			Assert.AreEqual("A/FormX.Designer.cs\tbutton1\t-1,2\t\t",listRows[1].ToLine());
			Assert.AreEqual("A/FormX.Designer.cs\t.\t\t\t300,200",listRows[2].ToLine());
			List<LayoutSnapshotRow> listChanged=LayoutSnapshotBuilder.Parse("A/FormX.Designer.cs",text.Replace("Size(56, 13)","Size(60, 13)"));
			List<string> listChanges=LayoutSnapshotBuilder.Compare(listRows,listChanged,new HashSet<string>());
			Assert.AreEqual(1,listChanges.Count);
			Assert.AreEqual(0,LayoutSnapshotBuilder.Compare(listRows,listChanged,new HashSet<string> { "A/FormX.Designer.cs\tlabel1" }).Count);
		}
	}
}
