// Open Dental 26.2's extraction graphic on the tooth chart (owner question O-20 of docs/specs/wpf-tooth-chart-plan.md; since the owner's rule of
// 2026-11-14, "keep it the same as OD latest version", on by default as in Open Dental 26.2). Package TC-5; the rules are in docs/port-notes/TC-5.md.
// Licensed under the GNU General Public License, version 2 or later. See LICENSE in the repository root.
using System;
using CodeBase;

namespace OpenPmsDentalBusiness {
	///<summary>The setting "Show completed extractions with an X on the tooth chart" (Setup, Preferences, Chart - General). On by default, as in
	///Open Dental 26.2 ("The preference is enabled by default.", its 26.2 blog): the tooth chart's one painter (ToothChartPainter.PaintProcedures,
	///in WpfControlsOD) draws the big X of a completed extraction (status C, EC or EO) in the colour of its status over the tooth's place, and the
	///tooth stays hidden by its missing initial, as Open Dental 26.2's screenshot shows. It is a plain switch: the X is drawn whatever else is
	///charted on the tooth (a completed implant or pontic included). Turned off, the chart is as it was before 26.2: a completed extraction shows
	///only as the missing tooth, with no X. One setting for the whole office: every chart draws it the same way (the Chart module, the big chart,
	///the Treatment Plan printout, sheets and the Patient Dashboard). The Chart module's date slider needs nothing: it already shows an extraction
	///completed after the slider date as planned, or not at all.
	///Open Dental's preference name is not public, so the value lives in the fork's own preference row PREF_NAME, which is not in PrefName and is
	///read and written by name (PrefC.GetRawOrDefault and PrefC.UpsertRaw, as RecallProphyPerioSwitch does): a database without the row (every new
	///or adopted database, and every fork database from before TC-5) reads as on, and the row is inserted, as "0", the first time the office turns
	///the setting off. No migration step is needed. If Open Dental's name becomes known, a fork migration step renames the row (as
	///OdSettingNamesStep does).</summary>
	public class ToothChartExtractionGraphic {
		///<summary>The fork's own preference row. Boolean, default on (Open Dental 26.2's default). Global. INFERRED: Open Dental's name is not public.</summary>
		public const string PREF_NAME="ForkToothChartShowCompletedExtractions";

		///<summary>False when the office turned the setting off. A missing row reads as on, the default. A cache that is not loaded yet also reads
		///as on: GetRawOrDefault gives the default then, and the catch below, which only an unexpected fault reaches, returns the same default
		///on purpose, so a chart painted before the cache loads looks like one painted after it in an office that never changed the setting.
		///Never throws, because every chart paint reads it.</summary>
		public static bool IsOn() {
			Meth.NoCheckMiddleTierRole();
			try {
				return PIn.Bool(PrefC.GetRawOrDefault(PREF_NAME,"1"));
			}
			catch(Exception ex) {
				ex.DoNothing();
				return true;
			}
		}

		///<summary>Stores the setting and updates this workstation's cache. Returns true if the value changed. Saving the default (on) over a
		///missing row writes nothing; the first time the office turns it off the row is inserted as "0", and a later change updates that row. The
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
