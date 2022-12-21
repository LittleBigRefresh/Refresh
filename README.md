# Refresh

A work-in-progress second-generation custom server for LittleBigPlanet that focuses on code quality and reliability.

[Join the Discord!](https://discord.gg/xN5yKdxmWG)

<p align="center">
  <img width="600" src="https://github.com/LittleBigRefresh/Branding/blob/main/logos/refresh_type_transparent.png">
</p>

# Building and running

> **Warning**:
> Refresh is a heavy work-in-progress. Expect things to break, things to be deleted, and these instructions to change at any time.

## Windows

1. Install [Git for Windows](https://gitforwindows.org/)
2. Install [.NET 7 SDK](https://dotnet.microsoft.com/en-us/download/dotnet/7.0)
3. Open up a command prompt (win+R, type `cmd`, hit enter)
4. `$ git clone https://github.com/LittleBigRefresh/Refresh.git`
5. `$ cd Refresh`
6. `$ dotnet run -c Release --project Refresh.GameServer/`

## Linux/macOS
1. `$ git clone https://github.com/LittleBigRefresh/Refresh.git`
1. `$ cd Refresh`
1. `$ dotnet run -c Release --project Refresh.GameServer/`

## Contributing
To contribute to our project, it may be helpful to refer to our [contributing guide](CONTRIBUTING.md)!

*Made with :heart: for the LittleBigPlanet community*
