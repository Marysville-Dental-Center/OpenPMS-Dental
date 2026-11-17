// The guard of S3b-FMT (decision 24 of docs/specs/net10-plan.md, docs/port-notes/S3b-FMT.md): tools\FormatSweep's scan of the shipped
// projects, with the compiler's types, finds no place that writes a double or float as text, or adds a fractional time to a DateTime or
// TimeSpan, without the helpers, other than the allowed ones (tools\FormatSweep\allowed-sites.txt, each with its reason). It reads the compile
// inputs the last lane build recorded (Directory.Build.targets, ODRecordCompileInputs).
// Licensed under the GNU General Public License, version 2 or later. See LICENSE in the repository root.
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using FormatSweep;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using ProjectFileTests;

namespace FormatParityTests {
	[TestClass]
	public class ScanGuardTests {
		///<summary>The repository root: the first folder above the test assembly that holds LICENSE and NOTICE.</summary>
		private static string RepoRoot() {
			DirectoryInfo directoryInfo=new DirectoryInfo(AppContext.BaseDirectory);
			while(directoryInfo!=null) {
				if(File.Exists(Path.Combine(directoryInfo.FullName,"LICENSE")) && File.Exists(Path.Combine(directoryInfo.FullName,"NOTICE"))) {
					return directoryInfo.FullName;
				}
				directoryInfo=directoryInfo.Parent;
			}
			Assert.Fail("Repository root not found above "+AppContext.BaseDirectory);
			return null;
		}

		[TestMethod]
		public void ShippedProjects_HaveNoUnroutedNumberTextOrFractionalTime() {
			string root=RepoRoot();
			string folderTool=Path.Combine(root,"tools","FormatSweep");
			List<string> listProjects=Scanner.ReadProjectList(Path.Combine(folderTool,"shipped-projects.txt"));
			Assert.IsTrue(listProjects.Count>=20,"shipped projects: "+listProjects.Count);
			ScanResult scanResult=Scanner.Scan(root,listProjects,null);
			if(scanResult.ListProjectsMissing.Count>0) {
				BuildOutput.NotBuilt("No compile inputs for "+string.Join(", ",scanResult.ListProjectsMissing)
					+": build OpenDental.Local.slnf through the lane first (Directory.Build.targets, ODRecordCompileInputs).");
			}
			List<AllowedSite> listAllowed=AllowList.Read(Path.Combine(folderTool,"allowed-sites.txt"));
			List<Site> listUnrouted=AllowList.GetUnrouted(scanResult.ListSites,listAllowed);
			List<AllowedSite> listUnused=listAllowed.Where(x => x.CountMatched==0).ToList();
			Assert.AreEqual(0,listUnrouted.Count,listUnrouted.Count+" places write a double or float as text, or add a fractional time, without the "
				+"helpers (route them with tools\\FormatSweep --rewrite, or by hand, or list one with its reason in allowed-sites.txt):\r\n"
				+string.Join("\r\n",listUnrouted.Take(60).Select(x => x.ToString())));
			Assert.AreEqual(0,listUnused.Count,"allowed-sites.txt lines that match nothing:\r\n"
				+string.Join("\r\n",listUnused.Select(x => x.RelativePath+" | "+x.Category+" | "+x.Code)));
		}

