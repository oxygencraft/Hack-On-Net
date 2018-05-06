# Hack-On-Net
This is a fan-made MP mod for Hacknet. It is in development and a WIP. It is currently not in a playable state yet so this should not be used until it reaches a playable state. The beta will begin in about a month and should be playable but with a few bugs at the time.

# Installation
This is just preparation for the beta. Please do not use this yet, the releases tab is empty at this time.

Prerequisites:
```
Hacknet
Hacknet Pathfinder (https://github.com/Arkhist/Hacknet-Pathfinder/releases)
SQL Server (if hosting server)
```

Client:
```
Download the latest version of Hacknet Pathfinder
Download the latest version of Hack on Net (https://github.com/Arkhist/Hack-On-Net/releases)
Extract all of the Pathfinder binary files into the directory containing Hacknet.exe (goto game properties in Steam, goto the local tab, and click browse local files)
Run PathfinderPatcher.exe
Start HacknetPathfinder.exe and then close (or you can merely create a Mods folder in the game directory)
Drop Hack on Net DLL into Mods directory
Start HacknetPathfinder.exe
```

Server:
```
Download latest version of Hack on Net server (https://github.com/Arkhist/Hack-On-Net/releases)
Extract binaries to a folder
Create database hacklinks in your SQL server
Run HackLinks Server.exe with parameter -r (-r rebuilds the data in hacklinks database)
If something happens and you want to restore, run the server with -r again (WARNING: this will wipe all data in DB)
(Only if you're playing over the internet and outside of your home network) Portforward 27015 (or whatever port you're using) on your router. Go to https://portforward.com/router.htm if you're having trouble
```

Connecting to the server:
```
Change HNMP.conf in the Mods folder if the server is on another computer (The ip should be the private ip of the computer on the network if the computer is on the network otherwise if your computer is outside of your network, it should be their public ip)
Start HacknetPathfinder.exe and click Open Multiplayer Mod
Click continue to terminal and the default username and password is username: test password: *blank*
```
