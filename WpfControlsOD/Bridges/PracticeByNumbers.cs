using System;
using CodeBase;
using OpenPmsDentalBusiness;

namespace OpenPmsDental.Bridges{
	///<summary>Link to Practice By Numbers Report Provider.</summary>
	public class PracticeByNumbers {

		///<summary></summary>
		public PracticeByNumbers() {}

		///<summary></summary>
		public static void ShowPage() {
			try {
				if(!Programs.IsEnabled(ProgramName.PracticeByNumbers)) {
					return;//Only reachable from buttons that show when the program link is enabled.
				}
				//UI-BRAND-2 (the owner's answer 4): Practice by Numbers' own public site, in place of Open Dental's manual page about the bridge.
				ODFileUtils.ProcessStart("https://www.practicebynumbers.com/");
			}
			catch(Exception ex) {
				MsgBox.Show("PracticeByNumbers","Failed to open web browser.  Please make sure you have a default browser set and are connected to the internet then try again.");
				ex.DoNothing();
			}
		}
	}
}







