# Changelog
This is the release of the TextMesh Pro UPM package. The initial release of this package which is version v1.0.25 which is based on release 1.0.56.xx.0b2 of TextMesh Pro.

See the following link for the Release Notes for version 1.0.56.xx.0b2 of TextMesh Pro. http://digitalnativestudios.com/forum/index.php?topic=1363.0

## [1.0.25] - 2018-01-05
### Added
- Fixed a minor issue with PreferredValues calculation in conjunction with using text auto-sizing.
- Improved Kerning handling where it is now possible to define positional adjustments for the first and second glyph in the pair.
- Renamed Kerning Info Table to Glyph Adjustment Table to better reflect the added functionality of this table.
- Added Search toolbar to the Glyph Adjustment Table.
- Fixed incorrect detection / handling of Asset Serialization mode in the Project Conversion Utility.
- Removed SelectionBase attribute from TMP components.
- Revised TMP Shaders to support the new UNITY_UI_CLIP_RECT shader keyword which can provide a performance improvement of up to 30% on some devices.
- Added TMP_PRESENT define as per the request of several third party asset publishers.

## [1.0.23] - 2017-11-14
### Added
- New menu option added to Import Examples and additional content like Font Assets, Materials Presets, etc for TextMesh Pro. This new menu option is located in "Window -> TextMeshPro -> Import Examples and Extra Content".
- New menu option added to Convert existing project files and assets created with either the Source Code or DLL only version of TextMesh Pro. Please be sure to backup your project before using this option. The new menu option is located in "Window -> TextMeshPro -> Project Files GUID Remapping Tool".
- Added Assembly Definitions for the TMP Runtime and Editor scripts.
- Added support for the UI DirtyLayoutCallback, DirtyVerticesCallback and DirtyMaterialCallback.