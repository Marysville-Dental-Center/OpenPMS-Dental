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
   - When the list cannot be set, the folder keeps its old access and stays usable. This happens when the folder belongs to another
     Windows account and the program is not elevated. The call returns a warning, which the X-ray quarantine writer logs, and capture is
     never refused for it.
   - Unlike the Users rule, a folder that was just made is not removed when the list fails.
4. **Setup** (`installer/OpenPmsDental.iss`, LimitImagingFolders, at ssPostInstall before the connection settings), as administrator,
   changes only the three folders that exist:
   - `icacls <folder> /reset /T /C /Q`, so everything inside inherits again;
   - then `icacls <folder> /inheritance:r /grant:r *S-1-5-18:(OI)(CI)F *S-1-5-32-544:(OI)(CI)F <who>:(OI)(CI)M /Q`.
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
  files between private folders (`Video\recording` to `Photos\unsaved`, quarantine to resolved), so this does not arise. Setup's `/reset /T`
  resets every file inside once.
- The X-ray capture's other folders under `Capture` (journal and the rest) inherit from `Capture`. An install whose `Capture` the program
  cannot change gives each subfolder its own list, where it can.

## Tests

- `tests/ImagingDeviceManagerTests/ImagingFolderAclTests.cs` (9).
  - Which folders are private: the capture folder names, slashes and case. The group name is the same as setup's.
  - Group or Users.
  - Real access lists on folders in the test's own temporary folder, with this Windows user standing in for the group:
    - the list is protected and has only the three;
    - a file and folder already inside take it, and a folder made later inherits it;
    - this user can still write and delete;
    - an extra account, or rights below Modify, are not private;
    - EnsureImagingFolder gives `Capture` the list, and the journal inherits it.
  - With stand-ins:
    - the root first, then the subfolder inherits;
    - `Photos` and `Video` keep the Users rule;
    - a failure leaves the folder usable, with a warning;
    - the old test overload changes no list.
- `InstallerTests.Iss_LimitsTheFoldersOfUnfiledImages_ToSystemAdministratorsAndTheProgramsUsers`: the group, the fallback SID, both icacls
  commands, the three folders, the call before the connection settings, and that setup never makes a group (`/add`).
- Setup's `[Code]` was compiled with ISCC (a code-only copy, since a full compile needs a release build). Both icacls commands were run on a
  temporary folder: a file inside took the new list, and a missing group fails, which is why `net localgroup` is asked first.
- No test touches ProgramData, a real group, or any folder outside its temporary folder.

Targeted x64 run:

| Suite | Passed | Total |
|---|---|---|
| ImagingDeviceManagerTests | 577 | 577 |
| ImagingCaptureTests | 846 | 846 |
| ReleaseFlowTests (InstallerTests) | 34 | 34 |
