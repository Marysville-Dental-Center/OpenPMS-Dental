// The print guard for the office's documents (package UI-2a-3): a sheet as a PDF (SheetDrawingJob.CreatePdf) and as a printed first page
// (SheetPrintingJob.DrawSheetFirstPage), a sheet grid drawn by SheetDrawingJob (bitmap and PDF, with the sheet's default font and with a field
// font), a claim form printed by FormClaimPrint's page event, the perio chart as FormPerio prints it, and the claim's perio chart attachment
// picture (FormClaimEdit.GetPerioAttachmentBitmap) are identical with today's font tokens and with UI-2b's. Synthetic rows in the scratch
// database ui2a3_*; nothing is printed and no window is shown.
// Licensed under the GNU General Public License, version 2 or later. See LICENSE in the repository root.
using System;
using System.Collections.Generic;
using System.Data;
using System.Drawing;
using System.Drawing.Printing;
using System.Linq;
using System.Reflection;
using System.Runtime.ExceptionServices;
using System.Runtime.Serialization;
using System.Windows.Forms;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using OpenPmsDental;
using OpenPmsDentalBusiness;
using PdfSharp.Drawing;
using PdfSharp.Pdf;

namespace PrintGuardTests {
	[TestClass]
	public class DocumentPrintGuardTests {
		#region Sheets
		[TestMethod]
		public void Sheet_PdfAndPrintedPage_AreTheSameWithUi2bFonts() {
			Guard.AssertSameWithUi2bFonts("Sheet_Pdf",() => Guard.GetPdfPages(new SheetDrawingJob().CreatePdf(NewSheet())),PrintGuardDb.Begin);
			Guard.AssertSameWithUi2bFonts("Sheet_PrintedPage",() => Guard.DrawToPixels(850,1100,g => {
				Type typeJob=typeof(FormOpenDental).Assembly.GetType("OpenPmsDental.SheetPrintingJob",true);
				object sheetPrintingJob=Activator.CreateInstance(typeJob,nonPublic:true);
				Invoke(typeJob,sheetPrintingJob,"DrawSheetFirstPage",g,NewSheet());
			}),PrintGuardDb.Begin);
		}

		[TestMethod]
		public void SheetGrid_BitmapAndPdf_AreTheSameWithUi2bFonts() {
			//The grid of a sheet as SheetDrawingJob draws it (a GridOD made IsForSheets), with the sheet's default font (the grid's print fonts) and
			//with a font of its own (FontForSheets).
			foreach(string fontName in new[] { "","Arial" }) {
				string name="SheetGrid_"+(fontName=="" ? "DefaultFont" : fontName);
				Guard.AssertSameWithUi2bFonts(name+"_Bitmap",() => Guard.DrawToPixels(850,1100,g => {
					Sheet sheet=NewSheet();
					new SheetDrawingJob().DrawFieldGridHelper(NewGridField(fontName),sheet,g,null,null,null,null,isPrinting:true,table:NewProcTable());
				}),PrintGuardDb.Begin);
				Guard.AssertSameWithUi2bFonts(name+"_Pdf",() => {
					Sheet sheet=NewSheet();
					PdfDocument pdfDocument=new PdfDocument();
					PdfPage pdfPage=pdfDocument.AddPage();
					using(XGraphics xGraphics=XGraphics.FromPdfPage(pdfPage)) {
						xGraphics.SmoothingMode=XSmoothingMode.HighQuality;//as CreatePdfPage; the grid's PDF methods convert to points themselves
						new SheetDrawingJob().DrawFieldGridHelper(NewGridField(fontName),sheet,null,xGraphics,null,null,null,table:NewProcTable());
					}
					return Guard.GetPdfPages(pdfDocument);
				},PrintGuardDb.Begin);
			}
		}
		#endregion Sheets

