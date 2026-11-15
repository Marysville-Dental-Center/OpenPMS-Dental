// The record of the client certificate this tool imported itself, so that it removes only that one when the settings stop using it.
// Licensed under the GNU General Public License, version 2 or later. See LICENSE in the repository root.
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using CodeBase;
using DataConnectionBase;

namespace OpenPmsDentalDbSettings {
	///<summary>What the tool's record says about the client certificate it imported (ImportedCertificateRecord), and what an import returns for
	///it (ClientCertificateStore.ImportPfx): its thumbprint, the private key the tool's import gave its store entry (KeyTool), and the key
	///provider information the entry had before, when the import left an earlier key of the entry aside (KeyBefore; DIRECT-CERT-FOLLOWUP
	///round 4, part B: the tool puts it back when it undoes or later removes the import).</summary>
	public sealed class ImportedCertificate {
		///<summary>The certificate's thumbprint, upper-case hex.</summary>
		public string Thumbprint="";
		///<summary>The key the tool's import gave the store's entry: its container, provider and type, key set (in Flags) and key spec. Null when
		///the record names no key: a record in the old format (a thumbprint line only, before round 4), or one written when the entry's key
		///could not be read. With a key named, the tool claims the entry's key as its own only when this names it (IsSameKeyStrict); without
		///one, as before round 4, the record's thumbprint alone makes the entry's key the tool's (see ImportedCertificateRecord).</summary>
		public CertificateKeyProvInfo KeyTool;
		///<summary>The store entry's key provider information before the tool's import gave the entry its key, when the import left that earlier
		///key aside (ClientCertificateEntryBefore.InStoreKeyLeft: a key in another key set, an unidentified one, a usable key the tool did not
		///create, or a key in the import's key set that does not open and that the tool did not create), exactly as
		///read; null when there was none, or the entry had no key, or its key provider information could not be read, or the import could not
		///read its own key's (KeyTool null): a record with KeyBefore but no KeyTool (only an import before the DCFU4 lead fix round wrote one)
		///leaves the tool unable to tell which key is its own, so its removal puts the earlier key back and deletes no key
		///(ClientCertificateStore.IsToolKeyUnknown).</summary>
		public CertificateKeyProvInfo KeyBefore;
		///<summary>True when the record was read from a file of the old format (a thumbprint line only).</summary>
		public bool IsOldFormat;

		///<summary>A record of this thumbprint with no key named: what a record of the old format, or a caller that only knows the thumbprint,
		///gives.</summary>
		public static ImportedCertificate FromThumbprint(string thumbprint) {
			ImportedCertificate importedCertificate=new ImportedCertificate();
			importedCertificate.Thumbprint=DbConnectionConfig.NormalizeThumbprint(thumbprint);
			return importedCertificate;
		}
	}

