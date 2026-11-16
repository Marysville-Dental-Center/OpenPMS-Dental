// Brings a winchk_ scratch copy to this build's fork schema level before any check starts the program (WINCHK-4): the dump of the synthetic
// test database is made once and can predate a fork migration step this build has, and the program's start-up would then ask to update the
// database (a message box no check answers). The copy is migrated through the business layer with the program's own code, as start-up runs it
// (PrefL.RunForkMigrations): the version guard decides from the copy's markers (ForkVersionGuard.Evaluate), and the registered runner runs the
// pending steps (ForkMigrationHandOff.Runner.RunPending). Only a database whose name starts with winchk_ on 127.0.0.1:3306 is ever connected
// to; the dump itself and the database it was made from are never touched.
// Licensed under the GNU General Public License, version 2 or later. See LICENSE in the repository root.
using System;
using System.Collections.Generic;
using System.Linq;
using DataConnectionBase;
using OpenPmsDentalBusiness;

namespace WindowChecksTests {
	public class CopyMigrationResult {
		///<summary>True when the copy is at this build's fork schema level (migrated now, or already there).</summary>
		public bool IsDone;
		///<summary>P0's line: "migrated the copy from A to B: steps ..." or "already at A", or why the copy could not be brought there.</summary>
		public string Text="";
		///<summary>What the runner logged (each step and its changes), for run.log.</summary>
		public List<string> ListLog=new List<string>();
	}

	public static class CopyMigration {
		///<summary>Connects the business layer of this process to the winchk_ database on 127.0.0.1:3306 and runs this build's pending fork migration
		///steps on it, as start-up does, without a prompt. Fails (IsDone false, with the reason) when the copy is not an adopted database this build
		///may migrate: older than the adoption point (no fork markers), updated by a newer build, or with inconsistent markers. Also fails when a step
		///throws, or when steps are still pending afterwards.</summary>
		public static CopyMigrationResult Run(string database) {
			if(string.IsNullOrEmpty(database) || !database.StartsWith(ScratchDb.PREFIX)) {
				throw new ArgumentException("The window checks only migrate databases whose name starts with "+ScratchDb.PREFIX);
			}
			CopyMigrationResult copyMigrationResult=new CopyMigrationResult();
			RemotingClient.MiddleTierRole=MiddleTierRole.ClientDirect;
			new DataConnection().SetDb(ScratchDb.Server+":"+ScratchDb.Port,database,"root","","","",DatabaseType.MySql,true,DevDbTls.GetSslCa(ScratchDb.Server));
			//The business layer's connection must be this copy, and nothing else, before anything is written.
			string databaseConnected=Db.GetScalar("SELECT DATABASE()");
			if(databaseConnected!=database) {
				throw new InvalidOperationException("The business layer is connected to '"+databaseConnected+"', not to "+database+".");
			}
			IForkMigrationRunner runner=ForkMigrationHandOff.Runner;
			ForkDbState forkDbState=ForkVersions.ReadState();
			ForkGuardResult forkGuardResult=ForkVersionGuard.Evaluate(forkDbState,runner);
			if(forkGuardResult.Action!=ForkGuardAction.ProceedFork) {
				string why;
				if(forkGuardResult.Action==ForkGuardAction.ProceedCompat || forkGuardResult.Action==ForkGuardAction.NeedsAdoption) {
					why="the dump is older than the adoption point (no fork schema level; DataBaseVersion "+(forkDbState.DataBaseVersion??"missing")
						+"): adopt the database the dump is made from, and make the dump again";
				}
				else if(forkGuardResult.Action==ForkGuardAction.NeedsNewerBuild) {
					why="the dump was updated by a newer build than this one";
				}
				else {
					why="this build refuses the copy";
				}
				copyMigrationResult.Text="the copy cannot be brought to this build's fork schema level "+runner.LatestLevel+": "+why+" (version guard: "
					+forkGuardResult.Action+", exit code "+forkGuardResult.ExitCode+": "+OneLine(forkGuardResult.Message)+")";
				return copyMigrationResult;
			}
			string levelBefore=forkDbState.ForkSchemaLevel;
			if(!forkGuardResult.HasPendingMigrations) {
				copyMigrationResult.IsDone=true;
				copyMigrationResult.Text="already at "+levelBefore+" (this build's fork schema level "+runner.LatestLevel+"; no step pending)";
				return copyMigrationResult;
			}
			string steps=string.Join(", ",forkGuardResult.ListPendingLevels);
			try {
				runner.RunPending(copyMigrationResult.ListLog.Add);
			}
			catch(Exception ex) {
				string inProgress=ForkVersions.GetMarker(OpenPmsDentalBusiness.ForkMigrations.ForkMigrationRunner.PrefNameInProgress);
				copyMigrationResult.Text="the fork migration of the copy from "+levelBefore+" (steps "+steps+") failed"
					+(string.IsNullOrEmpty(inProgress) ? "" : " in step "+inProgress)+": "+OneLine(ex.GetBaseException().Message);
				return copyMigrationResult;
			}
			ForkDbState forkDbStateAfter=ForkVersions.ReadState();
			ForkGuardResult forkGuardResultAfter=ForkVersionGuard.Evaluate(forkDbStateAfter,runner);
			if(forkGuardResultAfter.Action!=ForkGuardAction.ProceedFork || forkGuardResultAfter.HasPendingMigrations) {
				copyMigrationResult.Text="the fork migration of the copy from "+levelBefore+" ran (steps "+steps+"), but the version guard still says "
					+forkGuardResultAfter.Action+(forkGuardResultAfter.HasPendingMigrations ? " with steps "+string.Join(", ",forkGuardResultAfter.ListPendingLevels)
					+" pending" : "")+": "+OneLine(forkGuardResultAfter.Message);
				return copyMigrationResult;
			}
			copyMigrationResult.IsDone=true;
			copyMigrationResult.Text="migrated the copy from "+levelBefore+" to "+forkDbStateAfter.ForkSchemaLevel+": steps "+steps
				+" (the program's fork migration runner, as start-up runs it, without its prompt)";
			return copyMigrationResult;
		}

		private static string OneLine(string text) {
			return string.Join(" ",(text??"").Split(new[] { '\r','\n' },StringSplitOptions.RemoveEmptyEntries).Select(x => x.Trim()));
		}
	}
}
