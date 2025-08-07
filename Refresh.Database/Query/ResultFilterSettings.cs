using Bunkum.Core;
using Refresh.Database.Models.Authentication;

namespace Refresh.Database.Query;

public class ResultFilterSettings
{
    // The result types to return
    public bool DisplayLevels { get; set; } = true;
    public bool DisplayPlaylists { get; set; } = true;
    public bool DisplayUsers { get; set; } = true;

    // The game versions of levels to return
    public bool DisplayLbp1 { get; set; } = true;
    public bool DisplayLbp2 { get; set; } = true;
    public bool DisplayLbp3 { get; set; } = true;
    public bool DisplayVita { get; set; } = true;
    public bool DisplayPSP { get; set; } = true;
    public bool DisplayBeta { get; set; } = true;

    /// <summary>
    /// The game of the request, eg. to prevent LBP3 levels from appearing on LBP2 or LBP1
    /// </summary>
    public TokenGame GameVersion { get; set; }

    /// <summary>
    /// Whether to include, exclude, or only show levels which require a move controller
    /// </summary>
    public PropertyFilterType DisplayMoveLevels { get; set; } = PropertyFilterType.Include;
    /// <summary>
    /// Whether to include, exclude, or only show adventure levels
    /// </summary>
    public PropertyFilterType DisplayAdventures { get; set; } = PropertyFilterType.Include;
    /// <summary>
    /// The amount of players to filter for
    /// eg. only show levels which are compatible with that many players
    /// </summary>
    public byte Players { get; set; } = 0;
    /// <summary>
    /// The labels a level should include. Games can specify up to 5 labels.
    /// TODO: Set and use this attribute once level labels are implemented.
    /// </summary>
    public string[] Labels { get; set; } = [];
    /// <summary>
    /// Whether or not the user's own levels should be excluded from the results
    /// </summary>
    public bool ExcludeMyLevels { get; set; } = false;
    /// <summary>
    /// Whether or not to include levels the requesting user has played before.
    /// TODO: Implement this filter
    /// </summary>
    public bool IncludePlayedLevels { get; set; } = true;
    /// <summary>
    /// Whether to force showing/hiding modded content.
    /// </summary>
    public bool? ShowModdedLevels { get; set; }
    /// <summary>
    /// Whether to force showing/hiding reuploaded content.
    /// </summary>
    public bool? ShowReuploadedLevels { get; set; }
    /// <summary>
    /// The seed used for lucky dip/random levels.
    /// </summary>
    /// <seealso cref="RandomLevelsCategory"/>
    public int? Seed { get; set; }

    public ResultFilterSettings(TokenGame game)
    {
        this.GameVersion = game;
    }

    private static ResultFilterSettings FromAnyRequest(RequestContext context, TokenGame game)
    {
        ResultFilterSettings settings = new(game);

        string? playersStr = context.QueryString.Get("players");
        if (playersStr != null && byte.TryParse(playersStr, out byte players))
        {
            settings.Players = players;
        }

        // Lucky Dip randomization
        string? seedStr = context.QueryString.Get("seed");
        if (seedStr != null && int.TryParse(seedStr, out int seed))
        {
            settings.Seed = seed;
        }

        string? excludeMyLevelsStr = context.QueryString.Get("excludeMyLevels");
        if (excludeMyLevelsStr != null && bool.TryParse(excludeMyLevelsStr, out bool excludeMyLevels))
        {
            settings.ExcludeMyLevels = excludeMyLevels;
        }

        string? includePlayedLevelsStr = context.QueryString.Get("includePlayedLevels");
        if (includePlayedLevelsStr != null && bool.TryParse(includePlayedLevelsStr, out bool includePlayedLevels))
        {
            settings.IncludePlayedLevels = includePlayedLevels;
        }

        return settings;
    }

