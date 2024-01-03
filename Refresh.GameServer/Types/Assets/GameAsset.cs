using System.Security.Cryptography;
using Bunkum.Core.Storage;
using Realms;
using Refresh.GameServer.Authentication;
using Refresh.GameServer.Database;
using Refresh.GameServer.Importing;
using Refresh.GameServer.Resources;
using Refresh.GameServer.Types.UserData;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;
using SixLabors.ImageSharp.Processing;

namespace Refresh.GameServer.Types.Assets;

public partial class GameAsset : IRealmObject
{
    [PrimaryKey] public string AssetHash { get; set; } = string.Empty;
    public GameUser? OriginalUploader { get; set; }
    public DateTimeOffset UploadDate { get; set; }
    public bool IsPSP { get; set; }
    public int SizeInBytes { get; set; }
    [Ignored] public GameAssetType AssetType
    {
        get => (GameAssetType)this._AssetType;
        set => this._AssetType = (int)value;
    }

    // ReSharper disable once InconsistentNaming
    internal int _AssetType { get; set; }

    public IList<string> Dependencies { get; } = null!;

    [Ignored] public AssetSafetyLevel SafetyLevel => AssetSafetyLevelExtensions.FromAssetType(this.AssetType);

    public string? AsMainlineIconHash { get; set; }
    public string? AsMipIconHash { get; set; }
    
    /// <summary>
    /// Automatically crops and resizes an image into its corresponding icon form.
    /// </summary>
    /// <param name="image">The source image</param>
    /// <returns>The cropped and resized image, or null if its already fine</returns>
    private Image? CropToIcon(Image image)
    {
        const int maxWidth = 256;
        
        //If the image is already square, and already small enough for our uses, then we can just return it as-is
        if (image.Width == image.Height && image.Width <= maxWidth) return null;

        Image<Rgba32> copy = image.CloneAs<Rgba32>();

        //If the image is already square, just resize it.
        if (image.Width == image.Height)
        {
            copy.Mutate(ctx => ctx.Resize(maxWidth, maxWidth));

            return copy;
        }
        
        Rectangle cropRectangle;
        
        //If the image is wider than it is tall
        cropRectangle = image.Width > image.Height ? new Rectangle(image.Width / 2 - image.Height / 2, 0, image.Height, image.Height) :
            //If the image is taller than it is wide
            new Rectangle(0, image.Height / 2 - image.Width / 2, image.Width, image.Width);

        int targetWidth = Math.Min(maxWidth, cropRectangle.Width);
        
        copy.Mutate(ctx => ctx.Crop(cropRectangle).Resize(targetWidth, targetWidth));
        
        return copy;
    }
    
