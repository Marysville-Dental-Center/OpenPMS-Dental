using System;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;

namespace OpenPmsDentalBusiness{
	public class X12Generator {
		///<summary>ISA06: the sender's TIN, padded to 15. X12-SENDER: the clearinghouse's Sender TIN, or the practice's TIN when it is blank
		///(X12Sender.GetSender). Upstream wrote Open Dental Inc's TIN here when the Sender TIN was blank; this fork never does.</summary>
		public static string GetISA06(X12Sender sender) {
			return Sout(sender.TIN,15,15);//Validated to be length at least 2 (clearinghouse) or 9 digits (practice).
		}

		///<summary>GS02: the sender's TIN. X12-SENDER: the clearinghouse's Sender TIN, or the practice's TIN when it is blank.</summary>
		public static string GetGS02(X12Sender sender) {
			return Sout(sender.TIN,15,2);//Validated to be length at least 2 (clearinghouse) or 9 digits (practice).
		}

		///<summary>Returns the Provider Taxonomy code for the given specialty. Always 10 characters, validated.</summary>
		public static string GetTaxonomy(Provider provider){
			if(provider.TaxonomyCodeOverride!=""){
				return provider.TaxonomyCodeOverride;
			}
			string spec="1223G0001X";//general
			Def provSpec=Defs.GetDef(DefCat.ProviderSpecialties,provider.Specialty);
			if(provSpec==null) {
				return spec;
			}
			switch(provSpec.ItemName) {
				case "General": spec="1223G0001X"; break;
				case "Hygienist": spec="124Q00000X"; break;
				case "PublicHealth": spec="1223D0001X"; break;
				case "Endodontics": spec="1223E0200X"; break;
				case "Pathology": spec="1223P0106X"; break;
				case "Radiology": spec="1223X0008X"; break;
				case "Surgery": spec="1223S0112X"; break;
				case "Ortho": spec="1223X0400X"; break;
				case "Pediatric": spec="1223P0221X"; break;
				case "Perio": spec="1223P0300X"; break;
				case "Prosth": spec="1223P0700X"; break;
				case "Denturist": spec="122400000X"; break;
				case "Assistant": spec="126800000X"; break;
				case "LabTech": spec="126900000X"; break;
			}
			return spec;
		}

		///<summary>Converts any string to an acceptable format for X12. Converts to all caps and strips off all invalid characters. Optionally shortens the string to the specified length and/or makes sure the string is long enough by padding with spaces.</summary>
		public static string Sout(string inputStr,int maxL,int minL) {
			string retStr=inputStr.ToUpper();
			//Debug.Write(retStr+",");
			retStr=Regex.Replace(retStr,//replaces characters in this input string
				//Allowed: !"&'()+,-./;?=(space)#   # is actually part of extended character set
				"[^\\w!\"&'\\(\\)\\+,-\\./;\\?= #]",//[](any single char)^(that is not)\w(A-Z or 0-9) or one of the above chars.
				"");
			retStr=Regex.Replace(retStr,"[_]","");//replaces _
			if(maxL!=-1) {
				if(retStr.Length>maxL) {
					retStr=retStr.Substring(0,maxL);
				}
			}
			if(minL!=-1) {
				if(retStr.Length<minL) {
					retStr=retStr.PadRight(minL,' ');
				}
			}
			//Debug.WriteLine(retStr);
			return retStr;
		}
	}
}
