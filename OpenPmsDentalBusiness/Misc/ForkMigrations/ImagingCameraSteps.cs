// Image camera session: the fork-only tables mountitemdefguide (the guide of each photo mount template slot) and mountitemguide (the guide of each
// slot of a patient's mount), and the steps that create them (levels 2026112311 and 2026112312 of this feature's range 2026112311 to 2026112319).
// No step writes a row into either table.
// Licensed under the GNU General Public License, version 2 or later. See LICENSE in the repository root.
using System;

namespace OpenPmsDentalBusiness.ForkMigrations.FeatureSteps {
	///<summary>The Image camera session tables, exactly as SHOW CREATE TABLE prints them on MariaDB 10.11. Not Open Dental schema: the schema
	///verifier allows them as extra tables and ForkTables.VerifyDatabase checks them. The steps below create them empty and write no row into them,
	///so on an adopted database they change nothing else, and the adoption tools hold them to empty.</summary>
	public class ImagingCameraTables:ForkTableSet {
		public const string TABLE_MOUNT_ITEM_DEF_GUIDE="mountitemdefguide";
		public const string TABLE_MOUNT_ITEM_GUIDE="mountitemguide";

		///<summary>The guide of one slot of a mount template (the unique key MountItemDefNum): what the camera should shoot in the slot, whether it
		///is a mirror shot, and whether it is intraoral or extraoral. A slot without a row has no guide.</summary>
		public const string CREATE_MOUNT_ITEM_DEF_GUIDE=@"CREATE TABLE `mountitemdefguide` (
  `MountItemDefGuideNum` bigint(20) NOT NULL AUTO_INCREMENT,
  `MountItemDefNum` bigint(20) NOT NULL,
  `GuideLabel` varchar(255) NOT NULL,
  `IsMirror` tinyint(4) NOT NULL,
  `SlotKind` tinyint(4) NOT NULL,
  PRIMARY KEY (`MountItemDefGuideNum`),
  UNIQUE KEY `MountItemDefNum` (`MountItemDefNum`)
) ENGINE=MyISAM DEFAULT CHARSET=utf8mb3 COLLATE=utf8mb3_general_ci";

		///<summary>The guide of one slot of a patient's mount (the unique key MountItemNum), copied from the template's guide when the mount is
		///created, because a mount does not keep the key of its template.</summary>
		public const string CREATE_MOUNT_ITEM_GUIDE=@"CREATE TABLE `mountitemguide` (
  `MountItemGuideNum` bigint(20) NOT NULL AUTO_INCREMENT,
  `MountItemNum` bigint(20) NOT NULL,
  `GuideLabel` varchar(255) NOT NULL,
  `IsMirror` tinyint(4) NOT NULL,
  `SlotKind` tinyint(4) NOT NULL,
  PRIMARY KEY (`MountItemGuideNum`),
  UNIQUE KEY `MountItemNum` (`MountItemNum`)
) ENGINE=MyISAM DEFAULT CHARSET=utf8mb3 COLLATE=utf8mb3_general_ci";

		public override string Feature {
			get { return "Image camera session"; }
		}

		public override string Levels {
			get { return "2026112311 to 2026112312"; }
		}

		public override string[] ArrayCreateTables {
			get { return new[] { CREATE_MOUNT_ITEM_DEF_GUIDE,CREATE_MOUNT_ITEM_GUIDE }; }
		}
	}

	///<summary>Image camera session: the guide of each photo mount template slot. Creates the empty table; writes no row.</summary>
	[ForkMigration(2026112311,"Image camera session: add the mountitemdefguide table")]
	public class MountItemDefGuideTableStep:ForkMigrationStep {
		public override void Apply(ForkMigrationContext context) {
			context.CreateTableIfMissing(ImagingCameraTables.CREATE_MOUNT_ITEM_DEF_GUIDE);
		}
	}

	///<summary>Image camera session: the guide of each slot of a patient's mount. Creates the empty table; writes no row.</summary>
	[ForkMigration(2026112312,"Image camera session: add the mountitemguide table")]
	public class MountItemGuideTableStep:ForkMigrationStep {
		public override void Apply(ForkMigrationContext context) {
			context.CreateTableIfMissing(ImagingCameraTables.CREATE_MOUNT_ITEM_GUIDE);
		}
	}
}
