using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Management;
using System.ServiceProcess;
using System.Text;
using System.Text.RegularExpressions;
using System.Xml;

namespace CodeBase {
	///<summary>This is a helper class meant to be used to easily manage Windows services.</summary>
	public class ServicesHelper {
		//Services are installed and removed with the Windows Service Controller, sc.exe (S3a-7 of the .NET 10 move, docs/port-notes/S3a-7.md).
		//InstallUtil.exe and System.Configuration.Install do not exist on .NET, and InstallUtil cannot load a .NET 10 assembly. sc.exe creates
		//every service with the settings the HL7 service's ProjectInstaller gave through InstallUtil: its own process, started automatically
		//(not delayed), normal error control, the LocalSystem account, the display name equal to the service name, and no description.
		///<summary>sc.exe's type= value: a service in its own process (ServiceInstaller's Win32OwnProcess).</summary>
		public const string ScServiceType="own";
		///<summary>sc.exe's start= value: started automatically when Windows starts, not delayed (the ProjectInstaller's
		///ServiceStartMode.Automatic, DelayedAutoStart false).</summary>
		public const string ScStartType="auto";
		///<summary>sc.exe's error= value: Windows logs a start failure and goes on starting (ServiceInstaller's SERVICE_ERROR_NORMAL).</summary>
		public const string ScErrorControl="normal";
		///<summary>sc.exe's obj= value: the account the service runs as (the ProjectInstaller's ServiceAccount.LocalSystem).</summary>
		public const string ScAccount="LocalSystem";
		///<summary>The exit code (and Win32 error) of sc.exe when the caller may not install or remove services: not run as administrator.</summary>
		public const int ScErrorAccessDenied=5;
		///<summary>sc.exe's exit code when no service has the name (ERROR_SERVICE_DOES_NOT_EXIST).</summary>
		public const int ScErrorServiceDoesNotExist=1060;
		///<summary>sc.exe's exit code when the service is already marked for deletion (ERROR_SERVICE_MARKED_FOR_DELETE).</summary>
		public const int ScErrorServiceMarkedForDelete=1072;
		///<summary>sc.exe's exit code when a service with the name is already installed (ERROR_SERVICE_EXISTS).</summary>
		public const int ScErrorServiceExists=1073;

		///<summary>Starts a process (file name, arguments, working directory or empty), waits up to 10 seconds for it and returns its standard
		///output and exit code. Every process ServicesHelper starts (sc.exe, cmd.exe for sc sdshow/sdset, mysqld.exe --remove) goes through it.
		///Tests replace it with a fake, so that no test runs sc.exe or creates, deletes or configures a real service.</summary>
		internal static Func<string,string,string,ServiceProcessResult> FuncRunProcess=RunProcess;
		///<summary>Returns the ImagePath of the installed service with the name, from the registry, without its quotes; null when no service has
		///the name. Tests replace it with a fake.</summary>
		internal static Func<string,string> FuncGetImagePath=GetImagePathFromRegistry;
		///<summary>How long Uninstall waits for the service to stop after sc.exe delete, in seconds (S3a-FU1). InstallUtil's ServiceInstaller
		///waited 10 seconds for the stop and then slept 5 more; Stop's own default is 7.</summary>
		public const int UninstallStopTimeoutSeconds=15;
		///<summary>The Application event log, where InstallUtil's ServiceInstaller registered a source named after the service.</summary>
		public const string EventLogNameApplication="Application";
		///<summary>Stops the service with the name and waits for it up to UninstallStopTimeoutSeconds (Stop). Tests replace it with a fake, so that
		///no test stops a real service.</summary>
		internal static Func<string,bool> FuncStopService=serviceName => Stop(GetServiceByServiceName(serviceName),timeoutSeconds:UninstallStopTimeoutSeconds);
		///<summary>Registers the event log source in the Application log when it is not registered yet (RegisterEventLogSource). Called only
		///after sc.exe create succeeded, so only from an elevated process. Tests replace it with a fake, so that no test creates a real source.
		///</summary>
		internal static Action<string> ActionRegisterEventLogSource=RegisterEventLogSource;
		///<summary>Removes the event log source when it is registered in the Application log (RemoveEventLogSource). Called only after sc.exe delete
		///succeeded, so only from an elevated process. Tests replace it with a fake, so that no test deletes a real source.</summary>
		internal static Action<string> ActionRemoveEventLogSource=RemoveEventLogSource;

		///<summary>Executes a process and waits up to 10 seconds for the process to execute.</summary>
		private static void ExecuteProcess(string fileName,string arguments,out string standardOutput,out int exitCode,string workingDirectory="") {
			ServiceProcessResult serviceProcessResult=FuncRunProcess(fileName,arguments,workingDirectory??"");
			standardOutput=serviceProcessResult.StandardOutput??"";
			exitCode=serviceProcessResult.ExitCode;
		}

