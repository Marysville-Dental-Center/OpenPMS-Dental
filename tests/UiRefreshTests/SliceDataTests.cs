// UI-12T (docs/port-notes/UI-12.md, "Tooling (UI-12T)"): the grid's data folders of one file per rollout slice. No window (no snapshot row)
// is in two files, every row is in its slice's file, the superseded fit fixes name rows of fit-fixes.tsv, and the snapshot writer writes a
// slice's rows into its own file only. tools/UiFit/split_grid_data.py makes the folders (its --check run is the Python side's test).
// Licensed under the GNU General Public License, version 2 or later. See LICENSE in the repository root.
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using UiRefreshTests.LayoutSnapshot;
using UiRefreshTests.Scan;

namespace UiRefreshTests {
	[TestClass]
	public class SliceDataTests {
		private const string HEADER_HAND="designer_file\twindow\tcontrol\tproperty\told\tnew\trule";

		[TestMethod]
		public void SliceFolders_NoWindowInTwoFiles() {
			Dictionary<string,string> dictHeaders=new Dictionary<string,string> {
				{ "grid-windows","window\tslice\tmode\tnote" },
				{ "grid-hand",HEADER_HAND },
				{ "grid-known","window\trule\tkey\twhy" },
				{ "grid-runtime-texts","window\tcontrol\ttext\twidth\twhy" },
				{ "fit-fixes-superseded","designer_file\twindow\tcontrol" },
			};
			Dictionary<string,string> dictWindowSlices=SliceData.WindowSlices();
			Assert.IsTrue(dictWindowSlices.Count>=94,"The pilot's and slice 1's windows: "+dictWindowSlices.Count);
			foreach(KeyValuePair<string,string> pair in dictHeaders) {
				string folder=SliceData.Folder(pair.Key);
				Assert.IsFalse(File.Exists(folder+".tsv"),pair.Key+".tsv is split into its folder (run tools/UiFit/split_grid_data.py).");
				List<SliceLine> listLines=SliceData.ReadLines(folder,pair.Value);
				Assert.IsTrue(listLines.Count>0,pair.Key+" has rows.");
				int column=pair.Value.Split('\t').ToList().IndexOf("window");
				List<string> listTwice=SliceData.KeysInTwoFiles(listLines,column);
				Assert.AreEqual(0,listTwice.Count,pair.Key+": windows in two slice files: "+string.Join("; ",listTwice.Take(10)));
				if(pair.Key=="grid-windows") {
					Assert.AreEqual(listLines.Count,listLines.Select(x => x.ArrayFields[0]).Distinct().Count(),"grid-windows lists a window twice.");
					continue;
				}
				foreach(SliceLine sliceLine in listLines) {
					string slice;
					Assert.IsTrue(dictWindowSlices.TryGetValue(sliceLine.ArrayFields[column],out slice),pair.Key+" names a window grid-windows does not list: "+sliceLine.Line);
					Assert.AreEqual(slice,sliceLine.Slice,pair.Key+": the row is in its window's slice file: "+sliceLine.Line);
				}
			}
		}

		[TestMethod]
		public void LayoutSnapshot_EachDesignerFileInItsOwnersFile() {
			SliceData.SnapshotOwners snapshotOwners=SliceData.SnapshotOwners.FromData();
			List<SliceLine> listLines=SliceData.ReadLines(SliceData.Folder("layout-snapshot"),LayoutSnapshotBuilder.HEADER);
			List<SliceLine> listSliced=listLines.Where(x => x.Slice!=SliceData.UNSLICED).ToList();
			List<string> listTwice=SliceData.KeysInTwoFiles(listSliced,0,1);
			Assert.AreEqual(0,listTwice.Count,"Snapshot rows in two slice files: "+string.Join("; ",listTwice.Take(10)));
			List<string> listWrong=listSliced.Where(x => snapshotOwners.Owner(x.ArrayFields[0])!=x.Slice).Select(x => x.Slice+": "+x.ArrayFields[0]).Distinct().ToList();
			Assert.AreEqual(0,listWrong.Count,"Snapshot rows in a slice file that does not own their designer file: "+string.Join("; ",listWrong.Take(10)));
			Assert.IsTrue(listSliced.Count>1000 && listLines.Count>10000,"Snapshot rows: "+listSliced.Count+" sliced of "+listLines.Count);
		}

