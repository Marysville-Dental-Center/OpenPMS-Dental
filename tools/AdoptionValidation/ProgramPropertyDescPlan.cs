// Licensed under the GNU General Public License, version 2 or later. See LICENSE in the repository root.
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using OpenPmsDentalBusiness.ForkMigrations.FeatureSteps;
using SyntheticData.Infrastructure;
using Db=SyntheticData.Infrastructure.Db;

namespace AdoptionValidation {
	///<summary>What the fork migration's program property description step (ProgramPropertyDescRenameStep, package UI-BRAND-2) changes when adoption
	///brings an Open Dental 25.3.48 database to 26.2.0, worked out before adoption and read-only. On every database, an adopted office copy included
	///(Open Dental's conversions wrote both rows into every database), the step renames the PropertyDesc of Dental Studio's "UserName (clear to use
	///OD username)" and Podium's "Enter 0 to use Open Dental for sending review invitations, ..." rows to this program's texts; the values and every
	///other column stay. The plan is the step's own (ProgramPropertyDescRenames.Plan) on rows read with plain SQL. migration-preview prints it
	///(runbook step 0.2), and adopt's row check (AdoptionRowCheck) holds the adopted copy's programproperty rows to it. Reads program and
	///programproperty.</summary>
	public static class ProgramPropertyDescPlan {
		public const string TABLE_PROGRAMPROPERTY="programproperty";
		public const string COLUMN_PROPERTYDESC="PropertyDesc";

		///<summary>Level of the step, for messages.</summary>
		public static long StepLevel {
			get { return new ProgramPropertyDescRenameStep().Level; }
		}

		public class Plan {
			///<summary>False when the database has no program or programproperty table: the step then changes nothing.</summary>
			public bool HasTables;
			///<summary>The step's plan.</summary>
			public ProgramPropertyDescRenames.RowPlan Rows=new ProgramPropertyDescRenames.RowPlan();

			public bool IsChanging {
				get { return HasTables && Rows.IsChanging; }
			}

			///<summary>What migration-preview prints for the step.</summary>
			public List<string> Lines() {
				string step="Step "+StepLevel+" (program property descriptions)";
				if(!HasTables) {
					return new List<string> { step+" changes nothing: there is no program or programproperty table." };
				}
				if(!Rows.IsChanging && Rows.ListKept.Count==0) {
					return new List<string> { step+" changes nothing: no Dental Studio or Podium program property has the old description." };
				}
				List<string> listLines=new List<string>();
				if(Rows.IsChanging) {
					listLines.Add(step+": renames the PropertyDesc of "+Rows.ListRenamed.Count+" programproperty row(s) (ProgramPropertyNum "
						+string.Join(", ",Rows.ListRenamed.Select(x => x.Row.ProgramPropertyNum))+"), PropertyDesc only; their values stay. Adopt's row check "
						+"(step 4) requires exactly this: no other programproperty row or column changes, none is added or goes.");
				}
				if(Rows.ListKept.Count>0) {
					listLines.Add(step+": keeps "+Rows.ListKept.Count+" row(s) under the old description, because their program already has the new one for "
						+"the same clinic (ProgramPropertyNum "+string.Join(", ",Rows.ListKept.Select(x => x.Row.ProgramPropertyNum))+").");
				}
				return listLines;
			}
		}

		///<summary>The plan for a database on this server, read with plain SQL.</summary>
		public static Plan Make(Db db,string database) {
			LocalOnly.EnsureSafeIdentifier(database);
			Plan plan=new Plan();
			plan.HasTables=Migration26_1Plan.HasTable(db,database,DxcCreditScorePlan.TABLE_PROGRAM)
				&& Migration26_1Plan.HasTable(db,database,TABLE_PROGRAMPROPERTY);
			if(!plan.HasTables) {
				return plan;
			}
			plan.Rows=ProgramPropertyDescRenames.Plan(DxcCreditScorePlan.ReadPrograms(db,database),ReadProperties(db,database));
			return plan;
		}

		///<summary>Every programproperty row, with the columns the step reads, in ProgramPropertyNum order.</summary>
		public static List<ProgramPropertyDescRenames.PropertyRow> ReadProperties(Db db,string database) {
			LocalOnly.EnsureSafeIdentifier(database);
			return db.Rows("SELECT ProgramPropertyNum,ProgramNum,ClinicNum,PropertyDesc FROM `"+database+"`.programproperty ORDER BY ProgramPropertyNum")
				.Select(x => new ProgramPropertyDescRenames.PropertyRow {
					ProgramPropertyNum=Convert.ToInt64(x[0],CultureInfo.InvariantCulture),
					ProgramNum=Convert.ToInt64(x[1],CultureInfo.InvariantCulture),
					ClinicNum=Convert.ToInt64(x[2],CultureInfo.InvariantCulture),
					PropertyDesc=x[3]==null || x[3] is DBNull ? "" : Convert.ToString(x[3],CultureInfo.InvariantCulture),
				}).ToList();
		}
	}
}
