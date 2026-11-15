// Open Dental 26.2's extraction graphic on the tooth chart (owner question O-20 of docs/specs/wpf-tooth-chart-plan.md, decided as recommended on
// 2026-11-08). Package TC-5; the rules are in docs/port-notes/TC-5.md.
// Licensed under the GNU General Public License, version 2 or later. See LICENSE in the repository root.
using System;
using CodeBase;

namespace OpenPmsDentalBusiness {
	///<summary>The setting "Show completed extractions with an X on the tooth chart" (Setup, Preferences, Chart - General). Off by default, which is
	///the chart as it always was: a completed extraction (status C, EC or EO) shows only as the missing tooth. When it is on, the tooth chart's one
	///painter (ToothChartPainter.PaintProcedures, in WpfControlsOD) also draws the big X of the extraction in the colour of its status over the
	///tooth's place, and the tooth stays missing. One setting for the whole office: every chart draws it the same way (the Chart module, the big
	///chart, the Treatment Plan printout, sheets and the Patient Dashboard). The Chart module's date slider needs nothing: it already shows an
	///extraction completed after the slider date as planned, or not at all.
	///Open Dental's preference name is not public, so the value lives in the fork's own preference row PREF_NAME, which is not in PrefName and is
	///read and written by name (PrefC.GetRawOrDefault and PrefC.UpsertRaw, as RecallProphyPerioSwitch does): the row is inserted the first time
	///the office turns the setting on, and a database without it (every new or adopted database) reads as off. No migration step is needed. If
	///Open Dental's name becomes known, a fork migration step renames the row (as OdSettingNamesStep does).</summary>
	public class ToothChartExtractionGraphic {
		///<summary>The fork's own preference row. Boolean, default off. Global. INFERRED: Open Dental's name is not public.</summary>
		public const string PREF_NAME="ForkToothChartShowCompletedExtractions";

		///<summary>True when the office turned the setting on. A missing row, or a cache that is not loaded yet, reads as off. Never throws, because
		///every chart paint reads it.</summary>
		public static bool IsOn() {
			Meth.NoCheckMiddleTierRole();
			try {
				return PIn.Bool(PrefC.GetRawOrDefault(PREF_NAME,"0"));
			}
			catch(Exception ex) {
				ex.DoNothing();
				return false;
			}
		}

		///<summary>Stores the setting, inserting the row the first time, and updates this workstation's cache. Returns true if the value changed. The
		///caller signals InvalidType.Prefs so other workstations reload, and writes the audit entry (GetChangeLogLine).</summary>
		public static bool Save(bool isOn) {
			Meth.NoCheckMiddleTierRole();
			if(IsOn()==isOn) {
				return false;
			}
			return PrefC.UpsertRaw(PREF_NAME,POut.Bool(isOn));
		}

		///<summary>The audit line for the Chart preferences page, or "" when unchanged.</summary>
		public static string GetChangeLogLine(bool isOnOld,bool isOnNew) {
			if(isOnOld==isOnNew) {
				return "";
			}
			return "Show completed extractions with an X on the tooth chart changed from '"+isOnOld+"' to '"+isOnNew+"'";
		}
	}
}