		[TestMethod]
		public void FitFixesSuperseded_NameFitFixRows() {
			HashSet<string> hashSetFixes=new HashSet<string>(FitFixRow.ReadAll().Select(x => x.DesignerFile+"\t"+x.Window+"\t"+x.Control));
			List<SliceLine> listLines=SliceData.ReadLines(SliceData.Folder("fit-fixes-superseded"),"designer_file\twindow\tcontrol");
			Assert.IsTrue(listLines.Count>=1330,"The pilot's and slice 1's superseded fit fixes: "+listLines.Count);
			List<string> listMissing=listLines.Where(x => !hashSetFixes.Contains(x.Line)).Select(x => x.Line).ToList();
			Assert.AreEqual(0,listMissing.Count,"Superseded rows fit-fixes.tsv does not have: "+string.Join("; ",listMissing.Take(10)));
			Assert.AreEqual(listLines.Count,listLines.Select(x => x.Line).Distinct().Count(),"A superseded row is listed twice.");
			string[] arrayMarked=File.ReadAllLines(RepoPaths.DataFile("fit-fixes.tsv")).Where(x => x.Contains("superseded by the UI-12 grid")).ToArray();
			Assert.AreEqual(0,arrayMarked.Length,"fit-fixes.tsv notes carry the superseded mark again (list them in fit-fixes-superseded): "+string.Join("; ",arrayMarked.Take(5)));
		}

		///<summary>UI-12T item 5: docs\port-notes\UI-12-batches.tsv (tools/UiFit/plan_batches.py): every designer file in one batch, none that a
		///slice laid out already, and a group's files (a window and its user controls) in one batch.</summary>
		[TestMethod]
		public void Batches_EachFileOnceNoneLaidOut() {
			string path=Path.Combine(RepoPaths.Root(),"docs","port-notes","UI-12-batches.tsv");
			string[] arrayLines=File.ReadAllLines(path);
			Assert.AreEqual("batch\tkind\trank\tdesigner_file\twindow\tcontrols\tgroup",arrayLines[0]);
			SliceData.SnapshotOwners snapshotOwners=SliceData.SnapshotOwners.FromData();
			Dictionary<string,string> dictBatchOfFile=new Dictionary<string,string>();
			Dictionary<string,string> dictBatchOfGroup=new Dictionary<string,string>();
			foreach(string line in arrayLines.Skip(1).Where(x => x.Trim()!="")) {
				string[] arrayFields=line.Split('\t');
				Assert.AreEqual(7,arrayFields.Length,line);
				Assert.IsFalse(dictBatchOfFile.ContainsKey(arrayFields[3]),"A designer file in two batches: "+line);
				dictBatchOfFile[arrayFields[3]]=arrayFields[0];
				Assert.AreEqual(SliceData.UNSLICED,snapshotOwners.Owner(arrayFields[3]),"A batch names a file a slice laid out (rerun plan_batches.py): "+line);
				Assert.IsTrue(arrayFields[1]=="small" || arrayFields[1]=="large",line);
				string batchOfGroup;
				if(dictBatchOfGroup.TryGetValue(arrayFields[6],out batchOfGroup)) {
					Assert.AreEqual(batchOfGroup,arrayFields[0],"A group in two batches: "+line);
				}
				dictBatchOfGroup[arrayFields[6]]=arrayFields[0];
			}
			Assert.IsTrue(dictBatchOfFile.Count>100,"Batched files: "+dictBatchOfFile.Count);
		}