	///<summary>The client certificate the tool imported into LocalMachine\My from a .pfx (one whose store entry then uses the private key the
	///import stored: a certificate the store did not have, or one whose entry had no usable key of the tool's own, DbSettingsRunner), in
	///ConnectionSettings\imported-client-certificate.txt next to the office CA copy (owner decision of 2026-11-02;
	///docs/specs/db-connection-security.md section 4.3). Kept there, not in FreeDentalConfig.xml, because the ConnectionSettings folder is
	///administrator-only with a protected DACL (OfficeCaFile.WriteProtectedFile): what the tool may delete from the computer's store must never
	///come from a file a standard user could have written. A record that is missing, unreadable, malformed, a link or not owned by an
	///administrator reads as none, and with none the tool removes nothing.
	///
	///The format (DIRECT-CERT-FOLLOWUP round 4, part C), UTF-8 without a byte order mark, one item per line (CR LF), versioned by its first line:
	///  Open PMS - Dental imported client certificate, format 2
	///  thumbprint=(40 hex digits)
	///  tool.container=(text)          the key the tool's import gave the store's entry: these six lines together, or none
	///  tool.provider=(text)
	///  tool.providertype=(decimal; 0 for a CNG key storage provider)
	///  tool.keyset=machine|user
	///  tool.flags=(8 hex digits: CRYPT_KEY_PROV_INFO.dwFlags, whose 0x20 bit is the key set above)
	///  tool.keyspec=(decimal)
	///  earlier.container=(text)       the entry's key provider information before the import, when the import left that key aside: these six
	///  earlier.provider=(text)        lines together, then its provider parameters (rarely any), each "parameter,flags,data" in decimal, hex and
	///  earlier.providertype=(decimal) hex; or none
	///  earlier.keyset=machine|user
	///  earlier.flags=(8 hex digits)
	///  earlier.keyspec=(decimal)
	///  earlier.param=(decimal),(8 hex digits),(hex bytes)
	///Text values escape "%", CR and LF as %25, %0D and %0A. A line of another name, a name twice, a group with a line missing, a key set that
	///contradicts its flags, a tool key without a container, or another first line reads as none. A file of the old format (before round 4: one
	///line of 40 hex digits, spaces and colons allowed) is still read, as that thumbprint with no key named, and the next save that writes the
	///record writes it in this format (DbSettingsRunner).
	///
	///How a record with no key named decides whether a store key is the tool's: as before round 4, the thumbprint alone: the entry of the
	///recorded certificate in the import's key set is the tool's (its key gets the users' read access on a re-import, and a later save removes it
	///with its key). That claims no key the old rule did not already claim, and the record only names a certificate whose entry took the key the
	///tool's import stored, so its key at that time was the tool's. Claiming nothing instead would be no safer: the key the old tool made, which
	///the users can read, would then never be removed. When the next save rewrites the record, it names the entry's key at that time, but only
	///when the old rule would claim that key on a re-import: a key in the tool's key set (read without opening it) that opens and signs or
	///decrypts (ClientCertificateStore.CompleteRecord); from then on only that key is the tool's (IsSameKeyStrict), and a key an administrator
	///gives the entry by hand is not. An entry whose key is in another key set or does not open silently (an administrator may have re-keyed it
	///by hand, to a key of their own or a smart-card or TPM key) is rewritten in format 2 with no key named, so the old-format rule goes on
	///until an import records a key under the strict rule (DCFU4 second fix round, review finding 1). A recorded key outside the tool's key set
	///is never the tool's, whatever the record says.</summary>
	public static class ImportedCertificateRecord {
		///<summary>The first line of the record's format 2 (round 4).</summary>
		public const string FORMAT_LINE="Open PMS - Dental imported client certificate, format 2";
		///<summary>The start of the first line of every versioned format: one of another number is not read.</summary>
		private const string FORMAT_LINE_START="Open PMS - Dental imported client certificate, format ";
		private const string PREFIX_TOOL="tool.";
		private const string PREFIX_EARLIER="earlier.";
		private static readonly string[] _arrayKeyFields=new string[] { "container","provider","providertype","keyset","flags","keyspec" };

		///<summary>The record file in the folder of the office CA copy (DbConnectionConfig.GetOfficeCaCopyPath; tests pass their own).</summary>
		public static string GetPath(string caCopyPath) {
			return Path.Combine(Path.GetDirectoryName(Path.GetFullPath(caCopyPath)),ProgramNames.ImportedClientCertFileName);
		}

		///<summary>The recorded thumbprint (upper-case hex), or "" when there is no trustworthy record (ReadRecord). Never throws.</summary>
		public static string Read(string path) {
			ImportedCertificate importedCertificate=ReadRecord(path);
			return importedCertificate==null ? "" : importedCertificate.Thumbprint;
		}

