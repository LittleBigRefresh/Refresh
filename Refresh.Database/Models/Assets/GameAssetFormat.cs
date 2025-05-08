namespace Refresh.Database.Models.Assets;

public enum GameAssetFormat : byte
{
    Unknown = 0,
    Binary = (byte)'b',
    Text = (byte)'t',
    EncryptedBinary = (byte)'e',
    CompressedTexture = (byte)' ',
    GtfSwizzled = (byte)'s',
    GxtSwizzled = (byte)'S',
}

public static class GameAssetFormatExtensions
{
    /// <summary>
    /// Returns whether the asset format contains a dependency tree.
    /// </summary>
    /// <remarks>
    /// Since we only ever assign game asset formats to assets which follow standard LBP serialization, its safe to just assume "binary or encrypted binary means has dependency tree".
    /// </remarks>
    public static bool HasDependencyTree(this GameAssetFormat format) =>
        format is GameAssetFormat.Binary or GameAssetFormat.EncryptedBinary;
}