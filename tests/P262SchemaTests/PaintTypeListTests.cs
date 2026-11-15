// P262-SCHEMA owner question 2 (answered on 2026-11-07) hid the paint type TextBelow (18) from the procedure code paint type lists until the tooth
// chart drew it. The WPF and 2D charts draw it since TC-5 (docs/specs/wpf-tooth-chart-plan.md, O-21), so every list offers it, last, and the new
// code window's cast of the list index to the paint type stays valid. The rule without a database, and the two windows read from the sources.
// Licensed under the GNU General Public License, version 2 or later. See LICENSE in the repository root.
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using OpenPmsDentalBusiness;

namespace P262SchemaTests {
	[TestClass]
	public class PaintTypeListTests {
		[TestMethod]
		public void PaintTypes_EveryOneInNumberOrder_TextBelowLast() {
			List<ToothPaintingType> listPaintTypes=ProcedureCodes.GetPaintTypesForList();
			List<ToothPaintingType> listAll=Enum.GetValues(typeof(ToothPaintingType)).Cast<ToothPaintingType>().OrderBy(x => (int)x).ToList();
			CollectionAssert.AreEqual(listAll,listPaintTypes);
			CollectionAssert.Contains(listPaintTypes,ToothPaintingType.TextBelow,"The tooth chart draws TextBelow since TC-5.");
			Assert.AreEqual(ToothPaintingType.TextBelow,listPaintTypes.Last(),"TextBelow is the last value.");
			for(int i=0;i<listPaintTypes.Count;i++) {
				Assert.AreEqual(i,(int)listPaintTypes[i],"The new code window casts the list index to the paint type: no gap in the values.");
			}
			Assert.AreEqual(19,listPaintTypes.Count,"None to TextBelow: 0 to 18.");
		}

		[TestMethod]
		public void PaintTypes_TheSameListWhateverTheCodeHas() {
			foreach(ToothPaintingType toothPaintingType in Enum.GetValues(typeof(ToothPaintingType))) {
				CollectionAssert.AreEqual(ProcedureCodes.GetPaintTypesForList(),ProcedureCodes.GetPaintTypesForList(toothPaintingType),toothPaintingType.ToString());
			}
		}

		[TestMethod]
		public void ProcCodeWindows_UseTheList() {
			string edit=Read("OpenPmsDental","Forms","FormProcCodeEdit.cs");
			StringAssert.Contains(edit,"listPaintType.Items.AddList(ProcedureCodes.GetPaintTypesForList(_procedureCode.PaintType),");
			Assert.IsFalse(edit.Contains("AddEnums<ToothPaintingType>()"));
			string codeNew=Read("OpenPmsDental","Forms","FormProcCodeNew.cs");
			StringAssert.Contains(codeNew,"List<ToothPaintingType> listPaintTypes=ProcedureCodes.GetPaintTypesForList();");
			Assert.IsFalse(codeNew.Contains("Enum.GetNames(typeof(ToothPaintingType))"));
			StringAssert.Contains(codeNew,"procedureCode.PaintType=(ToothPaintingType)comboPaintType.SelectedIndex;","The cast the list order keeps valid.");
			//The paint text box shows for Text and TextBelow, the two paint types that draw it.
			StringAssert.Contains(edit,"if(listPaintType.GetSelected<ToothPaintingType>().In(ToothPaintingType.Text,ToothPaintingType.TextBelow)){");
		}

		private static string Read(params string[] arrayParts) {
			string path=Path.GetDirectoryName(typeof(PaintTypeListTests).Assembly.Location);
			while(path!=null && !(File.Exists(Path.Combine(path,"LICENSE")) && File.Exists(Path.Combine(path,"NOTICE")))) {
				path=Path.GetDirectoryName(path);
			}
			Assert.IsNotNull(path,"Repository root not found.");
			return File.ReadAllText(Path.Combine(new[] { path }.Concat(arrayParts).ToArray()));
		}
	}
}
