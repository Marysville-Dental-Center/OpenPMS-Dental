# Port note: IMG-ACL (access to unfiled patient images)

Why: the coordinator's follow-up to the camera work (IMG-P4b-3). The unsaved-photo folder and the X-ray quarantine hold patient images that
every Windows account on the computer could read, through ProgramData's own access list. The owner's answer is open in
[owner-questions.md](../owner-questions.md) ("Who can open unfiled patient images on a computer"). It is built as recommended there.

## What changes

1. **Three private folders** under `%ProgramData%\OpenPMS-Dental\Imaging` (`ImagingDeviceManager/Config/ImagingFolderAcl.cs`, ListPrivateRoots):
   - `Photos\unsaved`: camera photos and videos that could not be saved (PhotoQuarantine, VideoFiles.MoveToUnsaved);
   - `Video\recording`: a recording in progress, or one a crash cut off;
   - `Capture`: the X-ray capture journal, quarantine, resolved captures (with their `journal` copies) and the capture log.

   Everything under a private folder is private too. `Photos` and `Video` themselves, `capture.ini` and the Imaging folder keep the Users
   Modify rule they had.
2. **The access list**, set on each of the three folders and inherited by everything in them. It replaces the inherited list:
   - SYSTEM: Full control.
   - Administrators: Full control.
   - The people who use Open PMS: Modify. These are the local group "OpenPMS-Dental Users" when the computer has one. It is looked up as
     `<computer>\OpenPMS-Dental Users`, so a domain group of that name never counts. Otherwise it is the local Users group.

   Setup and the program never make the group or change its members.
3. **The program** (`LocalCaptureConfig.EnsureImagingFolder`, which every one of these folders is made through):
   - For a private folder, the folder at the top (for example `Capture` for `Capture\journal`) gets the list unless it already has it.
     The folder asked for then inherits it.
   - "Has it" (ImagingFolderAcl.IsPrivate) means every Allow rule, its own or inherited, is for one of the three, and they have Full
     control, Full control and Modify for the folder and everything in it.
   - The list is never set by a Windows user it would lock out (ImagingFolderAcl.IsCurrentUserAllowed). That is anyone whose sign-in
     token lacks the group, Administrators (elevated) and SYSTEM, including a user added to the group who has not signed in again. The
     folder then keeps its inherited access.
   - A folder that is private already, but does not let this user in, is reported as not usable. The error names the group and says to
     sign out and in, since writing there would fail anyway.
   - The list is never set through a junction or link. ImagingFolderAcl.HasReparsePoint checks the folder and every folder up to the
     Imaging folder; anyone may put one there, since the Imaging folder gives Users Modify.
   - When the list cannot be set (also when the folder belongs to another Windows account and the program is not elevated), the folder
     keeps its old access and stays usable, and capture is never refused for it.
   - The call returns a warning. A root's warning and its subfolder's are joined. Only the X-ray capture's writers (journal, quarantine,
     capture log) log the warning. The photo and video code ignores it, as it did the Users rule's.
   - Unlike the Users rule, a folder that was just made is not removed when the list fails.
   - The group look-up is reused for a minute.
4. **Setup** (`installer/OpenPmsDental.iss`, LimitImagingFolders, at ssPostInstall before the connection settings), as administrator,
   changes only the three folders that exist:
   - Nothing is changed when the folder, a folder above it up to ProgramData, or anything inside it is a junction or link, so setup never
     changes a folder outside these. Setup's log says so.
   - First `icacls <folder> /inheritance:r /grant:r *S-1-5-18:(OI)(CI)F *S-1-5-32-544:(OI)(CI)F <who>:(OI)(CI)M /Q`. When that fails, the
     folder keeps its old list.
   - Then `icacls <subfolder> /reset /Q` for each folder inside, so a subfolder that got a list of its own inherits the new one. Files
     made by the program inherit already, and Windows passes the new list on to them.
   - icacls never gets `/T`, since `/T` follows junctions.
   - `<who>` is the group when `net localgroup "OpenPMS-Dental Users"` finds it, otherwise `*S-1-5-32-545`.
   - A failure is logged in setup's log and does not stop setup.
   - Folders that do not exist yet are left to the program.
   - Uninstall changes nothing (ProgramData stays).
5. Not changed:
   - any other folder;
   - any Windows setting, account or group;
   - the patients' image folder;
   - the existing setup test that the config file gets no `Permissions:` entry and that there is no `[Dirs]` section. Setup still has
     neither; the list is set from `[Code]`.

## Notes

- A file moved into a private folder from elsewhere on the same disk keeps its own list until something resets it. The program only moves
  files between private folders (`Video\recording` to `Photos\unsaved`, quarantine to resolved), so this does not arise.
- A user could swap a folder for a junction between setup's check and its icacls call. That window is a fraction of a second during an
  administrator's install. Closing it fully would need setup to own the Imaging folder, which would end the Users Modify rule the program
  relies on.
- The program looks the group up by name and does not check that it is a group. A local user named "OpenPMS-Dental Users" would be used by
  the program, while setup's `net localgroup` would fall back to Users. This is left as an accepted edge case.
- The X-ray capture's other folders under `Capture` (journal and the rest) inherit from `Capture`. An install whose `Capture` the program
  cannot change gives each subfolder its own list, where it can.

## Tests

- `tests/ImagingDeviceManagerTests/ImagingFolderAclTests.cs` (14).
  - Which folders are private: the capture folder names, slashes and case. The group name is the same as setup's.
  - Group or Users.
  - Real access lists on folders in the test's own temporary folder, with this Windows user standing in for the group:
    - the list is protected and has only the three;
    - a file and folder already inside take it, and a folder made later inherits it;
    - this user can still write and delete;
    - an extra account, rights below Modify, or more than Modify for the group (it could change the list) are not private;
    - EnsureImagingFolder gives `Capture` the list, and the journal inherits it;
    - a junction (made with `mklink /J` in the temporary folder) is never followed, and the folder it points to keeps its list;
    - IsCurrentUserAllowed is true for Users and false for a group this user is not in.
  - With stand-ins:
    - the root first, then the subfolder inherits;
    - `Photos` and `Video` keep the Users rule;
    - a failure leaves the folder usable, with a warning, and a root's warning is kept;
    - a user the list would lock out never sets it, and is told what to do when it is set already;
    - the old test overload changes no list.
- `InstallerTests.Iss_LimitsTheFoldersOfUnfiledImages_ToSystemAdministratorsAndTheProgramsUsers` checks:
  - the group and the fallback SID;
  - both icacls commands, in order, and no `/T`;
  - the junction checks;
  - the three folders;
  - the call before the connection settings;
  - that setup never makes a group (`/add`).
- Setup's `[Code]` was compiled with ISCC, from a code-only copy, since a full compile needs a release build.
- LimitImagingFolder itself was compiled into a small non-elevated setup and run on temporary folders:
  - a normal tree was made private, and its file took the new list;
  - a tree with a junction inside was left unchanged;
  - a folder that is itself a junction was left unchanged;
  - neither junction's target changed.
- A missing group makes icacls fail, which is why `net localgroup` is asked first.
- No test touches ProgramData, a real group, or any folder outside its temporary folder.

Targeted x64 run:

| Suite | Passed | Total |
|---|---|---|
| ImagingDeviceManagerTests | 582 | 582 |
| ImagingCaptureTests | 846 | 846 |
| ReleaseFlowTests (InstallerTests) | 34 | 34 |
