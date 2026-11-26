// Tests for package REL-FILES (docs/port-notes/REL-FILES.md): the license list, THIRD-PARTY-NOTICES.txt, the product name shown in place of Open
// Dental's default title and SoftwareName, the About box copyright line, the removed Benco branding switch, and README.md and NOTICE.
// Licensed under the GNU General Public License, version 2 or later. See LICENSE in the repository root.
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using System.Text.RegularExpressions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using OpenPmsDental;

namespace AttributionTests {
	[TestClass]
	public class ReleaseIdentityTests {
		///<summary>Every third-party component the release ships, by its name in THIRD-PARTY-NOTICES.txt (report-release.md 1.3 to 1.5).</summary>
		private static readonly string[] _arrayShippedComponents=new string[] {
			"MySqlConnector",".NET libraries","Newtonsoft.Json","MimeKit","MailKit","MSAL.NET","SSH.NET","Bouncy Castle Cryptography","PDFsharp",
			"MigraDoc","Helix Toolkit",".NET Community Toolkit","Direct Project","WeCantSpell.Hunspell","Hunspell en_US dictionary",
			"Fluent UI System Icons",
			"RDL Project","Google APIs Client Library for .NET","Microsoft Edge WebView2 SDK","TWAIN","Pearl API notice",
			"HL7 CDA-core-xsl","AAFP CCR stylesheet","LOINC",
		};

		///<summary>Files of every listed component that ships in the output folder.</summary>
		private static readonly string[] _arrayShippedFiles=new string[] {
			"MySqlConnector.dll",
			"Microsoft.Extensions.DependencyInjection.Abstractions.dll","Microsoft.Extensions.Logging.Abstractions.dll","System.Buffers.dll",
			"System.Diagnostics.DiagnosticSource.dll","System.Formats.Asn1.dll","System.IO.Pipelines.dll","System.Memory.dll",
			"System.Numerics.Vectors.dll","System.Runtime.CompilerServices.Unsafe.dll","System.Text.Encodings.Web.dll","System.Text.Json.dll",
			"System.Threading.Tasks.Extensions.dll","Newtonsoft.Json.dll","MimeKit.dll","MailKit.dll",
			"Microsoft.Identity.Client.dll","Microsoft.IdentityModel.Abstractions.dll","Renci.SshNet.dll","BouncyCastle.Cryptography.dll",
			"PdfSharp-gdi.dll","PdfSharp.Charting-gdi.dll","PdfSharp.Shared.dll","PdfSharp.System.dll",
			"MigraDoc.DocumentObjectModel.dll","MigraDoc.Rendering-gdi.dll","Health.Direct.Agent.dll",
			"Health.Direct.Common.dll","Health.Direct.ResolverPlugins.dll","WeCantSpell.Hunspell.dll","RdlEngine.dll","RdlViewer.dll",
			"Google.Apis.dll","Google.Apis.Auth.dll","Google.Apis.Core.dll","Google.Apis.Gmail.v1.dll",
			"Microsoft.Web.WebView2.Core.dll","Microsoft.Web.WebView2.WinForms.dll","Microsoft.Web.WebView2.Wpf.dll","WebView2Loader.dll",
			//The WPF tooth chart's 3D helpers (TC-4): Helix Toolkit 3.1.2, the .NET Community Toolkit and System.ComponentModel.Annotations.
			"HelixToolkit.dll","HelixToolkit.Geometry.dll","HelixToolkit.Maths.dll","HelixToolkit.Wpf.dll","CommunityToolkit.Common.dll",
			"CommunityToolkit.Diagnostics.dll","CommunityToolkit.Mvvm.dll","System.ComponentModel.Annotations.dll",
			//The .NET 10 release (S3b-7): the Windows packages of the .NET libraries, Roslyn for the report engine's VB code, and the charts.
			"Microsoft.CodeAnalysis.dll","Microsoft.CodeAnalysis.VisualBasic.dll","System.DirectoryServices.AccountManagement.dll",
			"System.DirectoryServices.Protocols.dll","System.IO.Ports.dll","System.Management.dll","System.ServiceProcess.ServiceController.dll",
			"System.Speech.dll","WinForms.DataVisualization.dll","WinForms.DataVisualization.Utilities.dll",
		};

