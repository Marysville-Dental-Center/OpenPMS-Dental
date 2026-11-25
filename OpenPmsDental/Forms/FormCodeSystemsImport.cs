using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading;
using System.Windows.Forms;
using System.IO.Compression;
using CodeBase;
using OpenPmsDental.UI;
using OpenPmsDentalBusiness;

namespace OpenPmsDental {
	///<summary>Imports CPT codes from the CPT data file (.zip) that the office licenses from the American Medical Association, and the eCQM value sets
	///from the office's own download of VSAC's release for the 2027 measures, published 2026-05-14, with its Direct Reference Codes and Code System
	///Versions files, picked together (EhrValueSetImport; docs/specs/ehr-rebuild-plan.md, EHR-2; docs/specs/ehr-2027-measures-plan.md, EHR-6a),
	///which the EHR pick lists and the quality measures use. Both imports need the Setup permission (checked here, as well as by the setup window
	///that hides the button) and a direct connection.
	///The other code systems (CDCREC, CVX, HCPCS, ICD-9-CM, ICD-10-CM, LOINC, RxNorm, SNOMED CT, SOP and UCUM) used to be downloaded from Open Dental's
	///servers in Open Dental's own file format. Those downloads are removed and not replaced; that data is loaded into the database directly
	///(owner decision of 2026-10-18). The code-system status below the imports shows, for each code system, the rows in its table next to the
	///minimum the code list expects, so the office knows which tables it still has to load.</summary>
	public partial class FormCodeSystemsImport:FormODBase {
		///<summary>The CPT row of the codesystem table. Null if the database has no CPT row.</summary>
		private CodeSystem _codeSystemCpt;
		///<summary>Indicates if user has imported codes while in the window.</summary>
		private bool _hasImported;
		///<summary>Current status of the CPT import, shown in the grid.</summary>
		private string _status="";
		///<summary>Current status of the value-set import, shown in the grid.</summary>
		private string _statusValueSets="";
		///<summary>The Tag of the value-set row in gridMain.</summary>
		private const string TAG_VALUE_SETS="EhrValueSets";

		public FormCodeSystemsImport() {
			InitializeComponent();
			InitializeLayoutManager();
			Lan.F(this);
		}

		private void FormCodeSystemsImport_Load(object sender,EventArgs e) {
			UpdateCodeSystemThread.Finished+=new EventHandler(UpdateCodeSystemThread_FinishedSafe);
			FillGrid();
			FillGridStatus();
		}

		private void FillGrid() {
			_codeSystemCpt=CodeSystems.GetForCurrentVersion(true).FirstOrDefault(x => x.CodeSystemName=="CPT");
			gridMain.BeginUpdate();
			gridMain.Columns.Clear();
			GridColumn col;
			col=new GridColumn("Code System",200);
			gridMain.Columns.Add(col);
			col=new GridColumn("Current Version",100);
			gridMain.Columns.Add(col);
			col=new GridColumn("Import Status",100);
			col.IsWidthDynamic=true;
			gridMain.Columns.Add(col);
			gridMain.ListGridRows.Clear();
			if(_codeSystemCpt!=null) {
				GridRow row=new GridRow();
				row.Cells.Add(_codeSystemCpt.CodeSystemName);
				row.Cells.Add(_codeSystemCpt.VersionCur);
				row.Cells.Add(_status);
				row.Tag=_codeSystemCpt;
				gridMain.ListGridRows.Add(row);
			}
			GridRow rowValueSets=new GridRow();
			rowValueSets.Cells.Add(Lan.g("CodeSystemImporter","eCQM value sets (VSAC, eligible clinicians, 2026-05-14)"));
			rowValueSets.Cells.Add(GetValueSetVersionText(EhrValueSetImport.GetRelease()));
			rowValueSets.Cells.Add(_statusValueSets);
			rowValueSets.Tag=TAG_VALUE_SETS;
			gridMain.ListGridRows.Add(rowValueSets);
			gridMain.EndUpdate();
			bool isSetup=Security.IsAuthorized(EnumPermType.Setup,true);
			butImport.Enabled=(isSetup && _codeSystemCpt!=null && !UpdateCodeSystemThread.IsRunning);
			butImportValueSets.Enabled=(isSetup && !UpdateCodeSystemThread.IsRunning);
		}

