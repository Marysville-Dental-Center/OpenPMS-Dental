// The spell checker in a fresh process (IsolatedProcess): SpellCheckProbeWorker runs in the child process (TestProbeHost), where everything it
// calls sees the spell checker as a program that just started, and SpellCheckProbe is the test's side, which sends each call there. Only plain
// values (strings, numbers, lists of strings) cross between them, as ProbeText lines; the tests assert on them in the test's process. Before
// S3a-8 of the .NET 10 move the probe ran in a new AppDomain, which .NET cannot create.
// Licensed under the GNU General Public License, version 2 or later. See LICENSE in the repository root.
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.Threading;
using DataConnectionBase;
using OpenPmsDentalBusiness;
using TestProbeHost;
using WpfControls;

namespace LibSpellTests {
	///<summary>The test's side: each method runs the SpellCheckProbeWorker method of the same name in the child process.</summary>
	public class SpellCheckProbe {
		private readonly ProbeProcess _probeProcess;

		internal SpellCheckProbe(ProbeProcess probeProcess) {
			_probeProcess=probeProcess;
		}

		public int CountDictionaryReads() {
			return int.Parse(_probeProcess.Call(nameof(CountDictionaryReads)),CultureInfo.InvariantCulture);
		}

		public bool IsDictionaryRead() {
			return bool.Parse(_probeProcess.Call(nameof(IsDictionaryRead)));
		}

		public string DictionaryReadError() {
			return _probeProcess.Call(nameof(DictionaryReadError));
		}

		public double ReadInBackground(int count) {
			return double.Parse(_probeProcess.Call(nameof(ReadInBackground),count.ToString(CultureInfo.InvariantCulture)),CultureInfo.InvariantCulture);
		}

		public bool IsInDictionary(string word) {
			return bool.Parse(_probeProcess.Call(nameof(IsInDictionary),word));
		}

		public bool IsCorrect(string word) {
			return bool.Parse(_probeProcess.Call(nameof(IsCorrect),word));
		}

		public bool IsCustomWord(string word) {
			return bool.Parse(_probeProcess.Call(nameof(IsCustomWord),word));
		}

		public string[] Suggest(string word) {
			return ProbeText.SplitList(_probeProcess.Call(nameof(Suggest),word));
		}

		public string[] GetMisspelledWords(string text) {
			return ProbeText.SplitList(_probeProcess.Call(nameof(GetMisspelledWords),text));
		}

		public string CheckFromThreads(int countThreads,string[] arrayWords,bool[] arrayExpected) {
			return _probeProcess.Call(nameof(CheckFromThreads),countThreads.ToString(CultureInfo.InvariantCulture),ProbeText.JoinList(arrayWords),
				ProbeText.JoinList(arrayExpected.Select(x => x.ToString())));
		}

		public string CreateBoxes(int countEach) {
			return _probeProcess.Call(nameof(CreateBoxes),countEach.ToString(CultureInfo.InvariantCulture));
		}

		public void Connect(string databaseName) {
			_probeProcess.Call(nameof(Connect),databaseName);
		}

		public string[] AddCustomWord(string word) {
			return ProbeText.SplitList(_probeProcess.Call(nameof(AddCustomWord),word));
		}

		public void InsertCustomWordElsewhere(string word) {
			_probeProcess.Call(nameof(InsertCustomWordElsewhere),word);
		}

		public void DeleteCustomWordElsewhere(string word) {
			_probeProcess.Call(nameof(DeleteCustomWordElsewhere),word);
		}

		public void Disconnect() {
			_probeProcess.Call(nameof(Disconnect));
		}
	}

	///<summary>Runs in the child process. Handle is TestProbeHost's entry; the methods are those of SpellCheckProbe.</summary>
	public class SpellCheckProbeWorker {
		///<summary>The one worker of the child process: the signals it records belong to the process.</summary>
		private static readonly SpellCheckProbeWorker _worker=new SpellCheckProbeWorker();
		///<summary>The InvalidType lists DataValid.SetInvalid raised in this process, in order.</summary>
		private readonly List<string> _listSignals=new List<string>();

