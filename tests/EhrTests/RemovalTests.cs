// The deliberate deletion of package EHR-6b (docs/specs/ehr-2027-measures-plan.md, section 3 and EHR-6b; owner decisions 2 and 4 of
// 2026-11-14): the 2014 quality measures (both parts of QualityMeasures.cs, QualityMeasure.cs, the 2014 windows and their QRDA writers) and the
// 2011 engine with its window and its PQRI XML export are gone, with FormEHR.ProvKeyIsValid and the dead butQuality_Click. Source checks: the
// program's source (comments left out) names none of the deleted code, none of the twelve form files and neither business file exists, the
// project file names none of the four forms, and "Quality Meas" gives a message until EHR-5a opens the 2027 window. Reflection checks: neither
// assembly holds a deleted type or method. What section 2.1 keeps stays, each with its other callers. No database is used: the work-table helper
// that still drops an old tempehrquality table is covered on the scratch database by MeasureCalcTests, unchanged.
// Licensed under the GNU General Public License, version 2 or later. See LICENSE in the repository root.
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using OpenPmsDental;
using OpenPmsDentalBusiness;

namespace EhrTests {
	[TestClass]
	public class RemovalTests {
		///<summary>The names of the deleted code (the package's Tests): types, methods and windows of the two business files and the four forms.</summary>
		private static readonly string[] _arrayDeletedNames=new[] { "QualityMeasures","QualityMeasure","QualityType","QualityType2014","GetAll2014",
			"GetEhrCqmData","GenerateQRDA","GetPQRIMeasureNumber","GeneratePQRS_xml","EhrCqm","FormEhrQualityMeasures","FormEhrQualityMeasureEdit",
			"ProvKeyIsValid" };
		private static readonly string[] _arrayForms=new[] { "FormEhrQualityMeasures2014","FormEhrQualityMeasureEdit2014","FormEhrQualityMeasures",
			"FormEhrQualityMeasureEdit" };

		[TestMethod]
		public void Source_NamesNoneOfTheDeletedCode_CommentsLeftOut() {
			List<string> listProblems=new List<string>();
			foreach(string path in TestPaths.ProgramSourceFiles()) {
				string code=CSharpSource.Read(File.ReadAllText(path)).CodeWithoutComments;
				foreach(string name in _arrayDeletedNames) {
					if(code.Contains(name)) {
						listProblems.Add(TestPaths.Relative(path)+": "+name);
					}
				}
			}
			Assert.AreEqual(0,listProblems.Count,"Deleted code still named: "+string.Join("; ",listProblems));
		}