		#region Claim form
		[TestMethod]
		public void ClaimForm_PrintedPages_AreTheSameWithUi2bFonts() {
			long claimNum=0;
			long patNum=0;
			StaRunner.Run(() => {
				PrintGuardDb.Begin();
				SyntheticRows.CreateClaim(out claimNum,out patNum);
			});
			List<ClaimForm> listClaimForms=null;
			StaRunner.Run(() => {
				PrintGuardDb.Begin();
				listClaimForms=ClaimForms.GetInternalClaims().Where(x => x.Description.StartsWith("ADA") || x.Description==ClaimForms.DescriptionDentiCalDC217_2019)
					.OrderBy(x => x.Description).ToList();
			});
			Assert.IsTrue(listClaimForms.Count>=2,"The internal ADA and DC217 claim forms.");
			foreach(ClaimForm claimForm in listClaimForms) {
				Guard.AssertSameWithUi2bFonts("ClaimForm_"+new string(claimForm.Description.Where(char.IsLetterOrDigit).ToArray()),() => {
					List<byte> listBytes=new List<byte>();
					FormClaimPrint formClaimPrint=(FormClaimPrint)FormatterServices.GetUninitializedObject(typeof(FormClaimPrint));
					GC.SuppressFinalize(formClaimPrint);
					formClaimPrint.ClaimFormCur=claimForm;
					formClaimPrint.ClaimNum=claimNum;
					formClaimPrint.PatNum=patNum;
					//The page event sets this label's text after the last page; the window is never built.
					FieldInfo fieldLabel=typeof(FormClaimPrint).GetField("labelTotPages",BindingFlags.Instance|BindingFlags.NonPublic);
					fieldLabel.SetValue(formClaimPrint,Activator.CreateInstance(fieldLabel.FieldType));
					for(int page=0;page<5;page++) {
						bool hasMorePages=false;
						listBytes.AddRange(Guard.DrawToPixels(850,1100,g => {
							PrintPageEventArgs printPageEventArgs=new PrintPageEventArgs(g,new Rectangle(0,0,850,1100),new Rectangle(0,0,850,1100),new PageSettings());
							Invoke(typeof(FormClaimPrint),formClaimPrint,"pd2_PrintPage",null,printPageEventArgs);
							hasMorePages=printPageEventArgs.HasMorePages;
						}));
						if(!hasMorePages) {
							break;
						}
					}
					return listBytes.ToArray();
				},PrintGuardDb.Begin);
			}
		}
		#endregion Claim form

		#region Perio chart
		[TestMethod]
		public void PerioChart_PrintAndClaimAttachment_AreTheSameWithUi2bFonts() {
			long patNum=0;
			StaRunner.Run(() => {
				PrintGuardDb.Begin();
				patNum=SyntheticRows.CreatePerioExam();
			});
			//FormPerio.butPrint_Click: the chart at 96 dpi in Microsoft Sans Serif 8.25 at the printed size, drawn by pd2_PrintPage at (100,158) with
			//the four Arial 15 letters beside it.
			Guard.AssertSameWithUi2bFonts("PerioChart_Print",() => {
				FormOpenDental.PatNumCur=patNum;//the implant marker reads the patient's procedures
				using(ContrPerio contrPerio=new ContrPerio()) {
					contrPerio.LayoutManager.Is96dpi=true;
					contrPerio.Font=new Font("Microsoft Sans Serif",8.25f);
					contrPerio.Size=new Size(602,642+3*13);
					contrPerio.ListPerioExams=PerioExams.Refresh(patNum);
					contrPerio.ListPerioMeasures=PerioMeasures.GetForPatient(patNum);
					contrPerio.IdxExamSelected=contrPerio.ListPerioExams.Count-1;
					contrPerio.LoadData();
					return Guard.DrawToPixels(850,1100,g => {
						g.TranslateTransform(100,67+25+20+20+20+6);
						contrPerio.DrawChart(g);
						g.DrawString("F",new Font("Arial",15),Brushes.Black,new Point(-26,92));
						g.DrawString("L",new Font("Arial",15),Brushes.Black,new Point(-26,212+13));
						g.ResetTransform();
					});
				}
			},PrintGuardDb.Begin);
			//FormClaimEdit's Perio Chart attachment, through the program's own method.
			Guard.AssertSameWithUi2bFonts("PerioChart_ClaimAttachment",() => {
				FormOpenDental.PatNumCur=patNum;
				Patient patient=Patients.GetPat(patNum);
				using(HostFormOD hostFormOD=new HostFormOD())
				using(Bitmap bitmap=FormClaimEdit.GetPerioAttachmentBitmap(patient,hostFormOD.GetLayoutManager())) {
					return Guard.GetPixels(bitmap);
				}
			},PrintGuardDb.Begin);
		}
		#endregion Perio chart

