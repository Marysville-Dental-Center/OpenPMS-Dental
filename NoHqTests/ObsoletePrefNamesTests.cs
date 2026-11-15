// S3a-9 (iv) of the .NET 10 plan (docs/specs/net10-plan.md; docs/port-notes/S3a-9.md, section 4). On .NET 10, XmlSerializer throws while it
// builds a serializer for a type with a member marked [Obsolete(IsError=true)], enum constants included, and PrefName travels through the Middle
// Tier (Meth calls with a PrefName argument, ClinicPref.PrefName). The 25 PrefName members that were [Obsolete(...,true)] are therefore
// [Obsolete(...,false)] since S3a-9, which loses the compile error that kept code from naming them. These tests put the rule back: no PrefName
// member is obsolete as an error (so a newly ported upstream preference marked that way fails here, on .NET Framework, and not only on .NET 10),
// and no code outside Pref.cs names one of the 25 as PrefName.<name> (comments and strings, such as the convert scripts' SQL, may).
// Licensed under the GNU General Public License, version 2 or later. See LICENSE in the repository root.
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using OpenPmsDentalBusiness;

namespace NoHqTests {
	[TestClass]
	public class ObsoletePrefNamesTests {
		///<summary>The PrefName members that were [Obsolete("...",true)] until S3a-9, in the order of Pref.cs.</summary>
		private static readonly string[] _arrayNamesFormerErrors=new string[] {
			"AccountShowTrojanExpressCollect","AgingNegativeAdjsByAdjDate","ApptModuleUses2019Overhaul","CloudAllowedIpAddresses","EConnectorEnabled",
			"EmailInboxComputerName","InsBenBWCodes","InsBenExamCodes","InsBenPanoCodes","InsBenCancerScreeningCodes","InsBenProphyCodes",
			"InsBenFlourideCodes","InsBenSealantCodes","InsBenCrownCodes","InsBenSRPCodes","InsBenFullDebridementCodes","InsBenPerioMaintCodes",
			"InsBenDenturesCodes","InsBenImplantCodes","StatementShowCreditCard","StatementsUseSheets","TrojanExpressCollectBillingType",
			"TrojanExpressCollectPassword","TrojanExpressCollectPath","TrojanExpressCollectPreviousFileNumber",
		};

		///<summary>Folders that hold no code of this repository, or only build output: skipped wherever they are.</summary>
		private static readonly string[] _arrayFoldersSkipped=new string[] { "bin","obj","packages","third-party","node_modules","TestResults" };

		private static readonly Regex _regexFormerError=new Regex(@"\bPrefName\s*\.\s*("+string.Join("|",_arrayNamesFormerErrors)+@")\b");

		[TestMethod]
		public void PrefName_NoMemberIsObsoleteAsAnError() {
			List<string> listErrors=new List<string>();
			foreach(FieldInfo fieldInfo in typeof(PrefName).GetFields(BindingFlags.Public|BindingFlags.Static)) {
				ObsoleteAttribute obsoleteAttribute=fieldInfo.GetCustomAttribute<ObsoleteAttribute>();
				if(obsoleteAttribute!=null && obsoleteAttribute.IsError) {
					listErrors.Add(fieldInfo.Name);
				}
			}
			Assert.IsTrue(typeof(PrefName).GetFields(BindingFlags.Public|BindingFlags.Static).Length>1000,"PrefName is the whole enum.");
			Assert.AreEqual(0,listErrors.Count,"PrefName members marked [Obsolete(...,true)], which .NET 10's XmlSerializer refuses (use false): "
				+string.Join(", ",listErrors));
		}

		[TestMethod]
		public void PrefName_TheFormerErrors_AreStillThere_AndObsolete() {
			Assert.AreEqual(25,_arrayNamesFormerErrors.Distinct().Count());
			for(int i=0;i<_arrayNamesFormerErrors.Length;i++) {
				FieldInfo fieldInfo=typeof(PrefName).GetField(_arrayNamesFormerErrors[i],BindingFlags.Public|BindingFlags.Static);
				Assert.IsNotNull(fieldInfo,"PrefName."+_arrayNamesFormerErrors[i]+" is gone: take it out of this list.");
				ObsoleteAttribute obsoleteAttribute=fieldInfo.GetCustomAttribute<ObsoleteAttribute>();
				Assert.IsNotNull(obsoleteAttribute,"PrefName."+_arrayNamesFormerErrors[i]+" is no longer obsolete.");
				Assert.IsFalse(obsoleteAttribute.IsError,"PrefName."+_arrayNamesFormerErrors[i]);
			}
		}