		///<summary>The process start behind FuncRunProcess. Redirects standard output, so it uses CreateProcess (UseShellExecute=false): the caller
		///must already be elevated to install or remove a service (ServiceManager is, by its manifest).</summary>
		private static ServiceProcessResult RunProcess(string fileName,string arguments,string workingDirectory) {
			Process process=new Process();
			if(!string.IsNullOrEmpty(workingDirectory)) {
				process.StartInfo.WorkingDirectory=workingDirectory;
			}
			process.StartInfo.FileName=fileName;
			process.StartInfo.Arguments=arguments;
			process.StartInfo.UseShellExecute=false;
			process.StartInfo.RedirectStandardOutput=true;
			process.Start();
			ServiceProcessResult serviceProcessResult=new ServiceProcessResult();
			serviceProcessResult.StandardOutput=process.StandardOutput.ReadToEnd();
			process.WaitForExit(10000);
			serviceProcessResult.ExitCode=process.ExitCode;
			return serviceProcessResult;
		}

		///<summary>The full path of sc.exe in the Windows system folder: System32 (which file system redirection turns into SysWOW64 for a
		///32-bit process on 64-bit Windows, which has its own sc.exe), never a sc.exe found on the PATH or in the current folder.</summary>
		public static string GetScExePath() {
			return Path.Combine(Environment.SystemDirectory,"sc.exe");
		}

		///<summary>The arguments of the sc.exe command that installs the service: the name, the exe's full path in quotes as the ImagePath (as
		///InstallUtil wrote it; the HL7 service finds its own name by the folder of its ImagePath), and the settings of the Sc constants. Throws
		///ArgumentException for a name or path sc.exe cannot take.</summary>
		public static string GetScCreateArguments(string serviceName,string exeFullPath) {
			ValidateServiceName(serviceName);
			//A path that ends in a backslash would put \" before the closing quote, which sc.exe reads as an escaped quote (S3a-FU1); it names a
			//folder anyway, not an exe.
			if(string.IsNullOrWhiteSpace(exeFullPath) || exeFullPath.Contains("\"") || exeFullPath.EndsWith("\\") || !Path.IsPathRooted(exeFullPath)) {
				throw new ArgumentException("The service's executable must be a full path without quotes that does not end in a backslash: "+exeFullPath);
			}
			//binPath= "\"C:\...\x.exe\"": sc.exe takes the quoted path, quotes included, as the ImagePath. sc.exe needs the space after each "=".
			return "create \""+serviceName+"\""
				+" binPath= \"\\\""+exeFullPath+"\\\"\""
				+" type= "+ScServiceType
				+" start= "+ScStartType
				+" error= "+ScErrorControl
				+" obj= "+ScAccount
				+" DisplayName= \""+serviceName+"\"";
		}

		///<summary>The arguments of the sc.exe command that removes the service. Throws ArgumentException for a name sc.exe cannot take.</summary>
		public static string GetScDeleteArguments(string serviceName) {
			ValidateServiceName(serviceName);
			return "delete \""+serviceName+"\"";
		}

		///<summary>A service name is not empty and holds no quote, slash or backslash (Windows refuses slashes in service names).</summary>
		private static void ValidateServiceName(string serviceName) {
			if(string.IsNullOrWhiteSpace(serviceName) || serviceName.IndexOfAny(new char[] { '"','/','\\' })>=0) {
				throw new ArgumentException("Invalid service name: "+serviceName);
			}
		}

		///<summary>The Win32 error of an sc.exe run: its exit code when not 0, else the number in a "[SC] ... FAILED nnnn:" line of its output,
		///else 0 (success).</summary>
		public static int GetScErrorCode(int exitCode,string standardOutput) {
			if(exitCode!=0) {
				return exitCode;
			}
			Match match=Regex.Match(standardOutput??"",@"FAILED\s+(\d+)\s*:");
			if(match.Success && int.TryParse(match.Groups[1].Value,out int errorCode)) {
				return errorCode;
			}
			return 0;
		}

		///<summary>What an sc.exe error means for the person installing or removing a service, in one sentence; empty for 0 (success). The
		///output of sc.exe itself is shown below it.</summary>
		public static string GetScErrorText(int errorCode) {
			switch(errorCode) {
				case 0:
					return "";
				case ScErrorAccessDenied:
					return "Access is denied. Installing or removing a service needs an administrator: run the program as administrator.";
				case ScErrorServiceDoesNotExist:
					return "No service with this name is installed.";
				case ScErrorServiceMarkedForDelete:
					return "The service is already marked for removal. Close the Services window and any program that has the service open, or restart the computer.";
				case ScErrorServiceExists:
					return "A service with this name is already installed.";
				default:
					return "sc.exe failed with error "+errorCode+".";
			}
		}

		///<summary>Returns true if the service was installed successfully.</summary>
		public static bool Install(string serviceName,FileInfo serviceFileInfo) {
			try {
				string standardOutput;
				int exitCode;
				Install(serviceName,serviceFileInfo,out standardOutput,out exitCode);
				//Check to see if the service was successfully installed.
				ServiceController serviceController=GetServiceByServiceName(serviceName);
				if(serviceController!=null) {
					//Attempt to give Everyone permission to manage the newly installed service.  Call the method that takes a list so no exception is thrown.
					SetSecurityDescriptorToAllowEveryoneToManageServices(new List<ServiceController>() { serviceController });
					return true;
				}
			}
			catch(Exception ex) {
				ex.DoNothing();
			}
			return false;
		}

