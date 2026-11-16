// Where a certificate's private key is, as its key provider information names it, read and written without opening the key. Shared by the
// connection settings tool (OpenPmsDentalDbSettings, ClientCertificateStore) and the Direct certificate import (OpenPmsDentalBusiness,
// DirectCertificateImport): docs/port-notes/DIRECT-CERT-FOLLOWUP.md, round 4. Licensed under the GNU General Public License, version 2 or later.
// See LICENSE in the repository root.
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.InteropServices;
using System.Security.Cryptography.X509Certificates;
using System.Threading;

namespace CodeBase {
	///<summary>One provider parameter of a key provider information (CRYPT_KEY_PROV_PARAM): rarely any, kept so that a key provider information
	///can be written back exactly as it was read (CertificateKeyProvInfo.Write).</summary>
	public sealed class CertificateKeyProvParam {
		public uint Param;
		public uint Flags;
		public byte[] Data=new byte[0];
	}

	///<summary>Where a certificate's private key is, as its key provider information (CERT_KEY_PROV_INFO_PROP_ID) names it, read without opening
	///the key (Read): the key container (a CNG key's name), its provider (type 0 for a CNG key storage provider), its key set (the machine's or
	///the user's: the CRYPT_MACHINE_KEYSET bit of the flags, NCRYPT_MACHINE_KEY_FLAG for a CNG key, the same bit) and its key spec. Opening a key
	///on a TPM or a smart card can ask for a PIN, or fail, so the connection settings tool and the Direct certificate import compare keys by
	///this, never by opening another certificate's key. A certificate without the property names no key (IsNamed false); one whose property
	///cannot be read, or names no key container, uses an unknown key (Problem says why; the fields read are kept, so that Write can put a
	///property that names no container back as it was).</summary>
	public sealed class CertificateKeyProvInfo {
		///<summary>The property of a certificate context that names its private key: CRYPT_KEY_PROV_INFO.</summary>
		private const uint CERT_KEY_PROV_INFO_PROP_ID=2;
		///<summary>What CertGetCertificateContextProperty gives for a certificate without the property.</summary>
		private const int CRYPT_E_NOT_FOUND=unchecked((int)0x80092004);
		///<summary>The machine key set in CRYPT_KEY_PROV_INFO.dwFlags: CRYPT_MACHINE_KEYSET for a CAPI key, NCRYPT_MACHINE_KEY_FLAG for a CNG
		///key, the same bit.</summary>
		public const uint MACHINE_KEYSET_FLAG=0x20;
		///<summary>False when the certificate has no key provider information: it names no private key.</summary>
		public bool IsNamed;
		public string ContainerName="";
		public string ProviderName="";
		///<summary>The CAPI provider type; 0 for a CNG key storage provider.</summary>
		public int ProviderType;
		///<summary>CRYPT_KEY_PROV_INFO.dwFlags, as read: the key set bit (IsMachine) and any other.</summary>
		public uint Flags;
		///<summary>CRYPT_KEY_PROV_INFO.dwKeySpec: AT_KEYEXCHANGE (1) or AT_SIGNATURE (2) for a CAPI key; a CNG key's as Windows wrote it.</summary>
		public uint KeySpec;
		public List<CertificateKeyProvParam> ListProvParams=new List<CertificateKeyProvParam>();
		///<summary>Why the property could not be read, or names no key container; null when it was read and names one.</summary>
		public string Problem;
		///<summary>True when the fields were read from the property (also when it names no key container), so that Write can put them back
		///exactly; false without the property, or when it could not be read at all.</summary>
		public bool IsFieldsRead;

		[DllImport("crypt32.dll",SetLastError=true)]
		[return: MarshalAs(UnmanagedType.Bool)]
		private static extern bool CertGetCertificateContextProperty(IntPtr pCertContext,uint dwPropId,IntPtr pvData,ref uint pcbData);

		[DllImport("crypt32.dll",SetLastError=true)]
		[return: MarshalAs(UnmanagedType.Bool)]
		private static extern bool CertSetCertificateContextProperty(IntPtr pCertContext,uint dwPropId,uint dwFlags,IntPtr pvData);

