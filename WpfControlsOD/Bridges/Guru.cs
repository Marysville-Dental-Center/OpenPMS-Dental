using System;
using System.Collections;
using System.Diagnostics;
using System.IO;
using System.Text;
using System.Windows.Forms;
using CodeBase;
using OpenPmsDentalBusiness;
using OpenPmsDentalHelper32.Guru;

namespace OpenPmsDental.Bridges {
	///<summary>Guru (MedVisor). Since S3a-14 of the .NET 10 move (docs/port-notes/S3a-14.md) Guru's native MedVisorInterface.dll is loaded and
	///called in the 32-bit helper program (OpenPmsDentalHelper32, its Guru module), because the program is 64-bit after stage 3b and no 64-bit
	///MedVisorInterface.dll is known (open point O-14). The helper calls MVStart and MVSendPatient, and builds MVSendPatient's structure from the
	///MVPatient filled here with the same ToCPlusPlusStructure as before, byte for byte.</summary>
	public class Guru {
		///<summary>How long one call of MedVisorInterface.dll may take (MVStart starts Guru when it is not running).</summary>
		private static readonly TimeSpan _timeSpanCall=TimeSpan.FromMinutes(5);

		/// <summary></summary>
		public Guru() {

		}

		///<summary>MedVisorInterface.MVStart in the helper. A missing MedVisorInterface.dll throws DllNotFoundException, as the call in the program's
		///process did.</summary>
		private static int MVStart() {
			try {
				return IntFromResult(Helper32Client.Default.Call(GuruCommands.MODULE,GuruCommands.MV_START,_timeSpanCall));
			}
			catch(Helper32Exception ex) when(ex.Failure==Helper32Failure.Remote && ex.RemoteTypeName==typeof(DllNotFoundException).FullName) {
				throw new DllNotFoundException(ex.Message,ex);
			}
		}

		///<summary>MedVisorInterface.MVSendPatient in the helper, with the structure it builds from mvPatient (ToCPlusPlusStructure).</summary>
		private static int MVSendPatient(MVPatient mvPatient) {
			string[] arrayArgs=GuruCommands.EncodePatient(mvPatient,out byte[] data);
			Helper32Result helper32Result=Helper32Client.Default.Call(GuruCommands.MODULE,GuruCommands.MV_SEND_PATIENT,arrayArgs,data,_timeSpanCall);
			return IntFromResult(helper32Result.Results);
		}

		private static int IntFromResult(string[] arrayResults) {
			return int.Parse(arrayResults[0],System.Globalization.NumberStyles.AllowLeadingSign,System.Globalization.CultureInfo.InvariantCulture);
		}

