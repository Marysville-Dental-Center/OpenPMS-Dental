// Imaging adjust tools: the fork-only tables documentadjust (each image's saved display filters) and imageadjustpreset (the office's own adjust
// presets), the steps that create them (levels 2026112301 and 2026112302 of this feature's range 2026112301 to 2026112309) and the step that grants
// the new Image Adjust Presets permission (2026112303). No step writes a row into either table: the built-in presets live in code.
// Licensed under the GNU General Public License, version 2 or later. See LICENSE in the repository root.
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;

namespace OpenPmsDentalBusiness.ForkMigrations.FeatureSteps {
	///<summary>The Imaging adjust tables, exactly as SHOW CREATE TABLE prints them on MariaDB 10.11. Not Open Dental schema: the schema verifier
	///allows them as extra tables and ForkTables.VerifyDatabase checks them. The steps below create them empty and write no row into them, so on an
	///adopted database they change nothing else, and the adoption tools hold them to empty.</summary>
	public class ImagingAdjustTables:ForkTableSet {
		public const string TABLE_DOCUMENT_ADJUST="documentadjust";
		public const string TABLE_PRESET="imageadjustpreset";

		///<summary>One row per image (the unique key DocNum): its saved gamma, sharpen, smooth, auto contrast and invert. TrimStartMs and TrimEndMs are
		///for a later video phase; 0 means no trim.</summary>
		public const string CREATE_DOCUMENT_ADJUST=@"CREATE TABLE `documentadjust` (
  `DocumentAdjustNum` bigint(20) NOT NULL AUTO_INCREMENT,
  `DocNum` bigint(20) NOT NULL,
  `Gamma` double NOT NULL,
  `Sharpen` int(11) NOT NULL,
  `Smooth` tinyint(4) NOT NULL,
  `IsAutoContrast` tinyint(4) NOT NULL,
  `IsInverted` tinyint(4) NOT NULL,
  `TrimStartMs` int(11) NOT NULL,
  `TrimEndMs` int(11) NOT NULL,
  PRIMARY KEY (`DocumentAdjustNum`),
  UNIQUE KEY `DocNum` (`DocNum`)
) ENGINE=MyISAM DEFAULT CHARSET=utf8mb3 COLLATE=utf8mb3_general_ci";

