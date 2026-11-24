# UI-PLAIN-2 audit: G-UI3-uiplain2, every picture opened (docs/port-notes/UI-PLAIN.md section 8)

Before: this branch built with the looks off (`UiLooks.IsOffForGallery`: no look, no frame); its 21 slice pictures are byte for byte
slice 13's gallery pictures of `main` (G-UI3-ui12-13). The title-bar before pictures are Windows' frame as Windows prints a window that was
never shown in the 96 dpi test process (the classic caption; on screen Windows 11 draws its own), less the invisible resize borders. After:
this branch (`work/after-100`, `after-150`). Opened as contact sheets at 80% and 50%, the bands cropped at 100% (`work/au-*.png`, `ov-*.png`,
`sheet-*.png`). One line per picture: picture | changed % | what was checked | finding.

## Pass 1 (findings, fixed before pass 2)

- 3-title-bars/*: a strip of Windows' printed caption buttons showed under the band (the harness printed the whole window, whose
  classic caption lies where the client area now starts): the harness prints the client area alone (WM_PRINTCLIENT). Harness only.
- 4-title-bars-150/*: the title text at its 96 dpi size in a 150% band: the harness draws the point size at its scale (as a 150% screen does).
- 3-title-bars/*, 4-title-bars-150/*: the default window icon drawn on dialog frames (FixedDialog) and tool windows, where Windows draws
  none: the band shows the icon where Windows showed one. (FormODBase shows the program's icon on every window; a plain window has none.)
- MDIChild: drawn as a top-level window with the band; it is only ever an MDI child (Windows' MDI frame): left out of the title bars.

## Pass 2 (no findings)

### 1-slice-windows

- 1-slice-windows/ChartAxisCtl.png | 90.4119 | look: background token, framed inputs, combo faces, check boxes, radio buttons, group boxes, tabs, buttons (AcceptButton primary); a window with its title bar | ok
- 1-slice-windows/DialogDatabase-connection.png | 97.9094 | look: background token, framed inputs, combo faces, check boxes, radio buttons, group boxes, tabs, buttons (AcceptButton primary); a window with its title bar | ok
- 1-slice-windows/DialogDatabase-grouping.png | 86.8646 | look: background token, framed inputs, combo faces, check boxes, radio buttons, group boxes, tabs, buttons (AcceptButton primary); a window with its title bar | ok
- 1-slice-windows/DialogDatabase-parameters.png | 77.4982 | look: background token, framed inputs, combo faces, check boxes, radio buttons, group boxes, tabs, buttons (AcceptButton primary); a window with its title bar | ok
- 1-slice-windows/DialogDatabase.png | 90.3586 | look: background token, framed inputs, combo faces, check boxes, radio buttons, group boxes, tabs, buttons (AcceptButton primary); a window with its title bar | ok
- 1-slice-windows/DialogNewChart.png | 67.9659 | look: background token, framed inputs, combo faces, check boxes, radio buttons, group boxes, tabs, buttons (AcceptButton primary); a window with its title bar | ok
- 1-slice-windows/DialogNewTable.png | 82.4076 | look: background token, framed inputs, combo faces, check boxes, radio buttons, group boxes, tabs, buttons (AcceptButton primary); a window with its title bar | ok
- 1-slice-windows/DialogToolOptions-desktop.png | 96.4609 | look: background token, framed inputs, combo faces, check boxes, radio buttons, group boxes, tabs, buttons (AcceptButton primary); a window with its title bar | ok
- 1-slice-windows/DialogToolOptions-toolbar.png | 65.8765 | look: background token, framed inputs, combo faces, check boxes, radio buttons, group boxes, tabs, buttons (AcceptButton primary); a window with its title bar | ok
- 1-slice-windows/DialogToolOptions.png | 95.6280 | look: background token, framed inputs, combo faces, check boxes, radio buttons, group boxes, tabs, buttons (AcceptButton primary); a window with its title bar | ok
- 1-slice-windows/FindTab-goto.png | 98.4624 | look: background token, framed inputs, combo faces, check boxes, radio buttons, group boxes, tabs, buttons (AcceptButton primary); a window with its title bar | ok
- 1-slice-windows/FindTab-replace.png | 93.2319 | look: background token, framed inputs, combo faces, check boxes, radio buttons, group boxes, tabs, buttons (AcceptButton primary); a window with its title bar | ok
- 1-slice-windows/FindTab.png | 96.5385 | look: background token, framed inputs, combo faces, check boxes, radio buttons, group boxes, tabs, buttons (AcceptButton primary); a window with its title bar | ok
- 1-slice-windows/FormParameterInput.png | 99.8517 | look: background token, framed inputs, combo faces, check boxes, radio buttons, group boxes, tabs, buttons (AcceptButton primary); a window with its title bar | ok: its inputs are added at Load from the report: the band and the buttons; a dialog frame: no icon, as Windows shows none
- 1-slice-windows/InteractivityCtl.png | 77.1687 | look: background token, framed inputs, combo faces, check boxes, radio buttons, group boxes, tabs, buttons (AcceptButton primary); a window with its title bar | ok
- 1-slice-windows/PositionCtl.png | 87.5110 | look: background token, framed inputs, combo faces, check boxes, radio buttons, group boxes, tabs, buttons (AcceptButton primary); a window with its title bar | ok
- 1-slice-windows/ReportCtl.png | 79.9178 | look: background token, framed inputs, combo faces, check boxes, radio buttons, group boxes, tabs, buttons (AcceptButton primary); a window with its title bar | ok
- 1-slice-windows/ReportParameterCtl.png | 82.6740 | look: background token, framed inputs, combo faces, check boxes, radio buttons, group boxes, tabs, buttons (AcceptButton primary); a window with its title bar | ok
- 1-slice-windows/StyleBorderCtl.png | 85.5464 | look: background token, framed inputs, combo faces, check boxes, radio buttons, group boxes, tabs, buttons (AcceptButton primary); a window with its title bar | ok
- 1-slice-windows/StyleCtl.png | 85.2829 | look: background token, framed inputs, combo faces, check boxes, radio buttons, group boxes, tabs, buttons (AcceptButton primary); a window with its title bar | ok
- 1-slice-windows/StyleTextCtl.png | 84.4549 | look: background token, framed inputs, combo faces, check boxes, radio buttons, group boxes, tabs, buttons (AcceptButton primary); a window with its title bar | ok

### 2-at-150

- 2-at-150/DialogDatabase-150.png | 94.5653 | look at 150% (the window scaled as its AutoScale draws it), the band 39 px, the title 12.4 pt | ok
- 2-at-150/DialogNewChart-150.png | 67.6947 | look at 150% (the window scaled as its AutoScale draws it), the band 39 px, the title 12.4 pt | ok
- 2-at-150/DialogNewTable-150.png | 80.8842 | look at 150% (the window scaled as its AutoScale draws it), the band 39 px, the title 12.4 pt | ok
- 2-at-150/FormParameterInput-150.png | 99.9753 | look at 150% (the window scaled as its AutoScale draws it), the band 39 px, the title 12.4 pt | ok: its inputs are added at Load from the report: the band and the buttons; a dialog frame: no icon, as Windows shows none
- 2-at-150/StyleBorderCtl-150.png | 89.9748 | look at 150% (the window scaled as its AutoScale draws it), the band 39 px, the title 12.4 pt | ok
- 2-at-150/StyleCtl-150.png | 86.5578 | look at 150% (the window scaled as its AutoScale draws it), the band 39 px, the title 12.4 pt | ok

### 3-title-bars

- 3-title-bars/DialogAbout-frame.png | 61.8577 | title band 26 px in TitleBar, title text, icon as Windows showed it, caption buttons as the window has them, 1 px outline, content below the band | ok
- 3-title-bars/DialogDataSourceRef-frame.png | 81.2351 | title band 26 px in TitleBar, title text, icon as Windows showed it, caption buttons as the window has them, 1 px outline, content below the band | ok
- 3-title-bars/DialogDataSources-frame.png | 79.6637 | title band 26 px in TitleBar, title text, icon as Windows showed it, caption buttons as the window has them, 1 px outline, content below the band | ok
- 3-title-bars/DialogDatabase-frame.png | 89.6568 | title band 26 px in TitleBar, title text, icon as Windows showed it, caption buttons as the window has them, 1 px outline, content below the band | ok
- 3-title-bars/DialogEmbeddedImages-frame.png | 92.4651 | title band 26 px in TitleBar, title text, icon as Windows showed it, caption buttons as the window has them, 1 px outline, content below the band | ok
- 3-title-bars/DialogExprEditor-frame.png | 39.1614 | title band 26 px in TitleBar, title text, icon as Windows showed it, caption buttons as the window has them, 1 px outline, content below the band | ok
- 3-title-bars/DialogFilterOperator-frame.png | 96.4088 | title band 26 px in TitleBar, title text, icon as Windows showed it, caption buttons as the window has them, 1 px outline, content below the band | ok
- 3-title-bars/DialogListOfStrings-frame.png | 51.1966 | title band 26 px in TitleBar, title text, icon as Windows showed it, caption buttons as the window has them, 1 px outline, content below the band | ok
- 3-title-bars/DialogNew-frame.png | 18.6287 | title band 26 px in TitleBar, title text, icon as Windows showed it, caption buttons as the window has them, 1 px outline, content below the band | ok
- 3-title-bars/DialogNewChart-frame.png | 65.2589 | title band 26 px in TitleBar, title text, icon as Windows showed it, caption buttons as the window has them, 1 px outline, content below the band | ok
- 3-title-bars/DialogNewMatrix-frame.png | 68.4538 | title band 26 px in TitleBar, title text, icon as Windows showed it, caption buttons as the window has them, 1 px outline, content below the band | ok
- 3-title-bars/DialogNewTable-frame.png | 80.7825 | title band 26 px in TitleBar, title text, icon as Windows showed it, caption buttons as the window has them, 1 px outline, content below the band | ok
- 3-title-bars/DialogToolOptions-frame.png | 95.0855 | title band 26 px in TitleBar, title text, icon as Windows showed it, caption buttons as the window has them, 1 px outline, content below the band | ok
- 3-title-bars/DialogValidValues-frame.png | 99.8811 | title band 26 px in TitleBar, title text, icon as Windows showed it, caption buttons as the window has them, 1 px outline, content below the band | ok: no caption buttons: its ControlBox is off (Windows shows none either)
- 3-title-bars/DialogValidateRdl-frame.png | 47.3774 | title band 26 px in TitleBar, title text, icon as Windows showed it, caption buttons as the window has them, 1 px outline, content below the band | ok
- 3-title-bars/DocumentationBuilder-Form1-frame.png | 95.6334 | title band 26 px in TitleBar, title text, icon as Windows showed it, caption buttons as the window has them, 1 px outline, content below the band | ok
- 3-title-bars/DrillParametersDialog-frame.png | 95.7603 | title band 26 px in TitleBar, title text, icon as Windows showed it, caption buttons as the window has them, 1 px outline, content below the band | ok: no caption buttons: its ControlBox is off (Windows shows none either)
- 3-title-bars/FindTab-frame.png | 96.0831 | title band 26 px in TitleBar, title text, icon as Windows showed it, caption buttons as the window has them, 1 px outline, content below the band | ok
- 3-title-bars/FormCentralChooseDatabase-frame.png | 80.4591 | title band 26 px in TitleBar, title text, icon as Windows showed it, caption buttons as the window has them, 1 px outline, content below the band | ok
- 3-title-bars/FormCentralConnectionEdit-frame.png | 79.8975 | title band 26 px in TitleBar, title text, icon as Windows showed it, caption buttons as the window has them, 1 px outline, content below the band | ok
- 3-title-bars/FormCentralConnectionGroupEdit-frame.png | 31.0910 | title band 26 px in TitleBar, title text, icon as Windows showed it, caption buttons as the window has them, 1 px outline, content below the band | ok
- 3-title-bars/FormCentralConnectionGroups-frame.png | 39.9756 | title band 26 px in TitleBar, title text, icon as Windows showed it, caption buttons as the window has them, 1 px outline, content below the band | ok
- 3-title-bars/FormCentralConnections-frame.png | 42.7631 | title band 26 px in TitleBar, title text, icon as Windows showed it, caption buttons as the window has them, 1 px outline, content below the band | ok
- 3-title-bars/FormCentralGroupPermEdit-frame.png | 91.2980 | title band 26 px in TitleBar, title text, icon as Windows showed it, caption buttons as the window has them, 1 px outline, content below the band | ok
- 3-title-bars/FormCentralLogOn-frame.png | 66.1871 | title band 26 px in TitleBar, title text, icon as Windows showed it, caption buttons as the window has them, 1 px outline, content below the band | ok
- 3-title-bars/FormCentralManager-frame.png | 32.7806 | title band 26 px in TitleBar, title text, icon as Windows showed it, caption buttons as the window has them, 1 px outline, content below the band | ok
- 3-title-bars/FormCentralPasswordChange-frame.png | 89.4930 | title band 26 px in TitleBar, title text, icon as Windows showed it, caption buttons as the window has them, 1 px outline, content below the band | ok
- 3-title-bars/FormCentralPasswordCheck-frame.png | 91.1251 | title band 26 px in TitleBar, title text, icon as Windows showed it, caption buttons as the window has them, 1 px outline, content below the band | ok
- 3-title-bars/FormCentralPatientSearch-frame.png | 38.9675 | title band 26 px in TitleBar, title text, icon as Windows showed it, caption buttons as the window has them, 1 px outline, content below the band | ok
- 3-title-bars/FormCentralPatientTransfer-frame.png | 32.2676 | title band 26 px in TitleBar, title text, icon as Windows showed it, caption buttons as the window has them, 1 px outline, content below the band | ok
- 3-title-bars/FormCentralProdInc-frame.png | 94.0253 | title band 26 px in TitleBar, title text, icon as Windows showed it, caption buttons as the window has them, 1 px outline, content below the band | ok
- 3-title-bars/FormCentralUserEdit-frame.png | 31.1979 | title band 26 px in TitleBar, title text, icon as Windows showed it, caption buttons as the window has them, 1 px outline, content below the band | ok
- 3-title-bars/FormCentralUserGroupEdit-frame.png | 93.8363 | title band 26 px in TitleBar, title text, icon as Windows showed it, caption buttons as the window has them, 1 px outline, content below the band | ok
- 3-title-bars/FormCentralUserGroups-frame.png | 43.4067 | title band 26 px in TitleBar, title text, icon as Windows showed it, caption buttons as the window has them, 1 px outline, content below the band | ok
- 3-title-bars/FormCentralUserPasswordEdit-frame.png | 79.1982 | title band 26 px in TitleBar, title text, icon as Windows showed it, caption buttons as the window has them, 1 px outline, content below the band | ok
- 3-title-bars/FormCentralUserSettings-frame.png | 86.6812 | title band 26 px in TitleBar, title text, icon as Windows showed it, caption buttons as the window has them, 1 px outline, content below the band | ok
- 3-title-bars/FormDashboardEditCell-frame.png | 99.9483 | title band 26 px in TitleBar, title text, icon as Windows showed it, caption buttons as the window has them, 1 px outline, content below the band | ok
- 3-title-bars/FormDashboardEditTab-frame.png | 97.6790 | title band 26 px in TitleBar, title text, icon as Windows showed it, caption buttons as the window has them, 1 px outline, content below the band | ok
- 3-title-bars/FormDashboardNamePrompt-frame.png | 87.5021 | title band 26 px in TitleBar, title text, icon as Windows showed it, caption buttons as the window has them, 1 px outline, content below the band | ok
- 3-title-bars/FormDatabaseCheck-frame.png | 35.5060 | title band 26 px in TitleBar, title text, icon as Windows showed it, caption buttons as the window has them, 1 px outline, content below the band | ok
- 3-title-bars/FormDbSettings-frame.png | 88.9761 | title band 26 px in TitleBar, title text, icon as Windows showed it, caption buttons as the window has them, 1 px outline, content below the band | ok
- 3-title-bars/FormDebug-frame.png | 42.0238 | title band 26 px in TitleBar, title text, icon as Windows showed it, caption buttons as the window has them, 1 px outline, content below the band | ok
- 3-title-bars/FormMySQLUserEdit-frame.png | 87.3400 | title band 26 px in TitleBar, title text, icon as Windows showed it, caption buttons as the window has them, 1 px outline, content below the band | ok
- 3-title-bars/FormMySQLUserManager-frame.png | 74.2736 | title band 26 px in TitleBar, title text, icon as Windows showed it, caption buttons as the window has them, 1 px outline, content below the band | ok
- 3-title-bars/FormParameterInput-frame.png | 99.8600 | title band 26 px in TitleBar, title text, icon as Windows showed it, caption buttons as the window has them, 1 px outline, content below the band | ok: its inputs are added at Load from the report: the band and the buttons; a dialog frame: no icon, as Windows shows none
- 3-title-bars/FormPrintImage-frame.png | 23.6168 | title band 26 px in TitleBar, title text, icon as Windows showed it, caption buttons as the window has them, 1 px outline, content below the band | ok
- 3-title-bars/FormPrintSettings-frame.png | 23.5548 | title band 26 px in TitleBar, title text, icon as Windows showed it, caption buttons as the window has them, 1 px outline, content below the band | ok
- 3-title-bars/FormProgressExtended-frame.png | 81.7527 | title band 26 px in TitleBar, title text, icon as Windows showed it, caption buttons as the window has them, 1 px outline, content below the band | ok
- 3-title-bars/FormProgressStatus-frame.png | 99.9216 | title band 26 px in TitleBar, title text, icon as Windows showed it, caption buttons as the window has them, 1 px outline, content below the band | ok: no title bar: a window without a border keeps none (as before)
- 3-title-bars/FormSecretPrompt-frame.png | 89.4565 | title band 26 px in TitleBar, title text, icon as Windows showed it, caption buttons as the window has them, 1 px outline, content below the band | ok
- 3-title-bars/FormServiceManage-frame.png | 88.4364 | title band 26 px in TitleBar, title text, icon as Windows showed it, caption buttons as the window has them, 1 px outline, content below the band | ok
- 3-title-bars/FormWebConfigSettings-frame.png | 83.6291 | title band 26 px in TitleBar, title text, icon as Windows showed it, caption buttons as the window has them, 1 px outline, content below the band | ok
- 3-title-bars/MsgBoxCopyPaste@CodeBase-frame.png | 17.2617 | title band 26 px in TitleBar, title text, icon as Windows showed it, caption buttons as the window has them, 1 px outline, content below the band | ok
- 3-title-bars/MsgBoxCopyPaste@UpdateFileCopier-frame.png | 17.0093 | title band 26 px in TitleBar, title text, icon as Windows showed it, caption buttons as the window has them, 1 px outline, content below the band | ok
- 3-title-bars/PropertyDialog-frame.png | 99.9203 | title band 26 px in TitleBar, title text, icon as Windows showed it, caption buttons as the window has them, 1 px outline, content below the band | ok: its pages are added in code: the band, the empty page area, the AcceptButton primary
- 3-title-bars/RdlDesigner-frame.png | 99.9891 | title band 26 px in TitleBar, title text, icon as Windows showed it, caption buttons as the window has them, 1 px outline, content below the band | ok: the main window built from InitializeComponent only (its menus and toolbar are made in code before it): the band and the MDI area
- 3-title-bars/SQLCtl-frame.png | 42.5379 | title band 26 px in TitleBar, title text, icon as Windows showed it, caption buttons as the window has them, 1 px outline, content below the band | ok
- 3-title-bars/ServiceManager-FormMain-frame.png | 72.9324 | title band 26 px in TitleBar, title text, icon as Windows showed it, caption buttons as the window has them, 1 px outline, content below the band | ok
- 3-title-bars/SingleCtlDialog-frame.png | 99.9203 | title band 26 px in TitleBar, title text, icon as Windows showed it, caption buttons as the window has them, 1 px outline, content below the band | ok: its control is added in code: the band and the buttons
- 3-title-bars/UpdateFileCopier-FormMain-frame.png | 99.8023 | title band 26 px in TitleBar, title text, icon as Windows showed it, caption buttons as the window has them, 1 px outline, content below the band | ok
- 3-title-bars/xCrudGenerator-Form1-frame.png | 40.8978 | title band 26 px in TitleBar, title text, icon as Windows showed it, caption buttons as the window has them, 1 px outline, content below the band | ok

### 4-title-bars-150

- 4-title-bars-150/DialogAbout-frame-150.png | 60.6835 | the same at 150%: band 39 px, buttons 45 by 38, glyph slot 24, title 12.4 pt | ok
- 4-title-bars-150/DialogDataSourceRef-frame-150.png | 86.3677 | the same at 150%: band 39 px, buttons 45 by 38, glyph slot 24, title 12.4 pt | ok
- 4-title-bars-150/DialogDataSources-frame-150.png | 83.1805 | the same at 150%: band 39 px, buttons 45 by 38, glyph slot 24, title 12.4 pt | ok
- 4-title-bars-150/DialogDatabase-frame-150.png | 94.4525 | the same at 150%: band 39 px, buttons 45 by 38, glyph slot 24, title 12.4 pt | ok
- 4-title-bars-150/DialogEmbeddedImages-frame-150.png | 93.3902 | the same at 150%: band 39 px, buttons 45 by 38, glyph slot 24, title 12.4 pt | ok
- 4-title-bars-150/DialogExprEditor-frame-150.png | 34.7248 | the same at 150%: band 39 px, buttons 45 by 38, glyph slot 24, title 12.4 pt | ok
- 4-title-bars-150/DialogFilterOperator-frame-150.png | 97.8866 | the same at 150%: band 39 px, buttons 45 by 38, glyph slot 24, title 12.4 pt | ok
- 4-title-bars-150/DialogListOfStrings-frame-150.png | 49.2609 | the same at 150%: band 39 px, buttons 45 by 38, glyph slot 24, title 12.4 pt | ok
- 4-title-bars-150/DialogNew-frame-150.png | 14.7236 | the same at 150%: band 39 px, buttons 45 by 38, glyph slot 24, title 12.4 pt | ok
- 4-title-bars-150/DialogNewChart-frame-150.png | 66.7351 | the same at 150%: band 39 px, buttons 45 by 38, glyph slot 24, title 12.4 pt | ok
- 4-title-bars-150/DialogNewMatrix-frame-150.png | 69.9273 | the same at 150%: band 39 px, buttons 45 by 38, glyph slot 24, title 12.4 pt | ok
- 4-title-bars-150/DialogNewTable-frame-150.png | 80.3051 | the same at 150%: band 39 px, buttons 45 by 38, glyph slot 24, title 12.4 pt | ok
- 4-title-bars-150/DialogToolOptions-frame-150.png | 97.4082 | the same at 150%: band 39 px, buttons 45 by 38, glyph slot 24, title 12.4 pt | ok
- 4-title-bars-150/DialogValidValues-frame-150.png | 100.0000 | the same at 150%: band 39 px, buttons 45 by 38, glyph slot 24, title 12.4 pt | ok: no caption buttons: its ControlBox is off (Windows shows none either)
- 4-title-bars-150/DialogValidateRdl-frame-150.png | 44.2244 | the same at 150%: band 39 px, buttons 45 by 38, glyph slot 24, title 12.4 pt | ok
- 4-title-bars-150/DocumentationBuilder-Form1-frame-150.png | 97.3030 | the same at 150%: band 39 px, buttons 45 by 38, glyph slot 24, title 12.4 pt | ok
- 4-title-bars-150/DrillParametersDialog-frame-150.png | 97.7745 | the same at 150%: band 39 px, buttons 45 by 38, glyph slot 24, title 12.4 pt | ok: no caption buttons: its ControlBox is off (Windows shows none either)
- 4-title-bars-150/FindTab-frame-150.png | 97.9706 | the same at 150%: band 39 px, buttons 45 by 38, glyph slot 24, title 12.4 pt | ok
- 4-title-bars-150/FormCentralChooseDatabase-frame-150.png | 85.3427 | the same at 150%: band 39 px, buttons 45 by 38, glyph slot 24, title 12.4 pt | ok
- 4-title-bars-150/FormCentralConnectionEdit-frame-150.png | 81.8538 | the same at 150%: band 39 px, buttons 45 by 38, glyph slot 24, title 12.4 pt | ok
- 4-title-bars-150/FormCentralConnectionGroupEdit-frame-150.png | 30.7277 | the same at 150%: band 39 px, buttons 45 by 38, glyph slot 24, title 12.4 pt | ok
- 4-title-bars-150/FormCentralConnectionGroups-frame-150.png | 39.2947 | the same at 150%: band 39 px, buttons 45 by 38, glyph slot 24, title 12.4 pt | ok
- 4-title-bars-150/FormCentralConnections-frame-150.png | 42.3937 | the same at 150%: band 39 px, buttons 45 by 38, glyph slot 24, title 12.4 pt | ok
- 4-title-bars-150/FormCentralGroupPermEdit-frame-150.png | 93.5257 | the same at 150%: band 39 px, buttons 45 by 38, glyph slot 24, title 12.4 pt | ok
- 4-title-bars-150/FormCentralLogOn-frame-150.png | 65.6567 | the same at 150%: band 39 px, buttons 45 by 38, glyph slot 24, title 12.4 pt | ok
- 4-title-bars-150/FormCentralManager-frame-150.png | 22.8837 | the same at 150%: band 39 px, buttons 45 by 38, glyph slot 24, title 12.4 pt | ok
- 4-title-bars-150/FormCentralPasswordChange-frame-150.png | 95.0258 | the same at 150%: band 39 px, buttons 45 by 38, glyph slot 24, title 12.4 pt | ok
- 4-title-bars-150/FormCentralPasswordCheck-frame-150.png | 95.8088 | the same at 150%: band 39 px, buttons 45 by 38, glyph slot 24, title 12.4 pt | ok
- 4-title-bars-150/FormCentralPatientSearch-frame-150.png | 37.7784 | the same at 150%: band 39 px, buttons 45 by 38, glyph slot 24, title 12.4 pt | ok
- 4-title-bars-150/FormCentralPatientTransfer-frame-150.png | 31.6879 | the same at 150%: band 39 px, buttons 45 by 38, glyph slot 24, title 12.4 pt | ok
- 4-title-bars-150/FormCentralProdInc-frame-150.png | 94.9567 | the same at 150%: band 39 px, buttons 45 by 38, glyph slot 24, title 12.4 pt | ok
- 4-title-bars-150/FormCentralUserEdit-frame-150.png | 32.9630 | the same at 150%: band 39 px, buttons 45 by 38, glyph slot 24, title 12.4 pt | ok
- 4-title-bars-150/FormCentralUserGroupEdit-frame-150.png | 96.9082 | the same at 150%: band 39 px, buttons 45 by 38, glyph slot 24, title 12.4 pt | ok
- 4-title-bars-150/FormCentralUserGroups-frame-150.png | 42.9906 | the same at 150%: band 39 px, buttons 45 by 38, glyph slot 24, title 12.4 pt | ok
- 4-title-bars-150/FormCentralUserPasswordEdit-frame-150.png | 87.4157 | the same at 150%: band 39 px, buttons 45 by 38, glyph slot 24, title 12.4 pt | ok
- 4-title-bars-150/FormCentralUserSettings-frame-150.png | 89.7688 | the same at 150%: band 39 px, buttons 45 by 38, glyph slot 24, title 12.4 pt | ok
- 4-title-bars-150/FormDashboardEditCell-frame-150.png | 99.9904 | the same at 150%: band 39 px, buttons 45 by 38, glyph slot 24, title 12.4 pt | ok
- 4-title-bars-150/FormDashboardEditTab-frame-150.png | 97.7055 | the same at 150%: band 39 px, buttons 45 by 38, glyph slot 24, title 12.4 pt | ok
- 4-title-bars-150/FormDashboardNamePrompt-frame-150.png | 93.9481 | the same at 150%: band 39 px, buttons 45 by 38, glyph slot 24, title 12.4 pt | ok
- 4-title-bars-150/FormDatabaseCheck-frame-150.png | 41.0944 | the same at 150%: band 39 px, buttons 45 by 38, glyph slot 24, title 12.4 pt | ok
- 4-title-bars-150/FormDbSettings-frame-150.png | 94.3841 | the same at 150%: band 39 px, buttons 45 by 38, glyph slot 24, title 12.4 pt | ok
- 4-title-bars-150/FormDebug-frame-150.png | 44.8615 | the same at 150%: band 39 px, buttons 45 by 38, glyph slot 24, title 12.4 pt | ok
- 4-title-bars-150/FormMySQLUserEdit-frame-150.png | 94.0504 | the same at 150%: band 39 px, buttons 45 by 38, glyph slot 24, title 12.4 pt | ok
- 4-title-bars-150/FormMySQLUserManager-frame-150.png | 75.0379 | the same at 150%: band 39 px, buttons 45 by 38, glyph slot 24, title 12.4 pt | ok
- 4-title-bars-150/FormParameterInput-frame-150.png | 99.9849 | the same at 150%: band 39 px, buttons 45 by 38, glyph slot 24, title 12.4 pt | ok: its inputs are added at Load from the report: the band and the buttons; a dialog frame: no icon, as Windows shows none
- 4-title-bars-150/FormPrintImage-frame-150.png | 23.4734 | the same at 150%: band 39 px, buttons 45 by 38, glyph slot 24, title 12.4 pt | ok
- 4-title-bars-150/FormPrintSettings-frame-150.png | 23.4163 | the same at 150%: band 39 px, buttons 45 by 38, glyph slot 24, title 12.4 pt | ok
- 4-title-bars-150/FormProgressExtended-frame-150.png | 81.7849 | the same at 150%: band 39 px, buttons 45 by 38, glyph slot 24, title 12.4 pt | ok
- 4-title-bars-150/FormProgressStatus-frame-150.png | 99.9346 | the same at 150%: band 39 px, buttons 45 by 38, glyph slot 24, title 12.4 pt | ok: no title bar: a window without a border keeps none (as before)
- 4-title-bars-150/FormSecretPrompt-frame-150.png | 94.9367 | the same at 150%: band 39 px, buttons 45 by 38, glyph slot 24, title 12.4 pt | ok
- 4-title-bars-150/FormServiceManage-frame-150.png | 94.0054 | the same at 150%: band 39 px, buttons 45 by 38, glyph slot 24, title 12.4 pt | ok
- 4-title-bars-150/FormWebConfigSettings-frame-150.png | 87.4468 | the same at 150%: band 39 px, buttons 45 by 38, glyph slot 24, title 12.4 pt | ok
- 4-title-bars-150/MsgBoxCopyPaste@CodeBase-frame-150.png | 15.8278 | the same at 150%: band 39 px, buttons 45 by 38, glyph slot 24, title 12.4 pt | ok
- 4-title-bars-150/MsgBoxCopyPaste@UpdateFileCopier-frame-150.png | 15.7155 | the same at 150%: band 39 px, buttons 45 by 38, glyph slot 24, title 12.4 pt | ok
- 4-title-bars-150/PropertyDialog-frame-150.png | 100.0000 | the same at 150%: band 39 px, buttons 45 by 38, glyph slot 24, title 12.4 pt | ok: its pages are added in code: the band, the empty page area, the AcceptButton primary
- 4-title-bars-150/RdlDesigner-frame-150.png | 99.9978 | the same at 150%: band 39 px, buttons 45 by 38, glyph slot 24, title 12.4 pt | ok: the main window built from InitializeComponent only (its menus and toolbar are made in code before it): the band and the MDI area
- 4-title-bars-150/SQLCtl-frame-150.png | 39.0926 | the same at 150%: band 39 px, buttons 45 by 38, glyph slot 24, title 12.4 pt | ok
- 4-title-bars-150/ServiceManager-FormMain-frame-150.png | 72.4391 | the same at 150%: band 39 px, buttons 45 by 38, glyph slot 24, title 12.4 pt | ok
- 4-title-bars-150/SingleCtlDialog-frame-150.png | 100.0000 | the same at 150%: band 39 px, buttons 45 by 38, glyph slot 24, title 12.4 pt | ok: its control is added in code: the band and the buttons
- 4-title-bars-150/UpdateFileCopier-FormMain-frame-150.png | 99.9364 | the same at 150%: band 39 px, buttons 45 by 38, glyph slot 24, title 12.4 pt | ok
- 4-title-bars-150/xCrudGenerator-Form1-frame-150.png | 41.0463 | the same at 150%: band 39 px, buttons 45 by 38, glyph slot 24, title 12.4 pt | ok