		///<summary>Names of the entries Open Dental's list had that are gone: its commercial registration-key terms, the removed API's and the ADA's
		///end-user licenses, Sparks3D, the stdole pre-release license, the wrong "Drifty" text for DotNetZip, and components that do not ship.</summary>
		private static readonly string[] _arrayRemovedEntries=new string[] {
			"OpenDental","OpenDental API End User","CDT","Sparks3D","Stdole","Drifty","Zxing","AForge","Angular","Bouncy Castle","BSD","Microsoft",
			"Oracle","Tao_Freeglut","fo-dicom","NDde",
		};

		///<summary>What packages BIN-A and BIN-B delete, Open Dental's own closed binaries, the libraries the tier 1 library updates removed
		///(OpenPop.NET, SharpSSH with Mentalis.org and DiffieHellman, MimeKit 2's BouncyCastle.Crypto, the Google APIs 1.44 platform shims),
		///fo-dicom, replaced by the in-house DICOM reader, and the spell checker the library updates replaced (NHunspell with its native
		///Hunspellx86.dll and Hunspellx64.dll): never listed as shipped.</summary>
		private static readonly string[] _arrayNotShipped=new string[] {
			"Sparks3D","SigPlusNET","Interop.Word","Interop.Microsoft.Office","Interop.VBIDE","stdole","AcroPDF","Acrobat","zxing","Avalara","RS232","DotNetZip",
			"Ionic","Drifty","VBbridges","xImageDeviceManager","EZTwain","Eztwain","SuniUSB","VideoCamLib","OpenPop","SharpSSH","Mentalis",
			"DiffieHellman","BouncyCastle.Crypto.dll","PlatformServices","fo-dicom","Dicom.Core","Dicom.Native","NHunspell","Hunspellx86","Hunspellx64","NDde",
		};

		#region Helpers
		private static string RepoRoot() {
			DirectoryInfo directoryInfo=new DirectoryInfo(AppDomain.CurrentDomain.BaseDirectory);
			while(directoryInfo!=null) {
				if(File.Exists(Path.Combine(directoryInfo.FullName,"LICENSE")) && File.Exists(Path.Combine(directoryInfo.FullName,"NOTICE"))) {
					return directoryInfo.FullName;
				}
				directoryInfo=directoryInfo.Parent;
			}
			Assert.Fail("Repository root not found above "+AppDomain.CurrentDomain.BaseDirectory);
			return null;
		}

		private static string ReadRepoFile(params string[] pathParts) {
			string path=Path.Combine(new[] { RepoRoot() }.Concat(pathParts).ToArray());
			Assert.IsTrue(File.Exists(path),"Missing file: "+path);
			return File.ReadAllText(path);
		}

		private static string Unwrap(string text) {
			return string.Join(" ",text.Split(new[] { "\r\n","\n" },StringSplitOptions.None).Select(x => x.Trim()).Where(x => x!=""));
		}

		private static string NormalizeNewLines(string text) {
			return text.Replace("\r\n","\n").Replace("\r","\n");
		}

		private static List<NoticeSection> ParseNoticesFile() {
			return LicenseList.ParseNotices(ReadRepoFile("THIRD-PARTY-NOTICES.txt"));
		}

		private static Assembly MakeAssembly(Version version,string informationalVersion) {
			AssemblyName assemblyName=new AssemblyName("ReleaseIdentityTestsDynamic"+Guid.NewGuid().ToString("N"));
			assemblyName.Version=version;
			CustomAttributeBuilder[] arrayAttributes=new CustomAttributeBuilder[0];
			if(informationalVersion!=null) {
				ConstructorInfo constructorInfo=typeof(AssemblyInformationalVersionAttribute).GetConstructor(new[] { typeof(string) });
				arrayAttributes=new[] { new CustomAttributeBuilder(constructorInfo,new object[] { informationalVersion }) };
			}
			//AssemblyBuilder.DefineDynamicAssembly, which .NET has too (AppDomain.DefineDynamicAssembly is .NET Framework only; S3a-8).
			return AssemblyBuilder.DefineDynamicAssembly(assemblyName,AssemblyBuilderAccess.Run,arrayAttributes);
		}

