// UI-BRAND-2 (docs/port-notes/UI-BRAND.md section 10, the owner's answer 4): the two program property descriptions that named Open Dental get
// this program's name. Level 2026112451 (the package's range is 2026112451 to 2026112459).
// Licensed under the GNU General Public License, version 2 or later. See LICENSE in the repository root.
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;

namespace OpenPmsDentalBusiness.ForkMigrations.FeatureSteps {
	///<summary>Program Links shows each program property under its PropertyDesc, which is also the key the code looks the row up by. Two of them
	///named Open Dental: Dental Studio's "UserName (clear to use OD username)" and Podium's "Enter 0 to use Open Dental for sending review
	///invitations, or 1 to use the Service". This step renames those rows' PropertyDesc to the texts the code now looks up
	///(ProgramPropertyDescRenames), so an office keeps its values; PropertyValue and every other column stay. Runs on every database, an adopted
	///Open Dental 25.3.48 one included (Open Dental's own conversions wrote both rows into every database: ConvertDatabases3). The rule is
	///ProgramPropertyDescRenames.Plan, which AdoptionValidation's migration-preview prints and adopt's row check holds the adopted copy to; a second
	///run finds no old text and changes nothing.</summary>
	[ForkMigration(2026112451,"Rename the Dental Studio and Podium program property descriptions")]
	public class ProgramPropertyDescRenameStep:ForkMigrationStep {
		public override void Apply(ForkMigrationContext context) {
			if(!context.TableExists("program") || !context.TableExists("programproperty")) {
				return;
			}
			ProgramPropertyDescRenames.RowPlan plan=ProgramPropertyDescRenames.Plan(DxcCreditScoreRemoved.ReadPrograms(),ProgramPropertyDescRenames.ReadProperties());
			foreach(string change in ProgramPropertyDescRenames.Run(plan)) {
				context.Log(change);
			}
		}
	}

	///<summary>The rule of ProgramPropertyDescRenameStep, worked out without writing (Plan) and applied (Run), shared by the step, AdoptionValidation's
	///migration-preview and adopt's row check. A programproperty row is renamed when its program row's ProgName is the rename's program (compared as
	///MariaDB compares it: case and trailing spaces ignored) and its PropertyDesc is exactly the old text (the code compared it exactly too). A row
	///is left as it is when its program already has a row with the new text for the same ClinicNum (the code reads that one).</summary>
	public static class ProgramPropertyDescRenames {
		///<summary>Dental Studio's user name property (WpfControlsOD DentalStudio): blank sends the Open PMS user's own user name.</summary>
		public const string DentalStudioUserName="UserName (clear to use the Open PMS username)";
		///<summary>Dental Studio's user name property as Open Dental's 13.x conversion named it.</summary>
		public const string DentalStudioUserNameOld="UserName (clear to use OD username)";
		///<summary>Podium's property that chooses who sends the review invitations (Podium.PropertyDescs.UseService).</summary>
		public const string PodiumUseService="Enter 0 to use Open PMS for sending review invitations, or 1 to use the Service";
		///<summary>Podium's property as Open Dental's 17.x conversion named it (ConvertDatabases4).</summary>
		public const string PodiumUseServiceOld="Enter 0 to use Open Dental for sending review invitations, or 1 to use the Service";

		///<summary>One rename: the program's ProgName and the property's old and new PropertyDesc.</summary>
		public class Rename {
			public string ProgName;
			public string DescOld;
			public string DescNew;
		}

		///<summary>The renames, in order.</summary>
		public static readonly List<Rename> ListRenames=new List<Rename> {
			new Rename { ProgName=ProgramName.DentalStudio.ToString(),DescOld=DentalStudioUserNameOld,DescNew=DentalStudioUserName },
			new Rename { ProgName=ProgramName.Podium.ToString(),DescOld=PodiumUseServiceOld,DescNew=PodiumUseService },
		};

		///<summary>The columns of a programproperty row the plan reads.</summary>
		public class PropertyRow {
			public long ProgramPropertyNum;
			public long ProgramNum;
			public long ClinicNum;
			public string PropertyDesc="";
		}

		///<summary>A row the step renames, with its new PropertyDesc.</summary>
		public class RenamedRow {
			public PropertyRow Row;
			public string ProgName="";
			public string DescNew="";
		}

