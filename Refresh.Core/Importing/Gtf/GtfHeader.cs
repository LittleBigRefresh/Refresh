namespace Refresh.Core.Importing.Gtf;

public struct GtfHeader
{
    public GtfPixelFormat PixelFormat;
    public bool Mipmap;
    public byte Dimension;
    public bool Cubemap;
    public uint Remap;
    public ushort Width;
    public ushort Height;
    public ushort Depth;
    public byte Location;
    public uint Pitch;
    public uint Offset;

    public static GtfHeader Read(Stream stream)
    {
        BEBinaryReader reader = new(stream);

        GtfHeader header = new();
        
        header.PixelFormat = (GtfPixelFormat)reader.ReadByte();
        header.Mipmap = reader.ReadBoolean();
        header.Dimension = reader.ReadByte();
        header.Cubemap = reader.ReadBoolean();
        header.Remap = reader.ReadUInt32();
        header.Width = reader.ReadUInt16();
        header.Height = reader.ReadUInt16();
        header.Depth = reader.ReadUInt16();
        header.Location = reader.ReadByte();
        _ = reader.ReadByte(); //unused padding
        header.Pitch = reader.ReadUInt32();
        header.Offset = reader.ReadUInt32();

        return header;
    }
}