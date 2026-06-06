using System.Net;
using Refresh.Database.Models.Assets;

namespace Refresh.Core.Types.Assets.Validation;

public struct ValidatedAssetResult
{
    /// <summary>
    /// HTTP code to return if validation failed.
    /// OK: don't cancel request and proceed.
    /// </summary>
    public HttpStatusCode Status { get; set; }

    /// <summary>
    /// new reference (hash/guid/blank) to use
    /// </summary>
    public string NewAssetRef { get; set; }

    /// <summary>
    /// message to show to the user.
    /// null: don't show anything.
    /// </summary>
    public string? ErrorMessage { get; set; }

    public GameAsset? AssetInfo { get; set; }
    public DisallowedAsset? DisallowanceInfo { get; set; }
    public bool ExistsInDataStore { get; set; }

    public ValidatedAssetResult(HttpStatusCode status, string? newAssetRef = null, string? errorMessage = null, Action<string>? onNewAssetRefCallback = null,
        GameAsset? assetInfo = null, DisallowedAsset? disallowanceInfo = null, bool existsInDataStore = false)
    {
        this.Status = status;
        this.NewAssetRef = newAssetRef ?? "0";
        this.ErrorMessage = errorMessage;
        this.AssetInfo = assetInfo;
        this.DisallowanceInfo = disallowanceInfo;
        this.ExistsInDataStore = existsInDataStore;

        onNewAssetRefCallback?.Invoke(this.NewAssetRef);
    }
}