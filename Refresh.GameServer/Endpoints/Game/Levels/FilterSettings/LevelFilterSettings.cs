using Bunkum.Core;
using Refresh.GameServer.Authentication;
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
    /// The labels to filter by
    /// </summary>
    public string[]? Labels;
    /// <summary>
    /// The game of the request, eg. to prevent LBP3 levels from appearing on LBP2 or LBP1
    /// </summary>
    public TokenGame GameVersion;

    public LevelFilterSettings(TokenGame game)
    {
        this.GameVersion = game;
    }
    
    /// <summary>
    /// Gets the filter settings from a request
    /// </summary>
    public LevelFilterSettings(RequestContext context, TokenGame game) : this(game)
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
                _ => throw new ArgumentOutOfRangeException(),
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
                _ => throw new ArgumentOutOfRangeException(),
            };

        string? players = context.QueryString.Get("players");
        if(players != null)
            if (!byte.TryParse(players, out this.Players))
                this.Players = 0;

        string? labelFilter0 = context.QueryString.Get("labelFilter0");
        string? labelFilter1 = context.QueryString.Get("labelFilter1");
        string? labelFilter2 = context.QueryString.Get("labelFilter2");

        if (labelFilter2 != null)
        {
            this.Labels ??= new string[3];
            this.Labels[2] = labelFilter2;
        }
        if (labelFilter1 != null)
        {
            this.Labels ??= new string[2];
            this.Labels[1] = labelFilter1;
        }
        if (labelFilter0 != null)
        {
            this.Labels ??= new string[1];
            this.Labels[0] = labelFilter0;
        }
    }
}