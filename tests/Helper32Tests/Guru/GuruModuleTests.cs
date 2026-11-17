// The helper's Guru module against fakes of MedVisorInterface.dll's MVStart and MVSendPatient (S3a-14 of the .NET 10 move,
// docs/port-notes/S3a-14.md, section 4): the structure MVSendPatient gets is byte for byte the one today's ToCPlusPlusStructure built (GuruToday,
// the code of WpfControlsOD/Bridges/Guru.cs before S3a-14), after the patient crossed the pipe as the program sends it. MedVisorInterface.dll is
// never loaded and Guru is never started.
// Licensed under the GNU General Public License, version 2 or later. See LICENSE in the repository root.
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.InteropServices;
using Helper32Tests.Support;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using OpenPmsDentalHelper32;
using OpenPmsDentalHelper32.Guru;

namespace Helper32Tests.Guru {
	[TestClass]
	public class GuruModuleTests {
		///<summary>The size ToCPlusPlusStructure allocates: sizeof(char) times the nine field lengths.</summary>
		private const int SIZE_ALLOCATED=2*(65+65+65+65+65+2+9+65+260);
		///<summary>The bytes ToCPlusPlusStructure writes: each Marshal.Copy copies size chars (2*size bytes) and moves on size bytes, so the last
		///field, Directory, starts at 401 and ends at 401+2*260. The bytes after it were never written, today or now.</summary>
		private const int SIZE_WRITTEN=401+2*260;

		///<summary>A synthetic patient as Guru.SendData fills it (Tidy and its lengths), one MVPatient of each kind with the same arrays.</summary>
		private sealed class Patient {
			public GuruToday.MVPatientToday Today=new GuruToday.MVPatientToday();
			public MVPatient Now=new MVPatient();

			public static Patient Create(string lName,string fName,string sex,string birthDate,string id,string directory) {
				Patient patient=new Patient();
				patient.Today.LastName=GuruToday.Tidy(lName,64);
				patient.Today.FirstName=GuruToday.Tidy(fName,64);
				if(sex!=null) {
					patient.Today.Sex=GuruToday.Tidy(sex,1);
				}
				patient.Today.BirthDate=GuruToday.Tidy(birthDate,8);
				patient.Today.ID=GuruToday.Tidy(id,64);
				patient.Today.Directory=GuruToday.Tidy(directory,259);
				patient.Now.SetFields(new[] { patient.Today.LastName,patient.Today.MiddleName,patient.Today.FirstName,patient.Today.Prefix,
					patient.Today.Suffix,patient.Today.Sex,patient.Today.BirthDate,patient.Today.ID,patient.Today.Directory }
					.Select(x => (char[])x.Clone()).ToArray());
				return patient;
			}
		}

		private static byte[] ReadBytes(IntPtr pointer,int count) {
			byte[] arrayBytes=new byte[count];
			Marshal.Copy(pointer,arrayBytes,0,count);
			return arrayBytes;
		}

		private static byte[] GetTodaysBytes(GuruToday.MVPatientToday mvPatient) {
			IntPtr pointer=GuruToday.ToCPlusPlusStructure(mvPatient);
			try {
				return ReadBytes(pointer,SIZE_WRITTEN);
			}
			finally {
				GuruToday.DestroyCPlusPlusStruct(pointer);
			}
		}

		///<summary>Sends the patient as the program does (GuruCommands.EncodePatient, the request) to the module, whose fake MVSendPatient reads
		///the structure. Returns the written bytes the fake got.</summary>
		private static byte[] SendThroughTheModule(MVPatient mvPatient,int returnValue,out string[] arrayResults) {
			List<byte[]> listStructures=new List<byte[]>();
			GuruModule module=new GuruModule(() => throw new AssertFailedException("MVStart is not called"),x => {
				listStructures.Add(ReadBytes(x,SIZE_ALLOCATED));//the whole allocation is readable
				return returnValue;
			});
			string[] arrayArgs=GuruCommands.EncodePatient(mvPatient,out byte[] data);
			Helper32Reply reply=module.Handle(new Helper32Request { Module=GuruCommands.MODULE,Command=GuruCommands.MV_SEND_PATIENT,Args=arrayArgs,
				Data=data });
			Assert.IsTrue(reply.IsOk);
			arrayResults=reply.Results;
			Assert.AreEqual(1,listStructures.Count,"One MVSendPatient per request.");
			return listStructures[0].Take(SIZE_WRITTEN).ToArray();
		}