		///<summary>The value-set row's Current Version: the import's date and file, or that nothing was imported. A record of another release than
		///the one this program needs (EHR-2's import of VSAC's June 2013 release, kept in the same preference row) names that release and says that
		///the 2026-05-14 release has to be imported.</summary>
		public static string GetValueSetVersionText(EhrValueSetRelease release) {
			if(release==null) {
				return Lan.g("CodeSystemImporter","not imported");
			}
			string dateImported=release.DateImportedUtc??"";
			string date=dateImported.Length>=10 ? dateImported.Substring(0,10) : dateImported;
			if(release.Label!=EhrValueSetImport.RELEASE_LABEL) {
				string label=string.IsNullOrWhiteSpace(release.Label) ? Lan.g("CodeSystemImporter","an earlier release") : release.Label;
				return Lan.g("CodeSystemImporter","old release loaded")+" ("+label+", "+Lan.g("CodeSystemImporter","imported")+" "+date+": "
					+release.FileName+"). "+Lan.g("CodeSystemImporter","Import the release this program needs")+": "+EhrValueSetImport.RELEASE_LABEL+".";
			}
			return Lan.g("CodeSystemImporter","imported")+" "+date+": "+release.FileName;
		}

		///<summary>The code-system status (EHR-2, step 4): each code system's rows in the database next to the minimum the code list expects, the
		///value-set codes of that system, apart from them its direct reference codes, and (EHR-6a, section 7.4) the loaded table's version next to the version the imported release names.</summary>
		private void FillGridStatus() {
			List<EhrCodeSystemStatus> listStatuses=EhrCodes.GetCodeSystemStatuses();
			gridStatus.BeginUpdate();
			gridStatus.Columns.Clear();
			gridStatus.Columns.Add(new GridColumn(Lan.g("TableCodeSystemStatus","Code System"),110));
			gridStatus.Columns.Add(new GridColumn(Lan.g("TableCodeSystemStatus","Rows in Database"),106,HorizontalAlignment.Right));
			gridStatus.Columns.Add(new GridColumn(Lan.g("TableCodeSystemStatus","Minimum Expected"),117,HorizontalAlignment.Right));
			gridStatus.Columns.Add(new GridColumn(Lan.g("TableCodeSystemStatus","Value-Set Codes"),100,HorizontalAlignment.Right));
			gridStatus.Columns.Add(new GridColumn(Lan.g("TableCodeSystemStatus","Direct Codes"),80,HorizontalAlignment.Right));
			gridStatus.Columns.Add(new GridColumn(Lan.g("TableCodeSystemStatus","Loaded Version"),95));
			gridStatus.Columns.Add(new GridColumn(Lan.g("TableCodeSystemStatus","In the Release"),95));
			GridColumn col=new GridColumn(Lan.g("TableCodeSystemStatus","Status"),100);
			col.IsWidthDynamic=true;
			gridStatus.Columns.Add(col);
			gridStatus.ListGridRows.Clear();
			foreach(EhrCodeSystemStatus status in listStatuses) {
				GridRow row=new GridRow();
				row.Cells.Add(status.CodeSystem);
				row.Cells.Add(status.HasTable ? status.CountInDb.ToString("N0") : "");
				row.Cells.Add(status.HasTable ? (status.CountMinimum+1).ToString("N0") : "");
				row.Cells.Add(status.CountValueSetCodes.ToString("N0"));
				row.Cells.Add(status.CountDirectCodes.ToString("N0"));
				row.Cells.Add(status.VersionLoaded);
				row.Cells.Add(status.VersionInRelease);
				row.Cells.Add(GetStatusText(status));
				gridStatus.ListGridRows.Add(row);
			}
			gridStatus.EndUpdate();
		}

