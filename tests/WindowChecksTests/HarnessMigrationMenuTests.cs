// The two harness pieces of WINCHK-4, checked without the opt-in and without starting the program: the scratch copy brought to this build's
// fork schema level by the program's own fork migrations (CopyMigration, on planted winchk_ databases on 127.0.0.1:3306 that hold only the
// preference rows the version guard reads, dropped at the end), and the MenuStrip lookup RdlDesigner's H1 and H2 use (Uia.ChooseMenuStripItem,
// on a MenuStrip of a form built in this process, off the screen and never activated; its drop-downs are fully transparent while they show).
// Synthetic data only.
// Licensed under the GNU General Public License, version 2 or later. See LICENSE in the repository root.
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Threading;
using System.Windows.Forms;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using MySqlConnector;
using OpenPmsDentalBusiness.ForkMigrations;
using OpenPmsDentalBusiness.ForkMigrations.FeatureSteps;

namespace WindowChecksTests {
	[TestClass]
	public class HarnessMigrationMenuTests {
		///<summary>A planted copy's name: winchk_ (the only prefix the harness opens or drops) and "selftest_", which DropLeftovers never takes for a
		///run's copy (winchk_ and ten digits).</summary>
		private static string NewPlantedName() {
			return ScratchDb.PREFIX+"selftest_"+Guid.NewGuid().ToString("N").Substring(0,12);
		}

		///<summary>Creates the database with only the preference table (as SHOW CREATE TABLE prints Open Dental's) and the rows given.</summary>
		private static void Plant(string name,Dictionary<string,string> dictPrefs) {
			using(MySqlConnection connection=ScratchDb.Open("")) {
				ScratchDb.Exec(connection,"CREATE DATABASE `"+name+"` CHARACTER SET utf8mb3 COLLATE utf8mb3_general_ci");
			}
			using MySqlConnection connectionCopy=ScratchDb.Open(name);
			ScratchDb.Exec(connectionCopy,"CREATE TABLE `preference` (`PrefName` varchar(255) NOT NULL DEFAULT '',`ValueString` text NOT NULL,"
				+"`PrefNum` bigint(20) NOT NULL AUTO_INCREMENT,`Comments` text DEFAULT NULL,PRIMARY KEY (`PrefNum`),KEY `PrefName` (`PrefName`)) "
				+"ENGINE=MyISAM DEFAULT CHARSET=utf8mb3 COLLATE=utf8mb3_general_ci");
			foreach(KeyValuePair<string,string> pair in dictPrefs) {
				using MySqlCommand command=new MySqlCommand("INSERT INTO preference (PrefName,ValueString,Comments) VALUES (@name,@value,'')",connectionCopy);
				command.Parameters.AddWithValue("@name",pair.Key);
				command.Parameters.AddWithValue("@value",pair.Value);
				command.ExecuteNonQuery();
			}
		}

		private static void DropPlanted(string name) {
			if(!name.StartsWith(ScratchDb.PREFIX+"selftest_")) {
				throw new ArgumentException("Only planted self-test databases are dropped here: "+name);
			}
			using MySqlConnection connection=ScratchDb.Open("");
			ScratchDb.Exec(connection,"DROP DATABASE IF EXISTS `"+name+"`");
		}

		private static string Query(string name,string command) {
			using MySqlConnection connection=ScratchDb.Open(name);
			using MySqlCommand mySqlCommand=new MySqlCommand(command,connection);
			object value=mySqlCommand.ExecuteScalar();
			return value==null || value==DBNull.Value ? "" : value.ToString();
		}

