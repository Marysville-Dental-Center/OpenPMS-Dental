// The synthetic rows of the golden fixture database (docs/port-notes/S3a-4.md). Every name, address, number and code is made up: example names, the
// 97000 ZIP range, 555 phone numbers, Z procedure codes, a ZZ payer ID. Dates are fixed in 2025, so no output depends on the day the tests run, and
// every primary key is fixed: each table starts at its own AUTO_INCREMENT base (above every row the init file brings), and the rows go in in
// the same order every time.
// Licensed under the GNU General Public License, version 2 or later. See LICENSE in the repository root.
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using MySqlConnector;
using OpenPmsDentalBusiness;

namespace GoldenOutputTests {
	public static class GoldenData {
		///<summary>The day the fixture's statements are "sent" and its aging is computed; every row is dated before it.</summary>
		public static readonly DateTime DateStatement=new DateTime(2025,6,30);
		public const string PracticeTitle="Golden Synthetic Dental";
		public const string WikiListName="goldensynthetic";

		public static long ProvNum;
		public static long ClearinghouseNum5010;
		public static long ClearinghouseNum4010;
		///<summary>The first family: a patient with insurance, a claim, a treatment plan and a payment plan.</summary>
		public static Patient Patient1;
		///<summary>The second family: a procedure of 0.10, an adjustment of 0.70 and a payment of 0.80, so a limited statement totals about -1.1e-16.</summary>
		public static Patient Patient2;
		public static long ClaimNum;
		public static long TreatPlanNum;
		public static long PayPlanNum;
		public static long StatementNumFamily;
		public static long StatementNumLimited;
		public static List<long> ListProcNums1=new List<long>();
		public static long ProcNumZero;
		public static long AdjNumZero;
		public static long SplitNumZero;

		///<summary>The AUTO_INCREMENT base of each table the fixture writes to. The init file brings at most a few rows to any of them.</summary>
		private static readonly Dictionary<string,long> _dictAutoIncrementBases=new Dictionary<string,long> {
			{ "provider",9001 },{ "patient",9101 },{ "carrier",9201 },{ "insplan",9301 },{ "inssub",9401 },{ "patplan",9501 },
			{ "procedurecode",9601 },{ "procedurelog",9701 },{ "claim",9801 },{ "claimproc",9901 },{ "clearinghouse",9951 },
			{ "definition",90001 },{ "adjustment",10001 },{ "payment",10101 },{ "paysplit",10201 },{ "statement",10301 },{ "stmtlink",10351 },
			{ "treatplan",10401 },{ "proctp",10501 },{ "payplan",10601 },{ "payplancharge",10701 },{ "account",10801 },
			{ "journalentry",10901 },{ "transaction",10951 },{ "document",11001 },{ "wikilisthist",11101 },
		};