		///<summary>The status column of one code system. A loaded table older than the release's version says so; where the two versions cannot be
		///compared (no mapping for the code system, or a version that does not follow it) both are shown and nothing is flagged.</summary>
		public static string GetStatusText(EhrCodeSystemStatus status) {
			if(status.CodeSystem=="AdministrativeSex") {
				return Lan.g("CodeSystemImporter","No table needed");
			}
			if(!status.HasTable) {
				return Lan.g("CodeSystemImporter","No table in this program: these codes are never counted as in the database");
			}
			if(status.IsInDb) {
				if(status.IsOlderThanRelease) {
					return Lan.g("CodeSystemImporter","Loaded, but older than the release's version")+": "+status.CountCodesNewer.ToString("N0")+" "
						+Lan.g("CodeSystemImporter","codes of the value sets are of a newer version and may be missing from the pick lists");
				}
				return Lan.g("CodeSystemImporter","Loaded");
			}
			return Lan.g("CodeSystemImporter","Load this code system's table directly");
		}

		///<summary>Shows the status in the grid. Used for problems found before the import starts.</summary>
		private void SetStatus(string status) {
			_status=status;
			FillGrid();
		}

		///<summary>Shows the value-set import's status in the grid.</summary>
		private void SetStatusValueSets(string status) {
			_statusValueSets=status;
			FillGrid();
		}

		///<summary>The licence prompt before the three files are chosen (section 7.5 of the 2027 measures plan).</summary>
		public static string GetValueSetLicensePrompt() {
			return Lan.g("CodeSystemImporter","The eCQM value sets come from the Value Set Authority Center (VSAC) of the National Library of Medicine. "
				+"Import only the files your office downloaded itself, under its own UMLS license: VSAC's eCQM value sets published 2026-05-14 for the "
				+"2027 performance period, sorted by CMS ID (the Excel workbook, as VSAC gives it in a .zip, or the SVS XML), with that release's Direct "
				+"Reference Codes file and its Code System Versions file. Pick the three files together; the two other sortings of the value sets are "
				+"not needed. They hold codes of "
				+"licensed code systems: CPT (American Medical Association), CDT (American Dental Association) and SNOMED CT, under the terms of the UMLS "
				+"license and its appendix, for your office's own use. Do not pass the codes on. This program ships none of them.")
				+"\r\n\r\n"+Lan.g("CodeSystemImporter","The import replaces every value-set code and direct reference code in the database. Click OK to "
				+"choose the three files.");
		}