		///<summary>What the step does on one database.</summary>
		public class RowPlan {
			///<summary>The rows renamed, in ProgramPropertyNum order.</summary>
			public List<RenamedRow> ListRenamed=new List<RenamedRow>();
			///<summary>Rows with an old text that stay, because their program already has a row with the new text for the same ClinicNum.</summary>
			public List<RenamedRow> ListKept=new List<RenamedRow>();

			public bool IsChanging {
				get { return ListRenamed.Count>0; }
			}

			///<summary>What the step writes to the fork migration log, in order. Empty when it changes nothing.</summary>
			public List<string> GetLogLines() {
				List<string> listLines=ListRenamed.Select(x => "Renamed the "+x.ProgName+" program property (ProgramPropertyNum "+x.Row.ProgramPropertyNum
					+", ProgramNum "+x.Row.ProgramNum+", ClinicNum "+x.Row.ClinicNum+") to '"+x.DescNew+"'").ToList();
				listLines.AddRange(ListKept.Select(x => "Kept the "+x.ProgName+" program property (ProgramPropertyNum "+x.Row.ProgramPropertyNum
					+") under its old description: its program already has '"+x.DescNew+"' for ClinicNum "+x.Row.ClinicNum));
				return listLines;
			}
		}

		///<summary>True when progName is the rename's program, compared as the database compares it (case and trailing spaces ignored).</summary>
		public static bool IsProgName(string progName,string progNameRename) {
			return string.Equals((progName??"").TrimEnd(' '),progNameRename,StringComparison.OrdinalIgnoreCase);
		}

		///<summary>The plan for the given program and programproperty rows. Reads nothing.</summary>
		public static RowPlan Plan(List<DxcCreditScoreRemoved.ProgramRow> listPrograms,List<PropertyRow> listProperties) {
			RowPlan plan=new RowPlan();
			foreach(PropertyRow row in listProperties.OrderBy(x => x.ProgramPropertyNum)) {
				DxcCreditScoreRemoved.ProgramRow program=listPrograms.FirstOrDefault(x => x.ProgramNum==row.ProgramNum);
				if(program==null) {
					continue;
				}
				Rename rename=ListRenames.FirstOrDefault(x => IsProgName(program.ProgName,x.ProgName) && row.PropertyDesc==x.DescOld);
				if(rename==null) {
					continue;
				}
				RenamedRow renamedRow=new RenamedRow { Row=row,ProgName=rename.ProgName,DescNew=rename.DescNew };
				if(listProperties.Any(x => x.ProgramNum==row.ProgramNum && x.ClinicNum==row.ClinicNum && x.PropertyDesc==rename.DescNew)) {
					plan.ListKept.Add(renamedRow);
					continue;
				}
				plan.ListRenamed.Add(renamedRow);
			}
			return plan;
		}

		///<summary>Every programproperty row, with the columns the plan reads, in ProgramPropertyNum order.</summary>
		public static List<PropertyRow> ReadProperties() {
			DataTable table=Db.GetTable("SELECT ProgramPropertyNum,ProgramNum,ClinicNum,PropertyDesc FROM programproperty ORDER BY ProgramPropertyNum");
			List<PropertyRow> listRows=new List<PropertyRow>();
			for(int i=0;i<table.Rows.Count;i++) {
				listRows.Add(new PropertyRow {
					ProgramPropertyNum=PIn.Long(table.Rows[i]["ProgramPropertyNum"].ToString()),
					ProgramNum=PIn.Long(table.Rows[i]["ProgramNum"].ToString()),
					ClinicNum=PIn.Long(table.Rows[i]["ClinicNum"].ToString()),
					PropertyDesc=table.Rows[i]["PropertyDesc"].ToString(),
				});
			}
			return listRows;
		}

		///<summary>Renames the planned rows, PropertyDesc only, one UPDATE per row by its key. Returns the plan's log lines.</summary>
		public static List<string> Run(RowPlan plan) {
			foreach(RenamedRow renamedRow in plan.ListRenamed) {
				Db.NonQ("UPDATE programproperty SET PropertyDesc='"+POut.String(renamedRow.DescNew)+"' "
					+"WHERE ProgramPropertyNum="+POut.Long(renamedRow.Row.ProgramPropertyNum));
			}
			return plan.GetLogLines();
		}
	}
}
