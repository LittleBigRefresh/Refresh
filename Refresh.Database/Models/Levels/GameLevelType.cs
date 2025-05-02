namespace Refresh.GameServer.Types.Levels;

public enum GameLevelType : byte
{
    Normal = 0,
    Versus = 1,
    Cutscene = 2,
}

public static class GameLevelTypeExtensions
{
    public static GameLevelType FromGameString(string? str)
    {
        return str switch
        {
            "" => GameLevelType.Normal,
            "versus" => GameLevelType.Versus,
            "cutscene" => GameLevelType.Cutscene,
            _ => GameLevelType.Normal,
        };
    }
    
    public static string ToGameString(this GameLevelType type)
    {
        return type switch
        {
            GameLevelType.Normal => "",
            GameLevelType.Versus => "versus",
            GameLevelType.Cutscene => "cutscene",
            _ => "",
        };
    }
}