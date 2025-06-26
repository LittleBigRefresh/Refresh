# New Contributor's Survival Guide

This document details the steps and other important information on how to contribute to Refresh. Feel free to skip sections if you believe you're already set.

## Requirements:
- Basic knowledge of [C# and .NET 9](https://learn.microsoft.com/dotnet/)
- Ability to work with [Realm](https://realm.io), our database engine, when necessary. You can use [Realm Studio](https://github.com/realm/realm-studio/releases/) to inspect the database file.
- Basic knowledge of reverse engineering. In particular, reading packet captures and sometimes reading the game executable.

## Software prerequisites
- [.NET 9 SDK](https://dotnet.microsoft.com)
- [Git](https://git-scm.com)
- An IDE that supports .NET; Visual Studio, Visual Studio Code, JetBrains Rider, it's up to you!
- Any legally acquired PS3 or PSV LBP game, excluding Karting.
- Access to [HEN](https://www.psx-place.com/threads/tutorial-ps3hen-the-great-ps3-hen-all-in-one-guide.24369/)/CFW (PS3), [Henkaku/Enso](https://vita.hacks.guide) (PSV), and/or [RPCS3](https://rpcs3.net) (PS3 on PC).

# Preparing your new development environment
Create a fork by pressing the "Fork" button at the base of this repository.

Afterward, open a terminal to your working directory and clone the new fork using Git.

This is usually done with the following command:
`$ git@github.com:<YOUR_USERNAME>/Refresh.git`

Now, open the folder/workspace/solution with the IDE you chose. Explore the codebase, experiment, and have fun!

To run the server software in Debug mode correctly, use these commands:

- `$ mkdir -p bin/Debug/net9.0`
- `$ cd bin/Debug/net9.0`
- `$ dotnet run --project ../../../Refresh.GameServer.csproj`

To run in release mode, simply append `--configuration Release` to the `dotnet run` invocation.
You can also use your IDE to run the project for you.

Make sure to add [the upstream Refresh repository](https://github.com/LittleBigRefresh/Refresh) as the "**upstream**" remote using your IDE or GitHub Desktop. Happy hunting!

# Connecting locally
You can either use the website front-end called [refresh-web](https://github.com/LittleBigRefresh/refresh-web) to test the API, or you can connect using a game.

Here's how to identify the URL to patch to: https://docs.lbpbonsai.com/get-url.html

You can use these guides to find out how to patch your game:
- https://docs.lbpbonsai.com/ps3.html
- https://docs.lbpbonsai.com/rpcs3.html
- https://docs.lbpbonsai.com/psp.html
- https://docs.lbpbonsai.com/vita.html