    /// <summary>
    /// Gets the filter settings from a game request
    /// </summary>
    public static ResultFilterSettings FromGameRequest(RequestContext context, TokenGame game)
    {
        ResultFilterSettings settings = FromAnyRequest(context, game);

        bool gamesSpecified = false;
        string[]? gameFilters = context.QueryString.GetValues("gameFilter[]");
        string? gameFilterType = context.QueryString.Get("gameFilterType");

        if (game == TokenGame.BetaBuild)
        {
            // Don't allow beta builds to filter by game, always only show beta build levels regardless.
            // Also, LBP PSP and Vita never include game filter in their requests, so they don't need a special case here.
            settings.DisplayBeta = true;
        }
        else if (gameFilters != null)
        {
            // Filter used by LBP3 categories
            gamesSpecified = true;
            settings.DisplayLbp1 = false;
            settings.DisplayLbp2 = false;
            settings.DisplayLbp3 = false;

            foreach (string gameFilter in gameFilters)
            {
                switch (gameFilter)
                {
                    case "lbp1":
                        settings.DisplayLbp1 = true;
                        break;
                    case "lbp2":
                        settings.DisplayLbp2 = true;
                        break;
                    case "lbp3":
                        settings.DisplayLbp3 = true;
                        break;
                    default: 
                        throw new ArgumentOutOfRangeException(nameof(gameFilter), gameFilter, "Unsupported value");
                }
            }
        }
        else if (gameFilterType != null)
        {
            // Filter used by LBP2 and 3 outside of categories.
            // LBP3 uses a special query parameter in this case to also show LBP3 levels, but the user can't set filters outside of categories there,
            // so LBP3 will never request for LBP3 levels to be excluded in this case. GameVersion already takes care of filtering out incompatible
            // levels in LBP1 and 2, so it's safe to just leave DisplayLbp3 at true.
            switch (gameFilterType)
            {
                case "lbp1":
                    settings.DisplayLbp2 = false;
                    break;
                case "lbp2":
                    settings.DisplayLbp1 = false;
                    break;
                case "both":
                    // Do nothing, LBP1 and 2 are already set to show by default
                    break;
                default: 
                    throw new ArgumentOutOfRangeException(nameof(gameFilterType), gameFilterType, "Unsupported value");
            };
        }

        string[]? resultTypes = context.QueryString.GetValues("resultType[]");
        if (resultTypes != null)
        {
            settings.DisplayLevels = false;
            settings.DisplayPlaylists = false;
            settings.DisplayUsers = false;

            foreach (string resultType in resultTypes)
            {
                switch (resultType)
                {
                    case "slot":
                        settings.DisplayLevels = true;
                        break;
                    case "playlist":
                        settings.DisplayPlaylists = true;
                        break;
                    case "user":
                        settings.DisplayUsers = true;
                        break;
                }
            }
        }

        string? adventureStr = context.QueryString.Get("adventure");
        if (adventureStr != null)
        {
            settings.DisplayAdventures = adventureStr switch
            {
                "noneCan" => PropertyFilterType.Exclude,
                "dontCare" => PropertyFilterType.Include,
                // Sent when only adventures are allowed and no other result type at all (LBP3 will still include resultType[]=slot in this case)
                "allMust" => PropertyFilterType.Only,
                _ => throw new ArgumentOutOfRangeException(nameof(adventureStr), adventureStr, "Unsupported value"),
            };
        }

        string? moveStr = context.QueryString.Get("move");
        if (moveStr != null)
        {
            switch (moveStr)
            {
                // LBP2 options
                case "true":
                    settings.DisplayMoveLevels = PropertyFilterType.Include;
                    break;
                case "false":
                    settings.DisplayMoveLevels = PropertyFilterType.Exclude;
                    break;
                // Ignore case where only lbp1 and move levels are selected options,
                // because there are no lbp1 move levels
                case "only":
                    settings.DisplayMoveLevels = PropertyFilterType.Only;
                    break;

                // LBP3 options
                case "dontCare":
                    // atleast one game selected, but move unselected -> exclude move levels
                    if (gamesSpecified)
                        settings.DisplayMoveLevels = PropertyFilterType.Exclude;
                    // no games selected, move also unselected (default settings) -> show all levels
                    else
                        settings.DisplayMoveLevels = PropertyFilterType.Include;
                    break;
                case "allMust":
                    // atleast one game selected, move also selected -> show all levels of these games
                    if (gamesSpecified)
                        settings.DisplayMoveLevels = PropertyFilterType.Include;
                    // no games selected, but move selected -> only show move levels (of all games, can only be LBP2 and 3 anyway)
                    else
                        settings.DisplayMoveLevels = PropertyFilterType.Only;
                    break;

                default:
                    throw new ArgumentOutOfRangeException(nameof(moveStr), moveStr, "Unsupported value");
            }
        }

        return settings;
    }

    /// <summary>
    /// Gets the filter settings from an API request
    /// </summary>
    public static ResultFilterSettings FromApiRequest(RequestContext context)
    {
        ResultFilterSettings settings = FromAnyRequest(context, TokenGame.Website);

        string[]? gameFilters = context.QueryString.GetValues("game");
        if (gameFilters != null)
        {
            settings.DisplayLbp1 = false;
            settings.DisplayLbp2 = false;
            settings.DisplayLbp3 = false;
            settings.DisplayVita = false;
            settings.DisplayPSP = false;
            settings.DisplayBeta = false;

            foreach (string gameFilter in gameFilters)
                switch (gameFilter)
                {
                    case "lbp1":
                        settings.DisplayLbp1 = true;
                        break;
                    case "lbp2":
                        settings.DisplayLbp2 = true;
                        break;
                    case "lbp3":
                        settings.DisplayLbp3 = true;
                        break;
                    case "vita":
                        settings.DisplayVita = true;
                        break;
                    case "psp":
                        settings.DisplayPSP = true;
                        break;
                    case "beta":
                        settings.DisplayBeta = true;
                        break;
                }
        }

        string? moddedFilter = context.QueryString.Get("includeModded");
        if (moddedFilter != null)
        {
            if (!bool.TryParse(moddedFilter, out bool showModdedLevels))
                throw new FormatException("Could not parse modded filter setting because the boolean was invalid.");

            settings.ShowModdedLevels = showModdedLevels;
        }
        
        string? reuploadedFilter = context.QueryString.Get("includeReuploaded");
        if (reuploadedFilter != null)
        {
            if (!bool.TryParse(moddedFilter, out bool showReuploadedLevels))
                throw new FormatException("Could not parse reuploaded filter setting because the boolean was invalid.");

            settings.ShowModdedLevels = showReuploadedLevels;
        }

        return settings;
    }
}