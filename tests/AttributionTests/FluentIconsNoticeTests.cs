// Tests for the notices of Fluent UI System Icons (package UI-7a, docs/port-notes/UI-7a.md; docs/specs/ui-refresh-plan.md, section 4.1): the
// vendored MIT licence and SOURCE.txt, the entry in THIRD-PARTY-NOTICES.txt with the version the icon table was made from, and the entry of the
// license list that the About box shows (Help, About, License).
// Licensed under the GNU General Public License, version 2 or later. See LICENSE in the repository root.
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using CodeBase;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using OpenPmsDental;

namespace AttributionTests {
	[TestClass]
	public class FluentIconsNoticeTests {
		private const string COMPONENT="Fluent UI System Icons";
		private const string COPYRIGHT="Copyright (c) 2020 Microsoft Corporation";

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

		[TestMethod]
		public void VendoredLicense_IsTheMitTextOfMicrosoft() {
			string license=ReadRepoFile("third-party","fluentui-system-icons","LICENSE").Replace("\r\n","\n");
			Assert.IsTrue(license.StartsWith("MIT License\n\n"+COPYRIGHT+"\n\n"),license);
			StringAssert.Contains(license,"Permission is hereby granted, free of charge, to any person obtaining a copy");
			StringAssert.Contains(license,"The above copyright notice and this permission notice shall be included in all");
			StringAssert.Contains(license,"THE SOFTWARE IS PROVIDED \"AS IS\", WITHOUT WARRANTY OF ANY KIND");
		}

		[TestMethod]
		public void VendoredSource_NamesTheVersionDownloadAndCommit() {
			string source=ReadRepoFile("third-party","fluentui-system-icons","SOURCE.txt");
			StringAssert.Contains(source,"Package:   @fluentui/svg-icons "+IconData.FLUENT_VERSION+",");
			StringAssert.Contains(source,"https://registry.npmjs.org/@fluentui/svg-icons/-/svg-icons-"+IconData.FLUENT_VERSION+".tgz");
			StringAssert.Contains(source,"https://github.com/microsoft/fluentui-system-icons");
			StringAssert.Contains(source,COPYRIGHT);
			Assert.IsTrue(Regex.IsMatch(source,@"Integrity: sha512-[A-Za-z0-9+/]{86}=="),"The package's integrity hash.");
		}

		[TestMethod]
		public void ThirdPartyNotices_NamesFluentWithTheTablesVersion() {
			List<NoticeSection> listNoticeSections=LicenseList.ParseNotices(ReadRepoFile("THIRD-PARTY-NOTICES.txt"));
			NoticeSection noticeSection=listNoticeSections.SingleOrDefault(x => x.IsComponent && x.Title==COMPONENT);
			Assert.IsNotNull(noticeSection,"THIRD-PARTY-NOTICES.txt has no component "+COMPONENT+".");
			Assert.IsTrue(noticeSection.GetField("Version").StartsWith(IconData.FLUENT_VERSION+" "),noticeSection.GetField("Version"));
			Assert.AreEqual(COPYRIGHT,noticeSection.GetField("Copyright"));
			Assert.AreEqual("MIT License",noticeSection.GetField("License"));
			StringAssert.Contains(noticeSection.GetField("Source"),"https://github.com/microsoft/fluentui-system-icons");
			StringAssert.Contains(noticeSection.GetField("Source"),"third-party/fluentui-system-icons");
			StringAssert.Contains(noticeSection.GetField("Files"),"CodeBase.dll");
			//The manifest and the generated table name the same version.
			StringAssert.Contains(ReadRepoFile("icons","manifest.json"),"\"fluentVersion\": \""+IconData.FLUENT_VERSION+"\"");
		}

		[TestMethod]
		public void LicenseList_TheAboutBoxShowsFluentWithItsVersionAndTheMitText() {
			List<LicenseEntry> listLicenseEntries=LicenseList.GetEntries();
			LicenseEntry licenseEntry=listLicenseEntries.SingleOrDefault(x => x.Name==COMPONENT);
			Assert.IsNotNull(licenseEntry,"The license list (Help, About, License) has no entry "+COMPONENT+".");
			StringAssert.Contains(licenseEntry.Text,"Version:   "+IconData.FLUENT_VERSION);
			StringAssert.Contains(licenseEntry.Text,COPYRIGHT);
			StringAssert.Contains(licenseEntry.Text,"MIT License");
			StringAssert.Contains(licenseEntry.Text,"Permission is hereby granted, free of charge");
		}
	}
}
