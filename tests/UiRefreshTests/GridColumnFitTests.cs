// UI-COLS (docs/port-notes/UI-COLS.md): the grid columns of a fixed-format value hold their widest normal value in the program's grid font at
// 100% and 150% (GridColumnFit), and the display-field grids of the windows the harness builds (WindowBuilder) show every column of their
// default display fields inside the grid: none pushed past its right edge, the last one (which GridOD widens to the edge) wide enough for its
// value. No database (the preference the defaults read is put in the cache), nothing shown.
// Licensed under the GNU General Public License, version 2 or later. See LICENSE in the repository root.
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Windows.Forms;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using OpenPmsDental;
using OpenPmsDental.UI;
using OpenPmsDentalBusiness;
using UiRefreshTests.Scan;

namespace UiRefreshTests {
	[TestClass]
	public class GridColumnFitTests {
		///<summary>The display-field grids of windows the harness builds: window type, grid field, category.</summary>
		public static readonly Tuple<Type,string,DisplayFieldCategory>[] ArrayDisplayFieldGrids=new Tuple<Type,string,DisplayFieldCategory>[] {
			Tuple.Create(typeof(ControlAccount),"gridAccount",DisplayFieldCategory.AccountModule),
			Tuple.Create(typeof(FormApptEdit),"gridProc",DisplayFieldCategory.AppointmentEdit),
			Tuple.Create(typeof(FormApptEdit),"gridProc",DisplayFieldCategory.PlannedAppointmentEdit),
		};

		[TestMethod]
		public void GetKind_ReadsTheHeadings() {
			Assert.AreEqual("date",GridColumnFit.GetKind("Date"));
			Assert.AreEqual("date",GridColumnFit.GetKind("Due Date"));
			Assert.AreEqual("datetime",GridColumnFit.GetKind("Date Time"));
			Assert.AreEqual("time",GridColumnFit.GetKind("Time"));
			Assert.AreEqual("money",GridColumnFit.GetKind("Fee"));
			Assert.AreEqual("money",GridColumnFit.GetKind("Balance"));
			Assert.AreEqual("phone",GridColumnFit.GetKind("Wireless Phone"));
			Assert.AreEqual("tooth",GridColumnFit.GetKind("Tth"));
			Assert.AreEqual("surface",GridColumnFit.GetKind("Surf"));
			Assert.IsNull(GridColumnFit.GetKind("Description"));
			Assert.IsNull(GridColumnFit.GetKind("Patient"));
		}

		[TestMethod]
		public void Columns_OfAKnownValueType_HoldTheirWidestValue_At100And150() {
			List<ColumnKnown> listKnown=GridColumnFit.ReadKnown();
			List<ColumnSite> listSites=GridColumnFit.ScanSource(RepoPaths.Root(),listKnown);
			Assert.IsTrue(listSites.Count(x => x.Kind!="other")>400,"The scan finds the columns of a value type: "+listSites.Count(x => x.Kind!="other"));
			List<string> listProblems=new List<string>();
			foreach(ColumnSite columnSite in listSites) {
				string problem=GridColumnFit.GetProblem(columnSite);
				if(problem!="") {
					listProblems.Add(columnSite+" ("+columnSite.Kind+"): "+problem);
				}
			}
			Assert.AreEqual(0,listProblems.Count,"Columns too narrow for their value (widen them, taking the room from a column with room to spare, or list"
				+" them in Data\\"+GridColumnFit.FILE_KNOWN+" with the value they really show):\r\n"+string.Join("\r\n",listProblems));
		}

		[TestMethod]
		public void KnownList_EveryRowMatchesAColumn() {
			List<ColumnKnown> listKnown=GridColumnFit.ReadKnown();
			GridColumnFit.ScanSource(RepoPaths.Root(),listKnown);
			List<string> listStale=listKnown.Where(x => !x.IsUsed).Select(x => x.File+" \""+x.Title+"\" "+x.Width).ToList();
			Assert.AreEqual(0,listStale.Count,"Rows of Data\\"+GridColumnFit.FILE_KNOWN+" that match no column (file, title and width):\r\n"
				+string.Join("\r\n",listStale));
			foreach(ColumnKnown columnKnown in listKnown) {
				Assert.IsTrue(columnKnown.Kind=="other" || GridColumnFit.DictValues.ContainsKey(columnKnown.Kind),"Unknown kind: "+columnKnown.Kind);
				Assert.IsFalse(string.IsNullOrWhiteSpace(columnKnown.Reason),"Every row says why: "+columnKnown.File+" "+columnKnown.Title);
			}
		}

