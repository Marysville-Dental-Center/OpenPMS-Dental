// A small password prompt of the connection settings tool, so that a secret never has to go on a command line.
// Licensed under the GNU General Public License, version 2 or later. See LICENSE in the repository root.
using System;
using System.Drawing;
using System.Windows.Forms;

namespace OpenPmsDentalDbSettings {
	///<summary>Asks for one secret (a .pfx password or the database password) with a masked text box.</summary>
	public class FormSecretPrompt:Form {
		private readonly TextBox _textSecret;

		public FormSecretPrompt(string prompt) {
			Text=FormDbSettings.TITLE;
			FormBorderStyle=FormBorderStyle.FixedDialog;
			MaximizeBox=false;
			MinimizeBox=false;
			StartPosition=FormStartPosition.CenterScreen;
			AutoScaleDimensions=new SizeF(96F,96F);
			AutoScaleMode=AutoScaleMode.Dpi;
			ClientSize=new Size(420,120);
			ShowIcon=false;
			//UI-12 slice 14: on the layout grid (CodeBase.UiGrid): the padding, the prompt (two lines) a caption gap over its box, the bar a
			//section gap under the box, OK and Cancel bottom right a dialog gap apart.
			int widthContent=ClientSize.Width-2*CodeBase.UiGrid.PAD;
			Label labelPrompt=new Label() { Text=prompt,Location=new Point(CodeBase.UiGrid.PAD,CodeBase.UiGrid.PAD),Size=new Size(widthContent,34),
				TextAlign=ContentAlignment.BottomLeft };
			_textSecret=new TextBox() { Location=new Point(CodeBase.UiGrid.PAD,labelPrompt.Bottom+CodeBase.UiGrid.CAPTION_GAP),
				Size=new Size(widthContent,CodeBase.UiGrid.FIELD_HEIGHT),UseSystemPasswordChar=true };
			int yBar=_textSecret.Bottom+CodeBase.UiGrid.SECTION_GAP;
			Button butCancel=new Button() { Text="Cancel",Location=new Point(ClientSize.Width-CodeBase.UiGrid.PAD-75,yBar),Size=new Size(75,24),
				DialogResult=DialogResult.Cancel };
			Button butOK=new Button() { Text="OK",Location=new Point(butCancel.Left-CodeBase.UiGrid.DIALOG_GAP-75,yBar),Size=new Size(75,24),
				DialogResult=DialogResult.OK };
			ClientSize=new Size(ClientSize.Width,Math.Max(ClientSize.Height,yBar+24+CodeBase.UiGrid.PAD));
			Controls.AddRange(new Control[] { labelPrompt,_textSecret,butOK,butCancel });
			CodeBase.UiPlainWindow.Attach(this);//UI-PLAIN: the look of the program's windows (docs/port-notes/UI-PLAIN.md)
			AcceptButton=butOK;
			CancelButton=butCancel;
		}

		///<summary>The secret typed, or throws DbSettingsException (exit code 6) when the user cancels.</summary>
		public static string Ask(string prompt) {
			using(FormSecretPrompt formSecretPrompt=new FormSecretPrompt(prompt)) {
				if(formSecretPrompt.ShowDialog()!=DialogResult.OK) {
					throw new DbSettingsException(DbSettingsRunner.EXIT_CANCELLED,"Cancelled: nothing was saved.");
				}
				return formSecretPrompt._textSecret.Text;
			}
		}
	}
}