		[TestMethod]
		public void SliceOrder_PilotNumbersOthersUnsliced() {
			List<string> listSlices=new List<string> { SliceData.UNSLICED,"10","2","pilot","x","1" };
			listSlices.Sort(SliceData.CompareSlices);
			CollectionAssert.AreEqual(new List<string> { "pilot","1","2","10","x",SliceData.UNSLICED },listSlices);
		}

		[TestMethod]
		public void SnapshotOwners_FirstSliceByNameOrHandRow() {
			Dictionary<string,string> dictWindows=new Dictionary<string,string> { { "N.FormA","2" },{ "N.FormB","1" } };
			List<string[]> listHand=new List<string[]> { new string[] { "P/UserControlC.Designer.cs","N.FormB","c","Size","1,1","2,2","r" } };
			SliceData.SnapshotOwners snapshotOwners=new SliceData.SnapshotOwners(dictWindows,listHand);
			Assert.AreEqual("2",snapshotOwners.Owner("P/Forms/FormA.Designer.cs"));
			Assert.AreEqual("1",snapshotOwners.Owner("P/Forms/FormB.designer.cs"));
			Assert.AreEqual("1",snapshotOwners.Owner("P/UserControlC.Designer.cs"));
			Assert.AreEqual(SliceData.UNSLICED,snapshotOwners.Owner("P/Forms/FormD.Designer.cs"));
		}

		[TestMethod]
		public void WriteFolder_ASliceWritesOnlyItsOwnFile() {
			string folder=Path.Combine(Path.GetTempPath(),"UiRefreshTests-"+Guid.NewGuid().ToString("N"));
			try {
				SliceData.SnapshotOwners ownersBefore=new SliceData.SnapshotOwners(new Dictionary<string,string> { { "N.FormA","1" } },new List<string[]>());
				List<LayoutSnapshotRow> listBefore=new List<LayoutSnapshotRow> {
					Row("P/FormA.Designer.cs","a","1,1"),Row("P/FormB.Designer.cs","b","2,2"),Row("P/FormC.Designer.cs","c","3,3") };
				LayoutSnapshotBuilder.WriteFolder(folder,listBefore,ownersBefore);
				CollectionAssert.AreEqual(new List<string> { "1",SliceData.UNSLICED },SliceData.Files(folder).Select(SliceData.SliceOf).ToList());
				string unslicedBefore=File.ReadAllText(Path.Combine(folder,SliceData.UNSLICED+".tsv"));
				//Slice 2 lays out FormB: its rows go to 2.tsv; unsliced.tsv keeps its old FormB rows (pruned later by split_grid_data.py).
				SliceData.SnapshotOwners ownersAfter=new SliceData.SnapshotOwners(new Dictionary<string,string> { { "N.FormA","1" },{ "N.FormB","2" } },new List<string[]>());
				List<LayoutSnapshotRow> listAfter=new List<LayoutSnapshotRow> {
					Row("P/FormA.Designer.cs","a","1,1"),Row("P/FormB.Designer.cs","b","8,8"),Row("P/FormC.Designer.cs","c","3,3") };
				LayoutSnapshotBuilder.WriteFolder(folder,listAfter,ownersAfter);
				Assert.AreEqual(unslicedBefore,File.ReadAllText(Path.Combine(folder,SliceData.UNSLICED+".tsv")),"unsliced.tsv is not edited by a slice.");
				List<LayoutSnapshotRow> listRecord=LayoutSnapshotBuilder.ReadFolder(folder);
				Assert.AreEqual(0,LayoutSnapshotBuilder.Compare(listRecord,listAfter,new HashSet<string>()).Count,"The slice file wins over unsliced.tsv.");
				Assert.AreEqual(3,listRecord.Count);
			}
			finally {
				if(Directory.Exists(folder)) {
					Directory.Delete(folder,true);
				}
			}
		}

		private static LayoutSnapshotRow Row(string file,string control,string location) {
			return new LayoutSnapshotRow { File=file,Control=control,Location=location,Size="10,10",ClientSize="" };
		}
	}
}
