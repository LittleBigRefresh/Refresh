using System.Security.Cryptography;
using Bunkum.Core.Storage;
using Refresh.Common.Helpers;
using Refresh.GameServer.Authentication;
using Refresh.GameServer.Database;
using Refresh.GameServer.Importing;
using Refresh.GameServer.Importing.Mip;
using Refresh.GameServer.Types.Assets;
using Refresh.GameServer.Types.Data;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;
using SixLabors.ImageSharp.Processing;

namespace Refresh.GameServer.Extensions;

public static class GameAssetExtensions
{
    public static void TraverseDependenciesRecursively(this GameAsset asset, GameDatabaseContext database, Action<string, GameAsset?> callback)
    {
        callback(asset.AssetHash, asset);
        foreach (string internalAssetHash in database.GetAssetDependencies(asset))
        {
            GameAsset? internalAsset = database.GetAssetFromHash(internalAssetHash);
            
            // Only run this if this is null, since the next recursion will trigger its own callback
            if(internalAsset == null)
                callback(internalAssetHash, internalAsset);

            internalAsset?.TraverseDependenciesRecursively(database, callback);
        }
    }
    
    /// <summary>
    /// Automatically crops and resizes an image into its corresponding icon form.
    /// </summary>
    /// <param name="image">The source image</param>
    /// <returns>The cropped and resized image, or null if its already fine</returns>
    private static Image<Rgba32>? CropToIcon(Image image)
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

        int targetWidth = Math.Clamp(cropRectangle.Width, 16, maxWidth);

        //Round to the nearest multiple of 16, this is to make PSP happy
        targetWidth =
            (int)Math.Round(
                targetWidth / (double)16,
                MidpointRounding.AwayFromZero
            ) * 16;

        copy.Mutate(ctx => ctx.Crop(cropRectangle).Resize(targetWidth, targetWidth));

