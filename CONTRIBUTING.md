# New Contributor's Survival Guide

This document details the steps and general tips on how to contribute to the Refresh private server software! Feel free to skip sections if you believe you're already set.

## Requirements:
- Knowledge of the [.NET 7 SDK](https://learn.microsoft.com/dotnet/)
- Ability to learn [Realm](https://realm.io), an easy to pick up database software
- Basic knowledge of "reverse engineering," specifically reading packet captures and (rarely) reading the game executable for the LBP series.

## Software prerequisites
- [.NET 7 SDK](https://dotnet.microsoft.com)
- [Git](https://git-scm.com)
- An IDE that supports .NET; Visual Studio, Visual Studio Code, JetBrains Rider, it's up to you!
- Any legally acquired PS3 or PSV LBP game, excluding Karting.
- Access to [HEN](https://www.psx-place.com/threads/tutorial-ps3hen-the-great-ps3-hen-all-in-one-guide.24369/)/CFW (PS3), [Henkaku/Enso](https://vita.hacks.guide) (PSV), and/or [RPCS3](https://rpcs3.net) (PS3 on PC).

# Preparing your system
In order to use Git you must set up your configuration, this should ideally match your GitHub account's name and email. 

You can use [GitHub Desktop](https://desktop.github.com) or the IDE of your choice to help you with this! **Or if you're more technically inclined**, follow the CLI instructions below.

This will modify the **global** config, which will allow you to contribute to multiple projects with the same name and email with ease.

`$ git config --global user.name Your Name`

`$ git config --global user.email you@example.com`

# Preparing your new development environment
It's almost time to clone Refresh! Create a fork by pressing the "Fork" button at the base of this repository.

Afterwards, open a terminal to your working directory and clone the new fork using Git.

This is usually done with the following command:
` $ git@github.com:<YOUR_USERNAME>/Refresh.git`

Now, open the folder/workspace/solution with the IDE you chose. Explore the codebase, experiment, and have fun!

To run the server software in **DEBUG** mode, simply write:

`$ dotnet run --project Refresh.GameServer`

Or use your IDE to do this for you! 

Make sure to add [the upstream Refresh repository](https://github.com/LittleBigRefresh/Refresh) as the "**upstream**" remote using your IDE or GitHub Desktop. Happy hunting!

### Follow up in the patching documentation for connecting using your choice of device, TBW