		private void butImportValueSets_Click(object sender,EventArgs e) {
			if(RemotingClient.MiddleTierRole==MiddleTierRole.ClientMT) {//As the CPT import: the import writes the table directly.
				MsgBox.Show("CodeSystemImporter","Cannot import code systems when using the middle tier.");
				return;
			}
			if(!Security.IsAuthorized(EnumPermType.Setup)) {
				return;
			}
			if(!MsgBox.Show(MsgBoxButtons.OKCancel,GetValueSetLicensePrompt())) {
				return;
			}
			List<string> listPaths;
			using(OpenFileDialog openFileDialog=new OpenFileDialog()) {
				openFileDialog.Title=Lan.g("CodeSystemImporter","Choose the three files of the VSAC release");
				openFileDialog.InitialDirectory=Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
				openFileDialog.Filter=Lan.g("CodeSystemImporter","VSAC files")+" (*.xlsx;*.xml;*.zip)|*.xlsx;*.xml;*.zip";
				openFileDialog.RestoreDirectory=true;
				openFileDialog.Multiselect=true;//The value sets, the Direct Reference Codes and the Code System Versions, in one pick (section 7.1).
				if(openFileDialog.ShowDialog()!=DialogResult.OK) {
					return;
				}
				listPaths=openFileDialog.FileNames.ToList();
			}
			if(listPaths.Count!=3) {//Refused before any file is read: each parsed value-set file is held in memory (a 32-bit process).
				string refusal=EhrValueSetImport.GetPickCountText(listPaths.Count);
				MsgBox.Show(refusal);
				SetStatusValueSets(Lan.g("CodeSystemImporter","Files refused, nothing imported")+": "+refusal);
				return;
			}
			EhrValueSetPick pick=null;
			try {
				foreach(string path in listPaths) {
					if(new FileInfo(path).Length>EhrValueSetImport.MAX_FILE_BYTES) {
						SetStatusValueSets(Lan.g("CodeSystemImporter","A file is larger than the importer reads")+": "+Path.GetFileName(path));
						return;
					}
				}
				ProgressWin progressWin=new ProgressWin();
				progressWin.StartingMessage=Lan.g(this,"Reading the value sets...");
				progressWin.ShowCancelButton=false;
				//Each file is read from a stream, never whole into memory (this is a 32-bit process), under one row budget for the three, and a second
				//file of a kind already read is refused at its first row, so the pick never holds more than one parsed value-set file.
				progressWin.ActionMain=() => pick=EhrValueSetImport.ParsePick(listPaths,
					index => new FileStream(listPaths[index],FileMode.Open,FileAccess.Read,FileShare.Read));
				progressWin.ShowDialog();
			}
			catch(Exception ex) {
				SetStatusValueSets(Lan.g("CodeSystemImporter","Import failed")+"! -- "+ex.Message);
				return;
			}
			if(pick==null) {
				return;
			}
			using(MsgBoxCopyPaste msgBoxCopyPaste=new MsgBoxCopyPaste(EhrValueSetImport.GetReport(pick))) {
				msgBoxCopyPaste.Text=Lan.g("CodeSystemImporter","Value-set files");
				msgBoxCopyPaste.ShowDialog();
			}
			if(pick.IsRefused) {
				SetStatusValueSets(Lan.g("CodeSystemImporter","Files refused, nothing imported")+": "+pick.ListRefusals[0]);
				return;
			}
			if(!MsgBox.Show(MsgBoxButtons.OKCancel,Lan.g("CodeSystemImporter","Replace every value-set code and direct reference code in the database with the")
				+" "+pick.ListEhrCodesAll.Count+" "+Lan.g("CodeSystemImporter","codes of these three files?")))
			{
				return;
			}
			string error="";
			bool areRowsReplaced=false;
			try {
				ProgressWin progressWin=new ProgressWin();
				progressWin.StartingMessage=Lan.g(this,"Importing the value sets...");
				progressWin.ShowCancelButton=false;
				progressWin.ActionMain=() => error=EhrValueSetImport.Import(pick,out areRowsReplaced);
				progressWin.ShowDialog();
			}
			catch(Exception ex) {
				error=ex.Message;
			}
			if(error!="" && areRowsReplaced) {
				//The rows are the new ones, but the release record was not saved (it may be half written): not "Import failed".
				MsgBox.Show(error);
				DataValid.SetInvalid(InvalidType.EhrCodes,InvalidType.Prefs);
				SetStatusValueSets(Lan.g("CodeSystemImporter","Codes replaced, release not recorded")+"! -- "+error);
				FillGridStatus();
				return;
			}
			if(error!="") {
				MsgBox.Show(error);
				SetStatusValueSets(Lan.g("CodeSystemImporter","Import failed")+"! -- "+error);
				//The rows may have changed before the failure: every workstation reads them again.
				DataValid.SetInvalid(InvalidType.EhrCodes);
				FillGridStatus();
				return;
			}
			//Reads the codes again here (the IsInDb flags too) and tells the other workstations and the Middle Tier server to.
			DataValid.SetInvalid(InvalidType.EhrCodes,InvalidType.Prefs);
			SetStatusValueSets(Lan.g("CodeSystemImporter","Import complete")+"! -- "+Lan.g("CodeSystemImporter","Number of codes imported")+": "
				+pick.FileValueSets.ListEhrCodes.Count+" "+Lan.g("CodeSystemImporter","in")+" "+pick.FileValueSets.CountValueSets+" "
				+Lan.g("CodeSystemImporter","value sets")+", "+pick.ListEhrCodesDirect.Count+" "+Lan.g("CodeSystemImporter","direct reference codes"));
			FillGridStatus();
		}