        return copy;
    }
    /// <summary>
    /// Transforms an image to be consumed by a particular game
    /// </summary>
    /// <param name="game">The game which will consume the resulting asset</param>
    /// <param name="dataStore">The data store</param>
    /// <param name="decodeImage">The function used to decode an image from the data store</param>
    /// <param name="transformImage">The transformation function</param>
    /// <returns>The hash of the transformed asset</returns>
    /// <exception cref="NotImplementedException">That conversion step is unimplemented at the moment</exception>
    /// <exception cref="ArgumentOutOfRangeException">Invalid TokenGame</exception>
    private static string TransformImage(this GameAsset asset, TokenGame game, IDataStore dataStore, Func<string, Image<Rgba32>> decodeImage, Func<Image, Image<Rgba32>?> transformImage)
    {
        string dataStorePath = asset.IsPSP ? $"psp/{asset.AssetHash}" : asset.AssetHash;

        bool mainlineDoesntNeedConversion = asset.AssetType is GameAssetType.Png or GameAssetType.Texture or GameAssetType.GameDataTexture;

        switch (game)
        {
            case TokenGame.Website:
            case TokenGame.LittleBigPlanet1:
            case TokenGame.LittleBigPlanet2:
            case TokenGame.LittleBigPlanet3:
            case TokenGame.LittleBigPlanetVita: {
                Image sourceImage = decodeImage(dataStorePath);

                //Load the image from the data store and transform it
                Image? image = transformImage(sourceImage);
                //If its null, then no transformation was needed
                if (image == null)
                {
                    if (mainlineDoesntNeedConversion)
                    {
                        //Return the existing asset hash
                        return asset.AssetHash;
                    }

                    //Set the image to use to the source image
                    image = sourceImage;
                }

                //Save the image as a PNG file in a byte array in memory
                MemoryStream ms = new();
                image.SaveAsPng(ms);
                byte[] data = ms.ToArray();

                //Get the hash of the converted asset
                string convertedHash = HexHelper.BytesToHexString(SHA1.HashData(data));

                //Write the data to the store
                dataStore.WriteToStore(convertedHash, data);

                //Return the new icon hash
                return convertedHash;
            }
            case TokenGame.LittleBigPlanetPSP: {
                Image<Rgba32> sourceImage = decodeImage(dataStorePath);

                //Transform the image, if no transformation is needed, use the source image
                Image<Rgba32> image = transformImage(sourceImage) ?? sourceImage;

                MemoryStream ms = new();
                new MipEncoder().Encode(image, ms);
                //Get the used chunk of the underlying buffer
                Span<byte> data = ms.GetBuffer().AsSpan()[..((int)ms.Length)];
                //Encrypt the data
                byte[] encryptedData = ResourceHelper.PspEncrypt(data, Importer.PSPKey.Value);

                //Get the hash
                string convertedHash = HexHelper.BytesToHexString(SHA1.HashData(encryptedData));

                dataStore.WriteToStore($"psp/{convertedHash}", encryptedData);

                return convertedHash;
            }
            default:
                throw new ArgumentOutOfRangeException(nameof(game), game, null);
        }
    }
    
    /// <summary>
    /// Converts the asset into a suitable format to be used as a photo in the target game.
    /// </summary>
    /// <param name="game">The game to convert for</param>
    /// <param name="database">The database</param>
    /// <param name="dataStore">The data store</param>
    /// <returns>The new hash of the converted asset, or null if no conversion has taken place</returns>
    public static string? GetAsPhoto(this GameAsset asset, TokenGame game, DataContext dataContext)
    {
        return asset.GetAsGeneric(
            game,
            dataContext.Database,
            dataContext.DataStore,
            _ => null,
            () => asset.AsMainlinePhotoHash,
            hash => dataContext.Database.SetMainlinePhotoHash(asset, hash),
            () => throw new NotSupportedException(),
            _ => throw new NotSupportedException()
        );
    }
    
    /// <summary>
    /// Converts the asset into a suitable format to be used as an icon in the target game.
    /// </summary>
    /// <param name="game">The game to convert for</param>
    /// <param name="database">The database</param>
    /// <param name="dataStore">The data store</param>
    /// <returns>The new hash of the converted asset, or null if no conversion has taken place</returns>
    public static string? GetAsIcon(this GameAsset asset, TokenGame game, DataContext dataContext)
    {
        return asset.GetAsGeneric(
            game,
            dataContext.Database,
            dataContext.DataStore,
            CropToIcon,
            () => asset.AsMainlineIconHash,
            hash => dataContext.Database.SetMainlineIconHash(asset, hash),
            () => asset.AsMipIconHash,
            hash => dataContext.Database.SetMipIconHash(asset, hash)
        );
    }
    
    /// <summary>
    /// Converts the asset to the correct type for the specified game, using the provided transformation methods.
    ///
    /// Caches the converted mainline and MIP hashes using get/set functions passed in.
    /// </summary>
    /// <param name="game">The game to convert for</param>
    /// <param name="database">The database</param>
    /// <param name="dataStore">The data store</param>
    /// <param name="transformImage">The transform function for the image, returning null if no transformation needs to take place</param>
    /// <param name="getMainline">The method to get the cached mainline hash, or null if uncached</param>
    /// <param name="setMainline">The method to set the cached mainline hash</param>
    /// <param name="getMip">The method to get the cached MIP hash, or null if uncached</param>
    /// <param name="setMip">The method to set the cached MIP hash</param>
    /// <returns>The converted hash, or null if no conversion has taken place</returns>
    private static string? GetAsGeneric(this GameAsset asset, TokenGame game, GameDatabaseContext database, IDataStore dataStore, Func<Image, Image<Rgba32>?> transformImage, Func<string?> getMainline, Action<string> setMainline,
        Func<string?> getMip, Action<string> setMip)
    {
        try
        {
            switch (asset.AssetType)
            {
                case GameAssetType.Tga:
                case GameAssetType.Jpeg:
                case GameAssetType.Png:
                    switch (game)
                    {
                        case TokenGame.Website:
                        case TokenGame.LittleBigPlanet1:
                        case TokenGame.LittleBigPlanet2:
                        case TokenGame.LittleBigPlanet3:
                        case TokenGame.LittleBigPlanetVita: {
                            //If the cached icon hash is already set, early return it.
                            if (getMainline() != null) return getMainline()!;

                            string convertedHash = asset.TransformImage(game, dataStore, path => Image.Load<Rgba32>(dataStore.GetStreamFromStore(path)), transformImage);

                            setMainline(convertedHash);

                            //Return the new icon hash
                            return getMainline()!;
                        }
                        case TokenGame.LittleBigPlanetPSP: {
                            if (getMip() != null) return getMip()!;

                            string convertedHash = asset.TransformImage(game, dataStore, path => Image.Load<Rgba32>(dataStore.GetStreamFromStore(path)), transformImage);

                            setMip(convertedHash);

                            return getMip()!;
                        }
                        default:
                            throw new ArgumentOutOfRangeException(nameof(game), game, null);
                    }
                case GameAssetType.Texture:
                    switch (game)
                    {
                        case TokenGame.Website:
                        case TokenGame.LittleBigPlanet1:
                        case TokenGame.LittleBigPlanet2:
                        case TokenGame.LittleBigPlanet3:
                        case TokenGame.LittleBigPlanetVita: {
                            //If the cached icon hash is already set, early return it.
                            if (getMainline() != null) return getMainline()!;

                            string convertedHash = asset.TransformImage(game, dataStore, path => ImageImporter.LoadTex(dataStore.GetStreamFromStore(path)), transformImage);

                            setMainline(convertedHash);

                            //Return the new icon hash
                            return getMainline()!;
                        }
                        case TokenGame.LittleBigPlanetPSP: {
                            if (getMip() != null) return getMip()!;

                            string convertedHash = asset.TransformImage(game, dataStore, path => ImageImporter.LoadTex(dataStore.GetStreamFromStore(path)), transformImage);

                            setMip(convertedHash);

                            return getMip()!;
                        }
                            ;
                        default:
                            throw new ArgumentOutOfRangeException(nameof(game), game, null);
                    }
                case GameAssetType.GameDataTexture:
                    switch (game)
                    {
                        case TokenGame.Website:
                        case TokenGame.LittleBigPlanet1:
                        case TokenGame.LittleBigPlanet2:
                        case TokenGame.LittleBigPlanet3:
                        case TokenGame.LittleBigPlanetVita: {
                            //If the cached icon hash is already set, early return it.
                            if (getMainline() != null) return getMainline()!;

                            string convertedHash = asset.TransformImage(game, dataStore, path => ImageImporter.LoadGtf(dataStore.GetStreamFromStore(path)), transformImage);

                            setMainline(convertedHash);

                            //Return the new icon hash
                            return getMainline()!;
                        }
                        case TokenGame.LittleBigPlanetPSP: {
                            if (getMip() != null) return getMip()!;

                            string convertedHash = asset.TransformImage(game, dataStore, path => ImageImporter.LoadGtf(dataStore.GetStreamFromStore(path)), transformImage);

                            setMip(convertedHash);

                            return getMip()!;
                        }
                            ;
                        default:
                            throw new ArgumentOutOfRangeException(nameof(game), game, null);
                    }
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
                            if (getMainline() != null) return getMainline()!;

                            string convertedHash = asset.TransformImage(game, dataStore, path =>
                            {
                                //Load the data from the data store
                                byte[] rawData = dataStore.GetDataFromStore(path);
                                //Decrypt it
                                byte[] sourceData = ResourceHelper.PspDecrypt(rawData, Importer.PSPKey.Value);

                                //Create a memory stream from the decrypted asset data
                                using MemoryStream sourceDataStream = new(sourceData);

                                //Load the mip file
                                return ImageImporter.LoadMip(sourceDataStream);
                            }, transformImage);

                            setMainline(convertedHash);

                            //Return the new icon hash
                            return getMainline()!;
                        }
                        case TokenGame.LittleBigPlanetPSP: {
                            //If the cached icon hash is already set, early return it.
                            if (getMip() != null) return getMip()!;

                            string convertedHash = asset.TransformImage(game, dataStore, path =>
                            {
                                //Load the data from the data store
                                byte[] rawData = dataStore.GetDataFromStore(path);
                                //Decrypt it
                                byte[] sourceData = ResourceHelper.PspDecrypt(rawData, Importer.PSPKey.Value);

                                //Create a memory stream from the decrypted asset data
                                using MemoryStream sourceDataStream = new(sourceData);

                                //Load the mip file
                                return ImageImporter.LoadMip(sourceDataStream);
                            }, transformImage);

                            setMip(convertedHash);

                            return getMip()!;
                        }
                        default:
                            throw new ArgumentOutOfRangeException(nameof(game), game, null);
                    }
                case GameAssetType.Level:
                case GameAssetType.Painting:
                case GameAssetType.Plan:
                case GameAssetType.GfxMaterial:
                case GameAssetType.Material:
                case GameAssetType.Mesh:
                case GameAssetType.Palette:
                case GameAssetType.Script:
                case GameAssetType.ThingRecording:
                case GameAssetType.VoiceRecording:
                case GameAssetType.SyncedProfile:
                case GameAssetType.GriefSongState:
                case GameAssetType.SoftPhysicsSettings:
                case GameAssetType.Bevel:
                case GameAssetType.StreamingLevelChunk:
                case GameAssetType.Animation:
                case GameAssetType.Unknown:
                case GameAssetType.GuidSubstitution:
                case GameAssetType.SettingsCharacter:
                case GameAssetType.Fontface:
                case GameAssetType.DownloadableContent:
                case GameAssetType.Joint:
                case GameAssetType.GameConstants:
                case GameAssetType.PoppetSettings:
                case GameAssetType.CachedLevelData:
                case GameAssetType.Game:
                case GameAssetType.SettingsNetwork:
                case GameAssetType.Packs:
                case GameAssetType.BigProfile:
                case GameAssetType.SlotList:
                case GameAssetType.AdventureCreateProfile:
                case GameAssetType.LocalProfile:
                case GameAssetType.LimitsSettings:
                case GameAssetType.Tutorials:
                case GameAssetType.GuidList:
                case GameAssetType.AudioMaterials:
                case GameAssetType.SettingsFluid:
                case GameAssetType.TextureList:
                case GameAssetType.MusicSettings:
                case GameAssetType.MixerSettings:
                case GameAssetType.ReplayConfig:
                case GameAssetType.StaticMesh:
                case GameAssetType.AnimatedTexture:
                case GameAssetType.Pins:
                case GameAssetType.Instrument:
                case GameAssetType.OutfitList:
                case GameAssetType.PaintBrush:
                case GameAssetType.Quest:
                case GameAssetType.AnimationBank:
                case GameAssetType.AnimationSet:
                case GameAssetType.SkeletonMap:
                case GameAssetType.SkeletonRegistry:
                case GameAssetType.SkeletonAnimStyles:
                case GameAssetType.AdventureSharedData:
                case GameAssetType.AdventurePlayProfile:
                case GameAssetType.AnimationMap:
                case GameAssetType.CachedCostumeData:
                case GameAssetType.DataLabels:
                case GameAssetType.AdventureMaps:
                default:
                    // If we don't know what asset type this is, just hope that whatever is asking for it knows what it is
                    return null;
            }
        }
        catch
        {
            return null;
        }
    }
}