		///<summary>The record, or null when there is no trustworthy one: missing, unreadable, a link, not owned by an administrator, or malformed
		///(see the class summary). Never throws.</summary>
		public static ImportedCertificate ReadRecord(string path) {
			try {
				if(!File.Exists(path)) {
					return null;
				}
				OfficeCaFile.CheckExisting(Path.GetDirectoryName(Path.GetFullPath(path)),true,"the record of the imported client certificate");
				OfficeCaFile.CheckExisting(path,false,"the record of the imported client certificate");
				return Parse(File.ReadAllText(path,Encoding.UTF8));
			}
			catch(Exception ex) {
				ex.DoNothing();
				return null;
			}
		}

		///<summary>The record in this text, or null when it is malformed. Never throws.</summary>
		public static ImportedCertificate Parse(string text) {
			try {
				List<string> listLines=(text??"").Replace("\r\n","\n").Split('\n').ToList();
				while(listLines.Count>0 && listLines[listLines.Count-1].Trim()=="") {
					listLines.RemoveAt(listLines.Count-1);//The line break after the last line, or blank lines after it.
				}
				if(listLines.Count==0) {
					return null;
				}
				if(listLines[0].StartsWith(FORMAT_LINE_START,StringComparison.Ordinal)) {
					if(listLines[0]!=FORMAT_LINE) {
						return null;//Another format: not this tool's to read.
					}
					return ParseFormat2(listLines.Skip(1).ToList());
				}
				return ParseOldFormat(listLines);
			}
			catch(Exception ex) when(!CertificateKeyProvInfo.IsCriticalException(ex)) {
				return null;
			}
		}

		///<summary>The old format (before round 4): one line, 40 hex digits, with spaces or colons between them allowed (as Windows shows a
		///thumbprint). Anything else is no record.</summary>
		private static ImportedCertificate ParseOldFormat(List<string> listLines) {
			if(listLines.Count!=1 || listLines[0].Any(x => !Uri.IsHexDigit(x) && x!=' ' && x!=':' && x!='\t')) {
				return null;
			}
			string thumbprint=DbConnectionConfig.NormalizeThumbprint(listLines[0]);
			if(thumbprint.Length!=40) {
				return null;//A SHA-1 thumbprint, or nothing.
			}
			ImportedCertificate importedCertificate=ImportedCertificate.FromThumbprint(thumbprint);
			importedCertificate.IsOldFormat=true;
			return importedCertificate;
		}

		private static ImportedCertificate ParseFormat2(List<string> listLines) {
			Dictionary<string,string> dictValues=new Dictionary<string,string>(StringComparer.Ordinal);
			List<CertificateKeyProvParam> listParams=new List<CertificateKeyProvParam>();
			foreach(string line in listLines) {
				int indexEquals=line.IndexOf('=');
				if(indexEquals<=0) {
					return null;
				}
				string name=line.Substring(0,indexEquals);
				string value=line.Substring(indexEquals+1);
				if(name==PREFIX_EARLIER+"param") {
					listParams.Add(ParseParam(value));
					continue;
				}
				bool isKnown=name=="thumbprint" || _arrayKeyFields.Any(x => name==PREFIX_TOOL+x || name==PREFIX_EARLIER+x);
				if(!isKnown || dictValues.ContainsKey(name)) {
					return null;
				}
				dictValues[name]=value;
			}
			if(!dictValues.TryGetValue("thumbprint",out string thumbprint) || thumbprint.Length!=40 || thumbprint.Any(x => !Uri.IsHexDigit(x))) {
				return null;
			}
			ImportedCertificate importedCertificate=ImportedCertificate.FromThumbprint(thumbprint);
			importedCertificate.KeyTool=ParseKey(dictValues,PREFIX_TOOL);
			importedCertificate.KeyBefore=ParseKey(dictValues,PREFIX_EARLIER);
			if(importedCertificate.KeyTool!=null && importedCertificate.KeyTool.ContainerName=="") {
				throw new FormatException("A key the tool made always has a container name.");
			}
			if(importedCertificate.KeyBefore==null && listParams.Count>0) {
				throw new FormatException("Provider parameters without their key.");
			}
			if(importedCertificate.KeyBefore!=null) {
				importedCertificate.KeyBefore.ListProvParams=listParams;
			}
			return importedCertificate;
		}