		public static void Seed() {
			using(MySqlConnection connection=GoldenDb.OpenServer(GoldenDb.DbName)) {
				foreach(KeyValuePair<string,long> pair in _dictAutoIncrementBases) {
					GoldenDb.Exec(connection,"ALTER TABLE `"+pair.Key+"` AUTO_INCREMENT="+pair.Value);
				}
			}
			SeedPrefs();
			ProvNum=Providers.Insert(new Provider {
				Abbr="GOLD1",FName="Grace",LName="Goldsmith",MI="R",Suffix="DDS",SSN="123456789",UsingTIN=true,StateLicense="SYN12345",
				NationalProvID="1234567893",TaxonomyCodeOverride="1223G0001X",StateWhereLicensed="OR",ItemOrder=1,ProvColor=Color.White,
				OutlineColor=Color.Black,ProvStatus=ProviderStatus.Active,
			});
			Cache.Refresh(InvalidType.Providers);
			Prefs.UpdateLong(PrefName.PracticeDefaultProv,ProvNum);
			long defNumCategory=Defs.GetDefsForCategory(DefCat.ProcCodeCats,true).First().DefNum;
			long codeNumExam=InsertCode("Z9101","Synthetic periodic exam","SynExam",TreatmentArea.Mouth,defNumCategory);
			long codeNumFill=InsertCode("Z9102","Synthetic filling, three surfaces","SynFill",TreatmentArea.Surf,defNumCategory);
			long codeNumCrown=InsertCode("Z9103","Synthetic crown","SynCrown",TreatmentArea.Tooth,defNumCategory);
			long codeNumSmall=InsertCode("Z9104","Synthetic small charge","SynSmall",TreatmentArea.Mouth,defNumCategory);
			long defNumAdjType=Defs.Insert(new Def { Category=DefCat.AdjTypes,ItemName="Synthetic adjustment",ItemValue="+",ItemOrder=90 });
			long defNumPayType=Defs.Insert(new Def { Category=DefCat.PaymentTypes,ItemName="Synthetic check",ItemValue="",ItemOrder=90 });
			Cache.Refresh(InvalidType.Defs,InvalidType.ProcCodes);
			Patient1=InsertPatient("Goldtest","Ava","Q",new DateTime(1980,2,3),PatientGender.Female,"1 Example Street","Apt 4","97001");
			Patient2=InsertPatient("Goldzero","Ben","",new DateTime(1975,11,12),PatientGender.Male,"2 Example Street","","97002");
			SeedInsuranceAndClaim(codeNumExam,codeNumFill,codeNumCrown);
			SeedNearZeroFamily(codeNumSmall,defNumAdjType,defNumPayType);
			SeedTreatPlan();
			SeedPayPlan();
			SeedAccounts();
			Ledgers.ComputeAging(Patient1.PatNum,DateStatement);
			Ledgers.ComputeAging(Patient2.PatNum,DateStatement);
			Patient1=Patients.GetPat(Patient1.PatNum);
			Patient2=Patients.GetPat(Patient2.PatNum);
			SeedStatements();
			SeedWikiList();
		}

		///<summary>Every preference the 837 writers, the statements and the reports read for the practice, set here so no default of the init file
		///shows in an output.</summary>
		private static void SeedPrefs() {
			Prefs.UpdateString(PrefName.PracticeTitle,PracticeTitle);
			Prefs.UpdateString(PrefName.PracticeAddress,"100 Example Way");
			Prefs.UpdateString(PrefName.PracticeAddress2,"Suite 2");
			Prefs.UpdateString(PrefName.PracticeCity,"Exampleton");
			Prefs.UpdateString(PrefName.PracticeST,"OR");
			Prefs.UpdateString(PrefName.PracticeZip,"97000");
			Prefs.UpdateString(PrefName.PracticePhone,"5035550100");
			Prefs.UpdateString(PrefName.PracticeBillingAddress,"PO Box 100");
			Prefs.UpdateString(PrefName.PracticeBillingAddress2,"");
			Prefs.UpdateString(PrefName.PracticeBillingCity,"Exampleton");
			Prefs.UpdateString(PrefName.PracticeBillingST,"OR");
			Prefs.UpdateString(PrefName.PracticeBillingZip,"97000");
			Prefs.UpdateString(PrefName.PracticeBillingPhone,"5035550101");
			Prefs.UpdateString(PrefName.PracticePayToAddress,"");
			Prefs.UpdateString(PrefName.PracticePayToAddress2,"");
			Prefs.UpdateString(PrefName.PracticePayToCity,"");
			Prefs.UpdateString(PrefName.PracticePayToST,"");
			Prefs.UpdateString(PrefName.PracticePayToZip,"");
			Prefs.UpdateBool(PrefName.UseBillingAddressOnClaims,false);
			Prefs.UpdateBool(PrefName.EclaimsSeparateTreatProv,false);
			Prefs.UpdateBool(PrefName.EclaimsSubscIDUsesPatID,false);
			Prefs.UpdateBool(PrefName.ClaimMedProvTreatmentAsOrdering,false);
			Prefs.UpdateBool(PrefName.BalancesDontSubtractIns,false);
			Prefs.UpdateBool(PrefName.EasyNoClinics,true);
			Cache.Refresh(InvalidType.Prefs);
		}

		private static long InsertCode(string procCode,string descript,string abbr,TreatmentArea treatmentArea,long defNumCategory) {
			return ProcedureCodes.Insert(new ProcedureCode { ProcCode=procCode,Descript=descript,AbbrDesc=abbr,ProcTime="/X/",TreatArea=treatmentArea,
				ProcCat=defNumCategory,GraphicColor=Color.Empty,LaymanTerm="",DefaultNote="",SubstitutionCode="",MedicalCode="",DrugNDC="",
				RevenueCodeDefault="",AlternateCode1="",DefaultClaimNote="",DefaultTPNote="",TaxCode="",PaintText="",DiagnosticCodes="" });
		}