		///<summary>CRYPT_KEY_PROV_INFO, as CertGetCertificateContextProperty gives it for CERT_KEY_PROV_INFO_PROP_ID.</summary>
		[StructLayout(LayoutKind.Sequential)]
		private struct CRYPT_KEY_PROV_INFO {
			public IntPtr pwszContainerName;
			public IntPtr pwszProvName;
			public uint dwProvType;
			public uint dwFlags;
			public uint cProvParam;
			public IntPtr rgProvParam;
			public uint dwKeySpec;
		}

		///<summary>CRYPT_KEY_PROV_PARAM.</summary>
		[StructLayout(LayoutKind.Sequential)]
		private struct CRYPT_KEY_PROV_PARAM {
			public uint dwParam;
			public IntPtr pbData;
			public uint cbData;
			public uint dwFlags;
		}

		///<summary>True when the property could not be read, or names no key container: the key could be any.</summary>
		public bool IsUnknown {
			get {
				return Problem!=null;
			}
		}

		///<summary>True for a key of the machine key set (MACHINE_KEYSET_FLAG), false for one of a user's key set.</summary>
		public bool IsMachine {
			get {
				return (Flags & MACHINE_KEYSET_FLAG)!=0;
			}
		}

		///<summary>The key container in its key set, compared without case (CAPI and CNG names are). Not the provider: Microsoft's software CAPI
		///providers share their containers, and a certificate may name one through another; so two certificates that name one container name in
		///one key set count as one key, which keeps a key rather than delete one another certificate may use. Never for a claim that a key is
		///one's own: IsSameKeyStrict.</summary>
		public string Identity {
			get {
				return (IsMachine ? "machine" : "user")+"|"+ContainerName;
			}
		}

		///<summary>The certificate's key provider information, read with CertGetCertificateContextProperty on its context
		///(X509Certificate2.Handle), without opening the key. actionBeforeReadForTests: a tool's test seam, called with the certificate right before
		///the property is read; throwing there is a property that cannot be read (null in the program). Never throws: anything but a critical
		///exception (IsCriticalException) is a property that cannot be read.</summary>
		public static CertificateKeyProvInfo Read(X509Certificate2 x509Certificate2,Action<X509Certificate2> actionBeforeReadForTests=null) {
			CertificateKeyProvInfo certificateKeyProvInfo=new CertificateKeyProvInfo();
			try {
				IntPtr intPtrCertContext=x509Certificate2.Handle;
				if(intPtrCertContext==IntPtr.Zero) {
					certificateKeyProvInfo.IsNamed=true;
					certificateKeyProvInfo.Problem="The certificate was already released.";
					return certificateKeyProvInfo;
				}
				actionBeforeReadForTests?.Invoke(x509Certificate2);
				uint byteCount=0;
				if(!CertGetCertificateContextProperty(intPtrCertContext,CERT_KEY_PROV_INFO_PROP_ID,IntPtr.Zero,ref byteCount)) {
					int error=Marshal.GetLastWin32Error();
					if(error==CRYPT_E_NOT_FOUND) {
						return certificateKeyProvInfo;//No property: the certificate names no private key.
					}
					certificateKeyProvInfo.IsNamed=true;
					certificateKeyProvInfo.Problem=new Win32Exception(error).Message.Trim();
					return certificateKeyProvInfo;
				}
				certificateKeyProvInfo.IsNamed=true;
				IntPtr intPtrData=Marshal.AllocHGlobal((int)byteCount);
				try {
					if(!CertGetCertificateContextProperty(intPtrCertContext,CERT_KEY_PROV_INFO_PROP_ID,intPtrData,ref byteCount)) {
						certificateKeyProvInfo.Problem=new Win32Exception(Marshal.GetLastWin32Error()).Message.Trim();
						return certificateKeyProvInfo;
					}
					CRYPT_KEY_PROV_INFO cryptKeyProvInfo=(CRYPT_KEY_PROV_INFO)Marshal.PtrToStructure(intPtrData,typeof(CRYPT_KEY_PROV_INFO));
					certificateKeyProvInfo.ContainerName=Marshal.PtrToStringUni(cryptKeyProvInfo.pwszContainerName)??"";//Null for IntPtr.Zero.
					certificateKeyProvInfo.ProviderName=Marshal.PtrToStringUni(cryptKeyProvInfo.pwszProvName)??"";
					certificateKeyProvInfo.ProviderType=(int)cryptKeyProvInfo.dwProvType;
					certificateKeyProvInfo.Flags=cryptKeyProvInfo.dwFlags;
					certificateKeyProvInfo.KeySpec=cryptKeyProvInfo.dwKeySpec;
					int sizeParam=Marshal.SizeOf(typeof(CRYPT_KEY_PROV_PARAM));
					for(int i=0;i<cryptKeyProvInfo.cProvParam;i++) {
						CRYPT_KEY_PROV_PARAM cryptKeyProvParam=(CRYPT_KEY_PROV_PARAM)Marshal.PtrToStructure(
							new IntPtr(cryptKeyProvInfo.rgProvParam.ToInt64()+(long)i*sizeParam),typeof(CRYPT_KEY_PROV_PARAM));
						CertificateKeyProvParam certificateKeyProvParam=new CertificateKeyProvParam();
						certificateKeyProvParam.Param=cryptKeyProvParam.dwParam;
						certificateKeyProvParam.Flags=cryptKeyProvParam.dwFlags;
						certificateKeyProvParam.Data=new byte[cryptKeyProvParam.cbData];
						if(cryptKeyProvParam.cbData>0) {
							Marshal.Copy(cryptKeyProvParam.pbData,certificateKeyProvParam.Data,0,(int)cryptKeyProvParam.cbData);
						}
						certificateKeyProvInfo.ListProvParams.Add(certificateKeyProvParam);
					}
					certificateKeyProvInfo.IsFieldsRead=true;
					if(certificateKeyProvInfo.ContainerName=="") {
						certificateKeyProvInfo.Problem="The certificate's key provider information names no key container.";
					}
					return certificateKeyProvInfo;
				}
				finally {
					Marshal.FreeHGlobal(intPtrData);
				}
			}
			catch(Exception ex) when(!IsCriticalException(ex)) {
				certificateKeyProvInfo.IsNamed=true;
				certificateKeyProvInfo.Problem=ex.Message.Trim();
				return certificateKeyProvInfo;
			}
			finally {
				GC.KeepAlive(x509Certificate2);//Its certificate context is used until here.
			}
		}

