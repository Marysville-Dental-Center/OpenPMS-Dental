// Imaging adjust tools: the display filters saved for each image (documentadjust).
// Licensed under the GNU General Public License, version 2 or later. See LICENSE in the repository root.
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace OpenPmsDentalBusiness {
	///<summary>The display filters saved for each image (documentadjust, one row per DocNum). A row exists only while the image has a setting that is
	///not neutral: Upsert deletes the row when everything is neutral and no trim is set. Documents.Delete deletes the image's row. Not cached.</summary>
	public class DocumentAdjusts {
		#region Get Methods
		///<summary>The row of the image, or null when it has none (every filter neutral).</summary>
		public static DocumentAdjust GetForDoc(long docNum) {
			if(RemotingClient.MiddleTierRole==MiddleTierRole.ClientMT) {
				return Meth.GetObject<DocumentAdjust>(MethodBase.GetCurrentMethod(),docNum);
			}
			return Crud.DocumentAdjustCrud.SelectOne("SELECT * FROM documentadjust WHERE DocNum="+POut.Long(docNum));
		}

		///<summary>The rows of these images, in one query. An image without a row has none in the list.</summary>
		public static List<DocumentAdjust> GetForDocs(List<long> listDocNums) {
			if(RemotingClient.MiddleTierRole==MiddleTierRole.ClientMT) {
				return Meth.GetObject<List<DocumentAdjust>>(MethodBase.GetCurrentMethod(),listDocNums);
			}
			if(listDocNums==null || listDocNums.Count==0) {
				return new List<DocumentAdjust>();
			}
			return Crud.DocumentAdjustCrud.SelectMany("SELECT * FROM documentadjust WHERE DocNum IN("+string.Join(",",listDocNums.Distinct().Select(x => POut.Long(x)))+")");
		}
		#endregion Get Methods

		#region Modification Methods
		///<summary>Saves the image's settings: inserts the row, or updates the image's existing row (the DocumentAdjustNum of the argument is ignored).
		///When the settings are neutral and both trims are 0, there is nothing to keep, so the row is deleted instead.</summary>
		public static void Upsert(DocumentAdjust documentAdjust) {
			if(RemotingClient.MiddleTierRole==MiddleTierRole.ClientMT) {
				Meth.GetVoid(MethodBase.GetCurrentMethod(),documentAdjust);
				return;
			}
			if(documentAdjust.ToSettings().IsNeutral() && documentAdjust.TrimStartMs==0 && documentAdjust.TrimEndMs==0) {
				DeleteForDoc(documentAdjust.DocNum);
				return;
			}
			DocumentAdjust documentAdjustOld=GetForDoc(documentAdjust.DocNum);
			if(documentAdjustOld==null) {
				documentAdjust.DocumentAdjustNum=0;
				Crud.DocumentAdjustCrud.Insert(documentAdjust);
				return;
			}
			documentAdjust.DocumentAdjustNum=documentAdjustOld.DocumentAdjustNum;
			Crud.DocumentAdjustCrud.Update(documentAdjust,documentAdjustOld);
		}

		///<summary>Deletes the image's row, if it has one.</summary>
		public static void DeleteForDoc(long docNum) {
			if(RemotingClient.MiddleTierRole==MiddleTierRole.ClientMT) {
				Meth.GetVoid(MethodBase.GetCurrentMethod(),docNum);
				return;
			}
			DataCore.NonQ("DELETE FROM documentadjust WHERE DocNum="+POut.Long(docNum));
		}
		#endregion Modification Methods
	}
}