		private static Patient InsertPatient(string lName,string fName,string middleI,DateTime birthdate,PatientGender gender,string address,
			string address2,string zip)
		{
			Patient patient=new Patient();
			patient.LName=lName;
			patient.FName=fName;
			patient.MiddleI=middleI;
			patient.Birthdate=birthdate;
			patient.Gender=gender;
			patient.PatStatus=PatientStatus.Patient;
			patient.Address=address;
			patient.Address2=address2;
			patient.City="Exampleton";
			patient.State="OR";
			patient.Zip=zip;
			patient.HmPhone="(503)555-0110";
			patient.PriProv=ProvNum;
			patient.BillingType=PrefC.GetLong(PrefName.PracticeDefaultBillType);
			patient.Position=PatientPosition.Single;
			Patients.Insert(patient,false);
			Patient patientOld=patient.Copy();
			patient.Guarantor=patient.PatNum;
			Patients.Update(patient,patientOld);
			return patient;
		}

		private static long InsertProcedure(Patient patient,long codeNum,DateTime date,double fee,string toothNum,string surf,ProcStat procStat) {
			Procedure procedure=new Procedure();
			procedure.PatNum=patient.PatNum;
			procedure.CodeNum=codeNum;
			procedure.ProcDate=date;
			procedure.DateEntryC=date;
			procedure.DateTP=date;
			procedure.ProcFee=fee;
			procedure.UnitQty=1;
			procedure.ProcStatus=procStat;
			procedure.ProvNum=ProvNum;
			procedure.ToothNum=toothNum;
			procedure.Surf=surf;
			procedure.ToothRange="";
			procedure.PlaceService=PlaceOfService.Office;
			procedure.CodeMod1="";
			procedure.CodeMod2="";
			procedure.CodeMod3="";
			procedure.CodeMod4="";
			procedure.RevCode="";
			procedure.Prosthesis="";
			procedure.DiagnosticCode="";
			procedure.ClaimNote="";
			return Procedures.Insert(procedure,doCalcTax:false,skipDiscountPlanAdjustment:true);
		}