		[TestMethod]
		public void DisplayFieldGrids_InTheirWindows_ShowEveryColumn() {
			List<string> listProblems=new List<string>();
			StaRunner.Run(() => {
				List<ScanType> listScanTypes=WindowBuilder.GetScanTypes();
				foreach(Tuple<Type,string,DisplayFieldCategory> tuple in ArrayDisplayFieldGrids) {
					List<DisplayField> listDisplayFields=GetDefaultDisplayFields(tuple.Item3);
					foreach(float scale in GridColumnFit.ArrayScales) {
						ScanType scanType=listScanTypes.Single(x => x.Type==tuple.Item1);
						using(BuiltWindow builtWindow=WindowBuilder.Build(scanType)) {
							if(scale!=1f) {
								builtWindow.LayOutAt(scale);
							}
							GridOD gridOD=(GridOD)GetField(builtWindow.Root,tuple.Item2);
							foreach(string problem in GetProblems(gridOD.Width,listDisplayFields,scale)) {
								listProblems.Add(tuple.Item1.Name+"."+tuple.Item2+" ("+tuple.Item3+") at "+(int)Math.Round(scale*100)+"%: "+problem);
							}
						}
					}
				}
			});
			Assert.AreEqual(0,listProblems.Count,string.Join("\r\n",listProblems));
		}

		///<summary>The columns of the display fields in a grid this wide at the scale, as GridOD.ComputeColumns lays them out with its vertical
		///scroll bar shown: each at its scaled width from the left, the last one widened to the grid's edge. A column that ends past the edge, or a
		///last column narrower than its value, is a problem.</summary>
		public static List<string> GetProblems(int widthGrid,List<DisplayField> listDisplayFields,float scale) {
			List<string> listProblems=new List<string>();
			int right=widthGrid-2-GridColumnFit.ScaleI(17,scale);
			int x=0;
			for(int i=0;i<listDisplayFields.Count;i++) {
				string title=listDisplayFields[i].Description=="" ? listDisplayFields[i].InternalName : listDisplayFields[i].Description;
				int width=GridColumnFit.ScaleI(listDisplayFields[i].ColumnWidth,scale);
				string kind=GridColumnFit.GetKind(title);
				int widthNeeded=kind==null ? 1 : GridColumnFit.GetWidthNeeded(GridColumnFit.DictValues[kind],scale);
				bool isLast=i==listDisplayFields.Count-1;
				int widthShown=isLast ? right-x : Math.Min(width,right-x);
				if(widthShown<widthNeeded || (!isLast && x+width>right)) {
					listProblems.Add("\""+title+"\" from "+x+" to "+(x+width)+" shows "+widthShown+" px of the grid's "+right+" (needs "+widthNeeded+")");
				}
				x+=width;
			}
			return listProblems;
		}

		///<summary>DisplayFields' default list of the category for a dental practice (PracticeIsMedicalOnly off, in the cache for the call).</summary>
		public static List<DisplayField> GetDefaultDisplayFields(DisplayFieldCategory displayFieldCategory) {
			using(new SampleData.PrefScope(PrefName.PracticeIsMedicalOnly.ToString(),"0")) {
				return DisplayFields.GetDefaultList(displayFieldCategory);
			}
		}

		public static object GetField(object obj,string name) {
			for(Type type=obj.GetType();type!=null;type=type.BaseType) {
				FieldInfo fieldInfo=type.GetField(name,BindingFlags.Instance|BindingFlags.NonPublic|BindingFlags.Public);
				if(fieldInfo!=null) {
					return fieldInfo.GetValue(obj);
				}
			}
			throw new MissingFieldException(obj.GetType().Name,name);
		}
	}
}
