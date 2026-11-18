// The Chart module's TextBelow strip (paint type TextBelow, 18, Open Dental 26.2; package TC-5). Licensed under the GNU General Public License,
// version 2 or later. See LICENSE in the repository root.
using System;
using System.Collections.Generic;
using System.Data;
using System.Drawing;
using System.Windows.Forms;

namespace OpenPmsDental{
	///<summary>A one-line, read-only text box under the Chart module's date slider and above the treatment notes (Open Dental 26.2: "the
	///procedure's custom text displays below the odontogram", in such a box in its screenshot). It shows the paint text of the patient's TextBelow
	///procedures that the chart shows, joined in ProcDate order (ToothChartPainter.GetTextBelowStripText), in the Text token (UQ-6) in the standard text-box font,
	///whatever engine draws the chart, and hides itself when there is none. It is not part of the tooth picture, so sheets, printouts, the big
	///chart and the Patient Dashboard do not show it. ControlChart fills it from the procedure list it paints and places it (LayoutTextBelow).</summary>
	public class ToothChartTextBelowStrip:TextBox{
		public ToothChartTextBelowStrip(){
			Multiline=false;
			ReadOnly=true;
			TabStop=false;
			//UI-4c-2 by UQ-6: the strip is an ordinary window part and takes the new look: the Text token on SurfaceRaised (it was black on the Windows
			//window colour); it stays a Windows text box (section 3.6).
			BackColor=CodeBase.UiTheme.SurfaceRaised;
			ForeColor=CodeBase.UiTheme.Text;
			Visible=false;
		}

		///<summary>True when the strip has text to show, so ControlChart makes room for it. Not Visible, which is false while a parent is hidden.</summary>
		public bool HasText{
			get{
				return Text!="";
			}
		}

		///<summary>Shows the TextBelow texts of listDataRows, the procedure rows the Chart module paints (after the date slider and the chart view's
		///filters), or hides the strip when there is none. Returns true when HasText changed, so the caller lays out the controls under it again.</summary>
		public bool SetProcedures(List<DataRow> listDataRows){
			bool hadText=HasText;
			string text=ToothChartPainter.GetTextBelowStripText(listDataRows);
			if(Text!=text){
				Text=text;
				Select(0,0);
			}
			Visible=HasText;
			return hadText!=HasText;
		}
	}
}
