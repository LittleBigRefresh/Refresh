using Rgba32 = SixLabors.ImageSharp.PixelFormats.Rgba32;

namespace Refresh.GameServer.Importing.Mip;

public class MipHeader
{
    public uint ClutOffset;
    public required uint Width;
    public required uint Height;
    public required byte Bpp;
    public required byte NumBlocks;
    public required byte TexMode;
    public required bool Alpha;
    public uint DataOffset;

    public required Rgba32[] ColorLookupTable;
    
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
            ColorLookupTable = new Rgba32[256],
        };

        stream.Seek(header.ClutOffset, SeekOrigin.Begin);
        for (int i = 0; i < header.DataOffset && i < header.ColorLookupTable.Length; i++)
        {
            header.ColorLookupTable[i] = new Rgba32(reader.ReadByte(), reader.ReadByte(), reader.ReadByte(), reader.ReadByte());
        }

        return header;
    }

    public void Write(Stream stream)
    {
        const int colorLookupTableBase = 0x30;
        const int imageBase = 0x480;

        long start = stream.Position;
        
        BinaryWriter writer = new(stream);
        writer.Write((uint)colorLookupTableBase);
        writer.Write(this.Width);
        writer.Write(this.Height);
        writer.Write(this.Bpp);
        writer.Write(this.NumBlocks);
        writer.Write(this.TexMode);
        writer.Write((byte)(this.Alpha ? 1 : 0));
        writer.Write((uint)imageBase);

        long bytesUntilColorLookupTable = colorLookupTableBase - stream.Position - start;
        while (bytesUntilColorLookupTable > 0)
        {
            //Write a null byte to pad
            writer.Write((byte)0xAA);
            bytesUntilColorLookupTable--;
        }

        foreach (Rgba32 color in this.ColorLookupTable)
        {
            writer.Write(color.R);
            writer.Write(color.G);
            writer.Write(color.B);
            writer.Write(color.A);
        }
        
        long bytesUntilImageData = imageBase - stream.Position - start;
        while (bytesUntilImageData > 0)
        {
            //Write a null byte to pad
            writer.Write((byte)0xAA);
            bytesUntilImageData--;
        }
    }
}