using Refresh.GameServer.Types.Levels;

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
    
    public static bool CanPlay(this TokenGame game, GameLevel level)
    {
        switch (game)
        {
            //LBP1 can only play LBP1 levels.
            case TokenGame.LittleBigPlanet1:
                if (level.GameVersion != TokenGame.LittleBigPlanet1) 
                    return false;
                            
                break;
            //LBP2 can play LBP1,2
            case TokenGame.LittleBigPlanet2:
                if (level.GameVersion != TokenGame.LittleBigPlanet1 && level.GameVersion != TokenGame.LittleBigPlanet2) 
                    return false;
                            
                break;
            //LBP3 can play LBP1,2,3
            case TokenGame.LittleBigPlanet3:
                if (level.GameVersion != TokenGame.LittleBigPlanet1 && level.GameVersion != TokenGame.LittleBigPlanet2 && level.GameVersion != TokenGame.LittleBigPlanet3)
                    return false;
                            
                break;
            //LBPV can only play LBPV levels.
            case TokenGame.LittleBigPlanetVita:
                if (level.GameVersion != TokenGame.LittleBigPlanetVita)
                    return false;
                            
                break;
            //LBP PSP can only play LBP PSP levels.
            case TokenGame.LittleBigPlanetPSP:
                if (level.GameVersion != TokenGame.LittleBigPlanetPSP)
                    return false;
                            
                break;
            //Allow all for website
            case TokenGame.Website:
                break;
            default:
                throw new ArgumentOutOfRangeException();
        }
        
        return true;
    }
}