		///<summary>The written bytes of the program's in-process path (S3a-14n: WpfControlsOD/Bridges/GuruInProcess.cs, linked), the structure its
		///MVSendPatient passes to MedVisorInterface.dll.</summary>
		private static byte[] GetInProcessBytes(MVPatient mvPatient) {
			IntPtr pointer=OpenPmsDental.Bridges.GuruInProcess.ToCPlusPlusStructure(mvPatient);
			try {
				return ReadBytes(pointer,SIZE_WRITTEN);
			}
			finally {
				OpenPmsDental.Bridges.GuruInProcess.DestroyCPlusPlusStruct(pointer);
			}
		}

		private static void AssertSameStructure(Patient patient) {
			byte[] arrayToday=GetTodaysBytes(patient.Today);
			byte[] arrayNow=SendThroughTheModule(patient.Now,0,out string[] arrayResults);
			CollectionAssert.AreEqual(arrayToday,arrayNow,"The structure differs from today's at byte "
				+Enumerable.Range(0,SIZE_WRITTEN).FirstOrDefault(i => arrayToday[i]!=arrayNow[i]));
			CollectionAssert.AreEqual(new[] { "0" },arrayResults);
			byte[] arrayInProcess=GetInProcessBytes(patient.Now);
			CollectionAssert.AreEqual(arrayToday,arrayInProcess,"The in-process structure differs from today's at byte "
				+Enumerable.Range(0,SIZE_WRITTEN).FirstOrDefault(i => arrayToday[i]!=arrayInProcess[i]));
		}

		[TestMethod]
		public void MVSendPatient_TheStructureIsTodays_ByteForByte() {
			AssertSameStructure(Patient.Create("Synthetic","Patient","M","01311980","1001",@"C:\GuruImages\SyntheticP1001"));
			AssertSameStructure(Patient.Create("Test","Female","F","12252001","CH-77",@"\\images.example.test\Guru\TestF77"));
			AssertSameStructure(Patient.Create("Unknown","Gender","0","07041976","9",@"D:\G"));
		}

		[TestMethod]
		public void MVSendPatient_GenderNotSet_LongNames_NonAscii_AsToday() {
			//Another gender leaves Sex at new char[2]; names longer than 64 characters are cut; any character crosses as it is.
			AssertSameStructure(Patient.Create("Other","Gender",null,"01012000","5",@"C:\G"));
			AssertSameStructure(Patient.Create(new string('L',80),new string('F',65),"M","01012000",new string('9',70),new string('d',300)));
			AssertSameStructure(Patient.Create("M\u00fcller-\u00c9t\u00e9","\u4e2d\u6587\u540d","F","02292000","\u00e91","C:\\Bilder\\M\u00fcller"));
			AssertSameStructure(Patient.Create("","","M","","",""));
		}

		[TestMethod]
		public void MVSendPatient_ASurrogatePairCutByTidy_CrossesAsItIs() {
			//Tidy cuts at 64 characters, which can leave the high half of a surrogate pair at the end: as text (UTF-8) it would become U+FFFD, so
			//the characters cross as their own two bytes each.
			string lName=new string('a',63)+"\ud83d\ude00";
			Patient patient=Patient.Create(lName,"First","M","01012000","1","C:\\G");
			Assert.AreEqual('\ud83d',patient.Today.LastName[63]);
			AssertSameStructure(patient);
		}

		[TestMethod]
		public void MVSendPatient_ReturnsTheFunctionsValue() {
			Patient patient=Patient.Create("Synthetic","Patient","M","01311980","1001","C:\\G");
			SendThroughTheModule(patient.Now,7,out string[] arrayResults);
			CollectionAssert.AreEqual(new[] { "7" },arrayResults);
		}

		[TestMethod]
		public void EncodePatient_RoundTrips_AndBadRequestsAreRefused() {
			Patient patient=Patient.Create("Synthetic","Patient","M","01311980","1001","C:\\G");
			string[] arrayArgs=GuruCommands.EncodePatient(patient.Now,out byte[] data);
			CollectionAssert.AreEqual(new[] { "65","65","65","65","65","2","9","65","260" },arrayArgs);
			Assert.AreEqual(2*(65*6+2+9+260),data.Length);
			MVPatient mvPatient=GuruCommands.DecodePatient(arrayArgs,data);
			for(int i=0;i<MVPatient.FIELD_COUNT;i++) {
				CollectionAssert.AreEqual(patient.Now.GetFields()[i],mvPatient.GetFields()[i]);
			}
			Assert.ThrowsExactly<ArgumentException>(() => GuruCommands.DecodePatient(arrayArgs.Take(8).ToArray(),data));
			Assert.ThrowsExactly<ArgumentException>(() => GuruCommands.DecodePatient(arrayArgs,data.Take(data.Length-2).ToArray()));
			Assert.ThrowsExactly<ArgumentException>(() => GuruCommands.DecodePatient(arrayArgs,data.Concat(new byte[2]).ToArray()));
			Assert.ThrowsExactly<ArgumentException>(() => GuruCommands.DecodePatient(arrayArgs,null));
		}