		///<summary>The certificate's RSA private key as the CAPI key container its key provider information names (a provider type other than 0),
		///opened through CAPI, as the .NET Framework's X509Certificate2.PrivateKey opened it; null for a CNG key (type 0), no key, or a key
		///information that cannot be read. .NET's PrivateKey and GetRSAPrivateKey open a CAPI key through CNG first and give an RSACng (measured
		///on .NET 10 in S3b-5: DbConnectionSecurityTests, LibraryUpdateTests), whose Key.Delete leaves the CAPI container; so the connection
		///settings tool and the Direct certificate import open a CAPI key here instead. The caller owns the object. The key is not touched: the
		///constructor opens the existing container (UseExistingKey) and throws CryptographicException when it does not open (a smart card that is
		///not there, a container that is gone).</summary>
		public static System.Security.Cryptography.RSACryptoServiceProvider OpenCapiRsaPrivateKey(X509Certificate2 x509Certificate2) {
			CertificateKeyProvInfo certificateKeyProvInfo=Read(x509Certificate2);
			if(!certificateKeyProvInfo.IsNamed || certificateKeyProvInfo.IsUnknown || certificateKeyProvInfo.ProviderType==0) {
				return null;
			}
			System.Security.Cryptography.CspParameters cspParameters=new System.Security.Cryptography.CspParameters(certificateKeyProvInfo.ProviderType,
				certificateKeyProvInfo.ProviderName,certificateKeyProvInfo.ContainerName);
			cspParameters.KeyNumber=(int)certificateKeyProvInfo.KeySpec;
			cspParameters.Flags=System.Security.Cryptography.CspProviderFlags.UseExistingKey;
			if(certificateKeyProvInfo.IsMachine) {
				cspParameters.Flags|=System.Security.Cryptography.CspProviderFlags.UseMachineKeyStore;
			}
			return new System.Security.Cryptography.RSACryptoServiceProvider(cspParameters);
		}