		private static void SeedInsuranceAndClaim(long codeNumExam,long codeNumFill,long codeNumCrown) {
			Carrier carrier=new Carrier { CarrierName="Synthetic Mutual Dental",Address="200 Payer Road",Address2="",City="Claimsburg",State="OR",
				Zip="97003",Phone="(503)555-0120",ElectID="ZZ123",TIN="",ApptTextBackColor=Color.Black };
			Carriers.Insert(carrier);
			InsPlan insPlan=new InsPlan { CarrierNum=carrier.CarrierNum,GroupName="Synthetic Employers Group",GroupNum="GRP100",PlanType="",
				FeeSched=0,CobRule=EnumCobRule.Basic,PlanNote="",TrojanID="",DivisionNo="",CanadianPlanFlag="",CanadianDiagnosticCode="",
				CanadianInstitutionCode="",RxBIN="",SopCode="" };
			InsPlans.Insert(insPlan);
			InsSub insSub=new InsSub { PlanNum=insPlan.PlanNum,Subscriber=Patient1.PatNum,SubscriberID="SUB0001",ReleaseInfo=true,AssignBen=true,
				DateEffective=new DateTime(2024,1,1),BenefitNotes="",SubscNote="" };
			InsSubs.Insert(insSub);
			PatPlans.Insert(new PatPlan { PatNum=Patient1.PatNum,InsSubNum=insSub.InsSubNum,Ordinal=1,Relationship=Relat.Self,PatID="" });
			DateTime dateService=new DateTime(2025,3,10);
			ListProcNums1.Add(InsertProcedure(Patient1,codeNumExam,dateService,57.25,"","",ProcStat.C));
			ListProcNums1.Add(InsertProcedure(Patient1,codeNumFill,dateService,0.1+0.2,"30","MOD",ProcStat.C));
			ListProcNums1.Add(InsertProcedure(Patient1,codeNumCrown,dateService,1234.56,"3","",ProcStat.C));
			List<Procedure> listProcedures=Procedures.GetManyProc(ListProcNums1,false);
			Claim claim=new Claim();
			claim.PatNum=Patient1.PatNum;
			claim.DateService=dateService;
			claim.DateSent=new DateTime(2025,3,12);
			claim.DateSentOrig=new DateTime(2025,3,12);
			claim.ClaimStatus="W";
			claim.PlanNum=insPlan.PlanNum;
			claim.InsSubNum=insSub.InsSubNum;
			claim.ProvTreat=ProvNum;
			claim.ProvBill=ProvNum;
			claim.ClaimType="P";
			claim.PatRelat=Relat.Self;
			claim.PlaceService=PlaceOfService.Office;
			claim.EmployRelated=YN.No;
			claim.AccidentRelated="";
			claim.MedType=EnumClaimMedType.Dental;
			claim.ClaimIdentifier="GOLD0001";
			claim.ClaimNote="Synthetic claim note";
			claim.ClaimFee=listProcedures.Sum(x => x.ProcFee);
			claim.InsPayEst=claim.ClaimFee/2;
			claim.ClaimForm=0;
			claim.PreAuthString="";
			claim.IsProsthesis="N";
			claim.ReasonUnderPaid="";
			claim.RefNumString="";
			claim.AccidentST="";
			claim.UniformBillType="";
			claim.AdmissionTypeCode="";
			claim.AdmissionSourceCode="";
			claim.PatientStatusCode="";
			claim.PriorAuthorizationNumber="";
			claim.OrigRefNum="";
			claim.Narrative="";
			Claims.Insert(claim);
			ClaimNum=claim.ClaimNum;
			for(int i=0;i<listProcedures.Count;i++) {
				ClaimProc claimProc=new ClaimProc();
				claimProc.ProcNum=listProcedures[i].ProcNum;
				claimProc.ClaimNum=claim.ClaimNum;
				claimProc.PatNum=Patient1.PatNum;
				claimProc.ProvNum=ProvNum;
				claimProc.PlanNum=insPlan.PlanNum;
				claimProc.InsSubNum=insSub.InsSubNum;
				claimProc.Status=ClaimProcStatus.NotReceived;
				claimProc.FeeBilled=listProcedures[i].ProcFee;
				claimProc.InsPayEst=listProcedures[i].ProcFee/2;
				claimProc.ProcDate=dateService;
				claimProc.DateCP=dateService;
				claimProc.CodeSent=ProcedureCodes.GetStringProcCode(listProcedures[i].CodeNum);
				claimProc.LineNumber=(byte)(i+1);
				claimProc.Percentage=50;
				claimProc.Remarks="";
				claimProc.EstimateNote="";
				claimProc.ClaimAdjReasonCodes="";
				ClaimProcs.Insert(claimProc);
			}
			Clearinghouse clearinghouse5010=NewClearinghouse("Synthetic clearinghouse 5010",ElectronicClaimFormat.x837D_5010_dental);
			ClearinghouseNum5010=Clearinghouses.Insert(clearinghouse5010);
			Clearinghouse clearinghouse4010=NewClearinghouse("Synthetic clearinghouse 4010",ElectronicClaimFormat.x837D_4010);
			ClearinghouseNum4010=Clearinghouses.Insert(clearinghouse4010);
			Clearinghouses.RefreshCache();
		}

		private static Clearinghouse NewClearinghouse(string description,ElectronicClaimFormat electronicClaimFormat) {
			return new Clearinghouse { Description=description,Eformat=electronicClaimFormat,CommBridge=EclaimsCommBridge.None,ExportPath="",
				Payors="",ISA02="",ISA04="",ISA05="ZZ",SenderTIN="",ISA07="ZZ",ISA08="SYNRECEIVER",ISA15="T",GS03="SYNRECEIVER",SenderName="",
				SenderTelephone="",ISA16="",SeparatorData="",SeparatorSegment="",Password="",ResponsePath="",ClientProgram="",LoginID="",LocationID="",
				IsClaimExportAllowed=false,IsEraDownloadAllowed=EraBehaviors.None };
		}