		[TestMethod]
		public void MVStart_ReturnsItsValue_AndAMissingLibraryKeepsItsType() {
			GuruModule module=new GuruModule(() => 3,x => throw new AssertFailedException("MVSendPatient is not called"));
			Helper32Reply reply=module.Handle(new Helper32Request { Module=GuruCommands.MODULE,Command=GuruCommands.MV_START,Args=new string[0] });
			CollectionAssert.AreEqual(new[] { "3" },reply.Results);
			GuruModule moduleMissing=new GuruModule(() => throw new DllNotFoundException("Unable to load DLL 'MedVisorInterface.dll'"),x => 0);
			//The helper answers with the exception's type name, which Guru.cs turns back into DllNotFoundException for its message.
			Helper32Reply replyError=Helper32Reply.FromException(Assert.ThrowsExactly<DllNotFoundException>(() => moduleMissing.Handle(
				new Helper32Request { Module=GuruCommands.MODULE,Command=GuruCommands.MV_START,Args=new string[0] })));
			Assert.AreEqual(typeof(DllNotFoundException).FullName,replyError.ErrorType);
			Assert.ThrowsExactly<ArgumentException>(() => module.Handle(new Helper32Request { Module=GuruCommands.MODULE,Command="NoSuchCommand",
				Args=new string[0] }));
		}

		[TestMethod]
		public void Constructor_LoadsNoVendorCode() {
			//A DllImport loads its library on the first call: building the module (the helper builds every module at its start) loads nothing.
			GuruModule module=new GuruModule();
			Assert.AreEqual(GuruCommands.MODULE,module.Name);
			Assert.IsFalse(Process.GetCurrentProcess().Modules.Cast<ProcessModule>().Any(x => x.ModuleName.Equals("MedVisorInterface.dll",
				StringComparison.OrdinalIgnoreCase)));
		}

		[TestMethod]
		public void ProgramSide_GuruCs_SendsThePatientToTheHelper_AndKeepsItsMessages() {
			string source=TestPaths.ReadSource("WpfControlsOD","Bridges","Guru.cs");
			StringAssert.Contains(source,"GuruCommands.EncodePatient(mvPatient,out byte[] data)");
			StringAssert.Contains(source,"ex.RemoteTypeName==typeof(DllNotFoundException).FullName");
			StringAssert.Contains(source,"Could not find MedVisorInterface.dll. Verify that Guru is installed.");
			Assert.IsFalse(source.Contains("DllImport"),"Guru.cs loads no MedVisorInterface.dll itself.");
			Assert.IsFalse(source.Contains("ToCPlusPlusStructure("),"The structure is built in GuruInProcess or in the helper.");
			//S3a-14n: native first. Each of the two calls asks the chooser first and runs in the program's process when it chose it (GuruInProcess,
			//which declares MedVisorInterface.dll's two functions as Guru.cs did before S3a-14); the helper is the fallback.
			foreach(string call in new[] { "return GuruInProcess.MVStart();","return GuruInProcess.MVSendPatient(mvPatient);" }) {
				int indexCall=source.IndexOf(call,StringComparison.Ordinal);
				Assert.IsTrue(indexCall>0,call);
				int indexCheck=source.LastIndexOf("if(GuruInProcess.IsChosen()) {",indexCall,StringComparison.Ordinal);
				Assert.IsTrue(indexCheck>0 && indexCall-indexCheck<80,call+" follows the chooser's check.");
				int indexHelper=source.IndexOf("Helper32Client.Default.Call(GuruCommands.MODULE,",indexCall,StringComparison.Ordinal);
				Assert.IsTrue(indexHelper>indexCall,"The helper's call after "+call);
			}
			string sourceInProcess=TestPaths.ReadSource("WpfControlsOD","Bridges","GuruInProcess.cs");
			StringAssert.Contains(sourceInProcess,"NativeFirstChooser.Guru.ChooseForNativeDll(DLL_FILE_NAME,PeImageInfo.IMAGE_FILE_MACHINE_I386)==VendorPath.InProcess");
			StringAssert.Contains(sourceInProcess,"[DllImport(\"MedVisorInterface.dll\")]\n\t\tpublic static extern int MVStart();".Replace("\n",
				sourceInProcess.Contains("\r\n") ? "\r\n" : "\n"));
			Assert.AreEqual("MedVisorInterface.dll",OpenPmsDental.Bridges.GuruInProcess.DLL_FILE_NAME);
		}
	}
}
