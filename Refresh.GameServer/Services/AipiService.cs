using System.Diagnostics;
using System.Net.Http.Json;
using Bunkum.Core.Services;
using JetBrains.Annotations;
using NotEnoughLogs;
using Refresh.GameServer.Configuration;
using Refresh.GameServer.Importing;
using Refresh.GameServer.Types.Assets;
using Refresh.GameServer.Types.Data;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Formats;
using SixLabors.ImageSharp.Processing;

namespace Refresh.GameServer.Services;

// Referenced from DO.
public class AipiService : EndpointService
{
    private readonly HttpClient _client;
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

    public bool ScanAndHandleAsset(DataContext context, GameAsset asset)
    {
        // guard the fact that assets have an owner
        Debug.Assert(asset.OriginalUploader != null, $"Asset {asset.AssetHash} had no original uploader when trying to scan");
        if (asset.OriginalUploader == null)
            return false;

        // import the asset as png
        bool isPspAsset = asset.AssetHash.StartsWith("psp/");

        if (!context.DataStore.ExistsInStore("png/" + asset.AssetHash))
        {
            this._importer.ImportAsset(asset.AssetHash, isPspAsset, asset.AssetType, context.DataStore);
        }

        // do actual prediction
        using Stream stream = context.DataStore.GetStreamFromStore("png/" + asset.AssetHash);
        Dictionary<string, float> results = this.PredictEvaAsync(stream).Result;

        if (!results.Any(r => this._config.AipiBannedTags.Contains(r.Key)))
            return false;
        
        this._discord?.PostPredictionResult(results, asset);

        if (this._config.AipiRestrictAccountOnDetection)
        {
            const string reason = "Automatic restriction for posting disallowed content. This will usually be undone within 24 hours if this is a mistake.";
            context.Database.RestrictUser(asset.OriginalUploader, reason, DateTimeOffset.MaxValue);
        }
        
        return true;
    }
    
    private class AipiResponse<TData>
    {
        public bool Success { get; set; }
    
        public TData? Data { get; set; }
        public string? Reason { get; set; }
    }
}