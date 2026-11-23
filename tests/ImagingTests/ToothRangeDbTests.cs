// IMG-P2's review: the tooth picker and Item Info show teeth with Tooth.DisplayRange and read them back with Tooth.ParseRange. A range of three
// primary teeth ("A-C") was refused, because ParseRange read the end of a range with ToInt (primary C as permanent 6). Needs the preference
// cache (the tooth numbering), so it runs on the imaging test database. Licensed under the GNU General Public License, version 2 or later.
// See LICENSE in the repository root.
using Microsoft.VisualStudio.TestTools.UnitTesting;
using OpenPmsDentalBusiness;

namespace ImagingTests {
	[TestClass]
	public class ToothRangeDbTests {
		[TestInitialize]
		public void TestInitialize() {
			ImagingDb.Begin();
		}

		[TestMethod]
		public void ParseRange_ReadsWhatDisplayRangeWrites_PrimaryRangesToo() {
			foreach(string toothNumbers in new[] { "A,B,C","3,A,B,C","1,2,3","3,4,5,12,13,14","K,L,M,N","A,B,C,D,E,F,G,H,I,J","30,31,32" }) {
				string display=Tooth.DisplayRange(toothNumbers,ToothNumberingNomenclature.Universal);
				Assert.AreEqual(toothNumbers,Tooth.ParseRange(display),"Round trip of "+toothNumbers+" through '"+display+"'.");
			}
			Assert.AreEqual("A-C",Tooth.DisplayRange("A,B,C",ToothNumberingNomenclature.Universal));
			Assert.AreEqual("A,B,C",Tooth.ParseRange("A-C"));
			Assert.AreEqual("3,A,B,C",Tooth.ParseRange("A-C,3"));
		}

		[TestMethod]
		public void ParseRange_StillRefusesABackwardRange() {
			Assert.Throws<System.ApplicationException>(() => Tooth.ParseRange("5-3"));
			Assert.Throws<System.ApplicationException>(() => Tooth.ParseRange("C-A"));
		}
	}
}