		///<summary>The key of this group (PREFIX_TOOL or PREFIX_EARLIER): null when none of its lines is there. Throws FormatException for a group
		///with a line missing, a value that does not parse, or a key set that contradicts the flags.</summary>
		private static CertificateKeyProvInfo ParseKey(Dictionary<string,string> dictValues,string prefix) {
			int countThere=_arrayKeyFields.Count(x => dictValues.ContainsKey(prefix+x));
			if(countThere==0) {
				return null;
			}
			if(countThere!=_arrayKeyFields.Length) {
				throw new FormatException("A key's lines are incomplete.");
			}
			CertificateKeyProvInfo certificateKeyProvInfo=new CertificateKeyProvInfo();
			certificateKeyProvInfo.IsNamed=true;
			certificateKeyProvInfo.IsFieldsRead=true;
			certificateKeyProvInfo.ContainerName=Unescape(dictValues[prefix+"container"]);
			certificateKeyProvInfo.ProviderName=Unescape(dictValues[prefix+"provider"]);
			certificateKeyProvInfo.ProviderType=int.Parse(dictValues[prefix+"providertype"],NumberStyles.None,CultureInfo.InvariantCulture);
			certificateKeyProvInfo.Flags=ParseHex8(dictValues[prefix+"flags"]);
			certificateKeyProvInfo.KeySpec=uint.Parse(dictValues[prefix+"keyspec"],NumberStyles.None,CultureInfo.InvariantCulture);
			string keySet=dictValues[prefix+"keyset"];
			if(keySet!="machine" && keySet!="user") {
				throw new FormatException("Unknown key set.");
			}
			if((keySet=="machine")!=certificateKeyProvInfo.IsMachine) {
				throw new FormatException("The key set contradicts the flags.");
			}
			if(certificateKeyProvInfo.ContainerName=="") {
				certificateKeyProvInfo.Problem="The certificate's key provider information names no key container.";//As CertificateKeyProvInfo.Read.
			}
			return certificateKeyProvInfo;
		}

		private static CertificateKeyProvParam ParseParam(string value) {
			string[] arrayParts=value.Split(',');
			if(arrayParts.Length!=3 || arrayParts[2].Length%2!=0) {
				throw new FormatException("A provider parameter is malformed.");
			}
			CertificateKeyProvParam certificateKeyProvParam=new CertificateKeyProvParam();
			certificateKeyProvParam.Param=uint.Parse(arrayParts[0],NumberStyles.None,CultureInfo.InvariantCulture);
			certificateKeyProvParam.Flags=ParseHex8(arrayParts[1]);
			certificateKeyProvParam.Data=new byte[arrayParts[2].Length/2];
			for(int i=0;i<certificateKeyProvParam.Data.Length;i++) {
				certificateKeyProvParam.Data[i]=byte.Parse(arrayParts[2].Substring(i*2,2),NumberStyles.AllowHexSpecifier,CultureInfo.InvariantCulture);
			}
			return certificateKeyProvParam;
		}

		private static uint ParseHex8(string value) {
			if(value.Length!=8) {
				throw new FormatException("Flags are 8 hex digits.");
			}
			return uint.Parse(value,NumberStyles.AllowHexSpecifier,CultureInfo.InvariantCulture);
		}

