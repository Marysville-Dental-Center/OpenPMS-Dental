// The teeth of an image (imaging design P2, owner 2026-11-22): the rule for an image moved to another slot (design D6) and the tooth list's
// format. Licensed under the GNU General Public License, version 2 or later. See LICENSE in the repository root.
using System.Collections.Generic;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using OpenPmsDentalBusiness;

namespace ImagingTests {
	[TestClass]
	public class ImageTeethTests {
		[TestMethod]
		public void GetTeethAfterMove_TakesTheNewSlotsTeeth() {
			//slot 3-5 to slot 12-14: the image's teeth from the old slot become the new slot's
			Assert.AreEqual("12,13,14",ImageTeeth.GetTeethAfterMove("3,4,5","3,4,5","12,13,14"));
			//also when the user had changed them (owner, design D6: the slot it lands in)
			Assert.AreEqual("12,13,14",ImageTeeth.GetTeethAfterMove("4","3,4,5","12,13,14"));
			Assert.AreEqual("12,13,14",ImageTeeth.GetTeethAfterMove("","3,4,5","12,13,14"));
		}

		[TestMethod]
		public void GetTeethAfterMove_ANewSlotWithoutTeeth_DropsTheOldSlotsTeeth_KeepsTheUsers() {
			Assert.AreEqual("",ImageTeeth.GetTeethAfterMove("3,4,5","3,4,5",""),"The old slot gave them: dropped.");
			Assert.AreEqual("8",ImageTeeth.GetTeethAfterMove("8","3,4,5",""),"The user gave them: kept.");
			Assert.AreEqual("8",ImageTeeth.GetTeethAfterMove("8","",""),"A slot without teeth to another.");
			Assert.AreEqual("",ImageTeeth.GetTeethAfterMove(null,null,null));
		}

		[TestMethod]
		public void JoinTeeth_SortsPermanentBeforePrimary_EachOnce() {
			Assert.AreEqual("3,12,30,A,B,T",ImageTeeth.JoinTeeth(new List<string> { "B","30","T","3","A","12","3" }));
			Assert.AreEqual("",ImageTeeth.JoinTeeth(new List<string>()));
		}

		[TestMethod]
		public void SplitTeeth_ReadsTheDatabaseValue() {
			CollectionAssert.AreEquivalent(new[] { "3","4","A" },new List<string>(ImageTeeth.SplitTeeth("3, 4,,A")));
			Assert.AreEqual(0,ImageTeeth.SplitTeeth("").Count);
			Assert.AreEqual(0,ImageTeeth.SplitTeeth(null).Count);
			Assert.AreEqual("3,4,A",ImageTeeth.JoinTeeth(ImageTeeth.SplitTeeth("A,4,3")),"Split and join round trip in the database's order.");
		}
	}
}