		///<summary>CopyMigration on a planted adopted copy one fork level behind this build (every step applied but EHR-2's ehrcode step, as the gate's
		///dump was): the program's runner runs exactly the pending step (the table exists afterwards), the markers say every level of the build is
		///applied, and P0's line names both levels and the step; a second run finds nothing to do ("already at"). A copy a newer build updated, and a
		///planted copy older than the adoption point (24.3.63, no fork markers), are refused with the reason and left unchanged. A name without the
		///winchk_ prefix is refused before any connection. Both databases are dropped at the end.</summary>
		[TestMethod]
		public void CopyMigration_MigratesAnOlderCopy_RefusesUnadoptedAndNewer() {
			Assert.ThrowsExactly<ArgumentException>(() => CopyMigration.Run("opendental_selftest"));
			List<ForkMigrationStep> listSteps=ForkMigrationRunner.GetSteps();
			long levelEhr=listSteps.Single(x => x is EhrCodeTableStep).Level;
			string levelLatest=listSteps.Max(x => x.Level).ToString();
			List<long> listApplied=listSteps.Select(x => x.Level).Where(x => x!=levelEhr).ToList();
			string levelBefore=listApplied.Max().ToString();
			string nameAdopted=NewPlantedName();
			string nameUnadopted=NewPlantedName();
			try {
				Plant(nameAdopted,new Dictionary<string,string> {
					{ "DataBaseVersion","99.0.0.0" },
					{ "ProgramVersion","99.0.0.0" },
					{ "OdSchemaCompat","26.2.0" },
					{ "ForkAdoptedFrom","source=24.3.63.0;started=2026-11-01T14:05:11Z;adopted=2026-11-01T14:05:13Z" },
					{ "ForkSchemaLevel",levelBefore },
					{ "ForkMigrationsApplied",string.Join(",",listApplied) },
					{ "ForkMigrationInProgress","" },
				});
				CopyMigrationResult copyMigrationResult=CopyMigration.Run(nameAdopted);
				Assert.IsTrue(copyMigrationResult.IsDone,copyMigrationResult.Text);
				StringAssert.StartsWith(copyMigrationResult.Text,"migrated the copy from "+levelBefore+" to "+levelLatest+": steps "+levelEhr+" ",
					copyMigrationResult.Text);
				Assert.IsTrue(copyMigrationResult.ListLog.Any(x => x.StartsWith("Fork migration step "+levelEhr+" ") && x.EndsWith(" changed the database:")),
					string.Join(" | ",copyMigrationResult.ListLog));
				Assert.AreEqual("1",Query(nameAdopted,"SELECT COUNT(*) FROM information_schema.TABLES WHERE TABLE_SCHEMA='"+nameAdopted+"' AND TABLE_NAME='ehrcode'"));
				Assert.AreEqual(levelLatest,Query(nameAdopted,"SELECT ValueString FROM preference WHERE PrefName='ForkSchemaLevel'"));
				CollectionAssert.AreEquivalent(listSteps.Select(x => x.Level.ToString()).ToList(),
					Query(nameAdopted,"SELECT ValueString FROM preference WHERE PrefName='ForkMigrationsApplied'").Split(',').ToList());
				Assert.AreEqual("",Query(nameAdopted,"SELECT ValueString FROM preference WHERE PrefName='ForkMigrationInProgress'"));
				//Again: nothing pending.
				copyMigrationResult=CopyMigration.Run(nameAdopted);
				Assert.IsTrue(copyMigrationResult.IsDone,copyMigrationResult.Text);
				StringAssert.StartsWith(copyMigrationResult.Text,"already at "+levelLatest+" ",copyMigrationResult.Text);
				Assert.AreEqual(0,copyMigrationResult.ListLog.Count);
				//A level this build does not have (a newer build updated the dump): refused, nothing changed.
				using(MySqlConnection connection=ScratchDb.Open(nameAdopted)) {
					ScratchDb.Exec(connection,"UPDATE preference SET ValueString=CONCAT(ValueString,',2099010101') WHERE PrefName='ForkMigrationsApplied'");
				}
				string appliedNewer=Query(nameAdopted,"SELECT ValueString FROM preference WHERE PrefName='ForkMigrationsApplied'");
				copyMigrationResult=CopyMigration.Run(nameAdopted);
				Assert.IsFalse(copyMigrationResult.IsDone,copyMigrationResult.Text);
				StringAssert.Contains(copyMigrationResult.Text,"updated by a newer build",copyMigrationResult.Text);
				StringAssert.Contains(copyMigrationResult.Text,"NeedsNewerBuild",copyMigrationResult.Text);
				Assert.AreEqual(appliedNewer,Query(nameAdopted,"SELECT ValueString FROM preference WHERE PrefName='ForkMigrationsApplied'"));
				//Older than the adoption point: a 24.3.63 database without fork markers. Refused with the reason; no row and no table is added.
				Plant(nameUnadopted,new Dictionary<string,string> {
					{ "DataBaseVersion","24.3.63.0" },
					{ "ProgramVersion","24.3.63.0" },
				});
				copyMigrationResult=CopyMigration.Run(nameUnadopted);
				Assert.IsFalse(copyMigrationResult.IsDone,copyMigrationResult.Text);
				StringAssert.Contains(copyMigrationResult.Text,"older than the adoption point",copyMigrationResult.Text);
				StringAssert.Contains(copyMigrationResult.Text,"DataBaseVersion 24.3.63.0",copyMigrationResult.Text);
				Assert.AreEqual("2",Query(nameUnadopted,"SELECT COUNT(*) FROM preference"));
				Assert.AreEqual("1",Query(nameUnadopted,"SELECT COUNT(*) FROM information_schema.TABLES WHERE TABLE_SCHEMA='"+nameUnadopted+"'"));
			}
			finally {
				DropPlanted(nameAdopted);
				DropPlanted(nameUnadopted);
			}
			Assert.AreEqual("0",Query("","SELECT COUNT(*) FROM information_schema.SCHEMATA WHERE SCHEMA_NAME IN ('"+nameAdopted+"','"+nameUnadopted+"')"));
		}