		///<summary>The child process's handler: the command is the name of a SpellCheckProbe method, the arguments its arguments as text.</summary>
		public static string Handle(string command,string[] arrayArgs) {
			switch(command) {
				case nameof(CountDictionaryReads):
					return _worker.CountDictionaryReads().ToString(CultureInfo.InvariantCulture);
				case nameof(IsDictionaryRead):
					return _worker.IsDictionaryRead().ToString();
				case nameof(DictionaryReadError):
					return _worker.DictionaryReadError();
				case nameof(ReadInBackground):
					return _worker.ReadInBackground(int.Parse(arrayArgs[0],CultureInfo.InvariantCulture)).ToString("R",CultureInfo.InvariantCulture);
				case nameof(IsInDictionary):
					return _worker.IsInDictionary(arrayArgs[0]).ToString();
				case nameof(IsCorrect):
					return _worker.IsCorrect(arrayArgs[0]).ToString();
				case nameof(IsCustomWord):
					return _worker.IsCustomWord(arrayArgs[0]).ToString();
				case nameof(Suggest):
					return ProbeText.JoinList(_worker.Suggest(arrayArgs[0]));
				case nameof(GetMisspelledWords):
					return ProbeText.JoinList(_worker.GetMisspelledWords(arrayArgs[0]));
				case nameof(CheckFromThreads):
					return _worker.CheckFromThreads(int.Parse(arrayArgs[0],CultureInfo.InvariantCulture),ProbeText.SplitList(arrayArgs[1]),
						ProbeText.SplitList(arrayArgs[2]).Select(x => bool.Parse(x)).ToArray());
				case nameof(CreateBoxes):
					return _worker.CreateBoxes(int.Parse(arrayArgs[0],CultureInfo.InvariantCulture));
				case nameof(Connect):
					_worker.Connect(arrayArgs[0]);
					return "";
				case nameof(AddCustomWord):
					return ProbeText.JoinList(_worker.AddCustomWord(arrayArgs[0]));
				case nameof(InsertCustomWordElsewhere):
					_worker.InsertCustomWordElsewhere(arrayArgs[0]);
					return "";
				case nameof(DeleteCustomWordElsewhere):
					_worker.DeleteCustomWordElsewhere(arrayArgs[0]);
					return "";
				case nameof(Disconnect):
					_worker.Disconnect();
					return "";
				default:
					throw new ArgumentException("Unknown command "+command);
			}
		}

		public int CountDictionaryReads() {
			return SpellCheckL.CountDictionaryReads;
		}

		public bool IsDictionaryRead() {
			return SpellCheckL.IsDictionaryRead;
		}

		public string DictionaryReadError() {
			return SpellCheckL.DictionaryReadError;
		}

		///<summary>Calls ReadInBackground count times, as that many boxes being created do, and returns how long the calls took together in
		///milliseconds.</summary>
		public double ReadInBackground(int count) {
			Stopwatch stopwatch=Stopwatch.StartNew();
			for(int i=0;i<count;i++) {
				SpellCheckL.ReadInBackground();
			}
			return stopwatch.Elapsed.TotalMilliseconds;
		}

		///<summary>Waits for the dictionary if it is still being read (the first lookup does).</summary>
		public bool IsInDictionary(string word) {
			return SpellCheckL.IsInDictionary(word);
		}

		public bool IsCorrect(string word) {
			return SpellCheckL.IsCorrect(word);
		}

		public bool IsCustomWord(string word) {
			return SpellCheckL.IsCustomWord(word);
		}

		public string[] Suggest(string word) {
			return SpellCheckL.Suggest(word).ToArray();
		}

		public string[] GetMisspelledWords(string text) {
			return SpellCheckL.GetMisspelledWords(text).Select(x => x.Value).ToArray();
		}