    public string GetAsIcon(TokenGame game, GameDatabaseContext database, IDataStore dataStore)
    {
        string dataStorePath = this.IsPSP ? $"psp/{this.AssetHash}" : this.AssetHash;
        
        switch (this.AssetType)
        {
            case GameAssetType.Tga:
            case GameAssetType.Jpeg:
            case GameAssetType.Png:
                switch (game)
                {
                    case TokenGame.Website:
                    case TokenGame.LittleBigPlanet1: {
                        //If the cached icon hash is already set, early return it.
                        if (this.AsMainlineIconHash != null) return this.AsMainlineIconHash;

                        //Load the image from the data store and crop it to an icon
                        Image? croppedIcon = this.CropToIcon(Image.Load(dataStore.GetStreamFromStore(dataStorePath)));
                        //If its null, then its already safe to use,
                        if (croppedIcon == null)
                        {
                            database.SetAsMainlineIconHash(this, this.AssetHash);
                            //Return the existing asset hash
                            return this.AsMainlineIconHash!;
                        }
                        
                        MemoryStream memory = new();
                        croppedIcon.SaveAsPng(memory);
                        byte[] data = memory.ToArray();
                        
                        //Get the hash of the converted asset
                        string convertedHash = AssetImporter.BytesToHexString(SHA1.HashData(data));

                        //Write the data to the store
                        dataStore.WriteToStore(convertedHash, data);
                        
                        database.SetAsMainlineIconHash(this, convertedHash);
                        
                        //Return the new icon hash
                        return this.AsMainlineIconHash!;
                    }
                    //On LBP2, LBP3, and LBPVita, it can automatically crop images, so theres no need to do anything here
                    case TokenGame.LittleBigPlanet2:
                    case TokenGame.LittleBigPlanet3:
                    case TokenGame.LittleBigPlanetVita:
                        return this.AssetHash;
                    case TokenGame.LittleBigPlanetPSP:
                        if (this.AsMipIconHash != null) return this.AsMipIconHash;
                        
#if false //TODO: trim to 1:1 aspect ratio and convert to MIP
                        this.AsMipIconHash = convertedHash;
                        return this.AsMipIconHash;
#endif
                        
                        throw new NotImplementedException();
                    default:
                        throw new ArgumentOutOfRangeException(nameof(game), game, null);
                }
            case GameAssetType.Texture:
                switch (game)
                {
                    case TokenGame.Website:
                    case TokenGame.LittleBigPlanet1:
                        //If the cached icon hash is already set, early return it.
                        if (this.AsMainlineIconHash != null) return this.AsMainlineIconHash;

                        Image? croppedIcon = this.CropToIcon(ImageImporter.LoadTex(dataStore.GetStreamFromStore(dataStorePath)));
                        if (croppedIcon == null)
                        {
                            database.SetAsMainlineIconHash(this, this.AssetHash);

                            return this.AsMainlineIconHash!;
                        }
                        
                        MemoryStream memory = new();
                        croppedIcon.SaveAsPng(memory);
                        byte[] data = memory.ToArray();
                        
                        //Get the hash of the converted asset
                        string convertedHash = AssetImporter.BytesToHexString(SHA1.HashData(data));

                        //Write the data to the store
                        dataStore.WriteToStore(convertedHash, data);
                        
                        database.SetAsMainlineIconHash(this, convertedHash);
                        
                        //Return the new icon hash
                        return this.AsMainlineIconHash!;
                    case TokenGame.LittleBigPlanet2:
                    case TokenGame.LittleBigPlanet3:
                    case TokenGame.LittleBigPlanetVita:
                        return this.AssetHash;
                    case TokenGame.LittleBigPlanetPSP:
                        if (this.AsMipIconHash != null) return this.AsMipIconHash;
                        
#if false //TODO: trim to 1:1 aspect ratio and convert to MIP
                        this.AsMipIconHash = convertedHash;
                        return this.AsMipIconHash;
#endif
                        
                        throw new NotImplementedException();
                    default:
                        throw new ArgumentOutOfRangeException(nameof(game), game, null);
                }
            case GameAssetType.GameDataTexture:
                switch (game)
                {
                    case TokenGame.Website:
                    case TokenGame.LittleBigPlanet1:
                        //If the cached icon hash is already set, early return it.
                        if (this.AsMainlineIconHash != null) return this.AsMainlineIconHash;

                        Image? croppedIcon = this.CropToIcon(ImageImporter.LoadGtf(dataStore.GetStreamFromStore(dataStorePath)));
                        if (croppedIcon == null)
                        {
                            database.SetAsMainlineIconHash(this, this.AssetHash);

                            return this.AsMainlineIconHash!;
                        }
                        
                        MemoryStream memory = new();
                        croppedIcon.SaveAsPng(memory);
                        byte[] data = memory.ToArray();
                        
                        //Get the hash of the converted asset
                        string convertedHash = AssetImporter.BytesToHexString(SHA1.HashData(data));

                        //Write the data to the store
                        dataStore.WriteToStore(convertedHash, data);
                        
                        database.SetAsMainlineIconHash(this, convertedHash);
                        
                        //Return the new icon hash
                        return this.AsMainlineIconHash!;
                    case TokenGame.LittleBigPlanet2:
                    case TokenGame.LittleBigPlanet3:
                    case TokenGame.LittleBigPlanetVita:
                        return this.AssetHash;
                    case TokenGame.LittleBigPlanetPSP:
                        if (this.AsMipIconHash != null) return this.AsMipIconHash;
                        
#if false //TODO: trim to 1:1 aspect ratio and convert to MIP
                        this.AsMipIconHash = convertedHash;
                        return this.AsMipIconHash;
#endif
                        
                        throw new NotImplementedException();
                    default:
                        throw new ArgumentOutOfRangeException(nameof(game), game, null);
                }
            case GameAssetType.Painting:
                return "0";
            case GameAssetType.Mip:
                switch (game)
                {
                    //LBP1, LBP2, LBP3, and LBP Vita are unable to handle MIP files.
                    //The Website technically can utilize them after import,
                    //but using PNGs for the site will cause less load on the server, so lets do that!
                    case TokenGame.Website:
                    case TokenGame.LittleBigPlanet1:
                    case TokenGame.LittleBigPlanet2:
                    case TokenGame.LittleBigPlanet3:
                    case TokenGame.LittleBigPlanetVita: {
                        //If the cached icon hash is already set, early return it.
                        if (this.AsMainlineIconHash != null) return this.AsMainlineIconHash;

                        byte[] rawData = dataStore.GetDataFromStore(dataStorePath);
                        byte[] sourceData = ResourceHelper.PspDecrypt(rawData, Importer.PSPKey.Value);

                        using MemoryStream sourceDataStream = new(sourceData);
                        
                        //Load the source mip file
                        Image source = ImageImporter.LoadMip(sourceDataStream);
                        
                        //Crop the icon, if no transformation was needed, just use the source image for the conversion.
                        Image croppedIcon = this.CropToIcon(source) ?? source;

                        //Save the loaded icon to memory
                        MemoryStream memory = new();
                        croppedIcon.SaveAsPng(memory);
                        byte[] data = memory.ToArray();

                        //Get the hash of the converted asset
                        string convertedHash = AssetImporter.BytesToHexString(SHA1.HashData(data));

                        //Write the data to the store
                        dataStore.WriteToStore(convertedHash, data);

                        //Set the converted hash
                        database.SetAsMainlineIconHash(this, convertedHash);

                        //Return the new icon hash
                        return this.AsMainlineIconHash!;
                    }
                    case TokenGame.LittleBigPlanetPSP:
                        if (this.AsMipIconHash != null) return this.AsMipIconHash;

#if false //TODO: trim to 1:1 aspect ratio and convert to MIP
                        this.AsMipIconHash = convertedHash;
                        return this.AsMipIconHash;
#endif

                        throw new NotImplementedException();
                    default:
                        throw new ArgumentOutOfRangeException(nameof(game), game, null);
                }
            case GameAssetType.Level:
            case GameAssetType.Plan:
            case GameAssetType.Material:
            case GameAssetType.Mesh:
            case GameAssetType.Palette:
            case GameAssetType.Script:
            case GameAssetType.MoveRecording:
            case GameAssetType.VoiceRecording:
            case GameAssetType.SyncedProfile:
            case GameAssetType.Unknown:
            default:
                throw new ArgumentOutOfRangeException();
        }
    }
}