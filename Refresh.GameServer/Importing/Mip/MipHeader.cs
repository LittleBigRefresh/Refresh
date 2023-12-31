using Rgba32 = SixLabors.ImageSharp.PixelFormats.Rgba32;

namespace Refresh.GameServer.Importing.Mip;

public class MipHeader
{
    public uint ClutOffset;
    public uint Width;
    public uint Height;
    public byte Bpp;
    public byte NumBlocks;
    public byte TexMode;
    public bool Alpha;
    public uint DataOffset;

    public Rgba32[] Clut;
    
    public static MipHeader Read(Stream stream)
    {
        BinaryReader reader = new(stream);
        
        MipHeader header = new MipHeader
        {
            ClutOffset = reader.ReadUInt32(),
            Width = reader.ReadUInt32(),
            Height = reader.ReadUInt32(),
            Bpp = reader.ReadByte(),
            NumBlocks = reader.ReadByte(),
            TexMode = reader.ReadByte(),
            Alpha = reader.ReadByte() == 1,
            DataOffset = reader.ReadUInt32(),
            Clut = new Rgba32[256],
        };

        stream.Seek(header.ClutOffset, SeekOrigin.Begin);
        for (int i = 0; i < header.Clut.Length; i++)
        {
            header.Clut[i] = new Rgba32(reader.ReadByte(), reader.ReadByte(), reader.ReadByte(), reader.ReadByte());
        }

        return header;
    }
}