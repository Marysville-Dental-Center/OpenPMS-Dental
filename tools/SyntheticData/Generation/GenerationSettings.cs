// Licensed under the GNU General Public License, version 2. See LICENSE in the repository root.
using System;
using System.Globalization;

namespace SyntheticData.Generation {
	public enum DataSize {
		Small,
		Medium,
		Large,
	}

	///<summary>Everything that determines the generated data. The same settings always produce the same database.</summary>
	public class GenerationSettings {
		public const long DefaultSeed=20261018;
		public static readonly DateTime DefaultAsOf=new DateTime(2026,9,30);

		public long Seed=DefaultSeed;
		public DataSize Size=DataSize.Small;
		///<summary>Number of families. 0 uses the size preset.</summary>
		public int Families;
		///<summary>Years of history before the as-of date. 0 uses the size preset.</summary>
		public int Years;
		///<summary>Last day of history. Nothing is completed, paid or posted after it; appointments are scheduled up to FutureDays later.</summary>
		public DateTime AsOf=DefaultAsOf;
		public int FutureDays=90;
		///<summary>Optional local database (for example a fresh 24.3.63 dev database) whose configuration tables are copied first.</summary>
		public string ConfigFrom;
		///<summary>When above 0, every table still empty at the end gets this many schema-valid filler rows.</summary>
		public int FillRemaining;
		///<summary>The version labels written to DataBaseVersion and ProgramVersion. Same schema either way (25.3.48).</summary>
		public VersionLabels Labels=VersionLabels.Reference;

		///<summary>DataBaseVersion of the generated database: 25.3.48.0, or 25.3.47.0 for the office labels (Open Dental changes DataBaseVersion
		///only when a convert step runs, and 25.3.47 had the last one before 25.3.54).</summary>
		public string DataBaseVersion {
			get { return Labels==VersionLabels.Office ? "25.3.47.0" : "25.3.48.0"; }
		}

		///<summary>ProgramVersion of the generated database: 25.3.48.0, or 25.3.54.0 for the office labels.</summary>
		public string ProgramVersion {
			get { return Labels==VersionLabels.Office ? "25.3.54.0" : "25.3.48.0"; }
		}

		public int FamilyCount {
			get {
				if(Families>0) {
					return Families;
				}
				switch(Size) {
					case DataSize.Medium: return 600;
					case DataSize.Large: return 4000;
					default: return 40;
				}
			}
		}

		public int YearCount {
			get {
				if(Years>0) {
					return Years;
				}
				switch(Size) {
					case DataSize.Medium: return 2;
					case DataSize.Large: return 3;
					default: return 1;
				}
			}
		}

		public DateTime HistoryStart {
			get { return AsOf.AddYears(-YearCount).AddDays(1); }
		}

		public DateTime FutureEnd {
			get { return AsOf.AddDays(FutureDays); }
		}

		public int DentistCount {
			get { return Math.Max(2,(int)Math.Ceiling(FamilyCount/800.0)); }
		}

		public int HygienistCount {
			get { return Math.Max(2,(int)Math.Ceiling(FamilyCount*3.3/1600.0)); }
		}

		public int CarrierCount {
			get { return Math.Min(16,4+FamilyCount/150); }
		}

		public static DataSize ParseSize(string text) {
			switch((text??"").Trim().ToLowerInvariant()) {
				case "small": return DataSize.Small;
				case "medium": return DataSize.Medium;
				case "large": return DataSize.Large;
				default: throw new ArgumentException("Size must be small, medium or large.");
			}
		}

		public static VersionLabels ParseLabels(string text) {
			switch((text??"").Trim().ToLowerInvariant()) {
				case "reference": return VersionLabels.Reference;
				case "office": return VersionLabels.Office;
				default: throw new ArgumentException("Version labels must be reference (25.3.48.0 and 25.3.48.0) or office (25.3.47.0 and 25.3.54.0).");
			}
		}

		public string Describe() {
			return "seed "+Seed+", size "+Size.ToString().ToLowerInvariant()+" ("+FamilyCount+" families, "+YearCount+" year(s) of history), as of "
				+AsOf.ToString("yyyy-MM-dd",CultureInfo.InvariantCulture)
				+(Labels==VersionLabels.Reference ? "" : ", DataBaseVersion "+DataBaseVersion+" with ProgramVersion "+ProgramVersion);
		}
	}

	///<summary>The DataBaseVersion and ProgramVersion pair of a generated database. The schema is the 25.3.48 one either way.</summary>
	public enum VersionLabels {
		///<summary>DataBaseVersion and ProgramVersion 25.3.48.0, the version of the schema.</summary>
		Reference,
		///<summary>DataBaseVersion 25.3.47.0 and ProgramVersion 25.3.54.0, as the office database reads (ForkVersions.ListAdoptableLabels).</summary>
		Office,
	}
}
