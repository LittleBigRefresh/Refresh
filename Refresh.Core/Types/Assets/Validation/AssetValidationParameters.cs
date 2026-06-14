using Bunkum.Core.Storage;
using Refresh.Core.Importing;
using Refresh.Core.Services;
using Refresh.Core.Types.Data;
using Refresh.Database;
using Refresh.Database.Models.Authentication;
using Refresh.Database.Models.Users;

namespace Refresh.Core.Types.Assets.Validation;

public struct AssetValidationParameters
{
    /// <summary>
    /// The reference (hash/guid/blank) to validate
    /// </summary>
    public string AssetRef { get; set; } = "0";
    public GameUser? User { get; set; }
    public TokenGame GameToUseIn { get; set; }
    public TokenPlatform PlatformToUseIn { get; set; }
    public GameDatabaseContext Database { get; set; } = null!;
    public IDataStore DataStore { get; set; } = null!;
    public CacheService Cache { get; set; } = null!;
    public GuidCheckerService GuidChecker { get; set; } = null!;
    public AssetImporter AssetImporter { get; set; } = null!;
    public AipiService? Aipi { get; set; }

    public bool MayBeBlank { get; set; } = true;
    public bool MayBeGuid { get; set; } = true;
    public bool MayBeHash { get; set; } = true;
    public bool MustBeInDataStoreIfHash { get; set; } = true;
    public bool MustBeTexture { get; set; } = false;

    /// <summary>
    /// What the asset should be referred as in user-faced error messages and in logs, e.g. "planet asset" or "icon".
    /// If null, we will default to calling it "asset" or "image" depending on MustBeTexture.
    /// </summary>
    public string? AssetContextTypeStr { get; set; }

    /// <summary>
    /// Callback which is called with the new asset reference as parameter when constructing <see cref="ValidatedAssetResult"/>;
    /// useful to update asset references of entities during validation without requiring the caller to manually reassign them after validation;
    /// this way similar attributes like photo images or face icons can simply be iterated.
    /// If null, this will be skipped.
    /// </summary>
    public Action<string>? OnNewAssetRefCallback { get; set; }

    public AssetValidationParameters(string assetKey, DataContext dataContext, AssetImporter assetImporter, AipiService? aipi = null)
    {
        this.AssetRef = assetKey;
        this.User = dataContext.User;
        this.GameToUseIn = dataContext.Game;
        this.PlatformToUseIn = dataContext.Platform;
        this.Database = dataContext.Database;
        this.DataStore = dataContext.DataStore;
        this.Cache = dataContext.Cache;
        this.GuidChecker = dataContext.GuidChecker;
        this.AssetImporter = assetImporter;
        this.Aipi = aipi;
    }

    public AssetValidationParameters()
    {

    }
}