		private void butImport_Click(object sender,EventArgs e) {
			if(RemotingClient.MiddleTierRole==MiddleTierRole.ClientMT) {//Do not let users import code systems when using the middle tier.
				MsgBox.Show("CodeSystemImporter","Cannot import code systems when using the middle tier.");
				return;
			}
			if(!Security.IsAuthorized(EnumPermType.Setup)) {//Section 5.1 of the EHR plan: the CPT import needs Setup, as the value-set import does.
				return;
			}
			if(_codeSystemCpt==null) {
				return;
			}
			//CPT codes require user to choose a local file, so the file is checked and unzipped here on the main thread. Only the import runs on a thread.
			if(!MsgBox.Show("CodeSystemImporter",MsgBoxButtons.OKCancel,"CPT codes are licensed separately from the American Medical Association, "
				+"in its data file format. Most offices do not need these codes. If you have the CPT data file, click OK to browse to it."))
			{
				return;
			}
			using OpenFileDialog openFileDialog=new OpenFileDialog();
			openFileDialog.Title=Lan.g("CodeSystemImporter","Choose CPT .zip file");
			try {
				openFileDialog.InitialDirectory=Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
				openFileDialog.Filter="zip|*.zip";
				openFileDialog.RestoreDirectory=true;
				openFileDialog.Multiselect=false;
				if(openFileDialog.ShowDialog()!=DialogResult.OK) {
					return;
				}
				if(!openFileDialog.FileName.ToLower().EndsWith(".zip")) {
					SetStatus(Lan.g("CodeSystemImporter","Could not locate .zip file in specified folder."));
					return;
				}
				string versionID="";
				if(openFileDialog.FileName.ToLower().Contains("cpt-2014-data-files-download.zip")) {
					versionID="2014";
				}
				else if(openFileDialog.FileName.ToLower().Contains("cpt-2015-data-files.zip")) {
					versionID="2015";
				}
				else if(openFileDialog.FileName.ToLower().Contains("dl513216_2016.zip")) {
					versionID="2016";
				}
				else if(openFileDialog.FileName.ToLower().Contains("dl513217.zip")) {
					versionID="2017";
				}
				//Unzip the compressed file-----------------------------------------------------------------------------------------------------
				bool foundFile=false;
				string meduFileName="MEDU.txt";//MEDU stands for MEDium desciption Upper case.
				using FileStream fileStreamZip=new FileStream(openFileDialog.FileName,FileMode.Open,FileAccess.Read,FileShare.Read);
				using ZipArchive zipArchive=new ZipArchive(fileStreamZip,ZipArchiveMode.Read);
				foreach(ZipArchiveEntry zipArchiveEntry in zipArchive.Entries) {//unzip/write the MEDU file(s) to the temp directory
					if(ODZip.IsDirectory(zipArchiveEntry) || !zipArchiveEntry.FullName.ToLower().Contains("medu.txt")) {  //This file used to be called "medu.txt.txt" and is now called "medu.txt".  Uses .Contains() to catch both cases.
						continue;
					}
					meduFileName=zipArchiveEntry.FullName;
					//Overwrites a file left by an earlier import. Throws, extracting nothing, if the entry's path would leave the temp folder.
					ODZip.ExtractEntry(zipArchiveEntry,PrefC.GetTempFolderPath(),overwrite:true);
					foundFile=true;
				}
				if(!foundFile) {
					SetStatus(Lan.g("CodeSystemImporter","MEDU.txt file not found in zip archive."));
					return;
				}
				if(versionID=="") {
					//This prompt has the chance of allowing users to "corrupt" their data and import codes for one year and identify them as codes from a
					//different year. To fix this would require a manual query. If this is a common occurance, consider automating this step based on the codes
					//detected inside the MEDU file.
					InputBox input=new InputBox("What year are these CPT codes for?");
					input.ShowDialog();
					if(input.IsDialogCancel || !Regex.IsMatch(input.StringResult,@"^\d{4}$")) {//A four digit value was not entered.
						SetStatus(Lan.g("CodeSystemImporter","CPT code year must be specified."));
						return;
					}
					versionID=input.StringResult;
				}
				UpdateCodeSystemThread.Add(ODFileUtils.CombinePaths(PrefC.GetTempFolderPath(),meduFileName),_codeSystemCpt,
					new UpdateCodeSystemThread.UpdateCodeSystemArgs(UpdateCodeSystemThread_UpdateSafe),versionID);
			}
			catch(Exception ex) {
				SetStatus(ex.Message);
				return;
			}
			//We got this far so the local file was retrieved successfully. No initial status to report.
			_status="";
			//We will re-enable the Import button when we handle the UpdateCodeSystemThread.Finished event.
			if(UpdateCodeSystemThread.StartAll()) {
				_hasImported=true;
			}
			FillGrid();
		}