		///<summary>Utilizes "sc.exe create" to install the serviceFileInfo with the corresponding serviceName passed in, with the settings of the
		///Sc constants (GetScCreateArguments). The out parameters "standardOutput" and "exitCode" will contain the results of the "Process"
		///execution: exitCode is sc.exe's Win32 error, for example ScErrorServiceExists or ScErrorAccessDenied (GetScErrorText).
		///After a successful create the event log sources of GetEventLogSourcesToRegister are registered in the Application log, as InstallUtil's
		///ServiceInstaller did for the source named after the service (S3a-FU1).</summary>
		public static void Install(string serviceName,FileInfo serviceFileInfo,out string standardOutput,out int exitCode) {
			ExecuteProcess(GetScExePath()
				,GetScCreateArguments(serviceName,serviceFileInfo.FullName)
				,out standardOutput
				,out exitCode
				,serviceFileInfo.DirectoryName);
			if(GetScErrorCode(exitCode,standardOutput)!=0) {
				return;//Nothing installed; a refused create (5) also means this process is not elevated, and registering a source needs that too.
			}
			foreach(string source in GetEventLogSourcesToRegister(serviceName,serviceFileInfo.Name)) {
				ActionRegisterEventLogSource(source);
			}
		}

		///<summary>The event log sources Install registers for a new service: the one named after the service (InstallUtil's ServiceInstaller
		///registered it, and ServiceBase writes the service's start and stop entries under it), and for the HL7 service's exe also
		///ProgramNames.EventLogSourceHL7, the source its messages are written under whatever the service is named. Registering it while elevated
		///means a later run that is not elevated (the HL7 debug window) can write without creating it, which needs an administrator.
		///Uninstall removes only the source named after the service: another HL7 service may still write under EventLogSourceHL7.</summary>
		public static List<string> GetEventLogSourcesToRegister(string serviceName,string exeFileName) {
			List<string> listSources=new List<string>() { serviceName };
			if(string.Equals(exeFileName,ProgramNames.HL7ServiceName+".exe",StringComparison.OrdinalIgnoreCase)
				&& !listSources.Contains(ProgramNames.EventLogSourceHL7,StringComparer.OrdinalIgnoreCase))
			{
				listSources.Add(ProgramNames.EventLogSourceHL7);
			}
			return listSources;
		}

		///<summary>The real ActionRegisterEventLogSource: registers the source in the Application log unless it is registered already, in any log.
		///A failure is ignored: the service is installed, and Windows creates the source at the service's first write (it runs as LocalSystem).
		///</summary>
		private static void RegisterEventLogSource(string source) {
			try {
				if(!EventLog.SourceExists(source)) {
					EventLog.CreateEventSource(source,EventLogNameApplication);
				}
			}
			catch(Exception ex) {
				ex.DoNothing();
			}
		}

		///<summary>The real ActionRemoveEventLogSource: removes the source only when it is registered in the Application log and is not the main
		///program's source (ProgramNames.EventLogSource) or the log's own name; a source of another log is never touched. A failure is ignored: a
		///source left behind does no harm.</summary>
		private static void RemoveEventLogSource(string source) {
			try {
				if(string.Equals(source,ProgramNames.EventLogSource,StringComparison.OrdinalIgnoreCase)
					|| string.Equals(source,EventLogNameApplication,StringComparison.OrdinalIgnoreCase))
				{
					return;
				}
				if(EventLog.SourceExists(source)
					&& string.Equals(EventLog.LogNameFromSourceName(source,"."),EventLogNameApplication,StringComparison.OrdinalIgnoreCase))
				{
					EventLog.DeleteEventSource(source);
				}
			}
			catch(Exception ex) {
				ex.DoNothing();
			}
		}

		public static bool IsMySQLService(string pathToExe) {
			return pathToExe.ToLower().Contains("mysqld.exe");
		}

		///<summary>Returns true if the service was able to uninstall successfully.</summary>
		public static bool Uninstall(ServiceController service) {
			try {
				string standardOutput;
				int exitCode;
				Uninstall(service.ServiceName,out standardOutput,out exitCode);
				//Check to see if the service was successfully removed.
				return (GetServiceByServiceName(service.ServiceName)==null);
			}
			catch(Exception ex) {
				ex.DoNothing();
			}
			return false;
		}

		private static void UninstallMySQLService(string mySQLServiceImagePath,string serviceName,out string standardOutput,out int exitCode) {
			FuncStopService(serviceName);
			string pathToExe=GetMySQLPathToExe(mySQLServiceImagePath);
			//C:\Program Files\MariaDB 10.5\bin
			string pathBin=Path.GetDirectoryName(pathToExe);
			ExecuteProcess("CMD.exe","/C mysqld.exe --remove "+serviceName,out standardOutput,out exitCode,workingDirectory:pathBin);
		}

		///<summary>Utilizes "sc.exe delete" to uninstall the service with the serviceName passed in, then stops it if it runs (Windows removes a
		///service marked for deletion once it has stopped), in the order InstallUtil's ServiceInstaller used.
		///Uses the local machines registry in order to find the corresponding service that shares the serviceName passed in: a MariaDB or MySQL
		///service is removed by its own mysqld.exe --remove instead.
		///The out parameters "standardOutput" and "exitCode" will contain the results of the "Process" execution: exitCode is sc.exe's Win32
		///error, for example ScErrorServiceDoesNotExist or ScErrorAccessDenied (GetScErrorText).</summary>
		public static void Uninstall(string serviceName,out string standardOutput,out int exitCode) {
			Uninstall(serviceName,out standardOutput,out exitCode,out bool _);
		}

