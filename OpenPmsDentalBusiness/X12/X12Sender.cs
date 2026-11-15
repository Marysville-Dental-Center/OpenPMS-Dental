// X12-SENDER: who the sender of an X12 batch is (ISA06, GS02, loop 1000A NM1 and PER of the 837 4010 and 5010, ISA06 and GS02 of the 270).
// Upstream named Open Dental Inc (its TIN, name and telephone) whenever the clearinghouse's Sender TIN was blank. Owner decision of 2026-11-15
// (docs/owner-questions.md): "send your practice's own TIN". The rule and its reasons: docs/port-notes/X12-SENDER.md.
// Licensed under the GNU General Public License, version 2 or later. See LICENSE in the repository root.
using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using CodeBase;

namespace OpenPmsDentalBusiness {
	///<summary>The sender of one X12 batch: its TIN (ISA06, GS02, 1000A NM109), name (1000A NM103 and PER02) and telephone (1000A PER04).
	///When the clearinghouse has a Sender TIN, the clearinghouse's Sender TIN, Name and Telephone, as before. When it is blank, the practice:
	///the TIN of the practice's billing provider, the practice title and the practice's telephone as the claims' billing provider loop (2010AA)
	///already sends them. Open Dental Inc is never the sender.</summary>
	public class X12Sender {
		///<summary>ISA06, GS02 and 1000A NM109.</summary>
		public string TIN="";
		///<summary>1000A NM103 and PER02.</summary>
		public string Name="";
		///<summary>1000A PER04. Ten digits when the practice is the sender.</summary>
		public string Phone="";
		///<summary>True when the clearinghouse's own Sender TIN, Name and Telephone are used; false when the practice is the sender.</summary>
		public bool IsFromClearinghouse;

		///<summary>The sender of a batch for clearinghouseClin (a clinic-level clearinghouse from Clearinghouses.OverrideFields) and the clinic of the
		///batch's claims (0 for none; ignored while clinics are off). Throws an ODException that says what to fill in and where when the
		///clearinghouse has no Sender TIN and the practice's TIN, name or telephone is missing. Reads the caches only.</summary>
		public static X12Sender GetSender(Clearinghouse clearinghouseClin,long clinicNum) {
			if(clearinghouseClin==null) {
				throw new ODException(Lans.g("X12Sender","Error. Could not locate Clearinghouse."));
			}
			if(!string.IsNullOrEmpty(clearinghouseClin.SenderTIN)) {
				return FromClearinghouse(clearinghouseClin);
			}
			List<string> listMissing;
			X12Sender sender=GetPracticeSender(X12SenderSource.FromCache(clinicNum),out listMissing);
			if(sender==null) {
				throw new ODException(GetRefusalMessage(clearinghouseClin,listMissing));
			}
			return sender;
		}

		///<summary>What is missing for the practice to be the sender of a batch for clearinghouseClin and clinicNum: empty when the clearinghouse has
		///its own Sender TIN or the practice's TIN, name and telephone are all there. Each entry says what to fill in and where. Used by the claim
		///and 270 validators, so the Missing Info of a claim names it before the batch is made.</summary>
		public static List<string> GetMissing(Clearinghouse clearinghouseClin,long clinicNum) {
			if(clearinghouseClin==null || !string.IsNullOrEmpty(clearinghouseClin.SenderTIN)) {
				return new List<string>();
			}
			List<string> listMissing;
			GetPracticeSender(X12SenderSource.FromCache(clinicNum),out listMissing);
			return listMissing;
		}

		///<summary>The clearinghouse's own Sender TIN, Name and Telephone, as upstream sends them (validated by X12Validate.ISA).</summary>
		public static X12Sender FromClearinghouse(Clearinghouse clearinghouseClin) {
			return new X12Sender {
				TIN=clearinghouseClin.SenderTIN,
				Name=clearinghouseClin.SenderName??"",
				Phone=clearinghouseClin.SenderTelephone??"",
				IsFromClearinghouse=true,
			};
		}

