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
        for (int i = 0; i < header.DataOffset && i < header.Clut.Length; i++)
        {
            header.Clut[i] = new Rgba32(reader.ReadByte(), reader.ReadByte(), reader.ReadByte(), reader.ReadByte());
        }

        return header;
    }

    public void Write(Stream stream)
    {
        const int clutBase = 0x30;
        const int imageBase = 0x480;

        long start = stream.Position;
        
        BinaryWriter writer = new(stream);
        writer.Write((uint)clutBase);
        writer.Write(this.Width);
        writer.Write(this.Height);
        writer.Write(this.Bpp);
        writer.Write(this.NumBlocks);
        writer.Write(this.TexMode);
        writer.Write((byte)(this.Alpha ? 1 : 0));
        writer.Write((uint)imageBase);

        long amountUntilClut = clutBase - stream.Position - start;
        while (amountUntilClut > 0)
        {
            //Write a null byte to pad
            writer.Write((byte)0xAA);
            amountUntilClut--;
        }

        foreach (Rgba32 color in this.Clut)
        {
            writer.Write(color.R);
            writer.Write(color.G);
            writer.Write(color.B);
            writer.Write(color.A);
        }
        
        long amountUntilImage = imageBase - stream.Position - start;
        while (amountUntilImage > 0)
        {
            //Write a null byte to pad
            writer.Write((byte)0xAA);
            amountUntilImage--;
        }
    }
}