		///<summary>Removes // and /* */ comments (not inside string literals).</summary>
		private static string StripComments(string text) {
			return Regex.Replace(text,@"""(?:[^""\\]|\\.)*""|//[^\n]*|/\*.*?\*/",m => m.Value.StartsWith("/") ? "" : m.Value,RegexOptions.Singleline);
		}
		#endregion Helpers

		#region License list
		[TestMethod]
		public void LicenseList_ProgramLicenseIsFirst_GplV2OrLater() {
			List<LicenseEntry> listLicenseEntries=LicenseList.GetEntries();
			Assert.AreEqual(LicenseList.NameProgram,listLicenseEntries[0].Name);
			StringAssert.Contains(listLicenseEntries[0].Name,"GNU GPL v2 or later");
			string text=listLicenseEntries[0].Text;
			Assert.AreEqual(LicenseList.GetProgramLicenseText(),text,"FormLicense selects the first entry by its text.");
			StringAssert.Contains(text,AboutBoxInfo.Attribution);
			StringAssert.Contains(text,"either version 2 of the License, or (at your option) any later version");
			StringAssert.Contains(text,"GNU General Public License");
			StringAssert.Contains(text,"Version 2, June 1991","Resources/GPL.txt");
			StringAssert.Contains(text,"TERMS AND CONDITIONS FOR COPYING, DISTRIBUTION AND MODIFICATION");
			Assert.AreEqual(LicenseList.NameThirdPartyNotices,listLicenseEntries[1].Name);
		}

		[TestMethod]
		public void LicenseList_ListsEveryShippedComponentOnce_WithItsLicense() {
			List<LicenseEntry> listLicenseEntries=LicenseList.GetEntries();
			List<string> listNames=listLicenseEntries.Select(x => x.Name).ToList();
			Assert.AreEqual(listNames.Count,listNames.Distinct().Count(),"No name twice: "+string.Join(", ",listNames));
			CollectionAssert.AreEqual(_arrayShippedComponents,listNames.Skip(2).ToArray(),"One entry per component, in the notices file's order.");
			List<NoticeSection> listNoticeSections=LicenseList.ParseNotices(OpenPmsDental.Properties.Resources.ThirdPartyNotices);
			List<NoticeSection> listLicenseSections=listNoticeSections.FindAll(x => !x.IsComponent);
			Dictionary<string,string> dictVendorTexts=LicenseList.GetVendorTexts();
			foreach(NoticeSection noticeSection in listNoticeSections.FindAll(x => x.IsComponent)) {
				foreach(string field in new[] { "Files","Version","Copyright","License","Source" }) {
					Assert.AreNotEqual("",noticeSection.GetField(field),noticeSection.Title+": "+field);
				}
				string licenseText=LicenseList.GetLicenseText(noticeSection,listLicenseSections,dictVendorTexts);
				//Every component shows a license text, or names where the license is published.
				Assert.IsTrue(licenseText!="" || noticeSection.GetField("License").Contains("https://") || noticeSection.GetField("License")=="Public domain"
					|| noticeSection.Title=="Hunspell en_US dictionary",noticeSection.Title+" has no license text and no address.");
				LicenseEntry licenseEntry=listLicenseEntries.Single(x => x.Name==noticeSection.Title);
				//GetField joins a field's continuation lines with a space, while the entry keeps the notices file's line breaks: compare both unwrapped.
				StringAssert.Contains(Unwrap(licenseEntry.Text),noticeSection.GetField("Copyright"));
				if(licenseText!="") {
					StringAssert.Contains(Unwrap(licenseEntry.Text),Unwrap(licenseText));
				}
			}
			//Each project text kept from Open Dental's list belongs to a listed component.
			foreach(string name in dictVendorTexts.Keys) {
				CollectionAssert.Contains(_arrayShippedComponents,name);
				Assert.IsFalse(string.IsNullOrWhiteSpace(dictVendorTexts[name]),name);
			}
			StringAssert.Contains(listLicenseEntries.Single(x => x.Name=="MySqlConnector").Text,"Permission is hereby granted, free of charge");
			StringAssert.Contains(listLicenseEntries.Single(x => x.Name=="Google APIs Client Library for .NET").Text,"Version 2.0, January 2004");
			StringAssert.Contains(listLicenseEntries.Single(x => x.Name=="Direct Project").Text,"Redistribution and use in source and binary forms");
			//The spell checker's own licence file (license.txt of the package): Hunspell's tri-licence header with its contributors.
			string textWeCantSpell=listLicenseEntries.Single(x => x.Name=="WeCantSpell.Hunspell").Text;
			StringAssert.Contains(textWeCantSpell,"Version: MPL 1.1/GPL 2.0/LGPL 2.1");
			StringAssert.Contains(textWeCantSpell,"the GNU Lesser General Public License Version 2.1 or later");
			StringAssert.Contains(textWeCantSpell,"LICENSE-LGPL-2.1.txt");
			//Helix Toolkit (TC-4): its MIT text, and inside its own entry (no entry or resource may be named "BSD") the BSD notice of
			//LoopSubdivision that a binary redistribution must reproduce, and the origins of the SharpDX, SlimMath and MeshSimplification code.
			//No vendor text: one would replace the entry's MIT text (GetLicenseText).
			Assert.IsFalse(dictVendorTexts.ContainsKey("Helix Toolkit"));
			string textHelix=listLicenseEntries.Single(x => x.Name=="Helix Toolkit").Text;
			foreach(string expected in new[] { "Permission is hereby granted, free of charge","Copyright (c) 2009, Jesus Mena",
				"Redistributions in binary form must reproduce the above copyright","Copyright (c) 2010-2014 SharpDX - Alexandre Mutel",
				"Copyright (c) 2007-2011 SlimDX Group","Copyright (c) 2015-2019 Spacerat and contributors",
				"CanonicalSplineHelper.cs (c) 2009 by Charles Petzold" })
			{
				StringAssert.Contains(textHelix,expected);
			}
			StringAssert.Contains(listLicenseEntries.Single(x => x.Name==".NET Community Toolkit").Text,"Copyright (C) 2017 Pedro Lamas");
			//LOINC (EHR-6a, E-23 and E-37): the program's own files hold LOINC codes and display names, so LOINC's notice is listed, with its licence's
			//address and no licence text of the notices file.
			string textLoinc=listLicenseEntries.Single(x => x.Name=="LOINC").Text;
			StringAssert.Contains(textLoinc,"This material contains content from LOINC (http://loinc.org).");
			StringAssert.Contains(textLoinc,"https://loinc.org/license");
			//The licence's notice names the copyright years, 1995 to the year of the latest LOINC release.
			StringAssert.Matches(textLoinc,new System.Text.RegularExpressions.Regex(@"LOINC is copyright \(C\) 1995-20[2-9][0-9],\s+Regenstrief Institute"));
			//The notice wraps at its own column, so the trademark sentence may break anywhere between its words.
			StringAssert.Matches(textLoinc,new Regex(@"LOINC \(R\) is a registered\s+United States trademark of Regenstrief Institute"));
			Assert.IsFalse(textLoinc.Contains("Permission is hereby granted"),"No standard licence text under LOINC's notice.");
		}

		[TestMethod]
		public void LicenseList_DropsOpenDentalTermsAndWhatDoesNotShip() {
			List<LicenseEntry> listLicenseEntries=LicenseList.GetEntries();
			foreach(string name in _arrayRemovedEntries) {
				Assert.IsFalse(listLicenseEntries.Any(x => x.Name==name),"Still listed: "+name);
			}
			foreach(LicenseEntry licenseEntry in listLicenseEntries) {
				Assert.IsFalse(licenseEntry.Text.Contains("Registration Key"),licenseEntry.Name+": Open Dental's registration-key terms");
				Assert.IsFalse(licenseEntry.Text.Contains("Current Dental Terminology"),licenseEntry.Name+": the ADA CDT license");
				Assert.IsFalse(licenseEntry.Text.Contains("Drifty"),licenseEntry.Name);
			}
			//The resources are gone too, so nothing can show them again.
			List<string> listResourceNames=typeof(OpenPmsDental.Properties.Resources).GetProperties(BindingFlags.Public|BindingFlags.Static)
				.Select(x => x.Name).ToList();
			foreach(string resourceName in new[] { "OpenDentalLicense","OpenDentalApiEndUserLicense","CDT_Content_End_User_License",
				"CDT_Content_End_User_License1","Sparks3D","stdole","Ionic","Zxing","AForge","Angular","BouncyCastle","Bsd","Microsoft","Oracle",
				"Tao_Freeglut","Tamir","Mentalis" })
			{
				CollectionAssert.DoesNotContain(listResourceNames,resourceName);
			}
			foreach(string fileName in new[] { "opendentallicense.txt","opendentalapienduserlicense.txt","CDT Content End User License.txt",
				"CDT Content End User License.rtf","Sparks3D.txt","stdole.txt","Ionic.txt","Zxing.txt","Tamir.txt","Mentalis.txt" })
			{
				Assert.IsFalse(File.Exists(Path.Combine(RepoRoot(),"OpenPmsDental","Resources",fileName)),fileName);
			}
		}

		[TestMethod]
		public void LicenseForms_ShowTheSharedList() {
			string formLicense=StripComments(ReadRepoFile("OpenPmsDental","Forms","FormLicense.cs"));
			StringAssert.Contains(formLicense,"LicenseList.GetEntries()");
			StringAssert.Contains(formLicense,"_selectedLicense=LicenseList.GetProgramLicenseText();");
			string formRegistrationKey=StripComments(ReadRepoFile("OpenPmsDental","Forms","FormRegistrationKey.cs"));
			StringAssert.Contains(formRegistrationKey,"LicenseList.GetEntries()");
			StringAssert.Contains(formRegistrationKey,"listBoxRegistration.SetSelected(0);");
			foreach(string text in new[] { formLicense,formRegistrationKey }) {
				Assert.IsFalse(text.Contains("Properties.Resources.OpenDentalLicense"));
				Assert.IsFalse(text.Contains("Items.Add(\""),"No hand-made list left.");
			}
		}
		#endregion License list

		#region THIRD-PARTY-NOTICES.txt
		[TestMethod]
		public void ThirdPartyNotices_IsTheEmbeddedResource() {
			Assert.AreEqual(NormalizeNewLines(ReadRepoFile("THIRD-PARTY-NOTICES.txt")),NormalizeNewLines(OpenPmsDental.Properties.Resources.ThirdPartyNotices));
		}

		[TestMethod]
		public void ThirdPartyNotices_NamesEveryShippedFileAndSource() {
			List<NoticeSection> listNoticeSections=ParseNoticesFile();
			CollectionAssert.AreEqual(_arrayShippedComponents,listNoticeSections.FindAll(x => x.IsComponent).Select(x => x.Title).ToArray());
			string files=string.Join(" ",listNoticeSections.FindAll(x => x.IsComponent).Select(x => x.GetField("Files")));
			foreach(string fileName in _arrayShippedFiles) {
				Assert.IsTrue(Regex.IsMatch(files,@"(?<![\w.])"+Regex.Escape(fileName)+@"(?![\w.])"),"Not listed: "+fileName);
			}
			foreach(NoticeSection noticeSection in listNoticeSections.FindAll(x => x.IsComponent)) {
				string source=noticeSection.GetField("Source");
				Assert.IsTrue(source.Contains("https://") || source.Contains("this repository") || source=="not applicable",
					noticeSection.Title+": "+source);
			}
			CollectionAssert.AreEquivalent(new[] { "MIT License","BSD 3-Clause License","Apache License 2.0" },
				listNoticeSections.FindAll(x => !x.IsComponent).Select(x => x.Title).ToArray());
		}

		[TestMethod]
		public void ThirdPartyNotices_MarksTheGplQuestions() {
			List<NoticeSection> listNoticeSections=ParseNoticesFile();
			foreach(string title in new[] { "Google APIs Client Library for .NET" }) {
				StringAssert.Contains(listNoticeSections.Single(x => x.Title==title).GetField("Note"),"Owner question",title);
			}
			//fo-dicom (MS-PL) was the second; the in-house DICOM reader replaced it (docs/specs/dicom-reader.md). NDde (its own shared source
			//licence) was the third; it was removed on 2026-11-07, and the DentX, Dexis Integrator and Vipersoft program links that used it send
			//their DDE commands through the in-house CodeBase.ODDdeClient since 2026-11-08 (docs/port-notes/S0-R.md).
			Assert.IsFalse(listNoticeSections.Any(x => x.Title=="fo-dicom"));
			Assert.IsFalse(listNoticeSections.Any(x => x.Title=="NDde"));
		}

		[TestMethod]
		public void ThirdPartyNotices_ListsNothingThatMustNotShip() {
			string notices=ReadRepoFile("THIRD-PARTY-NOTICES.txt");
			foreach(string name in _arrayNotShipped) {
				Assert.IsFalse(notices.IndexOf(name,StringComparison.OrdinalIgnoreCase)>=0,"Listed although it does not ship: "+name);
			}
			Assert.IsFalse(Regex.IsMatch(notices,@"\bD\d{4}\b"),"No CDT code.");
		}
		#endregion THIRD-PARTY-NOTICES.txt

		#region Product name at display time
		[TestMethod]
		public void DisplayName_ShowsTheProductInPlaceOfOpenDentalsDefaults() {
			foreach(string value in new[] { "Open Dental","Open Dental Software"," open dental ","OPEN DENTAL SOFTWARE",
				"Benco Practice Management powered by Open Dental","Benco Practice Management" })
			{
				Assert.IsTrue(AboutBoxInfo.IsDefaultProductName(value),value);
				Assert.AreEqual("Open PMS - Dental",AboutBoxInfo.GetDisplayName(value),value);
			}
		}

		[TestMethod]
		public void DisplayName_KeepsAnOfficesOwnTitle() {
			foreach(string value in new[] { "Smile Dental - Front Desk","Open Dental Clinic North","","Open Dental 2" }) {
				Assert.IsFalse(AboutBoxInfo.IsDefaultProductName(value),value);
				Assert.AreEqual(value,AboutBoxInfo.GetDisplayName(value));
			}
			Assert.AreEqual("",AboutBoxInfo.GetDisplayName(null));
			Assert.IsFalse(AboutBoxInfo.IsDefaultProductName(null));
		}

		[TestMethod]
		public void DisplayName_IsUsedForTheTitleAndTheRestartPrompts() {
			string patientL=StripComments(ReadRepoFile("OpenPmsDental","Logic","PatientL.cs"));
			Assert.IsFalse(patientL.Contains("PrefName.MainWindowTitle"),"PatientL reads the title through AboutBoxInfo.");
			Assert.AreEqual(2,Regex.Matches(patientL,@"string retVal=AboutBoxInfo\.GetMainWindowTitle\(\);").Count,"GetMainTitle and GetMainTitleSamePat.");
			string formOpenDental=StripComments(ReadRepoFile("OpenPmsDental","Main Modules","FormOpenDental.cs"));
			Assert.IsFalse(formOpenDental.Contains("PrefName.SoftwareName"));
			Assert.AreEqual(3,Regex.Matches(formOpenDental,@"Lan\.g\(this,""You will have to restart""\)\+"" ""\+AboutBoxInfo\.GetSoftwareName\(\)").Count);
			string formUpdateInProgress=StripComments(ReadRepoFile("OpenPmsDental","Forms","FormUpdateInProgress.cs"));
			Assert.IsFalse(formUpdateInProgress.Contains("PrefName.SoftwareName"));
			StringAssert.Contains(formUpdateInProgress,"AboutBoxInfo.GetSoftwareName()");
			//The preferences are only read, never written, so adoption's row check is unaffected.
			string formAbout=StripComments(ReadRepoFile("OpenPmsDental","Forms","FormAbout.cs"));
			Assert.IsFalse(Regex.IsMatch(formAbout,@"Prefs\.Update"),"FormAbout.cs writes no preference.");
		}

		[TestMethod]
		public void AboutBox_CopyrightLineIsTheProgramsWhateverSoftwareNameSays() {
			string formAbout=StripComments(ReadRepoFile("OpenPmsDental","Forms","FormAbout.cs"));
			StringAssert.Contains(formAbout,
				"labelCopyright.Text=Lan.g(this,\"Copyright 2026-\")+DateTime.Now.ToString(\"yyyy\")+\", Open PMS - Dental contributors\";");
			StringAssert.Contains(formAbout,"labelAttribution.Text=AboutBoxInfo.Attribution;");
			int indexLoad=formAbout.IndexOf("private void FormAbout_Load(");
			int indexNext=formAbout.IndexOf("private void butDiagnostics_Click(");
			Assert.IsTrue(indexLoad>=0 && indexNext>indexLoad);
			Assert.IsFalse(formAbout.Substring(indexLoad,indexNext-indexLoad).Contains("SoftwareName"),"The copyright line is not built from SoftwareName.");
		}

		[TestMethod]
		public void UpdateHistory_IsLookedUpByReleaseThenCompatibilityVersion() {
			CollectionAssert.AreEqual(new[] { "0.0","24.3.63.0" },AboutBoxInfo.GetUpdateHistoryVersions(MakeAssembly(new Version(24,3,63,0),"0.0")));
			CollectionAssert.AreEqual(new[] { "24.3.63.0" },AboutBoxInfo.GetUpdateHistoryVersions(MakeAssembly(new Version(24,3,63,0),null)));
			StringAssert.Contains(ReadRepoFile("OpenPmsDental","Forms","FormAbout.cs"),"AboutBoxInfo.GetUpdateHistory(assemblyProgram)");
			StringAssert.Contains(ReadRepoFile("OpenPmsDental","Forms","FormUpdate.cs"),"AboutBoxInfo.GetUpdateHistory(assemblyProgram)");
		}

		[TestMethod]
		public void BencoSetup_NoLongerSwitchesBranding() {
			string frmBencoSetup=StripComments(ReadRepoFile("WpfControlsOD","Frms","FrmBencoSetup.xaml.cs"));
			Assert.IsFalse(frmBencoSetup.Contains("UpdateBencoSettings"));
			Assert.IsFalse(frmBencoSetup.Contains("PrefName.MainWindowTitle"));
			Assert.IsFalse(frmBencoSetup.Contains("PrefName.SoftwareName"));
			Assert.IsFalse(frmBencoSetup.Contains("Prefs.Update"));
			//The program link itself stays.
			StringAssert.Contains(frmBencoSetup,"Programs.Update(_program);");
			StringAssert.Contains(frmBencoSetup,"ToolButItems.Insert(newBut);");
		}
		#endregion Product name at display time

		#region README.md and NOTICE
		[TestMethod]
		public void Readme_SaysGplV2OrLater_AndHasNoStaleBuildStatements() {
			string readme=ReadRepoFile("README.md");
			StringAssert.Contains(readme,"GNU GPL v2 or later");
			StringAssert.Contains(readme,"either version 2 of the License, or (at your option) any later version");
			StringAssert.Contains(readme,"(THIRD-PARTY-NOTICES.txt)");
			Assert.IsFalse(readme.Contains("Prebuilt DLLs"),"No prebuilt Open Dental DLLs remain.");
			Assert.IsFalse(Regex.IsMatch(readme,"Open Dental.{1,3}internal prebuilt"),"Required dlls holds no Open Dental-internal assemblies.");
			Assert.IsFalse(readme.Contains("21 projects"));
			Assert.IsFalse(readme.Contains("merge it from its version branch"),"We don't merge from upstream (decoupling plan).");
			Assert.IsFalse(readme.Contains("Developer tools, not in the solution"),"tools/ and tests/ are in the solution filter.");
			StringAssert.Contains(readme,"every test project under `tests/`");
		}

		[TestMethod]
		public void InstallingDoc_HasTheOfficeInstall() {
			//The office install moved from README.md to docs/admin/installing.md (2026-11-21, the shorter README); the README links to it.
			StringAssert.Contains(ReadRepoFile("README.md"),"(docs/admin/installing.md)");
			string readme=ReadRepoFile("docs","admin","installing.md");
			foreach(string text in new[] {
				"# Installing in an office","MariaDB Server","**10.11**","sql_mode=","default-storage-engine=MyISAM","max_allowed_packet=40M",
				"at least 40 MB","Windows 11, 64-bit","No .NET to install","Visual C++ 2015-2022 Redistributable, x64","vc_redist.x64.exe",
				"(../../db/README.md)","db/init/new-database.sql","(../develop/release-process.md)","C:\\Program Files\\Open PMS - Dental\\","TWAINDSM.dll",
				"**Release | x64**","(adoption-runbook.md)" })
			{
				StringAssert.Contains(readme,text);
			}
			//Version 0.0, the .NET Framework 4.8 x86 build, keeps its own paragraph: its prerequisites and install folder are named only there.
			string paragraphV00=readme.Split(new[] { "\r\n","\n" },StringSplitOptions.None).Single(x => x.StartsWith("Version 0.0, the .NET Framework 4.8 build"));
			foreach(string text in new[] {
				".NET Framework 4.8","Visual C++ 2015-2022 Redistributable, x86","vc_redist.x86.exe","WebView2 Runtime, x86",
				"C:\\Program Files (x86)\\Open PMS - Dental\\" })
			{
				StringAssert.Contains(paragraphV00,text);
			}
		}

		[TestMethod]
		public void Notice_SaysGplV2OrLater_AndPointsToTheThirdPartyNotices() {
			string notice=Unwrap(ReadRepoFile("NOTICE"));
			StringAssert.Contains(notice,"GNU General Public License, version 2 or (at your option) any later version");
			StringAssert.Contains(notice,"THIRD-PARTY-NOTICES.txt");
			StringAssert.Contains(notice,"Help > About > View Licenses");
			Assert.IsTrue(File.Exists(Path.Combine(RepoRoot(),"THIRD-PARTY-NOTICES.txt")));
		}
		#endregion README.md and NOTICE
	}
}
