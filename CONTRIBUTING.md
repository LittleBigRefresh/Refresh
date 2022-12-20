# New Contributor's Survival Guide

This document details the steps and general tips on how to contribute to the Refresh private server software! Feel free to skip sections if you believe you're already set.

## Requirements:
- Knowledge of the [.NET 7 SDK](https://learn.microsoft.com/dotnet/) and [Realm](https://realm.io) 
- Basic knowledge of "reverse engineering," specifically reading packet captures and (rarely) reading the game executable for the LBP series.

## Software prerequisites
- [.NET 7 SDK](https://dotnet.microsoft.com)
- [Git](https://git-scm.com)
- An IDE that supports .NET; Visual Studio, Visual Studio Code, JetBrains Rider, Eclipse, it's up to you!
- Any legally acquired PS3 or PSV LBP game, excluding Karting.
- Access to [HEN](https://www.psx-place.com/threads/tutorial-ps3hen-the-great-ps3-hen-all-in-one-guide.24369/)/CFW (PS3), [Henkaku/Enso](https://vita.hacks.guide) (PSV), and/or [RPCS3](https://rpcs3.net) (PS3 on PC).

# Preparing your system
In order to use Git you must set up your configuration, this should ideally match your GitHub account's name and email. This will modify the **global** config, which will allow you to contribute to multiple projects with the same name and email with ease.

`$ git config --global user.name Your Name`

`$ git config --global user.email you@example.com`

# Preparing your new development environment
It's almost time to clone Refresh! Create a fork by pressing the "Fork" button at the base of this repository.

Afterwards, Open a terminal to your working directory and clone the repository using Git.

`$ git clone https://github.com/LittleBigRefresh/Refresh`

`$ cd Refresh`

Restore the dependencies needed for development.

`$ dotnet restore`

Now, open the folder/workspace/solution with the IDE you chose. Explore the codebase, experiment, and have fun!

To run the server software in **DEBUG** mode, simply write:

`$ dotnet run --project Refresh.GameServer`

Or use your IDE to do this for you.

# Preparing your game for a connection to Refresh (PS3)
# Preparing your game (continued) (PC)
# Preparing your game (continued) (PSV)
TBW