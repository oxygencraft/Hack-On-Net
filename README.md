# Hacklinks
Hacklinks is an open source online game about hacking. 
(The name is still subject to change)

### **Note**:

This game is currently under development and a WIP. The game is currently not in a playable state yet so this should not be played until it reaches a playable state. A beta will be happening when it's playable enough.

# Why did this project become a game?
This project used to be a mod but after a bit of discussion, we decided to ditch the Hacknet mod idea entirely and call it a game instead. The reason for this is the mod doesn't even interact with Hacknet at all. All we're using Hacknet for is just for GUI. So it'd make sense to just call it a separate game from Hacknet and not a mod.

# Installation
This is just preparation for the beta. Please do not use this yet, the releases tab is empty at this time.

Prerequisites:
- A client (either GUI or CLI, which can be downloaded from the releases tab)
- Hacknet
- Hacknet Pathfinder (https://github.com/Arkhist/Hacknet-Pathfinder/releases/latest)
- (Hacknet related things only required if you're using Hacknet mod)
- A server (which can be downloaded from the releases tab)
- SQL Server Software (Only if you are hosting a server and you may use anything you want like MySQL)

### **The game is being separated from Hacknet so these instructions will be out of date when the changes occur.**
### **Please do not use these instructions**

## Client: 
- Download the latest version of Hacknet Pathfinder
- Download the latest version of Hack on Net (https://github.com/Arkhist/Hack-On-Net/releases/latest)
- Extract all of the Pathfinder binary files into the directory containing Hacknet.exe (goto game properties in Steam, goto the local tab, and click browse local files)
- Run PathfinderPatcher.exe
- Start HacknetPathfinder.exe and then close (or you can merely create a Mods folder in the game directory)
- Drop Hack on Net DLL into Mods directory
- Start HacknetPathfinder.exe

## Server:
- Download latest version of Hack on Net server (https://github.com/Arkhist/Hack-On-Net/releases/latest)
- Extract binaries to a folder
- Create database hacklinks in your SQL server
- Run HackLinks Server.exe with parameter -r (-r rebuilds the data in hacklinks database)
- If something happens and you want to restore, run the server with -r again (WARNING: this will wipe all data in DB)
- (Only if you're playing over the internet and outside of your home network) Portforward 27015 (or whatever port you're using) on your router. Go to https://portforward.com/router.htm if you're having trouble

## Connecting to the server:
- Change HNMP.conf in the Mods folder if the server is on another computer (The ip should be the private ip of the computer on the network if the computer is on the network otherwise if your computer is outside of your network, it should be their public ip)
- Start HacknetPathfinder.exe and click Open Multiplayer Mod
- Click continue to terminal and the default username and password is username: test password: *blank*