		///<summary>Patient2's family: a procedure of 0.10, an adjustment of 0.70 and a payment of 0.80 on three days. The limited statement of the
		///three adds charges and credits as doubles in date order, (0.10+0.70)-0.80, which is about -1.1e-16 (docs/port-notes/S3a-3.md, section 4).
		///</summary>
		private static void SeedNearZeroFamily(long codeNumSmall,long defNumAdjType,long defNumPayType) {
			ProcNumZero=InsertProcedure(Patient2,codeNumSmall,new DateTime(2025,4,1),0.10,"","",ProcStat.C);
			Adjustment adjustment=new Adjustment { AdjDate=new DateTime(2025,4,2),ProcDate=new DateTime(2025,4,2),DateEntry=new DateTime(2025,4,2),
				AdjAmt=0.70,PatNum=Patient2.PatNum,AdjType=defNumAdjType,ProvNum=ProvNum,AdjNote="Synthetic adjustment note" };
			AdjNumZero=Adjustments.Insert(adjustment);
			Payment payment=new Payment { PayType=defNumPayType,PayDate=new DateTime(2025,4,3),DateEntry=new DateTime(2025,4,3),PayAmt=0.80,
				CheckNum="1001",BankBranch="",PayNote="Synthetic payment",IsSplit=false,PatNum=Patient2.PatNum,Receipt="",ExternalId="" };
			Payments.Insert(payment);
			PaySplit paySplit=new PaySplit { SplitAmt=0.80,PatNum=Patient2.PatNum,PayNum=payment.PayNum,ProvNum=ProvNum,DatePay=payment.PayDate,
				ProcDate=payment.PayDate,DateEntry=payment.PayDate };
			SplitNumZero=PaySplits.Insert(paySplit);
		}

		private static void SeedTreatPlan() {
			TreatPlan treatPlan=new TreatPlan { PatNum=Patient1.PatNum,DateTP=new DateTime(2025,4,15),Heading="Synthetic treatment plan",
				Note="Synthetic plan note",Signature="",SignaturePractice="",SignatureText="Ava Q Goldtest",SignaturePracticeText="Grace Goldsmith",
				TPStatus=TreatPlanStatus.Saved,TPType=TreatPlanType.Insurance };
			TreatPlanNum=TreatPlans.Insert(treatPlan);
			string[] arrayDescripts=new[] { "Synthetic crown","Synthetic filling, three surfaces","Synthetic small charge" };
			double[] arrayFees=new[] { 1234.56,0.1+0.2,0.80 };
			double[] arrayPatAmts=new[] { 617.28,0.15,0.0 };
			for(int i=0;i<arrayDescripts.Length;i++) {
				ProcTPs.Insert(new ProcTP { TreatPlanNum=TreatPlanNum,PatNum=Patient1.PatNum,ItemOrder=i,ToothNumTP=i==0 ? "3" : "",Surf=i==1 ? "MOD" : "",
					ProcCode="Z910"+(3-i%3).ToString(),Descript=arrayDescripts[i],FeeAmt=arrayFees[i],PriInsAmt=arrayFees[i]-arrayPatAmts[i],
					PatAmt=arrayPatAmts[i],Prognosis="",Dx="",ProcAbbr="",DateTP=treatPlan.DateTP,ProvNum=ProvNum });
			}
		}

		private static void SeedPayPlan() {
			PayPlan payPlan=new PayPlan { PatNum=Patient1.PatNum,Guarantor=Patient1.PatNum,PayPlanDate=new DateTime(2025,5,1),APR=18.5,
				Note="Synthetic payment plan",CompletedAmt=1250,NumberOfPayments=12,PayAmt=114.58,DownPayment=0,Signature="",
				ChargeFrequency=PayPlanFrequency.Monthly,DatePayPlanStart=new DateTime(2025,6,1),SecurityHash="" };
			PayPlanNum=PayPlans.Insert(payPlan);
		}

