# External Libs Folder

This folder shouldn't need to be touched in most cases.
It serves to allow developers to override the default behavior of referencing the following files
under ```%ProgramFiles(x86)%\Steam\SteamApps\common\Hacknet\```
* ```FNA.dll```
* ```"HacknetPathfinder.exe```
* ```"Pathfinder.dll"```

This might be required for a number of reasons, for example you obtained your copy of Hacknet from a different distribution platform.

## Usage

To Make use of this folder simply place a copy of each file listed above within this directory.
The presence of ```HacknetPathfinder.exe``` will instruct the system to make use of the files provided here instead.
