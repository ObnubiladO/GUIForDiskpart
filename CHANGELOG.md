# Changelog

## v1.1.0-rc.1 — 2026-09-26

- Let partition tiles and their row grow so the partition table and type line remains visible.
- Added a saved View > Dark mode option for the main window and dialogs.
- Moved the application to .NET 10 and PowerShell 7.6 for the next supported release.
- Made the solution and build output portable, with an in-repository test project.
- Reduced drive discovery work by reading physical disk media types in one WMI query instead of opening a PowerShell runspace for each name fragment.
- Made drive-letter removal target the selected partition, clarified the dialog, and kept Escape bound only to Cancel.
- Fixed drive-letter detection for `A:` and rejected invalid letter characters.
- Made extend and shrink controls work in whole megabytes without recursive slider and text updates.