		///<summary>Uninstall, and isMySQLService tells which branch it took (S3a-FU1): true when the registry's ImagePath was a mysqld.exe and the
		///service was removed by mysqld.exe --remove (exitCode is then mysqld's own exit code), false when sc.exe delete ran (exitCode is
		///sc.exe's Win32 error, read with GetScErrorCode). The caller reads the result by this, not by a path it has itself.
		///After a successful sc.exe delete (or 1072, already marked for deletion) the service is stopped and the event log source named after the
		///service is removed, as InstallUtil's ServiceInstaller did.</summary>
		public static void Uninstall(string serviceName,out string standardOutput,out int exitCode,out bool isMySQLService) {
			string imagePath=FuncGetImagePath(serviceName);
			isMySQLService=(imagePath!=null && IsMySQLService(imagePath));
			if(isMySQLService) {
				UninstallMySQLService(imagePath,serviceName,out standardOutput,out exitCode);
				return;
			}
			//No registry key: sc.exe answers ScErrorServiceDoesNotExist (1060) itself.
			ExecuteProcess(GetScExePath()
				,GetScDeleteArguments(serviceName)
				,out standardOutput
				,out exitCode);
			int errorCode=GetScErrorCode(exitCode,standardOutput);
			//Also stop a service already marked for deletion (1072): an earlier delete whose stop timed out leaves it running, and Windows removes
			//it only once it has stopped.
			if(errorCode==0 || errorCode==ScErrorServiceMarkedForDelete) {
				FuncStopService(serviceName);
				ActionRemoveEventLogSource(serviceName);
			}
		}