		/// <summary></summary>
		public static void SendData(Program ProgramCur,Patient pat) {
			if(pat==null) {
				MsgBox.Show("Guru","Please select a patient first.");
				return;
			}
			try {
				int errorNum=MVStart();
				if(errorNum!=0) {
					/* Error codes:
					MV_SUCCESS if successful
					MV_WRONG_PARAM if hPMS is not null and pContextString is null
					MV_WRONG_PARAM if pContextString is length 0 or is not null terminated within strSize characters
					MV_ERROR if an unknown error occurred

					Enumeration Name - E_MV_ERROR
					This enumeration defines the different return values of exported functions.  When successful, functions return MV_SUCCESS, an other value otherwise:  see the full list below.
					MV_SUCCESS - The function has succeeded
					MV_NOT_RUNNING - MV is not running
					MV_NOT_CREATED - MedVisor has not been created
					MV_NO_PATIENT - No patient has been set in MedVisor
					MV_REQUIRED_DATA - Required data has not been sent
					MV_WRONG_VALUE - Send data are not valid
					MV_FILE_NOT_FOUND - Sent file cannot be found
					MV_ERROR - General Error
					*/
					throw new ODException(Lang.g("Guru","MedVisorInterface.MVStart() returned with an error code of:")+$" {errorNum}");
				}
			}
			catch(DllNotFoundException e) {
				e.DoNothing();
				MessageBox.Show(Lans.g("Guru","Could not find MedVisorInterface.dll. Verify that Guru is installed."));
				return;
			}
			catch(Exception ex) {
				FriendlyException.Show(Lans.g("Guru","An error occurred when launching Guru."),ex);
				return;
			}
			MVPatient mvPatient = new MVPatient();
			mvPatient.LastName = Tidy(pat.LName,64);
			mvPatient.FirstName = Tidy(pat.FName,64);
			if(pat.Gender == PatientGender.Male) {
				mvPatient.Sex = Tidy("M",1);
			}
			else if(pat.Gender == PatientGender.Female) {
				mvPatient.Sex = Tidy("F",1);
			}
			else if(pat.Gender == PatientGender.Unknown) {
				mvPatient.Sex = Tidy("0",1);
			}
			mvPatient.BirthDate = Tidy(pat.Birthdate.ToString("MMddyyyy"),8);
			if(ProgramProperties.GetPropVal(ProgramCur.ProgramNum,"Enter 0 to use PatientNum, or 1 to use ChartNum")=="0") {
				mvPatient.ID=Tidy(pat.PatNum.ToString(),64);
			}
			else {
				mvPatient.ID=Tidy(pat.ChartNumber.ToString(),64);
			}
			if(pat.ImageFolder=="") {//Could happen if the images module has not been visited for a new patient.
				Patient patOld=pat.Copy();
				pat.ImageFolder=ImageStore.GetImageFolderName(pat);
				Patients.Update(pat,patOld);
			}
			string imagePath=CodeBase.ODFileUtils.CombinePaths(ProgramProperties.GetPropVal(ProgramCur.ProgramNum,"Guru image path"),pat.ImageFolder);
			mvPatient.Directory = Tidy(imagePath,259);
			if(MVSendPatient(mvPatient) != 0) {
				MsgBox.Show("Guru","An error has occurred.");
			}
		}
			//end


			//**********************************************************
			/*
			//It's common to build a string
			string str="";
			//ProgramProperties.GetPropVal() is the way to get program properties.
			if(ProgramProperties.GetPropVal(ProgramCur.ProgramNum,"Enter 0 to use PatientNum, or 1 to use ChartNum")=="0") {
				str+="Id="+pat.PatNum.ToString()+" ";
			}
			else {
				str+="Id="+Tidy(pat.ChartNumber)+" ";
			}
			//Nearly always tidy the names in one way or another
			str+=Tidy(pat.LName)+" ";
			//If birthdates are optional, only send them if they are valid.
			if(pat.Birthdate.Year>1880) {
				str+=pat.Birthdate.ToString("MM/dd/yyyy");
			}
			//This patterns shows a way to handle gender unknown when gender is optional.
			if(pat.Gender==PatientGender.Female) {
				str+="F ";
			}
			else if(pat.Gender==PatientGender.Male) {
				str+="M ";
			}
			try {
				ODFileUtils.ProcessStart(path,str);
			}
			catch(Exception ex) {
				MessageBox.Show(ex.Message);
			}	 
		}*/

		///<summary>Takes the input and modifies the length to the specified length. Appends a null, and returns the result as a character array.</summary>
		private static char[] Tidy(string input,int length) {
			string retVal=input;
			if(input.Length > length) {
				retVal=input.Substring(0,length);
			}
			if(input.Length < length) {
				retVal=input.PadRight(length,'\0');
			}
			retVal+='\0';
			return retVal.ToCharArray();
		}

