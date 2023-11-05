using Bunkum.Core;
using Refresh.GameServer.Extensions;
using Refresh.GameServer.Services;

namespace Refresh.GameServer.Endpoints.Game.Levels.FilterSettings;

public class LevelFilterSettings
{
    /// <summary>
    /// The game(s) to filter the results to
    /// </summary>
    public GameFilterType GameFilterType = GameFilterType.Both;
    /// <summary>
    /// Whether or not to display Lbp1 levels in the results (corresponds to gameFilter[]=lbp1)
    /// </summary>
    public bool DisplayLbp1 = true;
    /// <summary>
    /// Whether or not to display Lbp2 levels in the results (corresponds to gameFilter[]=lbp2)
    /// </summary>
    public bool DisplayLbp2 = true;
    /// <summary>
    /// Whether or not to display Lbp3 levels in the results (corresponds to gameFilter[]=lbp3)
    /// </summary>
    public bool DisplayLbp3 = true;
    /// <summary>
    /// Whether or not the user's own levels should be excluded from the results
    /// </summary>
    public bool ExcludeMyLevels = false;
    /// <summary>
    /// The amount of players to filter for
    /// eg. only show levels which are compatible with that many players
    /// </summary>
    public byte Players = 0;
    /// <summary>
    /// The filter setting for move levels
    /// </summary>
    public MoveFilterType MoveFilterType = MoveFilterType.True;
    /// <summary>
    /// The first label filter
    /// </summary>
    public string? LabelFilter0 = null;
    /// <summary>
    /// The second label filter
    /// </summary>
    public string? LabelFilter1 = null;
    /// <summary>
    /// The third label filter
    /// </summary>
    public string? LabelFilter2 = null;

    public LevelFilterSettings()
    {
    }
    
    /// <summary>
    /// Gets the filter settings from a request
    /// </summary>
    /// <param name="context"></param>
    public LevelFilterSettings(RequestContext context)
    {
        string[]? gameFilters = context.QueryString.GetValues(context.IsApi() ? "gameFilter" : "gameFilter[]");
        if (gameFilters != null)
        {
            this.DisplayLbp1 = false;
            this.DisplayLbp2 = false;
            this.DisplayLbp3 = false;
            foreach (string gameFilter in gameFilters)
                switch (gameFilter)
                {
                    case "lbp1":
                        this.DisplayLbp1 = true;
                        break;
                    case "lbp2":
                        this.DisplayLbp2 = true;
                        break;
                    case "lbp3":
                        this.DisplayLbp3 = true;
                        break;
                }
        }
        
        string? gameFilterType = context.QueryString.Get("gameFilterType");
        if (gameFilterType != null)
            this.GameFilterType = gameFilterType switch
            {
                "lbp1" => GameFilterType.LittleBigPlanet1,
                "lbp2" => GameFilterType.LittleBigPlanet2,
                "both" => GameFilterType.Both,
                //NOTE: We dont throw an exception here because an unknown filter setting could very well just be an unhandled case,
                //      and the wrong filter should just be ignored to at least give the user some response
                _ => GameFilterType.Both,
            };

        string? excludeMyLevels = context.QueryString.Get("excludeMyLevels");
        if (excludeMyLevels != null)
            if (!bool.TryParse(excludeMyLevels, out this.ExcludeMyLevels))
                this.ExcludeMyLevels = false;
        
        string? move = context.QueryString.Get("move");
        if (move != null)
            this.MoveFilterType = move switch
            {
                "true" => MoveFilterType.True,
                "false" => MoveFilterType.False,
                "only" => MoveFilterType.Only,
                //NOTE: We dont throw an exception here because an unknown filter setting could very well just be an unhandled case,
                //      and the wrong filter should just be ignored to at least give the user some response
                _ => MoveFilterType.True,
            };

        string? players = context.QueryString.Get("players");
        if(players != null)
            if (!byte.TryParse(players, out this.Players))
                this.Players = 0;

        this.LabelFilter0 = context.QueryString.Get("labelFilter0");
        this.LabelFilter1 = context.QueryString.Get("labelFilter1");
        this.LabelFilter2 = context.QueryString.Get("labelFilter2");
    }
}