		///<summary>Starts countThreads threads at once, each of which looks up every word and compares the answer with arrayExpected. Returns the
		///wrong answers and exceptions, one line each; "" when there were none.</summary>
		public string CheckFromThreads(int countThreads,string[] arrayWords,bool[] arrayExpected) {
			List<string> listProblems=new List<string>();
			object lockProblems=new object();
			using(ManualResetEventSlim manualResetEventSlimStart=new ManualResetEventSlim(false)) {
				List<Thread> listThreads=new List<Thread>();
				for(int t=0;t<countThreads;t++) {
					int threadNumber=t;
					Thread thread=new Thread(() => {
						manualResetEventSlimStart.Wait();
						try {
							for(int i=0;i<arrayWords.Length;i++) {
								//Every other thread starts at the end, so threads ask for different words at the same time.
								int index=(threadNumber%2==0) ? i : arrayWords.Length-1-i;
								bool isCorrect=SpellCheckL.IsCorrect(arrayWords[index]);
								if(isCorrect!=arrayExpected[index]) {
									lock(lockProblems) {
										listProblems.Add("Thread "+threadNumber+": "+arrayWords[index]+" is "+(isCorrect ? "correct" : "misspelled"));
									}
								}
							}
						}
						catch(Exception ex) {
							lock(lockProblems) {
								listProblems.Add("Thread "+threadNumber+": "+ex);
							}
						}
					});
					thread.IsBackground=true;
					thread.Start();
					listThreads.Add(thread);
				}
				manualResetEventSlimStart.Set();
				foreach(Thread thread in listThreads) {
					if(!thread.Join(TimeSpan.FromMinutes(2))) {
						listProblems.Add("A thread did not finish in 2 minutes.");
					}
				}
			}
			return string.Join("\r\n",listProblems);
		}

		///<summary>Builds countEach ODtextBoxes (Windows Forms) and countEach TextRiches (WPF) on an STA thread, never shown, as windows do when they
		///open. Returns "" or the exception.</summary>
		public string CreateBoxes(int countEach) {
			string error="";
			Thread thread=new Thread(() => {
				try {
					for(int i=0;i<countEach;i++) {
						using(OpenPmsDental.ODtextBox odTextBox=new OpenPmsDental.ODtextBox()) {
						}
						WpfControls.UI.TextRich textRich=new WpfControls.UI.TextRich();
					}
				}
				catch(Exception ex) {
					error=ex.ToString();
				}
			});
			thread.SetApartmentState(ApartmentState.STA);
			thread.IsBackground=true;
			thread.Start();
			if(!thread.Join(TimeSpan.FromMinutes(2))) {
				return "The boxes were not built in 2 minutes.";
			}
			return error;
		}

		#region The office's own words
		///<summary>Points this process's business layer at the scratch database, and stands in for FormOpenDental.DataValid_BecameInvalid, which
		///refreshes this computer's caches of the signalled types.</summary>
		public void Connect(string databaseName) {
			RemotingClient.MiddleTierRole=MiddleTierRole.ClientDirect;
			new DataConnection().SetDb(ScratchDb.Server,databaseName,ScratchDb.User,ScratchDb.Password,"","",DatabaseType.MySql,true,DevDbTls.GetSslCa(ScratchDb.Server));
			OpenPmsDental.DataValid.EventInvalid+=(sender,e) => {
				_listSignals.Add(string.Join(",",e.ITypes.Select(x => x.ToString())));
				Cache.Refresh(e.ITypes);
			};
		}

		///<summary>What "Add to Dictionary" does in both boxes. Returns the signals it raised.</summary>
		public string[] AddCustomWord(string word) {
			_listSignals.Clear();
			SpellCheckL.AddCustomWord(word);
			return _listSignals.ToArray();
		}

		///<summary>Another workstation's change, or Setup | Spell Check: the row is written directly, then this workstation gets the signal.</summary>
		public void InsertCustomWordElsewhere(string word) {
			DataCore.NonQ("INSERT INTO dictcustom (WordText) VALUES ('"+POut.String(word)+"')");
			Cache.Refresh(InvalidType.DictCustoms);
		}

		///<summary>Setup | Spell Check deletes the word; this workstation gets the signal.</summary>
		public void DeleteCustomWordElsewhere(string word) {
			DataCore.NonQ("DELETE FROM dictcustom WHERE WordText='"+POut.String(word)+"'");
			Cache.Refresh(InvalidType.DictCustoms);
		}

		///<summary>Closes the database connections of this process, before the database is dropped.</summary>
		public void Disconnect() {
			MySqlConnector.MySqlConnection.ClearAllPools();
		}
		#endregion The office's own words
	}
}