		///<summary>True when these two key provider informations name one key for certain, for a tool's claim that a key is its own: both named
		///and readable, the same key container in the same key set (Identity), and the same provider, its type and its name (without case).
		///Identity leaves the provider out on purpose, for the comparison before a key is deleted, where a false match only keeps a key; here a
		///false match would make a tool claim a key it did not create (a CNG key storage provider's key with the container name of the import's
		///CAPI key, say), so a pair of providers that differ, a CNG one (type 0) against a CAPI one above all, never matches. A false negative is
		///safe: the key is then not the tool's.</summary>
		public static bool IsSameKeyStrict(CertificateKeyProvInfo certificateKeyProvInfo1,CertificateKeyProvInfo certificateKeyProvInfo2) {
			if(certificateKeyProvInfo1==null || certificateKeyProvInfo2==null) {
				return false;
			}
			if(!certificateKeyProvInfo1.IsNamed || certificateKeyProvInfo1.IsUnknown || !certificateKeyProvInfo2.IsNamed || certificateKeyProvInfo2.IsUnknown) {
				return false;
			}
			return string.Equals(certificateKeyProvInfo1.Identity,certificateKeyProvInfo2.Identity,StringComparison.OrdinalIgnoreCase)
				&& certificateKeyProvInfo1.ProviderType==certificateKeyProvInfo2.ProviderType
				&& string.Equals(certificateKeyProvInfo1.ProviderName,certificateKeyProvInfo2.ProviderName,StringComparison.OrdinalIgnoreCase);
		}

		///<summary>True when the two hold the very same fields: whether the property is there, the container and provider names (exactly), the
		///provider type, the flags, the key spec and the provider parameters. For checking that a key provider information written back (Write)
		///is the one read before.</summary>
		public static bool IsSameExactly(CertificateKeyProvInfo certificateKeyProvInfo1,CertificateKeyProvInfo certificateKeyProvInfo2) {
			if(certificateKeyProvInfo1==null || certificateKeyProvInfo2==null) {
				return false;
			}
			if(certificateKeyProvInfo1.IsNamed!=certificateKeyProvInfo2.IsNamed) {
				return false;
			}
			if(!certificateKeyProvInfo1.IsNamed) {
				return true;
			}
			if(certificateKeyProvInfo1.ContainerName!=certificateKeyProvInfo2.ContainerName
				|| certificateKeyProvInfo1.ProviderName!=certificateKeyProvInfo2.ProviderName
				|| certificateKeyProvInfo1.ProviderType!=certificateKeyProvInfo2.ProviderType
				|| certificateKeyProvInfo1.Flags!=certificateKeyProvInfo2.Flags
				|| certificateKeyProvInfo1.KeySpec!=certificateKeyProvInfo2.KeySpec
				|| certificateKeyProvInfo1.ListProvParams.Count!=certificateKeyProvInfo2.ListProvParams.Count)
			{
				return false;
			}
			for(int i=0;i<certificateKeyProvInfo1.ListProvParams.Count;i++) {
				CertificateKeyProvParam certificateKeyProvParam1=certificateKeyProvInfo1.ListProvParams[i];
				CertificateKeyProvParam certificateKeyProvParam2=certificateKeyProvInfo2.ListProvParams[i];
				if(certificateKeyProvParam1.Param!=certificateKeyProvParam2.Param || certificateKeyProvParam1.Flags!=certificateKeyProvParam2.Flags
					|| !certificateKeyProvParam1.Data.SequenceEqual(certificateKeyProvParam2.Data))
				{
					return false;
				}
			}
			return true;
		}