		///<summary>A form that is never activated and never in the taskbar, placed off every screen.</summary>
		private class FormOffscreen:Form {
			private const int WS_EX_TOOLWINDOW=0x00000080;
			private const int WS_EX_NOACTIVATE=0x08000000;

			public FormOffscreen() {
				StartPosition=FormStartPosition.Manual;
				Location=new Point(-20000,-20000);
				Size=new Size(400,300);
				ShowInTaskbar=false;
				Text="WinChk MenuStrip self-test";
			}

			protected override bool ShowWithoutActivation {
				get {
					return true;
				}
			}

			protected override CreateParams CreateParams {
				get {
					CreateParams createParams=base.CreateParams;
					createParams.ExStyle|=WS_EX_TOOLWINDOW|WS_EX_NOACTIVATE;
					return createParams;
				}
			}
		}

		///<summary>Uia.ChooseMenuStripItem on a MenuStrip laid out as RdlDesigner's (File, View > Designer, RDL Text, Preview with their shortcuts, a
		///DropDownOpening handler that sets Preview's Enabled state, as RdlDesigner's menuView_Popup does): View > RDL Text is found by its text,
		///its menu opened (the handler ran) and its Click run once, and the menu is closed again; a Preview the handler disables is found and reported
		///disabled, not clicked; an item or a menu that is not there is reported absent with the texts that are. The form is built in this process
		///on a thread of its own, off the screen, and is closed at the end; no input is sent.</summary>
		[TestMethod]
		public void MenuStripLookup_ChoosesItemsByText() {
			FormOffscreen formOffscreen=null;
			ToolStripMenuItem menuView=null;
			List<string> listClicked=new List<string>();
			int countOpening=0;
			bool isPreviewEnabled=true;
			ManualResetEventSlim manualResetEventSlimShown=new ManualResetEventSlim(false);
			Exception exceptionThread=null;
			Thread thread=new Thread(() => {
				try {
					formOffscreen=new FormOffscreen();
					MenuStrip menuStrip=new MenuStrip { Name="menuMain",Dock=DockStyle.Top };
					EventHandler eventHandlerClick=(sender,e) => {
						lock(listClicked) {
							listClicked.Add(((ToolStripItem)sender).Text);
						}
					};
					ToolStripMenuItem menuFile=new ToolStripMenuItem("&File");
					menuFile.DropDownItems.Add(new ToolStripMenuItem("E&xit",null,eventHandlerClick,Keys.Control|Keys.Q));
					ToolStripMenuItem menuViewPreview=new ToolStripMenuItem("Preview",null,eventHandlerClick,Keys.F5);
					menuView=new ToolStripMenuItem("&View");
					menuView.DropDownItems.AddRange(new ToolStripItem[] { new ToolStripMenuItem("Designer",null,eventHandlerClick,Keys.F7),
						new ToolStripMenuItem("RDL Text",null,eventHandlerClick,Keys.Shift|Keys.F7),menuViewPreview,new ToolStripSeparator(),
						new ToolStripMenuItem("Properties...",null,eventHandlerClick,Keys.F4) });
					menuView.DropDownOpening+=(sender,e) => {
						Interlocked.Increment(ref countOpening);
						menuViewPreview.Enabled=Volatile.Read(ref isPreviewEnabled);
					};
					//The drop-downs show on the nearest screen even for a form off the screen: fully transparent, nothing is seen.
					menuFile.DropDown.Opacity=0;
					menuView.DropDown.Opacity=0;
					menuStrip.Items.AddRange(new ToolStripItem[] { menuFile,menuView });
					formOffscreen.Controls.Add(menuStrip);
					formOffscreen.MainMenuStrip=menuStrip;
					formOffscreen.Shown+=(sender,e) => manualResetEventSlimShown.Set();
					Application.Run(formOffscreen);
				}
				catch(Exception ex) {
					exceptionThread=ex;
					manualResetEventSlimShown.Set();
				}
			});
			thread.SetApartmentState(ApartmentState.STA);
			thread.IsBackground=true;
			thread.Start();
			try {
				Assert.IsTrue(manualResetEventSlimShown.Wait(30000),"The off-screen form shows");
				Assert.IsNull(exceptionThread,exceptionThread?.ToString());
				IntPtr hwnd=(IntPtr)formOffscreen.Invoke((Func<IntPtr>)(() => formOffscreen.Handle));
				Rectangle rectangle=(Rectangle)formOffscreen.Invoke((Func<Rectangle>)(() => formOffscreen.Bounds));
				Assert.IsFalse(Screen.AllScreens.Any(x => x.Bounds.IntersectsWith(rectangle)),"The form is off every screen: "+rectangle);
				int processId=Process.GetCurrentProcess().Id;
				//1. View > RDL Text: opened, chosen, closed.
				Uia.MenuChoice menuChoice=Uia.ChooseMenuStripItem(hwnd,processId,new[] { "View","RDL Text" });
				string evidence=menuChoice.Found+"; "+menuChoice.Note+"; "+menuChoice.Error;
				Assert.IsTrue(menuChoice.IsFound,evidence);
				Assert.IsFalse(menuChoice.IsDisabled,evidence);
				Assert.IsTrue(menuChoice.IsInvoked,evidence);
				Assert.AreEqual("",menuChoice.Error,evidence);
				StringAssert.StartsWith(menuChoice.Found,"File, View / Designer, RDL Text, Preview, Properties...",evidence);
				Assert.IsTrue(SpinWait.SpinUntil(() => { lock(listClicked) { return listClicked.Count>0; } },5000),"RDL Text's Click ran: "+evidence);
				lock(listClicked) {
					CollectionAssert.AreEqual(new[] { "RDL Text" },listClicked,evidence);
				}
				Assert.IsTrue(Volatile.Read(ref countOpening)>=1,"View's DropDownOpening ran: "+evidence);
				Assert.IsTrue(SpinWait.SpinUntil(() => !(bool)formOffscreen.Invoke((Func<bool>)(() => menuView.DropDown.Visible)),5000),"View's menu is closed again");
				//2. A Preview the menu's handler disables: found, reported disabled, not clicked; the menu is closed again.
				Volatile.Write(ref isPreviewEnabled,false);
				int countOpeningBefore=Volatile.Read(ref countOpening);
				menuChoice=Uia.ChooseMenuStripItem(hwnd,processId,new[] { "View","Preview" });
				evidence=menuChoice.Found+"; "+menuChoice.Note+"; "+menuChoice.Error;
				Assert.IsTrue(menuChoice.IsFound,evidence);
				Assert.IsTrue(menuChoice.IsDisabled,evidence);
				Assert.IsFalse(menuChoice.IsInvoked,evidence);
				Assert.IsTrue(Volatile.Read(ref countOpening)>countOpeningBefore,"View's DropDownOpening ran again: "+evidence);
				Assert.IsTrue(SpinWait.SpinUntil(() => !(bool)formOffscreen.Invoke((Func<bool>)(() => menuView.DropDown.Visible)),5000),"View's menu is closed again");
				//3. An item the menu does not have, and a menu the strip does not have: absent, with the texts that are there.
				menuChoice=Uia.ChooseMenuStripItem(hwnd,processId,new[] { "View","Show Report in Browser" });
				Assert.IsFalse(menuChoice.IsFound,menuChoice.Found);
				StringAssert.Contains(menuChoice.Found," / Designer, RDL Text, Preview",menuChoice.Found);
				menuChoice=Uia.ChooseMenuStripItem(hwnd,processId,new[] { "Tools","Options" });
				Assert.IsFalse(menuChoice.IsFound,menuChoice.Found);
				StringAssert.Contains(menuChoice.Found,"File, View",menuChoice.Found);
				lock(listClicked) {
					CollectionAssert.AreEqual(new[] { "RDL Text" },listClicked,"Nothing else was clicked");
				}
			}
			finally {
				if(formOffscreen!=null && formOffscreen.IsHandleCreated) {
					formOffscreen.BeginInvoke((Action)(() => formOffscreen.Close()));
				}
				thread.Join(30000);
			}
		}
	}
}
