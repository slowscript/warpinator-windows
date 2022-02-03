# Warpinator for Windows (unofficial)

This is an unofficial reimplementation of Linux Mint's file sharing tool [Warpinator](https://github.com/linuxmint/warpinator) for Windows 7-10.

## Download
Now available on the [Releases](https://github.com/slowscript/warpinator-windows/releases) page

## Building
Requires .NET SDK 4.6.2  

Simply build the project with Visual Studio

### Screenshot
![screenshot](screenshot.png)
## Translating
You will need a recent version of Visual Studio
1) Create a new Resource file in the Resources folder called Strings.xx.resx where xx is code of the language you are translating to
2) Copy the entire table from Strings.resx and translate the values. Comments are only for context
3) Open each of Controls\TransferPanel, Form1, SettingsForm and TransferFrom in designer
4) Select the toplevel element (whole window) and under Properties switch Language to your language
5) Select controls with text on them (buttons, labels, menus) and translate their "Text" property
6) You can also move and resize the controls to fit the new strings and it will only affect the currently selected language