		#region Helpers
		///<summary>A letter-size patient form with text in the sheet's default font, in a field font of its own (Microsoft Sans Serif, Arial and
		///Times New Roman, regular and bold), right-aligned and wrapped text, a rectangle, a line and a check box.</summary>
		public static Sheet NewSheet() {
			List<SheetField> listSheetFields=new List<SheetField> {
				TextField("Synthetic patient form, default font",50,50,""),
				TextField("Microsoft Sans Serif text 8.25",50,90,"Microsoft Sans Serif",8.25f),
				TextField("Bold Arial heading",50,130,"Arial",12f,isBold:true),
				TextField("Times New Roman paragraph that is long enough to wrap onto a second line within the field's width on the page.",50,170,"Times New Roman",10f),
				TextField("Right aligned",50,250,"Arial",9f,textAlign:HorizontalAlignment.Right),
				new SheetField { FieldType=SheetFieldType.Rectangle,XPos=40,YPos=40,Width=500,Height=260,ItemColor=Color.Black,FieldValue="",FieldName="",
					FontName="",RadioButtonValue="",RadioButtonGroup="",ReportableName="",UiLabelMobile="",UiLabelMobileRadioButton="" },
				new SheetField { FieldType=SheetFieldType.Line,XPos=40,YPos=320,Width=500,Height=0,ItemColor=Color.Black,FieldValue="",FieldName="",
					FontName="",RadioButtonValue="",RadioButtonGroup="",ReportableName="",UiLabelMobile="",UiLabelMobileRadioButton="" },
				new SheetField { FieldType=SheetFieldType.CheckBox,XPos=40,YPos=340,Width=12,Height=12,ItemColor=Color.Black,FieldValue="X",FieldName="misc",
					FontName="",RadioButtonValue="",RadioButtonGroup="",ReportableName="",UiLabelMobile="",UiLabelMobileRadioButton="" },
			};
			return new Sheet {
				SheetType=SheetTypeEnum.PatientForm,
				FontName="Microsoft Sans Serif",
				FontSize=9f,
				Width=850,
				Height=1100,
				IsLandscape=false,
				Description="Print guard synthetic",
				Parameters=new List<SheetParameter>(),
				SheetFields=listSheetFields,
			};
		}

		private static SheetField TextField(string text,int xPos,int yPos,string fontName,float fontSize=9f,bool isBold=false,
			HorizontalAlignment textAlign=HorizontalAlignment.Left)
		{
			return new SheetField {
				FieldType=SheetFieldType.StaticText,
				FieldName="",
				FieldValue=text,
				FontName=fontName,
				FontSize=fontSize,
				FontIsBold=isBold,
				XPos=xPos,
				YPos=yPos,
				Width=420,
				Height=40,
				TextAlign=textAlign,
				ItemColor=Color.Black,
				RadioButtonValue="",
				RadioButtonGroup="",
				ReportableName="",
				UiLabelMobile="",
				UiLabelMobileRadioButton="",
				GrowthBehavior=GrowthBehaviorEnum.None,
			};
		}

		///<summary>A ProcsWithFee grid field (the procedures of a referral letter), with the sheet's default font or a font of its own.</summary>
		private static SheetField NewGridField(string fontName) {
			return new SheetField {
				FieldType=SheetFieldType.Grid,
				FieldName="ProcsWithFee",
				FieldValue="",
				FontName=fontName,
				FontSize=fontName=="" ? 0 : 9f,
				XPos=60,
				YPos=120,
				Width=640,
				Height=300,
				ItemColor=Color.Black,
				RadioButtonValue="",
				RadioButtonGroup="",
				ReportableName="",
				UiLabelMobile="",
				UiLabelMobileRadioButton="",
				GrowthBehavior=GrowthBehaviorEnum.None,
			};
		}

