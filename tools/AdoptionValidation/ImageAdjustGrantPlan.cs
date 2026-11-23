// Licensed under the GNU General Public License, version 2 or later. See LICENSE in the repository root.
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using OpenPmsDentalBusiness;
using OpenPmsDentalBusiness.ForkMigrations.FeatureSteps;
using SyntheticData.Infrastructure;
using Db=SyntheticData.Infrastructure.Db;

namespace AdoptionValidation {
	///<summary>What the fork migration's Image Adjust Presets grant step (ImageAdjustPresetGrantStep, IMG-P3a) changes when adoption runs it, worked
	///out before adoption and read-only: every user group that has Setup gets Image Adjust Presets, one grouppermission row (FKey 0, no date limit),
	///unless it already has a row of that permission. The rows are planned by the step's own rule (ImageAdjustPresetGrants.Plan) on the database's
	///grouppermission rows; no step before it adds or removes a Setup row. A database whose ForkMigrationsApplied already holds the step's level gets
	///nothing (the step runs once). migration-preview prints the plan (runbook step 0.2), and adopt's row check (AdoptionRowCheck) holds the adopted
	///copy to the plan made from the original. Reads grouppermission and the ForkMigrationsApplied preference.</summary>
	public static class ImageAdjustGrantPlan {
		///<summary>Level of the step, for messages.</summary>
		public static long StepLevel {
			get { return new ImageAdjustPresetGrantStep().Level; }
		}

		public class Plan {
			///<summary>False when the database has no grouppermission table; then nothing is planned.</summary>
			public bool HasTable;
			///<summary>True when the step already ran on the database (its level is in ForkMigrationsApplied); then it changes nothing.</summary>
			public bool IsAlreadyApplied;
			///<summary>The user groups that have Setup, in UserGroupNum order.</summary>
			public List<long> ListUserGroupsSetup=new List<long>();
			///<summary>The rows the step inserts, in its order.</summary>
			public List<ImageAdjustPresetGrants.Grant> ListGrants=new List<ImageAdjustPresetGrants.Grant>();

			///<summary>The number of planned rows of one permission.</summary>
			public int CountFor(EnumPermType permType) {
				return ListGrants.Count(x => x.PermType==permType);
			}

			///<summary>What migration-preview prints for the step.</summary>
			public List<string> Lines() {
				string step="Step "+StepLevel+" (Image Adjust Presets permission)";
				List<string> listLines=new List<string>();
				if(!HasTable) {
					listLines.Add(step+" changes nothing: there is no grouppermission table.");
					return listLines;
				}
				if(IsAlreadyApplied) {
					listLines.Add(step+" changes nothing: it already ran on this database.");
					return listLines;
				}
				if(ListGrants.Count==0) {
					listLines.Add(step+" changes nothing: "+(ListUserGroupsSetup.Count==0 ? "no user group has Setup."
						: "each of the "+ListUserGroupsSetup.Count+" user group(s) with Setup has it already."));
					return listLines;
				}
				List<string> listParts=ImageAdjustPresetGrants.ArrayPermTypes.Where(x => CountFor(x)>0)
					.Select(x => x+" to "+CountFor(x)+" (user group "+string.Join(", ",ListGrants.Where(y => y.PermType==x).Select(y => y.UserGroupNum))+")")
					.ToList();
				listLines.Add(step+" adds "+ListGrants.Count+" grouppermission row(s) (FKey 0, no date limit) for the "+ListUserGroupsSetup.Count
					+" user group(s) with Setup: "+string.Join("; ",listParts)+". The adoption log says "
					+string.Join(" and ",ImageAdjustPresetGrants.ArrayPermTypes.Where(x => CountFor(x)>0).Select(x => "\""+ImageAdjustPresetGrants.Describe(x,CountFor(x))+"\""))
					+"; adopt's row check (step 4) requires exactly these rows.");
				return listLines;
			}
		}

		///<summary>The plan for a database on this server.</summary>
		public static Plan Make(Db db,string database) {
			LocalOnly.EnsureSafeIdentifier(database);
			Plan plan=new Plan();
			if(!Migration26_1Plan.HasTable(db,database,"grouppermission")) {
				return plan;
			}
			plan.HasTable=true;
			plan.IsAlreadyApplied=HeadlessAdoption.ReadState(db,database).GetAppliedLevels().Contains(StepLevel.ToString(CultureInfo.InvariantCulture));
			List<(long UserGroupNum,int PermType)> listRows=db.Rows("SELECT UserGroupNum,PermType FROM `"+database+"`.grouppermission")
				.Select(x => (Convert.ToInt64(x[0],CultureInfo.InvariantCulture),Convert.ToInt32(x[1],CultureInfo.InvariantCulture))).ToList();
			plan.ListUserGroupsSetup=listRows.Where(x => x.PermType==(int)EnumPermType.Setup).Select(x => x.UserGroupNum).Distinct().OrderBy(x => x)
				.ToList();
			if(!plan.IsAlreadyApplied) {
				plan.ListGrants=ImageAdjustPresetGrants.Plan(listRows);
			}
			return plan;
		}
	}
}