		#region thread handlers
		///<summary>Call this from external thread. Invokes to main thread to avoid cross-thread collision.</summary>
		private void UpdateCodeSystemThread_FinishedSafe(object sender,EventArgs e) {
			try {
				this.BeginInvoke(new EventHandler(UpdateCodeSystemThread_FinishedUnsafe),new object[] { sender,e });
			}
			//most likely because form is no longer available to invoke to
			catch { }
		}

		///<summary>Do not call this directly from external thread. Use UpdateCodeSystemThread_FinishedSafe.</summary>
		private void UpdateCodeSystemThread_FinishedUnsafe(object sender,EventArgs e) {
			FillGrid();//Shows the new current version and enables the Import button again.
			FillGridStatus();//The CPT table's row count.
		}

		///<summary>Call this from external thread. Invokes to main thread to avoid cross-thread collision.</summary>
		private void UpdateCodeSystemThread_UpdateSafe(CodeSystem codeSystem,string status,double percentDone,bool isDone,bool isSuccess,int numUpdated) {
			try {
				this.BeginInvoke(new UpdateCodeSystemThread.UpdateCodeSystemArgs(UpdateCodeSystemThread_UpdateUnsafe),new object[] { codeSystem,status,percentDone,isDone,isSuccess,numUpdated });
			}
			//most likely because form is no longer available to invoke to
			catch { }
		}

		///<summary>Do not call this directly from external thread. Use UpdateCodeSystemThread_UpdateSafe.</summary>
		private void UpdateCodeSystemThread_UpdateUnsafe(CodeSystem codeSystem,string status,double percentDone,bool isDone,bool isSuccess,int numUpdated) {
			//This is called a lot from the import thread so don't bother with the full FillGrid. Just find our row and column and update the cell's text.
			string cellText=((int)percentDone)+"%"+" -- "+status;
			if(isDone) {
				if(isSuccess) {
					//If done==true percentDone is the number of codes that were imported, not the percent done.
					//This is done so we don't have to change the signatures of exisiting functions but can still alert the user when no codes were actually imported.
					cellText=Lan.g("CodeSystemImporter","Import complete")+"! -- "+Lan.g("CodeSystemImporter","Number of codes imported")+": "
						+Convert.ToInt32(percentDone).ToString()+" "+Lan.g("CodeSystemImporter","Number of codes updated")+": "+numUpdated;
				}
				else {
					cellText=Lan.g("CodeSystemImporter","Import failed")+"! -- "+status;
				}
			}
			_status=cellText;
			gridMain.BeginUpdate();
			for(int i=0;i<gridMain.ListGridRows.Count;i++) {
				if(gridMain.ListGridRows[i].Tag==null
					|| !(gridMain.ListGridRows[i].Tag is CodeSystem)
					|| !(((CodeSystem)gridMain.ListGridRows[i].Tag).CodeSystemName==codeSystem.CodeSystemName)) {
					continue;
				}
				gridMain.ListGridRows[i].Cells[2].Text=cellText;
			}
			gridMain.EndUpdate();  //Need to call this instead of gridMain.Invalidate() because we need text wrapping to happen if there was a long error message.
		}
		#endregion