		///<summary>The rows of a ProcsWithFee grid: 25 synthetic procedures, some with a description that wraps.</summary>
		private static DataTable NewProcTable() {
			DataTable table=new DataTable();
			foreach(string column in new[] { "Procedure Code","Description","ToothNum","Surface","Fee" }) {
				table.Columns.Add(column);
			}
			for(int i=0;i<25;i++) {
				table.Rows.Add("D"+(2140+i),i%4==0 ? "Synthetic restoration with a description long enough to wrap in its column, line "+i : "Synthetic service "+i,
					(3+i%28).ToString(),i%3==0 ? "MOD" : "",(95.5+i*10).ToString("F2"));
			}
			return table;
		}

		public static object Invoke(Type type,object instance,string methodName,params object[] arrayArgs) {
			MethodInfo methodInfo=type.GetMethod(methodName,BindingFlags.Instance|BindingFlags.Public|BindingFlags.NonPublic);
			if(methodInfo==null) {
				throw new MissingMethodException(type.Name,methodName);
			}
			try {
				return methodInfo.Invoke(instance,arrayArgs);
			}
			catch(TargetInvocationException ex) {
				ExceptionDispatchInfo.Capture(ex.InnerException).Throw();
				return null;//Not reached.
			}
		}
		#endregion Helpers
	}

	///<summary>A FormODBase built off screen for its LayoutManager (96 dpi, zoom 100%), as the program's windows have one. Never shown.</summary>
	public class HostFormOD:FormODBase {
		public HostFormOD() {
			Name="HostFormOD";
			ClientSize=new Size(700,760);
			InitializeLayoutManager();
		}

		public LayoutManagerForms GetLayoutManager() {
			return LayoutManager;
		}
	}

	///<summary>Synthetic rows for the document tests.</summary>
	public static class SyntheticRows {
		///<summary>A patient with an insurance plan and a waiting claim of two completed procedures.</summary>
		public static void CreateClaim(out long claimNum,out long patNum) {
			string suffix=Guid.NewGuid().ToString("N").Substring(0,6);
			Provider provider=new Provider { Abbr="PG"+suffix,FName="Alex",LName="Synthetic",NationalProvID="1234567893",SigOnFile=true };
			OpenPmsDentalBusiness.Crud.ProviderCrud.Insert(provider);
			Patient patient=new Patient { LName="Printguard"+suffix,FName="Claim",Gender=PatientGender.Female,Birthdate=new DateTime(1985,5,4),
				Address="1 Synthetic Street",City="Example City",State="CA",Zip="95901",PatStatus=PatientStatus.Patient,PriProv=provider.ProvNum,ChartNumber="" };
			OpenPmsDentalBusiness.Crud.PatientCrud.Insert(patient);
			patient.Guarantor=patient.PatNum;
			OpenPmsDentalBusiness.Crud.PatientCrud.Update(patient);
			Carrier carrier=new Carrier { CarrierName="Synthetic Dental Carrier "+suffix,Address="2 Example Road",City="Example City",State="CA",Zip="95902" };
			OpenPmsDentalBusiness.Crud.CarrierCrud.Insert(carrier);
			InsPlan insPlan=new InsPlan { CarrierNum=carrier.CarrierNum,GroupName="Synthetic group",GroupNum="G-1001",PlanType="" };
			OpenPmsDentalBusiness.Crud.InsPlanCrud.Insert(insPlan);
			InsSub insSub=new InsSub { PlanNum=insPlan.PlanNum,Subscriber=patient.PatNum,SubscriberID="SYN"+suffix,ReleaseInfo=true,AssignBen=true };
			OpenPmsDentalBusiness.Crud.InsSubCrud.Insert(insSub);
			OpenPmsDentalBusiness.Crud.PatPlanCrud.Insert(new PatPlan { PatNum=patient.PatNum,InsSubNum=insSub.InsSubNum,Ordinal=1,Relationship=Relat.Self });
			Claim claim=new Claim { PatNum=patient.PatNum,PlanNum=insPlan.PlanNum,InsSubNum=insSub.InsSubNum,ClaimType="P",ClaimStatus="W",
				DateService=new DateTime(2026,9,2),ProvTreat=provider.ProvNum,ProvBill=provider.ProvNum,PatRelat=Relat.Self,PlaceService=PlaceOfService.Office };
			OpenPmsDentalBusiness.Crud.ClaimCrud.Insert(claim);
			double claimFee=0;
			for(int n=1;n<=2;n++) {
				ProcedureCode procedureCode=new ProcedureCode { ProcCode="Z"+suffix.Substring(0,3).ToUpperInvariant()+n.ToString("00"),Descript="Synthetic service "+n,
					AbbrDesc="Syn"+n,TreatArea=TreatmentArea.Tooth };
				OpenPmsDentalBusiness.Crud.ProcedureCodeCrud.Insert(procedureCode);
				Procedure procedure=new Procedure { PatNum=patient.PatNum,CodeNum=procedureCode.CodeNum,ProcDate=new DateTime(2026,9,1+n),ProcStatus=ProcStat.C,
					ProcFee=100+n,ProvNum=provider.ProvNum,PlaceService=PlaceOfService.Office,ToothNum=(n+10).ToString() };
				OpenPmsDentalBusiness.Crud.ProcedureCrud.Insert(procedure);
				ClaimProc claimProc=new ClaimProc { ProcNum=procedure.ProcNum,ClaimNum=claim.ClaimNum,PatNum=patient.PatNum,PlanNum=insPlan.PlanNum,
					InsSubNum=insSub.InsSubNum,ProvNum=provider.ProvNum,FeeBilled=procedure.ProcFee,CodeSent=procedureCode.ProcCode,ProcDate=procedure.ProcDate,
					Status=ClaimProcStatus.NotReceived,LineNumber=(byte)n };
				OpenPmsDentalBusiness.Crud.ClaimProcCrud.Insert(claimProc);
				claimFee+=procedure.ProcFee;
			}
			claim.ClaimFee=claimFee;
			OpenPmsDentalBusiness.Crud.ClaimCrud.Update(claim);
			Cache.Refresh(InvalidType.Prefs,InvalidType.Providers,InvalidType.Defs,InvalidType.ProcCodes,InvalidType.Carriers,InvalidType.ClaimForms);
			claimNum=claim.ClaimNum;
			patNum=patient.PatNum;
		}

