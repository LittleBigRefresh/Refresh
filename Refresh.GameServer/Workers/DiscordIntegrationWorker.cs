using Bunkum.Core.Storage;
using Discord;
using Discord.Webhook;
using NotEnoughLogs;
using Refresh.GameServer.Authentication;
using Refresh.GameServer.Configuration;
using Refresh.GameServer.Database;
using Refresh.GameServer.Types.Activity;
using Refresh.GameServer.Types.Levels;
using Refresh.GameServer.Types.UserData;
using Refresh.GameServer.Types.UserData.Leaderboard;

namespace Refresh.GameServer.Workers;

public class DiscordIntegrationWorker : IWorker
{
    private readonly IntegrationConfig _config;
    private readonly string _externalUrl;
    private readonly DiscordWebhookClient _client;
    
    private bool _firstCycle = true;

    private long _lastTimestamp;
    private static long Now => DateTimeOffset.Now.ToUnixTimeMilliseconds();
    public int WorkInterval => this._config.DiscordWorkerFrequencySeconds * 1000; // 60 seconds by default

    public DiscordIntegrationWorker(IntegrationConfig config, GameServerConfig gameConfig)
    {
        this._config = config;
        this._externalUrl = gameConfig.WebExternalUrl;

        this._client = new DiscordWebhookClient(config.DiscordWebhookUrl);
    }

    private void DoFirstCycle()
    {
        this._firstCycle = false;
        this._lastTimestamp = Now;
    }

    private string GetAssetUrl(string hash)
    {
        return $"{this._externalUrl}/api/v3/assets/{hash}/image";
    }

    private Embed? GenerateEmbedFromEvent(Event @event, GameDatabaseContext database)
    {
        EmbedBuilder embed = new();

        GameLevel? level = @event.StoredDataType == EventDataType.Level ? database.GetLevelById(@event.StoredSequentialId!.Value) : null;
        GameUser? user = @event.StoredDataType == EventDataType.User ? database.GetUserByObjectId(@event.StoredObjectId) : null;
        GameSubmittedScore? score = @event.StoredDataType == EventDataType.Score ? database.GetScoreByObjectId(@event.StoredObjectId) : null;

        if (score != null) level = score.Level;

        string levelTitle = string.IsNullOrWhiteSpace(level?.Title) ? "Unnamed Level" : level.Title;

        string? levelLink = level == null ? null : $"[{levelTitle}]({this._externalUrl}/level/{level.LevelId})";
        string? userLink = user == null ? null : $"[{user.Username}]({this._externalUrl}/u/{user.UserId})";

        string? description = @event.EventType switch
        {
            EventType.Level_Upload => $"uploaded the level {levelLink}",
            EventType.Level_Favourite => $"gave {levelLink} a heart",
            EventType.Level_Unfavourite => null,
            EventType.User_Favourite => $"hearted {userLink}",
            EventType.User_Unfavourite => null,
            EventType.Level_Play => null,
            EventType.Level_Tag => null,
            EventType.Level_TeamPick => $"team picked {levelLink}",
            EventType.RateLevelRelation_Create => null,
            EventType.Level_Review => null,
            EventType.SubmittedScore_Create => $"got {score!.Score:N0} points on {levelLink}",
            EventType.User_FirstLogin => "logged in for the first time",
            _ => null,
        };

        if (description == null) return null;
        embed.WithDescription($"[{@event.User.Username}]({this._externalUrl}/u/{@event.User.UserId}) {description}");

        if (level != null)
        {
            embed.WithThumbnailUrl(this.GetAssetUrl(level.GameVersion == TokenGame.LittleBigPlanetPSP ? $"psp/{level.IconHash}" : level.IconHash));
        } else if (user != null)
        {
            embed.WithThumbnailUrl(this.GetAssetUrl(user.IconHash));
        }
        
        embed.WithTimestamp(DateTimeOffset.FromUnixTimeMilliseconds(@event.Timestamp));
        embed.WithAuthor(@event.User.Username, this.GetAssetUrl(@event.User.IconHash), $"{this._externalUrl}/u/{@event.UserId}");

        return embed.Build();
    }

    public void DoWork(Logger logger, IDataStore dataStore, GameDatabaseContext database)
    {
        if (this._firstCycle)
        {
            this.DoFirstCycle();
        }

        DatabaseList<Event> activity = database.GetGlobalRecentActivity(new ActivityQueryParameters
        {
            Timestamp = Now,
            EndTimestamp = this._lastTimestamp,
            Count = 5,
        });
        
        if (!activity.Items.Any()) return;

        this._lastTimestamp = activity.Items
            .Select(e => e.Timestamp)
            .Max() + 1;

        IEnumerable<Embed> embeds = activity.Items
            .Reverse() // events are descending
            .Select(e => this.GenerateEmbedFromEvent(e, database))
            .Where(e => e != null)
            .ToList()!;

        if (!embeds.Any()) return;
        
        ulong id = this._client.SendMessageAsync(embeds: embeds, 
            username: this._config.DiscordNickname, avatarUrl: this._config.DiscordAvatarUrl).Result;
        
        logger.LogInfo(RefreshContext.Worker, $"Posted webhook containing {activity.Items.Count()} events with id {id}");
    }
}