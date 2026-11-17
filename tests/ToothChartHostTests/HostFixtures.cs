// Synthetic patients of the chart's host tests (package TC-8), in HostDb's scratch database. No real patient, practice or person: every name is
// "Synthetic", every procedure code a made-up Z code, every date fixed. One patient with work in each common paint type and status, tooth
// initials (missing teeth and a drawing) and a perio exam; one patient with nothing charted.
// Licensed under the GNU General Public License, version 2 or later. See LICENSE in the repository root.
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using DataConnectionBase;
using OpenPmsDentalBusiness;

namespace ToothChartHostTests {
	public static class HostFixtures {
		public static long ProvNum;
		public static long PatNumWork;
		public static long PatNumEmpty;
		public static long PerioExamNum;
		public static readonly DateTime DateWork=new DateTime(2024,5,1);
		public static readonly DateTime DateExam=new DateTime(2024,5,1);

		///<summary>Builds the fixtures once, on the connected scratch database (HostDb.EnsureOrSkip calls it).</summary>
		public static void Build() {
			//The chart's 18 colours (ChartGraphicColors): the new database's own definitions when it has all of them, otherwise synthetic ones, so the
			//hosts' colour indexes (0-17) exist.
			List<Def> listDefs=Defs.GetDefsForCategory(DefCat.ChartGraphicColors);
			if(listDefs.Count<18) {
				DataCore.NonQ("DELETE FROM definition WHERE Category="+POut.Int((int)DefCat.ChartGraphicColors));
				for(int i=0;i<18;i++) {
					OpenPmsDentalBusiness.Crud.DefCrud.Insert(new Def { Category=DefCat.ChartGraphicColors,ItemOrder=i,ItemName="Synthetic colour "+i,
						ItemValue="",ItemColor=Color.FromArgb(255,(i*13)%256,(40+i*7)%256,(200+i*11)%256),IsHidden=false });
				}
			}
			ProvNum=OpenPmsDentalBusiness.Crud.ProviderCrud.Insert(new Provider { Abbr="SYNH",LName="Synthetic",FName="Host" });
			long userNum=OpenPmsDentalBusiness.Crud.UserodCrud.Insert(new Userod { UserName="SyntheticHost",ProvNum=0 });
			Dictionary<string,long> dictCodeNums=new Dictionary<string,long>();
			object[][] arrayCodes=new object[][] {
				new object[] { "Z8001",ToothPaintingType.FillingDark,TreatmentArea.Surf },
				new object[] { "Z8002",ToothPaintingType.CrownDark,TreatmentArea.Tooth },
				new object[] { "Z8003",ToothPaintingType.RCT,TreatmentArea.Tooth },
				new object[] { "Z8004",ToothPaintingType.PostBU,TreatmentArea.Tooth },
				new object[] { "Z8005",ToothPaintingType.Extraction,TreatmentArea.Tooth },
				new object[] { "Z8006",ToothPaintingType.Implant,TreatmentArea.Tooth },
				new object[] { "Z8007",ToothPaintingType.Sealant,TreatmentArea.Tooth },
				new object[] { "Z8008",ToothPaintingType.FillingLight,TreatmentArea.Surf },
				new object[] { "Z8009",ToothPaintingType.Veneer,TreatmentArea.Tooth },
			};
			foreach(object[] arrayCode in arrayCodes) {
				ProcedureCode procedureCode=new ProcedureCode {
					ProcCode=(string)arrayCode[0],
					Descript="Synthetic "+arrayCode[1],
					AbbrDesc="Syn"+((string)arrayCode[0]).Substring(1),
					ProcTime="/X/",
					PaintType=(ToothPaintingType)arrayCode[1],
					TreatArea=(TreatmentArea)arrayCode[2],
					PaintText="",
				};
				dictCodeNums[procedureCode.ProcCode]=OpenPmsDentalBusiness.Crud.ProcedureCodeCrud.Insert(procedureCode);
			}
			Cache.Refresh(InvalidType.Defs,InvalidType.ProcCodes,InvalidType.Providers,InvalidType.Security);
			Security.CurUser=Userods.GetUser(userNum)??new Userod { UserNum=userNum,UserName="SyntheticHost" };
			PatNumWork=InsertPatient("Work");
			PatNumEmpty=InsertPatient("Empty");
			InsertProc(PatNumWork,dictCodeNums["Z8001"],"3","MOD",ProcStat.TP);
			InsertProc(PatNumWork,dictCodeNums["Z8001"],"30","O",ProcStat.C);
			InsertProc(PatNumWork,dictCodeNums["Z8002"],"14","",ProcStat.C);
			InsertProc(PatNumWork,dictCodeNums["Z8002"],"19","",ProcStat.TP);
			InsertProc(PatNumWork,dictCodeNums["Z8003"],"19","",ProcStat.TP);
			InsertProc(PatNumWork,dictCodeNums["Z8004"],"19","",ProcStat.TP);
			InsertProc(PatNumWork,dictCodeNums["Z8005"],"17","",ProcStat.TP);
			InsertProc(PatNumWork,dictCodeNums["Z8005"],"32","",ProcStat.C);
			InsertProc(PatNumWork,dictCodeNums["Z8006"],"29","",ProcStat.EC);
			InsertProc(PatNumWork,dictCodeNums["Z8007"],"2","",ProcStat.C);
			InsertProc(PatNumWork,dictCodeNums["Z8008"],"8","F",ProcStat.EO);
			InsertProc(PatNumWork,dictCodeNums["Z8009"],"9","",ProcStat.R);
			InsertProc(PatNumWork,dictCodeNums["Z8001"],"12","DO",ProcStat.Cn);
			OpenPmsDentalBusiness.Crud.ToothInitialCrud.Insert(new ToothInitial { PatNum=PatNumWork,ToothNum="1",InitialType=ToothInitialType.Missing });
			OpenPmsDentalBusiness.Crud.ToothInitialCrud.Insert(new ToothInitial { PatNum=PatNumWork,ToothNum="16",InitialType=ToothInitialType.Missing });
			OpenPmsDentalBusiness.Crud.ToothInitialCrud.Insert(new ToothInitial { PatNum=PatNumWork,InitialType=ToothInitialType.Drawing,
				DrawingSegment="40,250;60,262;80,270;100,272",ColorDraw=Color.FromArgb(255,0,90,200) });
			PerioExamNum=OpenPmsDentalBusiness.Crud.PerioExamCrud.Insert(new PerioExam { PatNum=PatNumWork,ExamDate=DateExam,ProvNum=ProvNum });
			for(int tooth=2;tooth<=31;tooth++) {
				if(tooth==16 || tooth==17) {
					continue;
				}
				int depth=2+(tooth%4);
				InsertPerio(tooth,PerioSequenceType.Probing,-1,depth,depth+1,depth,depth+1,depth,depth+2);
				InsertPerio(tooth,PerioSequenceType.GingMargin,-1,1,0,1,(tooth%3)-1,0,1);
				InsertPerio(tooth,PerioSequenceType.BleedSupPlaqCalc,-1,tooth%3==0 ? 1 : 0,0,tooth%5==0 ? 2 : 0,0,tooth%7==0 ? 1 : 0,0);
				if(tooth%6==0) {
					InsertPerio(tooth,PerioSequenceType.Mobility,1,-1,-1,-1,-1,-1,-1);
				}
				if(tooth==3 || tooth==14 || tooth==19 || tooth==30) {
					InsertPerio(tooth,PerioSequenceType.Furcation,-1,-1,2,-1,-1,1,-1);
				}
				InsertPerio(tooth,PerioSequenceType.MGJ,-1,-1,4,-1,-1,-1,-1);
			}
		}

