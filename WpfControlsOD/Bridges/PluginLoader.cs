using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using OpenPmsDentalBusiness;
using CodeBase;
using DataConnectionBase;

namespace OpenPmsDental {
	public class PluginLoader {
		///<summary>There is a similar method for Middle Tier at OpenPmsDentalBusiness.Plugins.LoadAllPlugins.</summary>
		public static void LoadAllPlugins(Form host) {
			Meth.NoCheckMiddleTierRole();
			List<PluginContainer> listPluginContainers=new List<PluginContainer>();
			//Loop through all programs that are enabled with a plug-in dll name set.
			List<Program> listPrograms=Programs.GetWhere(x => x.Enabled && !string.IsNullOrEmpty(x.PluginDllName));
			for(int i=0;i<listPrograms.Count;i++) {
				string dllPath=ODFileUtils.CombinePaths(Application.StartupPath,listPrograms[i].PluginDllName);
				if(RemotingClient.MiddleTierRole==MiddleTierRole.ServerMT) {
					//The folder of this thread's request (it was HttpContext.Current.Server.MapPath(null)); throws on a thread that handles no request.
					dllPath=ODFileUtils.CombinePaths(MiddleTierHost.GetRequestContentRoot(),listPrograms[i].PluginDllName);
				}
				//Check for the versioning trigger.
				//For example, the plug-in might be entered as MyPlugin[VersionMajMin].dll. The bracketed section will be removed when loading the dll.
				//So it will look for MyPlugin.dll as the dll to load. However, before it loads, it will look for a similar dll with a version number.
				//For example, if using version 14.3.23, it would look for MyPlugin14.3.dll. 
				//If that file is found, it would replace MyPlugin.dll with the contents of MyPlugin14.3.dll, and then it would load MyPlugin.dll as normal.
				if(dllPath.Contains("[VersionMajMin]")) {
					Version vers=Assembly.GetAssembly(typeof(Db)).GetName().Version;
					string dllPathWithVersion=dllPath.Replace("[VersionMajMin]",vers.Major.ToString()+"."+vers.Minor.ToString());
					dllPath=dllPath.Replace("[VersionMajMin]","");//now stripped clean
					if(File.Exists(dllPathWithVersion)) {
						try {
							File.Copy(dllPathWithVersion,dllPath,true);
						}
						catch(Exception e) {
							 // Another instance of OD is tying up the file
							 // No need to copy file again, already done by instance holding file
							 e.DoNothing();
						}
					}
					else{
						//try the Plugins folder
						if(PrefC.AtoZfolderUsed!=DataStorageType.InDatabase) {//must have an AtoZ folder to check
							string dllPathVersionCentral=OpenPmsDentalBusiness.FileIO.FileAtoZ.CombinePaths(ImageStore.GetPreferredAtoZpath(),"Plugins",
								listPrograms[i].PluginDllName.Replace("[VersionMajMin]",vers.Major.ToString()+"."+vers.Minor.ToString()));
							if(OpenPmsDentalBusiness.FileIO.FileAtoZ.Exists(dllPathVersionCentral)) {
								try {
//todo: fix for cloud:
									//FileAtoZ.Copy(dllPathVersionCentral,dllPath,FileAtoZSourceDestination.AtoZToLocal,doOverwrite:true);
									File.Copy(dllPathVersionCentral,dllPath,overwrite:true);
								}
								catch(Exception e) {
									// Another instance of OD is tying up the file
									// No need to copy file again, already done by instance holding file
									e.DoNothing();
								}
							}
						}
					}
				}
				//We now know the exact name of the dll for the plug-in.  Check to see if it is present.
				if(!File.Exists(dllPath)) {
					continue;//Nothing to do.
				}
				//The dll was found, try and load it in. Plugins.LoadPlugin checks first that this process can load it (its PE kind and target
				//framework, N64-A), and writes the reason for a plug-in it refuses or that fails to load to the log, naming the plug-in. On .NET 10 a
				//plug-in built for the .NET Framework is refused with a clear message and not loaded (decision 16 of docs/specs/net10-plan.md, S3b-4):
				//its TargetFrameworkAttribute is read with System.Reflection.Metadata (CodeBase.PortableExecutableFile), without loading the file.
				PluginContainer pluginContainer=Plugins.LoadPlugin(dllPath,listPrograms[i].PluginDllName,listPrograms[i].ProgramNum,host,out string error);
				if(pluginContainer==null) {
					//Never try and show message boxes when on the middle tier, there is no UI.  We should instead log to a file or the event viewer.
					if(RemotingClient.MiddleTierRole!=MiddleTierRole.ServerMT) {
						//Notify the user that their plug-in is not loaded.
						MessageBox.Show("Error loading Plugin:"+listPrograms[i].PluginDllName+"\r\n"+error);
					}
					continue;//Don't add it to plugin list.
				}
				//The plug-in was successfully loaded and will start getting hook notifications.  Add it to the list of loaded plug-ins.
				listPluginContainers.Add(pluginContainer);
			}
			Plugins.ListPlugins=listPluginContainers;
		}
	}
}