		///<summary>Sets this key provider information on the certificate's context with CertSetCertificateContextProperty, exactly as its fields
		///are (a certificate of a store opened for writing: the store keeps it; a certificate in memory: only that copy names the key). Opens no
		///key. Only for a named key provider information (IsNamed): a certificate without the property is not written to. Throws Win32Exception
		///when Windows refuses, InvalidOperationException for a released certificate or one that names no key.</summary>
		public void Write(X509Certificate2 x509Certificate2) {
			if(!IsNamed) {
				throw new InvalidOperationException("A key provider information that names no key is not written.");
			}
			IntPtr intPtrCertContext=x509Certificate2.Handle;
			if(intPtrCertContext==IntPtr.Zero) {
				throw new InvalidOperationException("The certificate was already released.");
			}
			List<IntPtr> listIntPtrs=new List<IntPtr>();
			try {
				IntPtr intPtrContainer=Marshal.StringToHGlobalUni(ContainerName);
				listIntPtrs.Add(intPtrContainer);
				IntPtr intPtrProvider=Marshal.StringToHGlobalUni(ProviderName);
				listIntPtrs.Add(intPtrProvider);
				IntPtr intPtrParams=IntPtr.Zero;
				int sizeParam=Marshal.SizeOf(typeof(CRYPT_KEY_PROV_PARAM));
				if(ListProvParams.Count>0) {
					intPtrParams=Marshal.AllocHGlobal(sizeParam*ListProvParams.Count);
					listIntPtrs.Add(intPtrParams);
					for(int i=0;i<ListProvParams.Count;i++) {
						CRYPT_KEY_PROV_PARAM cryptKeyProvParam=new CRYPT_KEY_PROV_PARAM();
						cryptKeyProvParam.dwParam=ListProvParams[i].Param;
						cryptKeyProvParam.dwFlags=ListProvParams[i].Flags;
						cryptKeyProvParam.cbData=(uint)ListProvParams[i].Data.Length;
						if(ListProvParams[i].Data.Length>0) {
							cryptKeyProvParam.pbData=Marshal.AllocHGlobal(ListProvParams[i].Data.Length);
							listIntPtrs.Add(cryptKeyProvParam.pbData);
							Marshal.Copy(ListProvParams[i].Data,0,cryptKeyProvParam.pbData,ListProvParams[i].Data.Length);
						}
						Marshal.StructureToPtr(cryptKeyProvParam,new IntPtr(intPtrParams.ToInt64()+(long)i*sizeParam),false);
					}
				}
				CRYPT_KEY_PROV_INFO cryptKeyProvInfo=new CRYPT_KEY_PROV_INFO();
				cryptKeyProvInfo.pwszContainerName=intPtrContainer;
				cryptKeyProvInfo.pwszProvName=intPtrProvider;
				cryptKeyProvInfo.dwProvType=(uint)ProviderType;
				cryptKeyProvInfo.dwFlags=Flags;
				cryptKeyProvInfo.cProvParam=(uint)ListProvParams.Count;
				cryptKeyProvInfo.rgProvParam=intPtrParams;
				cryptKeyProvInfo.dwKeySpec=KeySpec;
				IntPtr intPtrData=Marshal.AllocHGlobal(Marshal.SizeOf(typeof(CRYPT_KEY_PROV_INFO)));
				listIntPtrs.Add(intPtrData);
				Marshal.StructureToPtr(cryptKeyProvInfo,intPtrData,false);
				if(!CertSetCertificateContextProperty(intPtrCertContext,CERT_KEY_PROV_INFO_PROP_ID,0,intPtrData)) {
					throw new Win32Exception(Marshal.GetLastWin32Error());
				}
			}
			finally {
				foreach(IntPtr intPtr in listIntPtrs) {
					Marshal.FreeHGlobal(intPtr);
				}
				GC.KeepAlive(x509Certificate2);
			}
		}

		///<summary>"provider P, key container C, in the computer's key set" (or "in a user key set"), for the tools' messages.</summary>
		public string Describe() {
			return "provider "+ProviderName+", key container "+ContainerName+", in "+(IsMachine ? "the computer's key set" : "a user key set");
		}

		///<summary>True for the exceptions that a method documented "Never throws" lets through: OutOfMemoryException, StackOverflowException and
		///ThreadAbortException, after which nothing can go on safely. Every other one is an answer.</summary>
		public static bool IsCriticalException(Exception ex) {
			return ex is OutOfMemoryException || ex is StackOverflowException || ex is ThreadAbortException;
		}
	}
}
