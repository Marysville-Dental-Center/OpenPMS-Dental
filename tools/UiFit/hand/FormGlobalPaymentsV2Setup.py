"""FormGlobalPaymentsV2Setup on the grid (UI-12 slice 5, layout script on the planner's tree). The clinic picker drew its own "Clinic" label
in its first 35 px beside the window's labelClinic, which the code shows with clinics on (main too: "Clinic Clinic"): the picker no longer
draws its label (ShowLabel = false, the property's own advice for a separate label), its box on the check boxes' column (189) and labelClinic
right-aligned 6 px before it. In Payment Settings the key note (30 px tall, up to three lines at run time: "You need the permission ...")
made the App Key row 30 px and put its box 3 px under its label: the rows on the 27 px pitch, the note beside App Key and Account name
(two rows tall, its first line on App Key's text), the rows under them 7 px up and the window with them.

Fix round 2: the top rows (Enabled, Use Sandbox, the clinic picker) stood on a column of their own at 189, 47 px after the Payment
Settings group's field column (142 in the window): they take the group's field column, labelClinic right-aligned 6 px before the box."""


def layout(h):
    H = h.H
    fx = h.n("groupPaySettings").nx + h.n("textAppId").nx
    for name in ("checkEnabled", "checkUseSandbox"):
        h.set(name, fx, None)
    c = h.n("comboClinic")
    h.set("comboClinic", fx, None, c.nw - 35, H)
    lw = h.lw("labelClinic")
    h.set("labelClinic", fx - 6 - lw, c.ny, lw, H)
    h.text_align("labelClinic", "MiddleRight")
    y = 23
    for lab, fld in (("labelAppId", "textAppId"), ("labelAppKey", "textAppKey"), ("labelAccountName", "textAccountName"),
                     ("labelPaymentType", "comboPaymentType")):
        h.set(lab, None, y, None, H)
        h.set(fld, None, y, None, H)
        y += H + 4
    k = h.n("textAppKey")
    g = h.n("groupPaySettings")
    x = k.nx + k.nw + 6
    h.set("labelKeyStatus", x, 50 + 3, g.nw - 8 - x, 2 * H + 4 - 3)
    h.text_align("labelKeyStatus", "TopLeft")
    dy = y - (h.n("checkPrintReceipt").ny - 4)
    for name in ("checkPrintReceipt", "checkPreventSavingNewCC", "checkForceDuplicates", "labelClinicHint", "butTest", "labelTestResult"):
        h.set(name, None, h.n(name).ny + dy)
    h.set("groupPaySettings", None, None, None, g.nh + dy)
    for name in ("groupTerminals", "butSave"):
        h.set(name, None, h.n(name).ny + dy)
    h.set(".", None, None, None, h.tree.nh + dy)