		private static void SeedAccounts() {
			long accountNumChecking=Accounts.Insert(new Account { Description="Synthetic Checking",AcctType=AccountType.Asset,BankNumber="000111222",
				AccountColor=Color.White });
			long accountNumLoan=Accounts.Insert(new Account { Description="Synthetic Loan, long term",AcctType=AccountType.Liability,BankNumber="",
				AccountColor=Color.White });
			Accounts.Insert(new Account { Description="=Synthetic formula-like name",AcctType=AccountType.Equity,BankNumber="",AccountColor=Color.White });
			long accountNumFees=Accounts.Insert(new Account { Description="Synthetic \"Fees\"",AcctType=AccountType.Income,BankNumber="",
				AccountColor=Color.White });
			Accounts.Insert(new Account { Description="Synthetic Inactive",AcctType=AccountType.Asset,BankNumber="",Inactive=true,AccountColor=Color.White });
			InsertJournal(accountNumChecking,new DateTime(2025,1,15),1000.10,0,"Synthetic deposit");
			InsertJournal(accountNumFees,new DateTime(2025,1,15),0,1000.10,"Synthetic deposit");
			InsertJournal(accountNumChecking,new DateTime(2025,2,1),300,0,"Synthetic loan");
			InsertJournal(accountNumLoan,new DateTime(2025,2,1),0,300,"Synthetic loan");
		}

		private static void InsertJournal(long accountNum,DateTime date,double debit,double credit,string memo) {
			JournalEntries.Insert(new JournalEntry { AccountNum=accountNum,DateDisplayed=date,DebitAmt=debit,CreditAmt=credit,Memo=memo,Splits="",
				CheckNumber="" });
		}

		private static void SeedStatements() {
			Statement statementFamily=NewStatement(Patient1.PatNum,"Synthetic statement note");
			StatementNumFamily=Statements.Insert(statementFamily);
			Statement statementLimited=NewStatement(Patient2.PatNum,"Synthetic limited statement");
			statementLimited.StatementType=StmtType.LimitedStatement;
			StatementNumLimited=Statements.Insert(statementLimited);
			StmtLinks.AttachProcsToStatement(StatementNumLimited,new List<long> { ProcNumZero });
			StmtLinks.AttachAdjsToStatement(StatementNumLimited,new List<long> { AdjNumZero });
			StmtLinks.AttachPaySplitsToStatement(StatementNumLimited,new List<long> { SplitNumZero });
		}

		private static Statement NewStatement(long patNum,string note) {
			Statement statement=new Statement();
			statement.PatNum=patNum;
			statement.DateSent=DateStatement;
			statement.DateRangeFrom=new DateTime(2025,1,1);
			statement.DateRangeTo=DateStatement;
			statement.Mode_=StatementMode.InPerson;
			statement.StatementType=StmtType.NotSet;
			statement.Note=note;
			statement.NoteBold="";
			statement.IsBalValid=true;
			statement.Intermingled=true;
			statement.EmailSubject="";
			statement.EmailBody="";
			statement.ShortGUID="";
			statement.StatementURL="";
			statement.StatementShortURL="";
			return statement;
		}

		///<summary>A wiki list whose items hold text that XML must escape, a non-ASCII name, an empty cell and a line break.</summary>
		private static void SeedWikiList() {
			List<WikiListHeaderWidth> listHeaders=new List<WikiListHeaderWidth> {
				new WikiListHeaderWidth { ListName=WikiListName,ColName=WikiListName+"Num",ColWidth=60,PickList="" },
				new WikiListHeaderWidth { ListName=WikiListName,ColName="Supplier",ColWidth=150,PickList="" },
				new WikiListHeaderWidth { ListName=WikiListName,ColName="Phone",ColWidth=110,PickList="" },
				new WikiListHeaderWidth { ListName=WikiListName,ColName="Notes",ColWidth=220,PickList="" },
			};
			WikiLists.CreateNewWikiList(WikiListName,listHeaders);
			string[][] arrayRows=new[] {
				new[] { "Synthetic Supply & Co.","(503)555-0130","Orders <weekly>; \"rush\" fee 0.30" },
				new[] { "\u00c9clair Dental Lab","(503)555-0131","Caf\u00e9 hours: 8\u20135\r\nClosed Fridays" },
				new[] { "Empty Notes Ltd.","","" },
			};
			foreach(string[] arrayCells in arrayRows) {
				long itemNum=WikiLists.AddItem(WikiListName);
				System.Data.DataTable table=WikiLists.GetItem(WikiListName,itemNum);
				table.Rows[0]["Supplier"]=arrayCells[0];
				table.Rows[0]["Phone"]=arrayCells[1];
				table.Rows[0]["Notes"]=arrayCells[2];
				WikiLists.UpdateItem(WikiListName,table);
			}
		}
	}
}