		[TestMethod]
		public void Files_TheTwelveFormFilesAndTheTwoBusinessFiles_AreGone() {
			List<string> listPaths=new List<string> { @"OpenPmsDentalBusiness\Db Multi Table\QualityMeasures.cs",
				@"OpenPmsDentalBusiness\Db Multi Table\QualityMeasure.cs" };
			foreach(string form in _arrayForms) {
				listPaths.Add(@"OpenPmsDental\Forms\"+form+".cs");
				listPaths.Add(@"OpenPmsDental\Forms\"+form+".Designer.cs");
				listPaths.Add(@"OpenPmsDental\Forms\"+form+".resx");
			}
			Assert.AreEqual(14,listPaths.Count);
			List<string> listLeft=listPaths.Where(x => File.Exists(TestPaths.Combine(x))).ToList();
			Assert.AreEqual(0,listLeft.Count,"Still on disk: "+string.Join("; ",listLeft));
			//The folders that held them are still there, with their other files (the windows of the Measure Calc and the EHR window among them).
			Assert.IsTrue(File.Exists(TestPaths.Combine(@"OpenPmsDentalBusiness\Data Interface\EhrMeasures.cs")));
			Assert.IsTrue(File.Exists(TestPaths.Combine(@"OpenPmsDental\Forms\FormEhrMeasureCalc.cs")));
			Assert.IsTrue(File.Exists(TestPaths.Combine(@"OpenPmsDental\Forms\FormEHR.cs")));
		}

		[TestMethod]
		public void ProjectFiles_NameNoneOfTheFourForms_AndKeepTheirNeighbours() {
			string program=XmlText.WithoutComments(TestPaths.ReadSource(@"OpenPmsDental\OpenPmsDental.csproj"));
			Assert.IsFalse(program.Contains("FormEhrQualityMeasure"),"An item of the four forms is left (Compile or EmbeddedResource Update).");
			//The items next to the eight removed ones stay.
			StringAssert.Contains(program,"<Compile Update=\"Forms\\FormEhrProviderKeys.designer.cs\">");
			StringAssert.Contains(program,"<Compile Update=\"Forms\\FormEhrReminders.Designer.cs\">");
			StringAssert.Contains(program,"<EmbeddedResource Update=\"Forms\\FormEhrProviderKeys.resx\">");
			StringAssert.Contains(program,"<EmbeddedResource Update=\"Forms\\FormEhrReminders.resx\">");
			//The business project globs; it never named the two files, and names neither now.
			string business=XmlText.WithoutComments(TestPaths.ReadSource(@"OpenPmsDentalBusiness\OpenPmsDentalBusiness.csproj"));
			Assert.IsFalse(business.Contains("QualityMeasure"));
		}

		[TestMethod]
		public void Assemblies_HoldNoneOfTheDeletedTypesOrMethods() {
			Assembly assemblyBusiness=typeof(EhrMeasures).Assembly;
			foreach(string name in new[] { "QualityMeasures","QualityMeasure","QualityType","QualityType2014","CqmItemAbbreviation" }) {
				Assert.IsNull(assemblyBusiness.GetType("OpenPmsDentalBusiness."+name),name);
			}
			List<string> listTypes=GetTypes(assemblyBusiness).Where(x => x.Name.StartsWith("EhrCqm") || x.Name.StartsWith("QualityMeasure")
				|| x.Name.StartsWith("QualityType")).Select(x => x.FullName).ToList();
			Assert.AreEqual(0,listTypes.Count,string.Join("; ",listTypes));
			Assembly assemblyProgram=typeof(FormEHR).Assembly;
			foreach(string form in _arrayForms) {
				Assert.IsNull(assemblyProgram.GetType("OpenPmsDental."+form),form);
			}
			List<string> listForms=GetTypes(assemblyProgram).Where(x => x.Name.StartsWith("FormEhrQualityMeasure")).Select(x => x.FullName).ToList();
			Assert.AreEqual(0,listForms.Count,string.Join("; ",listForms));
			BindingFlags bindingFlags=BindingFlags.Public|BindingFlags.NonPublic|BindingFlags.Static|BindingFlags.Instance;
			Assert.IsNull(typeof(FormEHR).GetMethod("ProvKeyIsValid",bindingFlags));
			Assert.IsNull(typeof(FormEHR).GetMethod("butQuality_Click",bindingFlags));
			Assert.IsNotNull(typeof(FormEHR).GetMethod("but2014CQM_Click",bindingFlags),"The \"Quality Meas\" button keeps its handler.");
			//The resources of the four forms went with their .resx files.
			List<string> listResources=assemblyProgram.GetManifestResourceNames().Where(x => x.Contains("FormEhrQualityMeasure")).ToList();
			Assert.AreEqual(0,listResources.Count,string.Join("; ",listResources));
		}

		[TestMethod]
		public void QualityMeas_ShowsTheMessage_UntilTheWindowOfEhr5a() {
			string source=TestPaths.ReadSource(@"OpenPmsDental\Forms\FormEHR.cs");
			string handler=CSharpSource.GetBlock(source,"private void but2014CQM_Click(");
			StringAssert.Contains(handler,"MsgBox.Show(this,\"The 2027 quality measures are not in this build yet.\");");
			Assert.IsFalse(handler.Contains("new Form"),"No window opens.");
			Assert.IsFalse(handler.Contains("EhrFormResult.PatientSelect"),"No patient comes back from a message.");
			string designer=TestPaths.ReadSource(@"OpenPmsDental\Forms\FormEHR.Designer.cs");
			StringAssert.Contains(designer,"this.but2014CQM.Text = \"Quality Meas\";");
			StringAssert.Contains(designer,"this.but2014CQM.Click += new System.EventHandler(this.but2014CQM_Click);");
			//The Measure Calc window still hands its picked patient up as PatientSelect (EHR-4a), and the chart still reads it.
			StringAssert.Contains(source,"EhrFormResultOnClose=EhrFormResult.PatientSelect;");
		}

		///<summary>Section 2.1's outside callers: what the deleted code used and other code still uses stays (the package's Work).</summary>
		[TestMethod]
		public void KeptHelpers_Stay_WithTheirOtherCallers() {
			Assert.IsNotNull(typeof(EmailMessages).GetMethod("SendTestUnsecure",new[] { typeof(string),typeof(string),typeof(string) }));
			foreach(string form in new[] { "FormEhrEncryption","FormEhrHash","FormEhrLabPanels","FormEhrVaccines" }) {
				string code=CSharpSource.Read(TestPaths.ReadSource(@"OpenPmsDental\Forms\"+form+".cs")).CodeWithoutComments;
				StringAssert.Contains(code,"EmailMessages.SendTestUnsecure(",form);
			}
			Assert.IsNotNull(typeof(CodeBase.MsgBoxCopyPaste));
			Assert.IsNotNull(typeof(OpenPmsDental.MsgBoxCopyPaste));
			Assert.IsNotNull(typeof(CodeBase.ODZip).GetMethod("GetDefaultEncodingBytes",new[] { typeof(string) }));
			StringAssert.Contains(CSharpSource.Read(TestPaths.ReadSource(@"OpenPmsDentalBusiness\Eclaims\Ramq.cs")).CodeWithoutComments,
				"ODZip.GetDefaultEncodingBytes(");
			//The work-table helper of EHR-4a keeps the old engines' pattern: a tempehrquality table a stopped 2011 or 2014 run left is still dropped
			//(on the scratch database: MeasureCalcTests.WorkTables_OnlyOldOnes_OfThisDatabase_AreDropped).
			Assert.IsNotNull(typeof(EhrMeasures).GetMethod("DropOldWorkTables",new[] { typeof(int) }));
			int day=EhrMeasures.WORK_TABLE_MAX_AGE_SECONDS;
			Assert.IsTrue(EhrMeasures.IsOldWorkTable("tempehrquality7",day+1,day));
			StringAssert.Contains(CSharpSource.Read(TestPaths.ReadSource(@"OpenPmsDental\Forms\FormEhrMeasureCalc.cs")).CodeWithoutComments,
				"EhrMeasures.DropOldWorkTables(");
			//Kept although its only caller was the 2014 engine: EhrCodes.cs is EHR-6a's file, and a value-set lookup of the import (EHR-6b's port note).
			Assert.IsNotNull(typeof(EhrCodes).GetMethod("GetCodeAndCodeSystem",new[] { typeof(List<string>),typeof(bool) }));
			//The tables the measures read stay, with their types (no table, column or saved row is touched).
			foreach(Type type in new[] { typeof(Encounter),typeof(Intervention),typeof(EhrNotPerformed),typeof(EhrMeasureEvent),typeof(Vitalsign) }) {
				Assert.IsFalse(string.IsNullOrEmpty(CrudTableAttribute.GetTableName(type)),type.Name);
			}
		}

		private static List<Type> GetTypes(Assembly assembly) {
			try {
				return assembly.GetTypes().ToList();
			}
			catch(ReflectionTypeLoadException ex) {
				return ex.Types.Where(x => x!=null).ToList();
			}
		}
	}
}