		///<summary>The practice as the sender, from the given values. Null when something is missing; listMissing then says what and where.
		///Rule (docs/port-notes/X12-SENDER.md section 2):
		///TIN: the SSN/TIN field of the practice's billing provider, the provider the claims of this clinic name in 2010AA and whose TIN they send in
		///2010AA REF*EI: the clinic's Default Insurance Billing Provider with clinics on, otherwise the practice's (Setup, Practice). "Default
		///Practice Provider" means the practice's Default Provider, as Providers.GetBillingProvNum reads it. "Treating Provider" names no single
		///provider, so the clinic's Default Provider is used, or the practice's Default Provider when the clinic has none. The provider must be
		///marked TIN (not SSN) and the number must be 9 digits with no dashes, as the claims' validator wants it.
		///Name: the practice title, which the claims send as the billing contact name (2010AA PER02) for every clinic.
		///Telephone: the one the claims send in 2010AA PER04: the practice's billing phone when Use on Claims of the billing address is checked,
		///otherwise the clinic's phone with clinics on, otherwise the practice's phone. It must be 10 digits.</summary>
		public static X12Sender GetPracticeSender(X12SenderSource source,out List<string> listMissing) {
			listMissing=new List<string>();
			Clinic clinic=(source.HasClinicsEnabled ? source.Clinic : null);
			string clinicName=(clinic==null ? "" : (string.IsNullOrWhiteSpace(clinic.Abbr) ? clinic.Description : clinic.Abbr));
			#region TIN
			long insBillingProv=(clinic==null ? source.InsBillingProv : clinic.InsBillingProv);
			long provNum;
			if(insBillingProv>0) {
				provNum=insBillingProv;
			}
			else if(insBillingProv==-1 && clinic!=null && clinic.DefaultProv>0) {//Treating Provider: the clinic's Default Provider.
				provNum=clinic.DefaultProv;
			}
			else {//Default Practice Provider (0), or Treating Provider without a clinic default.
				provNum=source.PracticeDefaultProv;
			}
			Provider provider=(provNum>0 && source.FuncGetProv!=null ? source.FuncGetProv(provNum) : null);
			string whereBillingProv=(clinic==null
				? Lans.g("X12Sender","Setup > Practice > Default Insurance Billing Provider and Default Provider")
				: Lans.g("X12Sender","Lists > Clinics > ")+clinicName+Lans.g("X12Sender"," > Default Insurance Billing Provider and Default Provider"));
			string tin="";
			if(provider==null || provider.ProvNum==0) {
				listMissing.Add(Lans.g("X12Sender","the practice's billing provider, whose TIN is the sender TIN")+" ("+whereBillingProv+")");
			}
			else if(!provider.UsingTIN || !Regex.IsMatch(provider.SSN??"","^[0-9]{9}$")) {
				listMissing.Add(Lans.g("X12Sender","the 9 digit TIN of the practice's billing provider")+" "+provider.Abbr+", "
					+Lans.g("X12Sender","marked TIN, not SSN")+" ("+Lans.g("X12Sender","Lists > Providers > ")+provider.Abbr
					+Lans.g("X12Sender"," > SSN or TIN")+")");
			}
			else {
				tin=provider.SSN;
			}
			#endregion TIN
			#region Name
			string name=source.PracticeTitle??"";
			if(X12Generator.Sout(name,60,-1).Trim()=="") {
				listMissing.Add(Lans.g("X12Sender","the practice title")+" ("+Lans.g("X12Sender","Setup > Practice > Provider Name or Practice Title")+")");
			}
			#endregion Name
			#region Telephone
			string phone;
			string wherePhone;
			if(source.UseBillingAddressOnClaims) {
				phone=source.PracticeBillingPhone;
				wherePhone=Lans.g("X12Sender","Setup > Practice > Billing Address > Phone");
			}
			else if(clinic!=null) {
				phone=clinic.Phone;
				wherePhone=Lans.g("X12Sender","Lists > Clinics > ")+clinicName+Lans.g("X12Sender"," > Phone");
			}
			else {
				phone=source.PracticePhone;
				wherePhone=Lans.g("X12Sender","Setup > Practice > Phone");
			}
			phone=phone??"";
			if(!Regex.IsMatch(phone,@"^\d{10}$")) {
				listMissing.Add(Lans.g("X12Sender","a 10 digit telephone number")+" ("+wherePhone+")");
			}
			#endregion Telephone
			if(listMissing.Count>0) {
				return null;
			}
			return new X12Sender { TIN=tin,Name=name,Phone=phone,IsFromClearinghouse=false };
		}

		///<summary>The message of a refused batch: the clearinghouse has no Sender TIN, so the practice is the sender, and listMissing is missing.</summary>
		public static string GetRefusalMessage(Clearinghouse clearinghouseClin,List<string> listMissing) {
			return Lans.g("X12Sender","The batch was not created and nothing was sent.")+"\r\n"
				+Lans.g("X12Sender","The clearinghouse")+" "+clearinghouseClin.Description+" "
				+Lans.g("X12Sender","has no Sender TIN, so the practice is the sender of the batch. Missing:")+"\r\n"
				+"- "+string.Join("\r\n- ",listMissing)+"\r\n"
				+Lans.g("X12Sender","Fill in what is missing, or enter the Sender TIN, name and telephone your clearinghouse gave you in Setup > "
				+"Family/Insurance > Clearinghouses.");
		}
	}

	///<summary>The values the practice sender is made from (X12Sender.GetPracticeSender). FromCache reads them from the preference, clinic and
	///provider caches; tests fill them in directly.</summary>
	public class X12SenderSource {
		public bool HasClinicsEnabled;
		///<summary>The clinic of the batch's claims, or null. Ignored while clinics are off.</summary>
		public Clinic Clinic;
		///<summary>PrefName.InsBillingProv: 0 = Default Practice Provider, -1 = Treating Provider, otherwise a ProvNum.</summary>
		public long InsBillingProv;
		///<summary>PrefName.PracticeDefaultProv.</summary>
		public long PracticeDefaultProv;
		///<summary>Returns the provider for a ProvNum, or null.</summary>
		public Func<long,Provider> FuncGetProv;
		public string PracticeTitle="";
		///<summary>PrefName.UseBillingAddressOnClaims.</summary>
		public bool UseBillingAddressOnClaims;
		public string PracticeBillingPhone="";
		public string PracticePhone="";

		///<summary>The values of the caches for the clinic of the batch's claims (0 for none).</summary>
		public static X12SenderSource FromCache(long clinicNum) {
			X12SenderSource source=new X12SenderSource();
			source.HasClinicsEnabled=PrefC.HasClinicsEnabled;
			if(source.HasClinicsEnabled && clinicNum>0) {
				source.Clinic=Clinics.GetClinic(clinicNum);
			}
			source.InsBillingProv=PrefC.GetLong(PrefName.InsBillingProv);
			source.PracticeDefaultProv=PrefC.GetLong(PrefName.PracticeDefaultProv);
			source.FuncGetProv=(provNum) => Providers.GetProv(provNum);
			source.PracticeTitle=PrefC.GetString(PrefName.PracticeTitle);
			source.UseBillingAddressOnClaims=PrefC.GetBool(PrefName.UseBillingAddressOnClaims);
			source.PracticeBillingPhone=PrefC.GetString(PrefName.PracticeBillingPhone);
			source.PracticePhone=PrefC.GetString(PrefName.PracticePhone);
			return source;
		}
	}
}
