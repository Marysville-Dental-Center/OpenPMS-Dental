// A copy of InsVerifies.TableToListInsVerifyGridObjects as it was before INSV4 (docs/port-notes/INSVERIFY.md), unchanged since Open Dental
// 24.3: it looks up each row's plan and patient plan in the grid rows it has built so far with List.Find, so its time grows with the square of
// the rows. INSV4 made the business layer's method linear with dictionaries; GridRowsEquivalenceTests runs both on the same tables and requires
// the same grid rows, in the same order, with the same field values. Kept here, in the test project only, as the reference: do not change it.
// Licensed under the GNU General Public License, version 2 or later. See LICENSE in the repository root.
using System.Collections.Generic;
using System.Data;
using OpenPmsDentalBusiness;

namespace INSVERIFYTests {
	internal static class GridRowsBeforeInsv4 {
		///<summary>Converts a table of insverify objects into a list of InsVerifyGridObject. The method of the business layer before INSV4, word for
		///word but for its name and class.</summary>
		internal static List<InsVerifyGridObject> TableToListInsVerifyGridObjects(DataTable table,bool isForMedicaid) {
			Meth.NoCheckMiddleTierRole();
			List<InsVerify> listInsVerifies=OpenPmsDentalBusiness.Crud.InsVerifyCrud.TableToList(table);
			bool checkBenefitYear=PrefC.GetBool(PrefName.InsVerifyFutureDateBenefitYear);
			bool checkPatEnrollmentYear=PrefC.GetBool(PrefName.InsVerifyFutureDatePatEnrollmentYear);
			bool checkBenefitAndPatEnrollmentYearOn=(checkBenefitYear && checkPatEnrollmentYear);
			bool checkBenefitAndPatEnrollmentYearOff=(!checkBenefitYear && !checkPatEnrollmentYear);
			List<InsVerifyGridObject> listInsVerifyGridObjects=new List<InsVerifyGridObject>();
			for(int i=0;i<table.Rows.Count;i++) {
				DataRow row=table.Rows[i];
				InsVerify insVerify=listInsVerifies[i].Clone();
				insVerify.PatNum=PIn.Long(row["PatNum"].ToString());
				insVerify.PlanNum=PIn.Long(row["PlanNum"].ToString());
				insVerify.PatPlanNum=PIn.Long(row["PatPlanNum"].ToString());
				insVerify.ClinicName=PIn.String(row["ClinicName"].ToString());
				string patName=PIn.String(row["LName"].ToString())
					+", ";
				if(PIn.String(row["Preferred"].ToString())!="") {
					patName+="'"+PIn.String(row["Preferred"].ToString())+"' ";
				}
				patName+=PIn.String(row["FName"].ToString());
				insVerify.PatientName=patName;
				insVerify.CarrierName=PIn.String(row["CarrierName"].ToString());
				insVerify.AppointmentDateTime=PIn.DateT(row["AptDateTime"].ToString());
				insVerify.AptNum=PIn.Long(row["AptNum"].ToString());
				insVerify.ClinicNum=PIn.Long(row["ClinicNum"].ToString());//Non DB column. Used in OpenDentalService.
				insVerify.InsSubNum=PIn.Long(row["InsSubNum"].ToString());//Non DB column. Used in OpenDentalService.
				insVerify.CarrierNum=PIn.Long(row["CarrierNum"].ToString());//Non DB column. Used in OpenDentalService.
				if(insVerify.VerifyType==VerifyTypes.InsuranceBenefit) {
					InsVerifyGridObject insVerifyGridObjectPlanExists=listInsVerifyGridObjects.Find(x => x.InsVerifyPlan!=null && x.InsVerifyPlan.PlanNum==insVerify.PlanNum);
					if(insVerifyGridObjectPlanExists!=null) {
						continue;
					}
					InsVerifyGridObject insVerifyGridObjectExists=listInsVerifyGridObjects.Find(x => x.InsVerifyPat!=null
						&& x.InsVerifyPat.PatPlanNum==insVerify.PatPlanNum
						&& x.InsVerifyPat.PlanNum==insVerify.PlanNum
						&& x.InsVerifyPat.Note==insVerify.Note
						&& x.InsVerifyPat.DefNum==insVerify.DefNum
						&& x.InsVerifyPlan==null);
					if((checkBenefitAndPatEnrollmentYearOn || checkBenefitAndPatEnrollmentYearOff) && insVerifyGridObjectExists!=null) {//Both prefs on/off means combine pat/ins rows.
						insVerifyGridObjectExists.InsVerifyPlan=insVerify;
					}
					else {
						InsVerifyGridObject insVerifyGridObject=new InsVerifyGridObject();
						insVerifyGridObject.InsVerifyPlan=insVerify;
						insVerifyGridObject.IsForMedicaidPlan=isForMedicaid;
						listInsVerifyGridObjects.Add(insVerifyGridObject);
					}
					continue;
				}
				if(insVerify.VerifyType!=VerifyTypes.PatientEnrollment) {
					continue;
				}
				InsVerifyGridObject insVerifyGridObjectPatExists=listInsVerifyGridObjects.Find(x => x.InsVerifyPat!=null && x.InsVerifyPat.PatPlanNum==insVerify.PatPlanNum);
				if(insVerifyGridObjectPatExists!=null) {
					continue;
				}
				InsVerifyGridObject insVerifyGridObjectObjExists=listInsVerifyGridObjects.Find(x => x.InsVerifyPlan!=null
					&& x.InsVerifyPlan.PlanNum==insVerify.PlanNum
					&& x.InsVerifyPlan.Note==insVerify.Note
					&& x.InsVerifyPlan.DefNum==insVerify.DefNum
					&& x.InsVerifyPat==null);
				if((checkBenefitAndPatEnrollmentYearOn || checkBenefitAndPatEnrollmentYearOff) && insVerifyGridObjectObjExists!=null) {//Both prefs on/off means combine pat/ins rows.
					insVerifyGridObjectObjExists.InsVerifyPat=insVerify;
				}
				else {
					InsVerifyGridObject insVerifyGridObject=new InsVerifyGridObject();
					insVerifyGridObject.InsVerifyPat=insVerify;
					insVerifyGridObject.IsForMedicaidPlan=isForMedicaid;
					listInsVerifyGridObjects.Add(insVerifyGridObject);
				}
			}
			return listInsVerifyGridObjects;
		}
	}
}
