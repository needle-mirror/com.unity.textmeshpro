# Changelog
This is the release of the TextMesh Pro UPM package. This release is the equivalent of release 1.0.56.xx.0b3 of TextMesh Pro.

See the following link for the Release Notes for version 1.0.56.xx.0b3 of TextMesh Pro. http://digitalnativestudios.com/forum/index.php?topic=1363.0

## [1.2.1] - 2018-02-14
### Changes
- Package is now backwards compatible with Unity 2018.1.
- Renamed Assembly Definitions (.asmdef) to new UPM package conventions.
- Added DisplayName for TMP UPM package.
- Revised Editor and Playmode tests to ignore / skip over the tests if the required resources are not present in the project.
- Revised implementation of Font Asset Creator progress bar to use Unity's EditorGUI.ProgressBar instead of custom texture.
- Fixed an issue where using the material tag in conjunction with fallback font assets was not handled correctly.
- Fixed an issue where changing the fontStyle property in conjunction with using alternative typefaces / font weights would not correctly trigger a regeneration of the text object.

## [1.2.0] - 2018-01-23
### Changes
- Package version # increased to 1.2.0 which is the first release for Unity 2018.2.

## [1.1.0] - 2018-01-23
### Changes
- Package version # increased to 1.1.0 which is the first release for Unity 2018.1. 

## [1.0.27] - 2018-01-16
### Changes
- Fixed an issue where setting the TMP_InputField.text property to null would result in an error.
- Fixed issue with Raycast Target state not getting serialized properly when saving / reloading a scene.
- Changed reference to PrefabUtility.GetPrefabParent() to PrefabUtility.GetCorrespondingObjectFromSource() to reflect public API change in 2018.2
- Option to import package essential resources will only be presented to users when accessing a TMP component or the TMP Settings file via the project menu.

## [1.0.26] - 2018-01-10
### Added
- Removed Tizen player references in the TMP_InputField as the Tizen player is no longer supported as of Unity 2018.1.

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