		///<summary>Worker thread class. Imports a code system from a local file on its own thread so that the window stays responsive.</summary>
		private class UpdateCodeSystemThread {
			///<summary>Static lis of threads. All managed internally. Must always be locked by _lock when accessed!!!</summary>
			private static List<UpdateCodeSystemThread> _threads=new List<UpdateCodeSystemThread>();
			///<summary>All access of _threads member MUST BE enclosed with lock statement in order to prevent thread-lock and race conditions.</summary>
			private static object _lock=new object();
			///<summary>The code system being updated.</summary>
			private CodeSystem _codeSystem;
			///<summary>The import function will check this flag occasionally to see if it should abort prematurely.</summary>
			private bool _quit=false;
			///<summary>Function signature required to send an update.  When done==true, percentDone is the number of codes imported.</summary>
			public delegate void UpdateCodeSystemArgs(CodeSystem codeSystem,string status,double percentDone,bool done,bool success,int numUpdated);
			///<summary>Required by ctor. Used to keep main thread aware of update progress.</summary>
			private UpdateCodeSystemArgs _updateHandler;
			///<summary>Event will be fired when the final thread has finished and all threads have been cleared from the list.</summary>
			public static EventHandler Finished;
			///<summary>The file to import, which the user provided. For CPT this is the MEDU.txt file extracted from the CPT zip file.</summary>
			private string _localFilePath;
			///<summary>The version of the code system being imported.</summary>
			private string _versionID;

			///<summary>Aborts the thread. Only called by StopAll.</summary>
			private void Quit() {
				_quit=true;
			}

			///<summary>Indicates if there are still 1 or more active threads.</summary>
			public static bool IsRunning {
				get {
					lock(_lock){
						return _threads.Count>=1;
					}
				}
			}

			///<summary>Private ctor. Will only be used internally by Add.</summary>
			private UpdateCodeSystemThread(string localFilePath,CodeSystem codeSystem,UpdateCodeSystemArgs updateCodeSystemArgs,string versionID) {
				_localFilePath=localFilePath;
				_codeSystem=codeSystem;
				_updateHandler+=updateCodeSystemArgs;
				_versionID=versionID;
			}

			///<summary>Provide a nice ledgible identifier.</summary>
			public override string ToString() {
				return _codeSystem.CodeSystemName;
			}

			///<summary>Thread list manager needs this to remove threads. Required for List.Contains.</summary>
			public override bool Equals(object obj) {
				return ((UpdateCodeSystemThread)obj)._codeSystem.CodeSystemNum==_codeSystem.CodeSystemNum;
			}

			///<summary>Just to prevent a VS warning.</summary>
			public override int GetHashCode() {
				return base.GetHashCode();
			}

			///<summary>Add a thread to the queue. These threads will not be started until StartAll is called subsequent to adding all necessary threads.
			///The file at localFilePath must exist locally.</summary>
			public static void Add(string localFilePath,CodeSystem codeSystem,UpdateCodeSystemArgs updateCodeSystemArgs,string versionID) {
				UpdateCodeSystemThread updateCodeSystemThread=new UpdateCodeSystemThread(localFilePath,codeSystem,updateCodeSystemArgs,versionID);
				lock(_lock) {
					_threads.Add(updateCodeSystemThread);
				}
			}

			///<summary>Use this to start the threads once all threads have been added using Add.</summary>
			public static bool StartAll() {
				bool hasStartedAtLeastOne=false;
				lock(_lock) {
					foreach(UpdateCodeSystemThread thread in _threads) {
						Thread th=new Thread(new ThreadStart(thread.Run));
						th.Name=thread.ToString();
						th.Start();
						hasStartedAtLeastOne=true;
					}
				}
				return hasStartedAtLeastOne;
			}

			///<summary>Sets the Quit flag for all threads. Use this if early abort is desired.</summary>
			public static void StopAll() {
				lock(_lock) {
					foreach(UpdateCodeSystemThread thread in _threads) {
						thread.Quit();
					}
					_threads.Clear();
				}
			}