		[TestMethod]
		public void PrefName_TheFormerErrors_AreNamedByNoCodeOutsidePrefCs() {
			string pathRoot=RemovedAssembliesTests.GetRepositoryRoot();
			string pathPref=Path.GetFullPath(Path.Combine(pathRoot,"OpenPmsDentalBusiness","TableTypes","Pref.cs"));
			Assert.IsTrue(File.Exists(pathPref),pathPref);
			List<string> listHits=new List<string>();
			int countFiles=0;
			foreach(string path in GetSourceFiles(pathRoot)) {
				if(string.Equals(Path.GetFullPath(path),pathPref,StringComparison.OrdinalIgnoreCase)) {
					continue;
				}
				countFiles++;
				string text=File.ReadAllText(path);
				if(text.IndexOf("PrefName",StringComparison.Ordinal)<0) {
					continue;
				}
				foreach(string hit in FindFormerErrors(StripCommentsAndStrings(text))) {
					listHits.Add(path.Substring(pathRoot.Length).TrimStart('\\','/')+": "+hit);
				}
			}
			Assert.IsTrue(countFiles>3000,"Only "+countFiles+" C# files scanned.");
			Assert.AreEqual(0,listHits.Count,"Code names a deprecated preference that was [Obsolete(...,true)] before S3a-9 (it is a warning now, "
				+"no longer an error):\r\n"+string.Join("\r\n",listHits));
		}

		///<summary>The scanner itself: code that names a former error is found (also fully qualified, with spaces, and inside an interpolation
		///hole); comments, strings, verbatim strings, characters and the text of interpolated strings are not.</summary>
		[TestMethod]
		public void Scanner_FindsCode_NotCommentsOrStrings() {
			string text="bool a=PrefC.GetBool(PrefName.StatementsUseSheets);\n"
				+"string b=OpenPmsDentalBusiness.PrefName . InsBenBWCodes.ToString();\n"
				+"string c=$\"{PrefName.EConnectorEnabled} and {{PrefName.InsBenExamCodes}}\";\n"
				+"//PrefName.TrojanExpressCollectPath in a comment\n"
				+"/* PrefName.TrojanExpressCollectPassword\n in a block comment */\n"
				+"string d=\"UPDATE preference SET ValueString='' WHERE PrefName='InsBenPanoCodes' -- PrefName.InsBenPanoCodes\";\n"
				+"string e=@\"PrefName.InsBenCrownCodes \"\" PrefName.InsBenSRPCodes\";\n"
				+"char f='\"'; string g=\"\\\"PrefName.InsBenDenturesCodes\\\"\";\n"
				+"string h=$@\"{x} PrefName.InsBenImplantCodes {PrefName.CloudAllowedIpAddresses}\";\n"
				+"PrefName.InsBenBWCodesMore x=PrefName.StatementShowCreditCardX;\n";
			List<string> listHits=FindFormerErrors(StripCommentsAndStrings(text));
			CollectionAssert.AreEqual(new[] { "line 1: PrefName.StatementsUseSheets","line 2: PrefName . InsBenBWCodes","line 3: PrefName.EConnectorEnabled",
				"line 10: PrefName.CloudAllowedIpAddresses" },listHits,string.Join(" | ",listHits));
		}

		///<summary>Every .cs file under the root, without build output, packages, third-party sources and folders whose name starts with a dot
		///(.git, .vs, .claude).</summary>
		private static List<string> GetSourceFiles(string pathRoot) {
			List<string> listFiles=new List<string>();
			Stack<string> stackFolders=new Stack<string>();
			stackFolders.Push(pathRoot);
			while(stackFolders.Count>0) {
				string folder=stackFolders.Pop();
				listFiles.AddRange(Directory.GetFiles(folder,"*.cs"));
				foreach(string folderSub in Directory.GetDirectories(folder)) {
					string name=Path.GetFileName(folderSub);
					if(name.StartsWith(".") || _arrayFoldersSkipped.Contains(name,StringComparer.OrdinalIgnoreCase)) {
						continue;
					}
					stackFolders.Push(folderSub);
				}
			}
			return listFiles;
		}

		///<summary>"line N: PrefName.Name" for each former error named in the code.</summary>
		private static List<string> FindFormerErrors(string code) {
			List<string> listHits=new List<string>();
			foreach(Match match in _regexFormerError.Matches(code)) {
				int line=1+code.Take(match.Index).Count(x => x=='\n');
				listHits.Add("line "+line+": "+match.Value);
			}
			return listHits;
		}

