"""FormHL7MsgEdit on the grid (UI-12 slice 4, layout script on the planner's tree). The planner left 8 px between HL7 Status and AptNum
(the others 4) and between AptNum and Message Text: AptNum and Message Text on the 27 px pitch, the message box's bottom kept."""


def layout(h):
    P = h.H + 4
    y = h.n("textHL7Status").ny + P
    h.set("textAptNum", None, y)
    h.set("labelAptNum", None, y)
    m = h.n("textMsgTxt")
    y2 = y + P
    h.set("textMsgTxt", None, y2, None, m.ny + m.nh - y2)
    h.set("labelMsgTxt", None, y2)
