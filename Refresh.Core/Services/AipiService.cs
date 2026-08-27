using System.Diagnostics;
using System.Net.Http.Json;
using Bunkum.Core.Services;
using Bunkum.Core.Storage;
using JetBrains.Annotations;
using NotEnoughLogs;
using Refresh.Common;
using Refresh.Core.Configuration;
using Refresh.Core.Importing;
using Refresh.Core.Types.Data;
using Refresh.Database;
using Refresh.Database.Models.Assets;
using Refresh.Database.Models.Users;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Formats;
using SixLabors.ImageSharp.Processing;

namespace Refresh.Core.Services;

// Referenced from DO.
public class AipiService : EndpointService
{
    protected HttpClient _client { get; init; }
    private readonly IntegrationConfig _config;
    private readonly DiscordStaffService? _discord;

    private readonly ImageImporter _importer;
    
    [UsedImplicitly]
    public AipiService(Logger logger, IntegrationConfig config, ImportService import, DiscordStaffService discord) : base(logger)
    {
        this._discord = discord;
        this._config = config;

        this._client = new HttpClient
        {
            BaseAddress = new Uri(config.AipiBaseUrl),
        };

        this._importer = import.ImageImporter;
    }

    public override void Initialize()
    {
        if (!this._config.DiscordStaffWebhookEnabled)
        {
            this.Logger.LogWarning(RefreshContext.Aipi, 
                "The Discord staff webhook is not enabled, but AIPI is. This is probably behavior you don't want.");
        }
        this.TestConnectivityAsync().Wait();
    }

    private async Task TestConnectivityAsync()
    {
        try
        {
            HttpResponseMessage response = await this._client.GetAsync("/");
            string content = await response.Content.ReadAsStringAsync();

            if (response.IsSuccessStatusCode && content == "AIPI scanning service")
                this.Logger.LogInfo(RefreshContext.Aipi, "AIPI appears to be working correctly");
            else
                this.Logger.LogError(RefreshContext.Aipi,
                    $"AIPI seems to be down. Status code: {response.StatusCode}, content: {content}");
        }
        catch (Exception e)
        {
            this.Logger.LogError(RefreshContext.Aipi, "AIPI connection failed: {0}", e.ToString());
        }
    }
    
    private async Task<TData> PostAsync<TData>(string endpoint, Stream data)
    {
        HttpResponseMessage response = await this._client.PostAsync(endpoint, new StreamContent(data));
        AipiResponse<TData>? aipiResponse = await response.Content.ReadFromJsonAsync<AipiResponse<TData>>();
        
        if (aipiResponse == null) throw new Exception("No response was received from the server.");
        if (!aipiResponse.Success) throw new Exception($"{response.StatusCode}: {aipiResponse.Reason}");

        return aipiResponse.Data!;
    }
    
    private async Task<Dictionary<string, float>> PredictEvaAsync(Stream data)
    {
        Stopwatch stopwatch = new();
        this.Logger.LogTrace(RefreshContext.Aipi, "Pre-processing image data...");

        DecoderOptions options = new()
        {
            MaxFrames = 1,
            Configuration = SixLabors.ImageSharp.Configuration.Default,
        };

        Image image = await Image.LoadAsync(options, data);
        // Technically, we don't read videos in Refresh like in DO, but a couple of users are currently using APNGs as their avatar.
        // I don't want to break APNGs as they're harmless, so let's handle this by just reading the first frame for now.
        if (image.Frames.Count > 0)
            image = image.Frames.CloneFrame(0);
        
        image.Mutate(x => x.Resize(new ResizeOptions
        {
            Size = new Size(512),
            Mode = ResizeMode.Max,
        }));

        using MemoryStream processedData = new();
        await image.SaveAsPngAsync(processedData);
        // await image.SaveAsPngAsync($"/tmp/{DateTimeOffset.Now.ToUnixTimeMilliseconds()}.png");
        processedData.Seek(0, SeekOrigin.Begin);

        float threshold = this._config.AipiThreshold;
        
        this.Logger.LogDebug(RefreshContext.Aipi, $"Running prediction for image @ threshold={threshold}...");

        stopwatch.Start();
        Dictionary<string, float> prediction = await this.PostAsync<Dictionary<string, float>>($"/eva/predict?threshold={threshold}", processedData);    
        stopwatch.Stop();

        this.Logger.LogInfo(RefreshContext.Aipi, $"Got prediction result in {stopwatch.ElapsedMilliseconds}ms.");
        this.Logger.LogDebug(RefreshContext.Aipi, JsonConvert.SerializeObject(prediction));
        return prediction;
    }

    public bool ScanAndHandleAsset(DataContext context, GameAsset asset, GameUser user)
    {
        return this.ScanAndHandleAsset(context.Database, context.DataStore, asset, user);
    }
    
    // Use the passed user instead of the asset's OriginalUploader because the user trying to use this asset
    // is not necessarily also its uploader. If we really want to, we should auto-punish the user instead of the uploader,
    // and since OriginalUploader can be null here, punishing the uploader will not always work anyway.
    // Also, we should expect asset upload endpoints to pass the uploader as parameter anyway.
    public bool ScanAndHandleAsset(GameDatabaseContext database, IDataStore dataStore, GameAsset asset, GameUser user)
    {
        // import the asset as png
        bool isPspAsset = asset.AssetHash.StartsWith("psp/");

        if (!dataStore.ExistsInStore("png/" + asset.AssetHash))
        {
            this._importer.ImportAsset(asset.AssetHash, isPspAsset, asset.AssetType, dataStore);
        }

        // do actual prediction
        using Stream stream = dataStore.GetStreamFromStore("png/" + asset.AssetHash);
        Dictionary<string, float> results = this.PredictEvaAsync(stream).Result;

        if (!results.Any(r => this._config.AipiBannedTags.Contains(r.Key)))
            return false;
        
        this._discord?.PostPredictionResult(results, asset, user);
        // TODO also log this in our own mod log

        if (this._config.AipiRestrictAccountOnDetection)
        {
            this.Logger.LogInfo(RefreshContext.Aipi, $"Auto-restricting {user} because their image '{asset.AssetHash}' was determined to contain disallowed content.");
            const string reason = "Automatic restriction for posting or using disallowed content. This will usually be undone within 24 hours if this is a mistake.";
            database.RestrictUser(user, reason, DateTimeOffset.MaxValue);
        }
        
        return true;
    }
}
    
public class AipiResponse<TData>
{
    public bool Success { get; set; }

    public TData? Data { get; set; }
    public string? Reason { get; set; }
}