		///<summary>The C# text with every comment, string and character literal blanked out (line ends kept); the code in the holes of an
		///interpolated string stays.</summary>
		internal static string StripCommentsAndStrings(string text) {
			StringBuilder stringBuilder=new StringBuilder(text.Length);
			int i=0;
			StripCode(text,ref i,stringBuilder,false);
			return stringBuilder.ToString();
		}

		///<summary>Copies code from i on. In an interpolation hole (isHole), stops before the '}' that closes it.</summary>
		private static void StripCode(string text,ref int i,StringBuilder stringBuilder,bool isHole) {
			int depthBraces=0;
			while(i<text.Length) {
				char c=text[i];
				char next=i+1<text.Length ? text[i+1] : '\0';
				if(c=='/' && next=='/') {
					while(i<text.Length && text[i]!='\n') {
						Blank(text,ref i,stringBuilder);
					}
					continue;
				}
				if(c=='/' && next=='*') {
					Blank(text,ref i,stringBuilder);
					Blank(text,ref i,stringBuilder);
					while(i<text.Length && !(text[i]=='*' && i+1<text.Length && text[i+1]=='/')) {
						Blank(text,ref i,stringBuilder);
					}
					if(i<text.Length) {
						Blank(text,ref i,stringBuilder);
						Blank(text,ref i,stringBuilder);
					}
					continue;
				}
				if(c=='$' || c=='@') {
					bool isInterpolated=c=='$' || next=='$';
					bool isVerbatim=c=='@' || next=='@';
					int lengthPrefix=(next=='$' || next=='@') ? 2 : 1;
					if(i+lengthPrefix<text.Length && text[i+lengthPrefix]=='"' && (lengthPrefix==2 || next=='"')) {
						for(int j=0;j<lengthPrefix;j++) {
							Blank(text,ref i,stringBuilder);
						}
						StripString(text,ref i,stringBuilder,isVerbatim,isInterpolated);
						continue;
					}
				}
				if(c=='"') {
					StripString(text,ref i,stringBuilder,false,false);
					continue;
				}
				if(c=='\'') {
					Blank(text,ref i,stringBuilder);
					while(i<text.Length && text[i]!='\'' && text[i]!='\n') {
						if(text[i]=='\\') {
							Blank(text,ref i,stringBuilder);
						}
						if(i<text.Length) {
							Blank(text,ref i,stringBuilder);
						}
					}
					if(i<text.Length && text[i]=='\'') {
						Blank(text,ref i,stringBuilder);
					}
					continue;
				}
				if(isHole) {
					if(c=='{') {
						depthBraces++;
					}
					else if(c=='}') {
						if(depthBraces==0) {
							return;
						}
						depthBraces--;
					}
				}
				stringBuilder.Append(c);
				i++;
			}
		}

		///<summary>Blanks a string literal that starts with the '"' at i, up to and with its closing '"'. The code in the holes of an
		///interpolated string is copied (StripCode).</summary>
		private static void StripString(string text,ref int i,StringBuilder stringBuilder,bool isVerbatim,bool isInterpolated) {
			Blank(text,ref i,stringBuilder);//the opening quote
			while(i<text.Length) {
				char c=text[i];
				char next=i+1<text.Length ? text[i+1] : '\0';
				if(!isVerbatim && c=='\\') {
					Blank(text,ref i,stringBuilder);
					if(i<text.Length) {
						Blank(text,ref i,stringBuilder);
					}
					continue;
				}
				if(c=='"') {
					if(isVerbatim && next=='"') {
						Blank(text,ref i,stringBuilder);
						Blank(text,ref i,stringBuilder);
						continue;
					}
					Blank(text,ref i,stringBuilder);//the closing quote
					return;
				}
				if(!isVerbatim && c=='\n') {
					return;//An unterminated string ends with its line.
				}
				if(isInterpolated && c=='{') {
					if(next=='{') {
						Blank(text,ref i,stringBuilder);
						Blank(text,ref i,stringBuilder);
						continue;
					}
					Blank(text,ref i,stringBuilder);
					StripCode(text,ref i,stringBuilder,true);
					if(i<text.Length) {
						Blank(text,ref i,stringBuilder);//the closing brace
					}
					continue;
				}
				Blank(text,ref i,stringBuilder);
			}
		}

		///<summary>Appends a space for the character at i (a line end as it is) and moves on.</summary>
		private static void Blank(string text,ref int i,StringBuilder stringBuilder) {
			stringBuilder.Append(text[i]=='\n' || text[i]=='\r' ? text[i] : ' ');
			i++;
		}
	}
}
