namespace Refresh.GameServer.Authentication;

public enum TokenGame
{
    LittleBigPlanet1 = 0,
    LittleBigPlanet2 = 1,
    LittleBigPlanet3 = 2,
    LittleBigPlanetVita = 3,
    LittleBigPlanetPSP = 4,
    Website = 5,
}

public static class TokenGameExtensions
{
    public static int ToSerializedGame(this TokenGame game) 
        => game switch
        {
            TokenGame.LittleBigPlanet1 => 0,
            TokenGame.LittleBigPlanet2 => 1,
            TokenGame.LittleBigPlanet3 => 2,
            TokenGame.LittleBigPlanetVita => 1,
            TokenGame.LittleBigPlanetPSP => 0,
            TokenGame.Website => throw new InvalidOperationException("https://osuhow.com/"),
            _ => throw new ArgumentOutOfRangeException(),
        };
}