		///<summary>The office's own adjust presets. The built-in presets live in code, not in this table.</summary>
		public const string CREATE_PRESET=@"CREATE TABLE `imageadjustpreset` (
  `ImageAdjustPresetNum` bigint(20) NOT NULL AUTO_INCREMENT,
  `Description` varchar(255) NOT NULL,
  `ItemOrder` int(11) NOT NULL,
  `WindowingMin` int(11) NOT NULL,
  `WindowingMax` int(11) NOT NULL,
  `Gamma` double NOT NULL,
  `Sharpen` int(11) NOT NULL,
  `Smooth` tinyint(4) NOT NULL,
  `IsAutoContrast` tinyint(4) NOT NULL,
  `IsInverted` tinyint(4) NOT NULL,
  `IsHidden` tinyint(4) NOT NULL,
  PRIMARY KEY (`ImageAdjustPresetNum`)
) ENGINE=MyISAM DEFAULT CHARSET=utf8mb3 COLLATE=utf8mb3_general_ci";

		public override string Feature {
			get { return "Image adjust tools"; }
		}

		public override string Levels {
			get { return "2026112301 to 2026112302"; }
		}

		public override string[] ArrayCreateTables {
			get { return new[] { CREATE_DOCUMENT_ADJUST,CREATE_PRESET }; }
		}
	}

	///<summary>Image adjust tools: each image's saved display filters. Creates the empty table; writes no row.</summary>
	[ForkMigration(2026112301,"Image adjust tools: add the documentadjust table")]
	public class DocumentAdjustTableStep:ForkMigrationStep {
		public override void Apply(ForkMigrationContext context) {
			context.CreateTableIfMissing(ImagingAdjustTables.CREATE_DOCUMENT_ADJUST);
		}
	}

	///<summary>Image adjust tools: the office's own adjust presets. Creates the empty table; writes no row.</summary>
	[ForkMigration(2026112302,"Image adjust tools: add the imageadjustpreset table")]
	public class ImageAdjustPresetTableStep:ForkMigrationStep {
		public override void Apply(ForkMigrationContext context) {
			context.CreateTableIfMissing(ImagingAdjustTables.CREATE_PRESET);
		}
	}

	///<summary>Image adjust tools: Image Adjust Presets for every user group that has Setup (owner decision of 2026-11-23). Plan works out the
	///grouppermission rows to insert without writing; the step (ImageAdjustPresetGrantStep) inserts them, and AdoptionValidation's migration-preview
	///and adopt's row check (ImageAdjustGrantPlan) use Plan too, so the three never disagree.</summary>
	public static class ImageAdjustPresetGrants {
		///<summary>The permissions granted, in the order they are inserted for each user group.</summary>
		public static readonly EnumPermType[] ArrayPermTypes={ EnumPermType.ImageAdjustPresets };

		///<summary>One grouppermission row to insert: FKey 0, NewerDate 0001-01-01, NewerDays 0, as the other grant steps write theirs.</summary>
		public class Grant {
			public long UserGroupNum;
			public EnumPermType PermType;
		}

		///<summary>The rows to insert, given every grouppermission row of the database as (UserGroupNum, PermType): for each user group that has a
		///Setup row, in UserGroupNum order, each permission of ArrayPermTypes that the group has no row of (whatever that row's FKey or date limits).
		///Nothing for any other group, and nothing when every Setup group has it already.</summary>
		public static List<Grant> Plan(List<(long UserGroupNum,int PermType)> listRows) {
			List<Grant> listGrants=new List<Grant>();
			List<long> listUserGroupNumsSetup=listRows.Where(x => x.PermType==(int)EnumPermType.Setup).Select(x => x.UserGroupNum)
				.Distinct().OrderBy(x => x).ToList();
			HashSet<(long,int)> setExisting=new HashSet<(long,int)>(listRows.Select(x => (x.UserGroupNum,x.PermType)));
			foreach(long userGroupNum in listUserGroupNumsSetup) {
				foreach(EnumPermType permType in ArrayPermTypes) {
					if(setExisting.Contains((userGroupNum,(int)permType))) {
						continue;
					}
					listGrants.Add(new Grant { UserGroupNum=userGroupNum,PermType=permType });
				}
			}
			return listGrants;
		}

		///<summary>Every grouppermission row of the connected database as (UserGroupNum, PermType), read straight from the table (not the cache).
		///</summary>
		public static List<(long UserGroupNum,int PermType)> ReadRows() {
			DataTable table=DataCore.GetTable("SELECT UserGroupNum,PermType FROM grouppermission");
			return table.Select().Select(x => (PIn.Long(x[0].ToString()),PIn.Int(x[1].ToString()))).ToList();
		}

		///<summary>The log line of one permission's grants, as the step logs it: "Granted ImageAdjustPresets to 2 Setup user group(s)".</summary>
		public static string Describe(EnumPermType permType,int count) {
			return "Granted "+permType+" to "+count+" Setup user group(s)";
		}

		///<summary>Inserts the planned rows into the connected database. Returns one log line per permission granted (Describe); none when no row was
		///missing.</summary>
		public static List<string> Run() {
			List<Grant> listGrants=Plan(ReadRows());
			foreach(Grant grant in listGrants) {
				DataCore.NonQ("INSERT INTO grouppermission (NewerDate,NewerDays,UserGroupNum,PermType,FKey) VALUES('0001-01-01',0,"
					+POut.Long(grant.UserGroupNum)+","+POut.Int((int)grant.PermType)+",0)");
			}
			return ArrayPermTypes.Select(x => (permType:x,count:listGrants.Count(y => y.PermType==x)))
				.Where(x => x.count>0)
				.Select(x => Describe(x.permType,x.count))
				.ToList();
		}
	}

	///<summary>Image adjust tools: gives Image Adjust Presets to every user group that has Setup (ImageAdjustPresetGrants), inserting only the rows
	///that are missing and changing nothing else. Once per database: the runner runs it once, and a forced rerun (RunAll) of a database whose
	///ForkMigrationsApplied already holds this level skips it, so a grant an administrator removed since is not given back. On the same rows a second
	///run inserts nothing.</summary>
	[ForkMigration(2026112303,"Image adjust tools: grant Image Adjust Presets to the Setup groups")]
	public class ImageAdjustPresetGrantStep:ForkMigrationStep {
		public override void Apply(ForkMigrationContext context) {
			if(ForkMigrationRunner.GetAppliedLevels().Contains(Level)) {
				return;
			}
			foreach(string change in ImageAdjustPresetGrants.Run()) {
				context.Log(change);
			}
		}
	}
}
