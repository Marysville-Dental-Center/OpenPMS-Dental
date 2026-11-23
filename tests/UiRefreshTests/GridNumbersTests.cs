// UI-12T item 2: the layout grid's numbers have one source, Data\grid-numbers.tsv (read by tools/UiFit/grid_tools.py and grid_hand.py);
// CodeBase.UiGrid has the same numbers as constants for product code. This test fails when the two differ.
// Licensed under the GNU General Public License, version 2 or later. See LICENSE in the repository root.
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Reflection;
using CodeBase;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using UiRefreshTests.Scan;

namespace UiRefreshTests {
	[TestClass]
	public class GridNumbersTests {
		///<summary>name -> value of Data\grid-numbers.tsv.</summary>
		public static Dictionary<string,double> ReadGridNumbers() {
			Dictionary<string,double> dictNumbers=new Dictionary<string,double>();
			foreach(string line in File.ReadAllLines(RepoPaths.DataFile("grid-numbers.tsv"))) {
				if(line.Trim()=="" || line.StartsWith("#") || line.StartsWith("name\t")) {
					continue;
				}
				string[] arrayFields=line.Split('\t');
				Assert.AreEqual(3,arrayFields.Length,"grid-numbers.tsv: name, value, why: "+line);
				Assert.IsFalse(dictNumbers.ContainsKey(arrayFields[0]),"grid-numbers.tsv lists "+arrayFields[0]+" twice.");
				dictNumbers[arrayFields[0]]=double.Parse(arrayFields[1],CultureInfo.InvariantCulture);
			}
			return dictNumbers;
		}

		[TestMethod]
		public void UiGrid_EqualsTheDataFile() {
			Dictionary<string,double> dictNumbers=ReadGridNumbers();
			Dictionary<string,double> dictConstants=typeof(UiGrid).GetFields(BindingFlags.Public|BindingFlags.Static)
				.Where(x => x.IsLiteral)
				.ToDictionary(x => x.Name,x => Convert.ToDouble(x.GetRawConstantValue(),CultureInfo.InvariantCulture));
			List<string> listDiffer=new List<string>();
			foreach(KeyValuePair<string,double> pair in dictNumbers) {
				double value;
				if(!dictConstants.TryGetValue(pair.Key.ToUpperInvariant(),out value)) {
					listDiffer.Add(pair.Key+": no UiGrid."+pair.Key.ToUpperInvariant());
				}
				else if(value!=pair.Value) {
					listDiffer.Add(pair.Key+": "+pair.Value+" in the file, "+value+" in UiGrid");
				}
			}
			foreach(string name in dictConstants.Keys) {
				if(!dictNumbers.ContainsKey(name.ToLowerInvariant())) {
					listDiffer.Add("UiGrid."+name+" is not in grid-numbers.tsv");
				}
			}
			Assert.AreEqual(0,listDiffer.Count,string.Join("; ",listDiffer));
		}

		[TestMethod]
		public void GridNumbers_PitchesAreTheirSums() {
			Dictionary<string,double> dictNumbers=ReadGridNumbers();
			Assert.AreEqual(dictNumbers["field_height"]+dictNumbers["row_gap"],dictNumbers["pitch"]);
			Assert.AreEqual(dictNumbers["check_height"]+dictNumbers["list_gap"],dictNumbers["list_pitch"]);
			Assert.AreEqual(27,UiGrid.PITCH,"The owner's row pitch of 2026-11-21.");
			Assert.AreEqual(23,UiGrid.FIELD_HEIGHT);
		}
	}
}