		///<summary>The record's text in format 2 (see the class summary).</summary>
		public static string Format(ImportedCertificate importedCertificate) {
			StringBuilder stringBuilder=new StringBuilder();
			stringBuilder.Append(FORMAT_LINE+"\r\n");
			stringBuilder.Append("thumbprint="+DbConnectionConfig.NormalizeThumbprint(importedCertificate.Thumbprint)+"\r\n");
			if(importedCertificate.KeyTool!=null) {
				AppendKey(stringBuilder,PREFIX_TOOL,importedCertificate.KeyTool);
			}
			if(importedCertificate.KeyBefore!=null) {
				AppendKey(stringBuilder,PREFIX_EARLIER,importedCertificate.KeyBefore);
				foreach(CertificateKeyProvParam certificateKeyProvParam in importedCertificate.KeyBefore.ListProvParams) {
					stringBuilder.Append(PREFIX_EARLIER+"param="+certificateKeyProvParam.Param.ToString(CultureInfo.InvariantCulture)+","
						+certificateKeyProvParam.Flags.ToString("X8",CultureInfo.InvariantCulture)+","
						+string.Concat(certificateKeyProvParam.Data.Select(x => x.ToString("X2",CultureInfo.InvariantCulture)))+"\r\n");
				}
			}
			return stringBuilder.ToString();
		}

		private static void AppendKey(StringBuilder stringBuilder,string prefix,CertificateKeyProvInfo certificateKeyProvInfo) {
			stringBuilder.Append(prefix+"container="+Escape(certificateKeyProvInfo.ContainerName)+"\r\n");
			stringBuilder.Append(prefix+"provider="+Escape(certificateKeyProvInfo.ProviderName)+"\r\n");
			stringBuilder.Append(prefix+"providertype="+certificateKeyProvInfo.ProviderType.ToString(CultureInfo.InvariantCulture)+"\r\n");
			stringBuilder.Append(prefix+"keyset="+(certificateKeyProvInfo.IsMachine ? "machine" : "user")+"\r\n");
			stringBuilder.Append(prefix+"flags="+certificateKeyProvInfo.Flags.ToString("X8",CultureInfo.InvariantCulture)+"\r\n");
			stringBuilder.Append(prefix+"keyspec="+certificateKeyProvInfo.KeySpec.ToString(CultureInfo.InvariantCulture)+"\r\n");
		}

		private static string Escape(string text) {
			return (text??"").Replace("%","%25").Replace("\r","%0D").Replace("\n","%0A");
		}

		///<summary>The text with %25, %0D and %0A back; throws FormatException for any other "%".</summary>
		private static string Unescape(string text) {
			StringBuilder stringBuilder=new StringBuilder();
			for(int i=0;i<text.Length;i++) {
				if(text[i]!='%') {
					stringBuilder.Append(text[i]);
					continue;
				}
				string code=i+2<text.Length ? text.Substring(i+1,2) : "";
				if(code=="25") {
					stringBuilder.Append('%');
				}
				else if(code=="0D") {
					stringBuilder.Append('\r');
				}
				else if(code=="0A") {
					stringBuilder.Append('\n');
				}
				else {
					throw new FormatException("An unknown escape.");
				}
				i+=2;
			}
			return stringBuilder.ToString();
		}

		///<summary>Records the thumbprint with no key named (format 2), or deletes the record for "". Protected like the office CA copy. Throws
		///DbSettingsException (exit code 3), IOException or UnauthorizedAccessException.</summary>
		public static void Write(string path,string thumbprint) {
			thumbprint=DbConnectionConfig.NormalizeThumbprint(thumbprint);
			Write(path,thumbprint=="" ? null : ImportedCertificate.FromThumbprint(thumbprint));
		}

		///<summary>Records the certificate in format 2 (Format), or deletes the record for null. Protected like the office CA copy. Throws
		///DbSettingsException (exit code 3), IOException or UnauthorizedAccessException.</summary>
		public static void Write(string path,ImportedCertificate importedCertificate) {
			if(importedCertificate==null || DbConnectionConfig.NormalizeThumbprint(importedCertificate.Thumbprint)=="") {
				if(File.Exists(path)) {
					OfficeCaFile.CheckExisting(path,false,"the record of the imported client certificate");
					File.Delete(path);
				}
				return;
			}
			OfficeCaFile.WriteProtectedFile(path,new UTF8Encoding(false).GetBytes(Format(importedCertificate)),"the record of the imported client certificate");
		}
	}
}
