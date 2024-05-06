namespace Refresh.GameServer.Types.Assets;

public enum GameSerializationMethod : byte
{
    Unknown = 0,
    Binary = (byte)'b',
    Text = (byte)'t',
    EncryptedBinary = (byte)'e',
    CompressedTexture = (byte)' ',
    GtfSwizzled = (byte)'s',
    GxtSwizzled = (byte)'S',
}