			///<summary>Called internally each time time a thread has completed. Will trigger the Finished event if this is the last thread to complete.</summary>
			private void Done(string status,bool isSuccess, int numCodesImported,int numCodesUpdated) {
				_updateHandler(_codeSystem,status,Convert.ToDouble(numCodesImported),true,isSuccess,numCodesUpdated);//Pass in the number of codes imported as percentDone.  This was done so can display the number of codes imported withouth having to add a new perameter to the delegate function.
				bool isFinished=false;
				lock(_lock) {
					if(_threads.Contains(this)) {
						_threads.Remove(this);
					}
					isFinished=_threads.Count<=0;
				}
				if(isFinished && Finished!=null) {
					Finished("UpdateCodeSystemThread",new EventArgs());
				}
			}

			///<summary>Update the current status of this import thread. Thread owner is required to handle this as the delegat is required in the ctor.</summary>
			private void Update(string status,int numDone,int numTotal) {
				double percentDone=0;
				//Guard against illegal division.
				if(numTotal>0) {
					percentDone=100*(numDone/(double)numTotal);
				}
				_updateHandler(_codeSystem,status,percentDone,false,true,0);
			}

			///<summary>Helper used internally.</summary>
			private void ImportProgress(int numDone,int numTotal) {
				Update(Lan.g("CodeSystemImporter","Importing"),numDone,numTotal);
			}

			///<summary>The thread function.</summary>
			private void Run() {
				string failText="";
				int numCodesImported=0;
				int numCodesUpdated=0;
				if(!ImportHelper(ref failText,ref numCodesImported,ref numCodesUpdated)) {
					Done(Lan.g("CodeSystemImporter","Error")+": "+failText,false,0,0);
					return;
				}
				//set current version=imported version
				CodeSystems.UpdateCurrentVersion(_codeSystem,_versionID);
				Done(Lan.g("CodeSystemImporter","Import Complete"),true,numCodesImported,numCodesUpdated);
			}

			///<summary>Imports codeSystem from the local file. Returns false if unsuccessful.</summary>
			private bool ImportHelper(ref string failText,ref int numCodesImported,ref int numCodesUpdated) {
				try {
					if(!File.Exists(_localFilePath)) {
						throw new Exception(Lan.g("CodeSystemImporter","Local file not found ")+": "+_localFilePath);
					}
					switch(_codeSystem.CodeSystemName) {
						case "CPT":
							CodeSystems.ImportCpt(_localFilePath,new CodeSystems.ProgressArgs(ImportProgress),ref _quit,ref numCodesImported,ref numCodesUpdated,
								_versionID);
							break;
						default:
							throw new Exception(Lan.g("CodeSystemImporter","Unsupported Code System")+": "+_codeSystem.CodeSystemName);
					}
					//Import succeded so delete the import file where necessary.
					DeleteImportFileIfNecessary();
					//We got here so everything succeeded.
					return true;
				}
				catch(Exception ex) {
					failText=ex.Message;
				}
				//We got here so something failed.
				return false;
			}

			///<summary>Delete the import file which was extracted from the zip archive. It is temporary and can be deleted.</summary>
			private void DeleteImportFileIfNecessary() {
				//Don't bother if the file isn't there.
				if(!File.Exists(_localFilePath)) {
					return;
				}
				//We got this far so assume the file is safe to delete.
				File.Delete(_localFilePath);
			}
		}

		///<summary>If there are still import threads running then prompt the user to see if they want to abort the imports prematurely.</summary>
		private void FormCodeSystemsImport_FormClosing(object sender,FormClosingEventArgs e) {
			if(!UpdateCodeSystemThread.IsRunning) { //All done, exit.
				if(_hasImported) {
					DataValid.SetInvalid(InvalidType.EhrCodes);//Update in-memory list of codes for all other workstations
				}
				return;
			}
			if(MsgBox.Show("CodeSystemImporter",MsgBoxButtons.YesNo,"Import in progress. Would you like to abort?")) {
				//User wants abort the threads.
				UpdateCodeSystemThread.StopAll();
				_hasImported=false;
				return;
			}
			//User elected to continue waiting so cancel the Close event.
			e.Cancel=true;
		}
	}

}