		///<summary>A patient with one perio exam: probing, bleeding, gingival margin and mobility on a few teeth.</summary>
		public static long CreatePerioExam() {
			Patient patient=new Patient { LName="Printguard",FName="Perio",Gender=PatientGender.Male,Birthdate=new DateTime(1979,3,2),
				PatStatus=PatientStatus.Patient,ChartNumber="" };
			OpenPmsDentalBusiness.Crud.PatientCrud.Insert(patient);
			patient.Guarantor=patient.PatNum;
			OpenPmsDentalBusiness.Crud.PatientCrud.Update(patient);
			long perioExamNum=PerioExams.Insert(new PerioExam { PatNum=patient.PatNum,ExamDate=new DateTime(2026,1,5),DateTMeasureEdit=new DateTime(2026,1,5,8,30,0) });
			Action<PerioSequenceType,int,int,int,int,int,int,int,int> actionInsert=(sequenceType,tooth,toothValue,mb,b,db,ml,l,dl) =>
				OpenPmsDentalBusiness.Crud.PerioMeasureCrud.Insert(new PerioMeasure { PerioExamNum=perioExamNum,SequenceType=sequenceType,IntTooth=tooth,
					ToothValue=toothValue,MBvalue=mb,Bvalue=b,DBvalue=db,MLvalue=ml,Lvalue=l,DLvalue=dl });
			actionInsert(PerioSequenceType.Probing,3,-1,3,5,4,2,3,6);
			actionInsert(PerioSequenceType.Probing,14,-1,2,3,2,4,4,3);
			actionInsert(PerioSequenceType.Probing,19,-1,2,2,3,4,5,3);
			actionInsert(PerioSequenceType.Probing,30,-1,5,6,7,3,2,2);
			actionInsert(PerioSequenceType.GingMargin,3,-1,1,2,1,-1,-1,-1);
			actionInsert(PerioSequenceType.BleedSupPlaqCalc,3,-1,1,0,3,0,4,1);//bleeding, suppuration, plaque
			actionInsert(PerioSequenceType.Mobility,19,2,-1,-1,-1,-1,-1,-1);
			return patient.PatNum;
		}
	}
}
