using System;
using System.Collections.Generic;
using System.Linq;

namespace OpenPmsDentalBusiness {
	///<summary>The teeth of an image in the Imaging module (imaging design P2, owner 2026-11-22). Tooth numbers are in the database's format: American
	///numbers ("1"-"32", primary "A"-"T") separated by commas.</summary>
	public static class ImageTeeth {
		///<summary>The teeth of an image moved from a slot with toothNumbersFrom to a slot with toothNumbersTo: the new slot's teeth (owner, design
		///D6). When the new slot has none, the image keeps teeth the user gave it and drops the ones the old slot gave it.</summary>
		public static string GetTeethAfterMove(string toothNumbersImage,string toothNumbersFrom,string toothNumbersTo){
			toothNumbersImage=toothNumbersImage??"";
			toothNumbersFrom=toothNumbersFrom??"";
			toothNumbersTo=toothNumbersTo??"";
			if(toothNumbersTo!=""){
				return toothNumbersTo;
			}
			if(toothNumbersImage==toothNumbersFrom){
				return toothNumbersTo;
			}
			return toothNumbersImage;
		}

		///<summary>The teeth in the database's format: sorted with the permanent teeth before the primary ones (Tooth.ToOrdinal: 1-32, then 33-52),
		///separated by commas, each once.</summary>
		public static string JoinTeeth(IEnumerable<string> listTeeth){
			return string.Join(",",listTeeth.Distinct().OrderBy(x => Tooth.ToOrdinal(x)));
		}

		///<summary>The teeth of a database value; empty for blank. Spaces are ignored.</summary>
		public static HashSet<string> SplitTeeth(string toothNumbers){
			if(string.IsNullOrWhiteSpace(toothNumbers)){
				return new HashSet<string>();
			}
			return new HashSet<string>(toothNumbers.Replace(" ","").Split(new[] { ',' },StringSplitOptions.RemoveEmptyEntries));
		}
	}
}