		[TestMethod]
		public void Scan_FindsEachKindOfPlace_InAFixture() {
			//The finder on a small compilation of its own: every kind it routes, and the ones it leaves (integral, decimal, routed already).
			string source=@"
using System;
using System.Text;
using System.Globalization;
using CodeBase;
static class Fixture {
	static void M(double d,float f,double? n,int i,decimal m,DateTime dt,StringBuilder sb) {
		string s1=d.ToString(""F2"");
		string s2=Convert.ToString(d,CultureInfo.InvariantCulture);
		string s3=""a""+d+i+m;
		string s4=$""{d:c} {f} {i}"";
		string s5=string.Format(""{0:N2} {1}"",d,i);
		sb.Append(f);
		DateTime dt2=dt.AddHours(d);
		DateTime dt3=dt.AddHours(i);
		DateTime dt4=dt.AddMinutes(0.5);
		DateTime dt5=dt.AddMilliseconds(1/3.0);
		TimeSpan ts=TimeSpan.FromHours(d);
		string s6=n.ToString();
		string s7=d.ToStringNetFx(""F2"");
		string s8=string.Join("","",new double[] { d });
		s1+=d;
		object o=d;
		Console.WriteLine(o);
		string s9=""pi ""+Math.PI+"" half ""+0.5;
	}
}";
			List<Site> listSites=FixtureScan.Run(source);
			string report=string.Join("\r\n",listSites.Select(x => x.Category+": "+x.Code));
			Assert.AreEqual(1,listSites.Count(x => x.Category==Categories.TO_STRING && x.Code=="d.ToString(\"F2\")"),report);
			Assert.AreEqual(1,listSites.Count(x => x.Category==Categories.TO_STRING && x.Code=="n.ToString()"),report);
			Assert.AreEqual(1,listSites.Count(x => x.Category==Categories.CONVERT_TO_STRING),report);
			Assert.AreEqual(3,listSites.Count(x => x.Category==Categories.CONCAT),"\"a\"+d, s1+=d and Math.PI (not the int, the decimal or 0.5):\r\n"+report);
			Assert.AreEqual(2,listSites.Count(x => x.Category==Categories.INTERPOLATION),report);
			Assert.AreEqual(1,listSites.Count(x => x.Category==Categories.COMPOSITE && x.Format=="N2"),report);
			Assert.AreEqual(1,listSites.Count(x => x.Category==Categories.APPEND),report);
			Assert.AreEqual(2,listSites.Count(x => x.Category==Categories.DATETIME_ADD),"AddHours(d) and AddMilliseconds(1/3.0), not AddHours(i) or AddMinutes(0.5):\r\n"+report);
			Assert.AreEqual(1,listSites.Count(x => x.Category==Categories.TIMESPAN_FROM),report);
			Assert.AreEqual(1,listSites.Count(x => x.Category==Categories.JOIN),report);
			Assert.IsFalse(listSites.Any(x => x.Code.Contains("ToStringNetFx")),"A routed place is not found again.\r\n"+report);
		}

		[TestMethod]
		public void Rewrite_RoutesEachKind_AndTheResultCompilesAndIsFoundNoMore() {
			string source=@"using System;
using System.Text;
static class Fixture {
	static string M(double d,float f,double? n,DateTime dt,StringBuilder sb) {
		string s=""a""+d+(d*2)+$""{d,5:F2}{f}""+string.Format(""{0:c}"",-d)+Convert.ToString(d)+n.ToString()+d.ToString(""N2"");
		sb.Append(d+1);
		s+=f;
		return s+dt.AddHours(d/3).AddMinutes(d)+TimeSpan.FromSeconds(d);
	}
}";
			string rewritten=FixtureScan.Rewrite(source);
			StringAssert.Contains(rewritten,"using CodeBase;");
			StringAssert.Contains(rewritten,"\"a\"+d.ToStringNetFx()+(d*2).ToStringNetFx()");
			StringAssert.Contains(rewritten,"{d.AsNetFx(),5:F2}{f.AsNetFx()}");
			StringAssert.Contains(rewritten,"string.Format(\"{0:c}\",(-d).AsNetFx())");
			StringAssert.Contains(rewritten,"d.ToStringNetFx()+n.ToStringNetFx()+d.ToStringNetFx(\"N2\")");
			StringAssert.Contains(rewritten,"sb.Append((d+1).ToStringNetFx());");
			StringAssert.Contains(rewritten,"s+=f.ToStringNetFx();");
			StringAssert.Contains(rewritten,"dt.AddHoursNetFx(d/3).AddMinutesNetFx(d)+ODTimeSpan.FromSeconds(d)");
			Assert.AreEqual(0,FixtureScan.Run(rewritten).Count,"Nothing is found in the rewritten code:\r\n"+rewritten);
			Assert.AreEqual(0,FixtureScan.CountErrors(rewritten),"The rewritten code compiles:\r\n"+rewritten);
		}
	}
}
