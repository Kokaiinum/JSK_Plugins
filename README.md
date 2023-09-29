# JSK_Plugins
Some BepInEx plugins for JSK's Unity games.
Well, to be more accurate, one plugin, one half-finished one, and one that is obsolete now. You're probably here for TextureReplacer.
It is recommended that you use these plugins with [ConfigurationManager](https://github.com/BepInEx/BepInEx.ConfigurationManager/releases/) and [MessageCenter](https://github.com/BepInEx/BepInEx.Utility/releases), but these are not strictly required.

## Installation
Download the latest release from the [releases page](https://github.com/Kokaiinum/JSK_Plugins/releases).
Put the relevant plugin .dll into BepInEx\plugins.

## TextureReplacer
TextureReplacer allows the replacement of textures at runtime from files on disk (currently accepting .png files), and hot reloading of said replacements, either individually or by selecting a new folder entirely.
It also allows runtime dumping of said textures.

### Usage
When you first run the game with the plugin present, the plugin will generate two folders in BepInEx\plugins, ReplacementTextures and DumpedTextures.

![FolderExample](https://github.com/Kokaiinum/JSK_Plugins/assets/42316813/cb5dfaf5-a819-48a4-b6ff-8bd0cda4ec3c)

Replacements can be loaded by creating a subfolder inside ReplacementTextures and putting your textures inside. Textures replace game textures if their names match. Currently only .png files are accepted for replacement.

![ReplacementFolderExample](https://github.com/Kokaiinum/JSK_Plugins/assets/42316813/484f8d06-6591-4dff-a284-75b10cd4ad9f)

Once you've done that, you can select the replacement folder you want to use either via ConfigManager or by manually editing the .cfg file in BepInEx\config.

![ReplacementSelectionExample](https://github.com/Kokaiinum/JSK_Plugins/assets/42316813/bdf2bfd9-0b6b-4810-8f60-ebc1c3fecf1a)

Pressing the dump hotkey will enable dumping. Dumping will dump a copy of all textures loaded by the game into the DumpedTextures folder. Textures are dumped as .png files, with their original name plus a "#" symbol and then some letters. 
The # and everything after it is needed because some games have textures with the same name, so the numbers are to make them distinct. 
The plugin ignores everything after the # when loading the file.

**Important:** the plugin dumps via an inbuilt Unity function that encodes the texture to .png. This means that some detail *may* be lost when exporting or re-importing, simply due to how image formats work.
Testers of the plugin haven't reported any such issues, but if you encounter any please report them in the Issues tab of this repo. If you find yourself wanting more accurate exports, please consider using one of the bundle extractor solutions like [AssetTools.NET](https://github.com/nesrak1/AssetsTools.NET) or [UABE](https://github.com/SeriousCache/UABE).

The plugin also has the option to watch the current replacement folder for changes, such as edits or new files being added, and reload without prompting.

![LiveReloadDemo2](https://github.com/Kokaiinum/JSK_Plugins/assets/42316813/d0cee22a-778e-4ae6-97c6-f57cfce20b0f)