		private static long InsertPatient(string firstName) {
			return OpenPmsDentalBusiness.Crud.PatientCrud.Insert(new Patient { LName="Synthetic",FName=firstName,PriProv=ProvNum,
				PatStatus=PatientStatus.Patient,Birthdate=new DateTime(1980,1,1) });
		}

		private static void InsertProc(long patNum,long codeNum,string toothNum,string surf,ProcStat procStat) {
			long procNum=OpenPmsDentalBusiness.Crud.ProcedureCrud.Insert(new Procedure { PatNum=patNum,CodeNum=codeNum,ToothNum=toothNum,Surf=surf,
				ProcStatus=procStat,ProvNum=ProvNum,ProcDate=DateWork,DateTP=DateWork });
			DataCore.NonQ("UPDATE procedurelog SET DateEntryC="+POut.Date(DateWork)+" WHERE ProcNum="+POut.Long(procNum));
		}

		private static void InsertPerio(int intTooth,PerioSequenceType perioSequenceType,int toothValue,int mb,int b,int db,int ml,int l,int dl) {
			OpenPmsDentalBusiness.Crud.PerioMeasureCrud.Insert(new PerioMeasure { PerioExamNum=PerioExamNum,SequenceType=perioSequenceType,IntTooth=intTooth,
				ToothValue=toothValue,MBvalue=mb,Bvalue=b,DBvalue=db,MLvalue=ml,Lvalue=l,DLvalue=dl });
		}

		///<summary>The patient with work, read from the database.</summary>
		public static Patient GetPatientWork() {
			return Patients.GetPat(PatNumWork);
		}

		///<summary>The plan of the patient's planned procedures (every TP, by ProcNum), as DashToothChart and the Treatment Plan sheet hand it on.</summary>
		public static TreatPlan GetTreatPlanOfTPs(long patNum) {
			TreatPlan treatPlan=new TreatPlan { PatNum=patNum,TPStatus=TreatPlanStatus.Active,Heading="Synthetic plan" };
			treatPlan.ListProcTPs=Procedures.Refresh(patNum).Where(x => x.ProcStatus==ProcStat.TP).OrderBy(x => x.ProcNum)
				.Select(x => new ProcTP { ProcNumOrig=x.ProcNum,ToothNumTP=Tooth.Display(x.ToothNum),ProcCode=ProcedureCodes.GetStringProcCode(x.CodeNum),
					Surf=Tooth.SurfTidyFromDbToDisplay(x.Surf,x.ToothNum) }).ToList();
			return treatPlan;
		}
	}
}