		/////<summary>Defines the different return values of exported functions.</summary>
		//public enum E_MV_ERROR {
		//  ///<summary>The function has succeeded.</summary>
		//  MV_SUCCESS,
		//  ///<summary>MV is not running.</summary>
		//  MV_NOT_RUNNING,
		//  ///<summary>MedVisor has not been created.</summary>
		//  MV_NOT_CREATED,
		//  ///<summary>No patient has been set in MedVisor.</summary>
		//  MV_NO_PATIENT,
		//  ///<summary>Required data has not been sent.</summary>
		//  MV_REQUIRED_DATA,
		//  ///<summary>Send data are not valid.</summary>
		//  MV_WRONG_VALUE,
		//  ///<summary>Sent file cannot be found.</summary>
		//  MV_FILE_NOT_FOUND,
		//  ///<summary>General Error.</summary>
		//  MV_ERROR,
		//}
	}
}

/*
			//Insert Guru Bridge
				if(DataConnection.DBtype==DatabaseType.MySql) {
					command="INSERT INTO program (ProgName,ProgDesc,Enabled,Path,CommandLine,Note"
				    +") VALUES("
				    +"'Guru', "
				    +"'Guru from guru.waziweb.com', "
				    +"'0', "
				    +"'"+POut.String(@"C:\Program Files (x86)\AaTemplate\AaTemplate.exe")+"',"
				    +"'', "
				    +"'')";
					long programNum=Db.NonQ(command,true);
					command="INSERT INTO programproperty (ProgramNum,PropertyDesc,PropertyValue"
				    +") VALUES("
				    +"'"+POut.Long(programNum)+"', "
				    +"'Enter 0 to use PatientNum, or 1 to use ChartNum', "
				    +"'0')";
					Db.NonQ(command);
					command="INSERT INTO programproperty (ProgramNum,PropertyDesc,PropertyValue"
				    +") VALUES("
				    +"'"+POut.Long(programNum)+"', "
				    +"'Guru image path', "
				    +"'C:\')";
					Db.NonQ(command);
					command="INSERT INTO toolbutitem (ProgramNum,ToolBar,ButtonText) "
				    +"VALUES ("
				    +"'"+POut.Long(programNum)+"', "
				    +"'"+POut.Int(((int)ToolBarsAvail.ChartModule))+"', "
				    +"'Guru')";
					Db.NonQ(command);
				}
				else {//oracle
					command="INSERT INTO program (ProgramNum,ProgName,ProgDesc,Enabled,Path,CommandLine,Note"
				    +") VALUES("
				    +"(SELECT MAX(ProgramNum)+1 FROM program),"
				    +"'Guru', "
				    +"'Guru from guru.waziweb.com/', "
				    +"'0', "
				    +"'"+POut.String(@"C:\Program Files (x86)\AaTemplate\AaTemplate.exe")+"',"
				    +"'', "
				    +"'')";
					long programNum=Db.NonQ(command,true);
					command="INSERT INTO programproperty (ProgramPropertyNum,ProgramNum,PropertyDesc,PropertyValue"
				    +") VALUES("
				    +"(SELECT MAX(ProgramPropertyNum+1) FROM programproperty),"
				    +"'"+POut.Long(programNum)+"', "
				    +"'Enter 0 to use PatientNum, or 1 to use ChartNum', "
				    +"'0')";
					Db.NonQ(command);
					command="INSERT INTO programproperty (ProgramPropertyNum,ProgramNum,PropertyDesc,PropertyValue"
				    +") VALUES("
				    +"(SELECT MAX(ProgramPropertyNum+1) FROM programproperty),"
				    +"'"+POut.Long(programNum)+"', "
				    +"'Guru image path', "
				    +"'C:\')";
					Db.NonQ(command);
					command="INSERT INTO toolbutitem (ToolButItemNum,ProgramNum,ToolBar,ButtonText) "
				    +"VALUES ("
				    +"(SELECT MAX(ToolButItemNum)+1 FROM toolbutitem),"
				    +"'"+POut.Long(programNum)+"', "
				    +"'"+POut.Int(((int)ToolBarsAvail.ChartModule))+"', "
				    +"'Guru')";
					Db.NonQ(command);
				}//end Guru bridge
*/