		///<summary>The ImagePath of the installed service from HKLM\System\CurrentControlSet\Services\serviceName without its quotes, or null
		///when there is no such key or value.</summary>
		private static string GetImagePathFromRegistry(string serviceName) {
			using(RegistryKey registryKey=Registry.LocalMachine.OpenSubKey(@"System\CurrentControlSet\Services\"+serviceName)) {
				object imagePath=registryKey?.GetValue("ImagePath");
				if(imagePath==null) {
					return null;
				}
				return imagePath.ToString().Replace("\"","");
			}
		}

		///<summary>Returns true if the service was able to start successfully.  Set hasExceptions to true if the exception is desired instead.</summary>
		public static bool Start(string serviceName,bool hasExceptions=false) {
			return Start(GetServiceByServiceName(serviceName),hasExceptions);
		}

		///<summary>Returns true if the service was able to start successfully.  Set hasExceptions to true if the exception is desired instead.
		///Set timeoutSeconds to the number of seconds that the service controller should wait for the service to report 'Running'.</summary>
		public static bool Start(ServiceController service,bool hasExceptions=false,int timeoutSeconds=7) {
			try {
				//Only attempt to start services that are of status stopped or stop pending.
				//If we do not do this, an InvalidOperationException will throw that says "An instance of the service is already running"
				if(service.Status!=ServiceControllerStatus.Stopped && service.Status!=ServiceControllerStatus.StopPending) {
					return true;
				}
				service.MachineName=Environment.MachineName;
				service.Start();
				service.WaitForStatus(ServiceControllerStatus.Running,TimeSpan.FromSeconds(timeoutSeconds));
			}
			catch(Exception ex) {
				if(hasExceptions) {
					throw ex;
				}
				return false;
			}
			return true;
		}

		///<summary>Starts all services passed in.
		///If hasExceptions is set to false then a string will be returned indicating which services did not start.  Empty string if all started.
		///If hasExceptions is set to true, there is a chance that not all services will start.</summary>
		public static string StartServices(List<ServiceController> listServices,bool hasExceptions=false) {
			StringBuilder stringBuilderErrors=new StringBuilder();
			foreach(ServiceController service in listServices) {
				try {
					if(!Start(service,hasExceptions)) {
						stringBuilderErrors.AppendLine(service.DisplayName);
					}
				}
				catch(Exception ex) {
					if(hasExceptions) {
						throw ex;
					}
					stringBuilderErrors.AppendLine(service.DisplayName);
				}
			}
			return stringBuilderErrors.ToString();
		}

		///<summary>Returns true if the service was able to stop successfully.  Set hasExceptions to true if the exception is desired instead.</summary>
		public static bool Stop(string serviceName,bool hasExceptions=false) {
			return Stop(GetServiceByServiceName(serviceName),hasExceptions);
		}

		///<summary>Returns true if the service was able to stop successfully.  Set hasExceptions to true if the exception is desired instead.</summary>
		public static bool Stop(ServiceController service,bool hasExceptions=false,int timeoutSeconds=7) {
			try {
				if(service.Status==ServiceControllerStatus.Stopped || service.Status==ServiceControllerStatus.StopPending) {
					return true;//Already stopped.  Calling Stop again will guarantee an exception is thrown.  Do not try calling Stop on a stopped service.
        }
        service.MachineName=Environment.MachineName;
        service.Stop();
				service.WaitForStatus(ServiceControllerStatus.Stopped,TimeSpan.FromSeconds(timeoutSeconds));
			}
			catch(Exception ex) {
				if(hasExceptions) {
					throw ex;
				}
				return false;
			}
			return true;
		}

		///<summary>Stops all services passed in.
		///If hasExceptions is set to false then a string will be returned indicating which services did not stop.  Empty string if all stopped.
		///If hasExceptions is set to true, there is a chance that not all services will be stopped.</summary>
		public static string StopServices(List<ServiceController> listServices,bool hasExceptions=false) {
			StringBuilder stringBuilderErrors=new StringBuilder();
			foreach(ServiceController service in listServices) {
				try {
					if(!Stop(service,hasExceptions)) {
						stringBuilderErrors.AppendLine(service.DisplayName);
					}
				}
				catch(Exception ex) {
					if(hasExceptions) {
						throw ex;
					}
					stringBuilderErrors.AppendLine(service.DisplayName);
				}
			}
			return stringBuilderErrors.ToString();
		}

		///<summary>Returns a list of all services.</summary>
		public static List<ServiceController> GetServices() {
			return ServiceController.GetServices().ToList();
		}

		///<summary>Returns a list of all services that have a registry entry where the ImagePath is pointing to the executable name passed in.
		///Computers can have mulitple services installed each named uniquely but all pointing to the same executable or different versions of the same
		///executable.  Sometimes an action needs to be taken on all services that share an exe.  E.g. shutting down all eConnectors.</summary>
		public static List<ServiceController> GetServicesByRegistryImagePath(string serviceExeName) {
			List<ServiceController> listServices=GetServices();
			List<ServiceController> listMatchingServices=new List<ServiceController>();
			foreach(ServiceController service in listServices) {
				RegistryKey hklm=Registry.LocalMachine;
				hklm=hklm.OpenSubKey(@"System\CurrentControlSet\Services\"+service.ServiceName);
				string[] arrayExePath=hklm.GetValue("ImagePath").ToString().Replace("\"","").Split('\\');
				//This will not work if in the future we allow command line args for the listener service that include paths.
				if(arrayExePath[arrayExePath.Length-1].StartsWith(serviceExeName)) {
					listMatchingServices.Add(service);
				}
			}
			return listMatchingServices;
		}

		///<summary>Returns the service with the specified name.  Returns null if not found.</summary>
		public static ServiceController GetServiceByServiceName(string serviceName,bool isCaseSensitive=true,bool canPartialMatch=false) {
			if(isCaseSensitive && canPartialMatch) {
				return GetServices().FirstOrDefault(x => x.ServiceName.Contains(serviceName));
			}
			else if(isCaseSensitive && !canPartialMatch) {
				return GetServices().FirstOrDefault(x => x.ServiceName==serviceName);
			}
			else if(!isCaseSensitive && canPartialMatch) {
				return GetServices().FirstOrDefault(x => x.ServiceName.ToLower().Contains(serviceName.ToLower()));
			}
			else if(!isCaseSensitive && !canPartialMatch) {
				return GetServices().FirstOrDefault(x => x.ServiceName.ToLower()==serviceName.ToLower());
			}
			return null;
		}

		///<summary>Returns all services whose names start with ProgramNames.ServicePrefix, or with ProgramNames.LegacyServicePrefix (Open
		///Dental's services, or ones installed before the rename). For listing only (ServiceManager); never start these. The services the program
		///starts are GetServicesToStart.</summary>
		public static List<ServiceController> GetAllOpenDentServices() {
			return GetServices().FindAll(x => ProgramNames.HasServicePrefix(x.ServiceName));
		}

		///<summary>Executable names of Open Dental Software, Inc.'s own services that relay to Open Dental HQ: the eConnector (heartbeats, eService
		///relay, statistics and log uploads, supplemental backups to HQ) and the legacy CustListener it replaced. Neither is built from this repo.</summary>
		private static readonly string[] _arrayHqRelayServiceExeNames=new string[] { "OpenDentalEConnector.exe","OpenDentalCustListener.exe" };

		///<summary>The installed services the program starts by itself (FormOpenDental's service starter, on the computer named in the
		///WebServiceServerName preference): this program's own services only, see GetServiceNamesToStart. Services that run an Open Dental HQ
		///relay exe are found by their exe and left out.</summary>
		public static List<ServiceController> GetServicesToStart() {
			List<ServiceController> listServiceControllers=GetServices().FindAll(x => ProgramNames.IsProgramServiceName(x.ServiceName));
			if(listServiceControllers.Count==0) {
				return listServiceControllers;
			}
			List<string> listServiceNamesExcluded=new List<string>();
			for(int i=0;i<_arrayHqRelayServiceExeNames.Length;i++) {
				//GetServicesByExe reads each service's ImagePath from the registry and throws if a key can't be read.
				try {
					listServiceNamesExcluded.AddRange(GetServicesByExe(_arrayHqRelayServiceExeNames[i]).Select(x => x.ServiceName));
				}
				catch(Exception ex) {
					ex.DoNothing();
				}
			}
			List<string> listServiceNamesToStart=GetServiceNamesToStart(listServiceControllers.Select(x => x.ServiceName),listServiceNamesExcluded);
			return listServiceControllers.FindAll(x => listServiceNamesToStart.Contains(x.ServiceName));
		}

		///<summary>The names in listServiceNames of the services the program may start by itself, in their order: only this program's own
		///services, whose names start with ProgramNames.ServicePrefix (ProgramNames.IsProgramServiceName). A name that starts with
		///ProgramNames.LegacyServicePrefix is Open Dental's service (OpenDentalService, its HL7 service, an eConnector left behind by an Open
		///Dental install) and is never started. Also left out: the names in listServiceNamesExcluded (the services that run an HQ relay exe), and any
		///name containing EConnector or CustListener, in case the lookup by exe failed. Never null.</summary>
		public static List<string> GetServiceNamesToStart(IEnumerable<string> listServiceNames,IEnumerable<string> listServiceNamesExcluded) {
			List<string> listServiceNamesToStart=new List<string>();
			if(listServiceNames==null) {
				return listServiceNamesToStart;
			}
			List<string> listExcluded=(listServiceNamesExcluded==null) ? new List<string>() : listServiceNamesExcluded.ToList();
			foreach(string serviceName in listServiceNames) {
				if(!ProgramNames.IsProgramServiceName(serviceName)
					|| listExcluded.Contains(serviceName)
					|| serviceName.IndexOf("EConnector",StringComparison.OrdinalIgnoreCase)>=0
					|| serviceName.IndexOf("CustListener",StringComparison.OrdinalIgnoreCase)>=0)
				{
					continue;
				}
				listServiceNamesToStart.Add(serviceName);
			}
			return listServiceNamesToStart;
		}

		public static string GetMySQLPathToExe(string imagePath) {
			//"C:\Program Files\MariaDB 10.5\bin\mysqld.exe" --defaults-file="C:\Program Files\MariaDB 10.5\my.ini" MariaDB
			imagePath=imagePath.Replace("\"","");
			Match match=Regex.Match(imagePath,@"^(.*?mysqld.exe)");
			if(match.Success) {
				return match.Groups[1].Value;
			}
			return imagePath;
		}

		///<summary>Returns all services that their "Path to executeable" contains the passed in executable name.  Throws exceptions.</summary>
		///<param name="exeName">E.g. OpenDentalCustListener.exe</param>
		public static List<ServiceController> GetServicesByExe(string exeName) {
			RegistryKey hklm;
			List<ServiceController> retVal=new List<ServiceController>();
			List<ServiceController> listServices=GetServices();
			foreach(ServiceController serviceCur in listServices) {
				hklm=Registry.LocalMachine;
				hklm=hklm.OpenSubKey(Path.Combine(@"System\CurrentControlSet\Services\",serviceCur.ServiceName));
				if(hklm.GetValue("ImagePath")==null) {
					continue;
				}
				string installedServicePath=hklm.GetValue("ImagePath").ToString().Replace("\"","");
				if(installedServicePath.Contains(exeName)) {
					retVal.Add(serviceCur);
				}
			}
			return retVal;
		}

		///<summary>Returns one service that has "Path to executeable" set to the full path passed in.  Returns null if not found.</summary>
		///<param name="exeFullPath">E.g. C:\Program Files(x86)\Open Dental\OpenDentalCustListener\OpenDentalCustListener.exe</param>
		public static ServiceController GetServiceByExeFullPath(string exeFullPath) {
			return GetServicesByExe(exeFullPath).FirstOrDefault();
		}

		///<summary>Returns true if the service passed in allows "Everyone" to manage the service (start / stop).
		///Throws exceptions if anything went wrong accessing the security descriptor for the service controller passed in.</summary>
		public static bool GetIsEveryoneAllowedToManageService(ServiceController service) {
			//Use the Windows Service Controller to show the "security descriptor" for the current service.
			string standardOutputShow;
			int exitCodeShow;
			GetSecurityDescriptorForService(service.ServiceName,out standardOutputShow,out exitCodeShow);
			if(exitCodeShow!=0) {
				throw new ApplicationException("Error showing security descriptor.  Error code: "+exitCodeShow
					+"\r\n"+standardOutputShow);
			}
			//The security descriptor for all Open Dental services should ALWAYS contain the portion to grant all users permissions to stop and start.
			//https://msdn.microsoft.com/en-us/library/aa374928(v=vs.85)
			//The "ace type" section------------------------------------------------------------------------------------------------------------------------
			//A: ACCESS_ALLOWED_ACE_TYPE
			//The "rights" section--------------------------------------------------------------------------------------------------------------------------
			//RP: ADS_RIGHT_DS_READ_PROP - Read the properties of a DS object.
			//WP: ADS_RIGHT_DS_WRITE_PROP - Write properties for a DS object.
			//CR: ADS_RIGHT_DS_CONTROL_ACCESS - Access allowed only after extended rights checks supported by the object are performed. 
			//		This flag can be used alone to perform all extended rights checks on the object or it can be combined with an identifier of a specific 
			//		extended right to perform only that check.
			//The last two letters define the security principal assigned with these permissions a SID or well known aliases--------------------------------
			//WD - Everyone
			return standardOutputShow.Contains("(A;;RPWPCR;;;WD)");
		}

		///<summary>Adds the ability for Everyone to manage the passed in services by manipulating the security descriptor.
		///Tries to manipulate the security descriptor for every service passed in.  Silently fails if unable to apply the new permission.</summary>
		public static void SetSecurityDescriptorToAllowEveryoneToManageServices(List<ServiceController> listServices) {
			foreach(ServiceController service in listServices) {
				try {
					SetSecurityDescriptorToAllowEveryoneToManageService(service);
				}
				catch(Exception ex) {
					ex.DoNothing();
				}
			}
		}

		///<summary>Adds the ability for Everyone to manage the passed in service by manipulating the security descriptor.
		///Throws exceptions if anything goes wrong manipulating the security descriptor for the service passed in.</summary>
		public static void SetSecurityDescriptorToAllowEveryoneToManageService(ServiceController service) {
			if(GetIsEveryoneAllowedToManageService(service)) {
				return;//Nothing to do, everyone can already manage the service passed in.
			}
			string standardOutputShow;
			int exitCodeShow;
			GetSecurityDescriptorForService(service.ServiceName,out standardOutputShow,out exitCodeShow);
			if(exitCodeShow!=0) {
				throw new ApplicationException("Error showing security descriptor.  Error code: "+exitCodeShow
					+"\r\n"+standardOutputShow);
			}
			//All users cannot correctly manage the service yet so we need to use the security descriptor setter to add the correct permission.
			//Always preserve whatever permissions were already set for the service.
			//However, we need to insert the new permission just before the SACL section ("S:" section).
			//https://msdn.microsoft.com/en-us/library/aa379570(v=vs.85)
			string securityDescriptor=standardOutputShow.Trim();
			int startIndex=securityDescriptor.IndexOf("S:");
			if(startIndex==-1) {
				//Unneeded components can be omitted from the security descriptor string and I'm not quite sure what to do in this case.
				//For now we will throw an error and have the customers call us so we can investigate further.  It has always been present in my testing.
				throw new ApplicationException("Error setting security descriptor; No SACL found.");
			}
			securityDescriptor=securityDescriptor.Insert(startIndex,"(A;;RPWPCR;;;WD)");
			//Update the current security descriptor with our new permission.
			string standardOutputSet;
			int exitCodeSet;
			SetSecurityDescriptorForService(service.ServiceName,securityDescriptor,out standardOutputSet,out exitCodeSet);
			if(exitCodeSet!=0) {
				throw new ApplicationException("Error setting security descriptor.  Error code: "+exitCodeSet
					+"\r\n"+standardOutputSet);
			}
		}

		///<summary>Uses the service controller sdshow command to get the security descriptor for the passed in serviceName.
		///The out parameters "standardOutput" and "exitCode" will contain the results of the "Process" execution.</summary>
		private static void GetSecurityDescriptorForService(string serviceName,out string standardOutput,out int exitCode) {
			//Use the Windows Service Controller to show the "security descriptor" for the current service via a command prompt.
			ExecuteProcess("cmd.exe"
				,"/C sc sdshow "+serviceName
				,out standardOutput
				,out exitCode);
		}

		///<summary>Uses the service controller sdset command to set the passed in serviceName to the passed in securityDescriptor.
		///The out parameters "standardOutput" and "exitCode" will contain the results of the "Process" execution.</summary>
		private static void SetSecurityDescriptorForService(string serviceName,string securityDescriptor,out string standardOutput,out int exitCode) {
			//Use the Windows Service Controller to set the "security descriptor" for the current service via a command prompt.
			ExecuteProcess("cmd.exe"
				,"/C sc sdset "+serviceName+" \""+securityDescriptor+"\""
				,out standardOutput
				,out exitCode);
		}
		
		///<summary>Creates a default ServiceConfig file at the full file path provided.
		///The config contains the current connection settings in DataConnection and defaults LogLevelOfApplication to 'Error'.</summary>
		public static bool CreateServiceConfigFile(string filePath,string serverName,string databaseName,string mySqlUser,string mySqlPass
			,string mySqlPassHash,string mySqlUserLow="",string mySqlUserPassLow="",string note=null) 
		{
			XmlDocument document=new XmlDocument();
			//Creating Nodes
			XmlNode nodeConnSettings=document.CreateNode(XmlNodeType.Element,"ConnectionSettings","");
			XmlNode nodeDbeConn=document.CreateNode(XmlNodeType.Element,"DatabaseConnection","");
			XmlNode nodeCompName=document.CreateNode(XmlNodeType.Element,"ComputerName","");
			nodeCompName.InnerText=serverName;
			XmlNode nodeDatabase=document.CreateNode(XmlNodeType.Element,"Database","");
			nodeDatabase.InnerText=databaseName;
			XmlNode nodeUser=document.CreateNode(XmlNodeType.Element,"User","");
			nodeUser.InnerText=mySqlUser;
			XmlNode nodePassword=document.CreateNode(XmlNodeType.Element,"Password","");
			nodePassword.InnerText=string.IsNullOrEmpty(mySqlPassHash) ? mySqlPass : "";//write plain text pwd if encryption failed
			XmlNode nodePassHash=document.CreateNode(XmlNodeType.Element,"MySQLPassHash","");
			nodePassHash.InnerText=mySqlPassHash ?? "";//write encrypted password, if it's null then write an empty string
			XmlNode nodeUserLow=document.CreateNode(XmlNodeType.Element,"UserLow","");
			nodeUserLow.InnerText=mySqlUserLow;
			XmlNode nodePasswordLow=document.CreateNode(XmlNodeType.Element,"PasswordLow","");
			nodePasswordLow.InnerText=mySqlUserPassLow;
			XmlNode nodeDbType=document.CreateNode(XmlNodeType.Element,"DatabaseType","");
			nodeDbType.InnerText="MySql";//Not going to support Oracle until someone complains.
			XmlNode nodeLogLevelOfApp=document.CreateNode(XmlNodeType.Element,"LogLevelOfApplication","");
			nodeLogLevelOfApp.InnerText="Error";
			//Assigning Structure
			nodeDbeConn.AppendChild(nodeCompName);
			nodeDbeConn.AppendChild(nodeDatabase);
			nodeDbeConn.AppendChild(nodeUser);
			nodeDbeConn.AppendChild(nodePassword);
			nodeDbeConn.AppendChild(nodePassHash);
			nodeDbeConn.AppendChild(nodeUserLow);
			nodeDbeConn.AppendChild(nodePasswordLow);
			nodeDbeConn.AppendChild(nodeDbType);
			if(note!=null) {
				XmlNode nodeNote=document.CreateNode(XmlNodeType.Element,"Note","");
				nodeNote.InnerText=note;
				nodeDbeConn.AppendChild(nodeNote);
			}
			nodeConnSettings.AppendChild(nodeDbeConn);
			nodeConnSettings.AppendChild(nodeLogLevelOfApp);
			document.AppendChild(nodeConnSettings);
			//Outputting completed XML document
			StringBuilder strb=new StringBuilder();
			XmlWriterSettings settings=new XmlWriterSettings();
			settings.Indent=true;
			settings.IndentChars="   ";
			settings.NewLineChars="\r\n";
			settings.OmitXmlDeclaration=true;
			try {
				using(XmlWriter xmlWriter=XmlWriter.Create(strb,settings)) {
					document.WriteTo(xmlWriter);
					xmlWriter.Flush();
					File.WriteAllText(filePath,strb.ToString());
				}
				return true;
			}
			catch {
				return false;
			}
		}

		///<summary>Returns true if a service is currently installed with the specified service name.
		///Optionally pass in the file info for the desired service and this method will also return true if there is a service utilizing the same 
		///executable.</summary>
		public static bool HasService(string serviceName,FileInfo serviceFileInfo = null) {
			//Old way is to search the registry. This can tend to throw exceptions based on Windows user permissions, especially if runnong on a Domain Controller.
			//Let's try it this way first so as not to break any back-compatibility.
			try {
				List<ServiceController> listServices=GetServices();
				foreach(ServiceController service in listServices) {
					if(serviceName==service.ServiceName) {
						return true;
					}
					if(serviceFileInfo==null) {
						continue;
					}
					RegistryKey hklm=Registry.LocalMachine;
					hklm=hklm.OpenSubKey(@"System\CurrentControlSet\Services\"+service.ServiceName);
					string installedServicePath=hklm.GetValue("ImagePath").ToString().Replace("\"","");
					if(installedServicePath.Contains(serviceFileInfo.FullName)) {
						return true;
					}
				}
				return false;
			}
			catch(Exception e) {
				e.DoNothing();
			}
			//Querying the registry failed so let's try querying Window Management Interface (WMI).
			//If this throws also then let it throw. It means we absolutely cannot determine that status of this service.
			var asWmi=ODWmiService.GetServices();
			if(asWmi.Any(x => string.Compare(x.Name,serviceName,true)==0)) { //Name match. Already exists.
				return true;
			}
			//Check the new path against all installed service paths. Typically installed service paths are encapsulated by \"  \" so use Contains().
			if(serviceFileInfo!=null && asWmi.Any(x => x.PathName.ToLower().Contains(serviceFileInfo.FullName.ToLower()))) { //Path match. Already exists.
				return true;
			}
			//Service does not exist.
			return false;
		}

		///<summary>Throws exception if method cannot find a service. Gets the service name that matches the process ID we are searching for. If a valid process ID is passed in, use that. Otherwise, use the current process's ID.</summary>
		public static string GetProcessServiceName(int processId=0) {
			int processIdCur=processId;
			if(processId <= 0) {
				processIdCur=Process.GetCurrentProcess().Id;
			}
			ODWmiService curService=ODWmiService.GetServices().FirstOrDefault(x => x.ProcessId==processIdCur);
			if(curService==null) {
				throw new Exception("Unable to find service name.");
			}
			return curService.Name;
		}

		///<summary>Helper class that offers an alternative to querying windows to find out which services are installed.
		///Use this alternative when the old "search the registry" routine fails.</summary>
		private class ODWmiService {
			public string Description;
			public string DisplayName;
			public string Name;
			public string PathName;
			public bool Started;
			public string StartMode;
			public string StartName;
			public string State;
			public uint ProcessId;

			public static List<ODWmiService> GetServices() {
				ManagementObjectSearcher searcher=new ManagementObjectSearcher("SELECT * FROM Win32_Service");
				ManagementObjectCollection collection=searcher.Get();
				return collection
					.Cast<ManagementObject>()
					.Select(x => new ODWmiService() {
						Description=(string)x.Properties["Description"].Value??"",
						DisplayName=(string)x.Properties["DisplayName"].Value??"",
						Name=(string)x.Properties["Name"].Value??"",
						PathName=(string)x.Properties["PathName"].Value??"",
						Started=(bool)x.Properties["Started"].Value,
						StartMode=(string)x.Properties["StartMode"].Value??"",
						StartName=(string)x.Properties["StartName"].Value??"",
						State=(string)x.Properties["State"].Value??"",
						ProcessId=(uint)x.Properties["ProcessId"].Value,
					}).ToList();
			}
		}
	}

	///<summary>What a process ServicesHelper ran gave back: its standard output and its exit code.</summary>
	public class ServiceProcessResult {
		public string StandardOutput="";
		public int ExitCode;
	}
}
