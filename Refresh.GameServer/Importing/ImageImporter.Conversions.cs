using ICSharpCode.SharpZipLib.Zip.Compression;
using Pfim;

namespace Refresh.GameServer.Importing;

public partial class ImageImporter // Conversions
{
    // FIXME: this function is frankly slow, inefficient, and bad.
    // this will almost certainly cause tons of memory allocations!
    // tsk, tsk, tsk!
    //
    // maybe try refactoring IDataStore to support streams/spans?
    private static byte[] JpegToPng(byte[] data)
    {
        using Image image = Image.Load(data);
        using MemoryStream ms = new();
        image.SaveAsPng(ms);
        
        return ms.ToArray();
    }

    private static byte[] TextureToPng(byte[] data)
    {
        using MemoryStream ms = new();
        using BinaryWriter writer = new(ms);
        
        TextureToDds(writer, data);
        ms.Seek(0, SeekOrigin.Begin);
        
        return DdsToPng(ms);
    }

    private static void TextureToDds(BinaryWriter writer, byte[] data)
    {
        // Shamelessly stolen from Project Lighthouse, which I shamelessly stole from toolkit.
        // Two layers of me stealing things! Hi me from the future when you inevitably come to steal this, too.
        // https://github.com/LBPUnion/ProjectLighthouse/blob/main/ProjectLighthouse/Files/LbpImageHelper.cs#L127
        // https://github.com/ennuo/toolkit/blob/d996ee4134740db0ee94e2cbf1e4edbd1b5ec798/src/main/java/ennuo/craftworld/utilities/Compressor.java#L40
        
        // lighthouse can i copy your homework?
        // yeah just change it up a bit so it doesn't look obvious you copied
        // ok
        //
        // the homework:

        using MemoryStream dataMs = new(data);
        using BEBinaryReader reader = new(dataMs);

        for (int i = 0; i < 4; i++) reader.ReadByte(); // Skip header, we've already determined this is a TEX

        reader.ReadInt16();
        short chunks = reader.ReadInt16();

        int[] compressed = new int[chunks];
        int[] decompressed = new int[chunks];

        for (int i = 0; i < chunks; i++)
        {
            compressed[i] = reader.ReadUInt16();
            decompressed[i] = reader.ReadUInt16();
        }

        for (int i = 0; i < chunks; i++)
        {
            byte[] deflated = reader.ReadBytes(compressed[i]);
            byte[] inflated = new byte[decompressed[i]];
            
            if (compressed[i] == decompressed[i])
            {
                writer.Write(deflated);
                continue;
            }

            Inflater inflater = new();
            inflater.SetInput(deflated);
            inflater.Inflate(inflated);
            
            writer.Write(inflated);
        }
    }

    private static byte[] DdsToPng(Stream stream)
    {
        Dds dds = Dds.Create(stream, new PfimConfig());
        if(dds.Compressed) dds.Decompress();

        // ReSharper disable once SwitchExpressionHandlesSomeKnownEnumValuesWithExceptionInDefault
        Image image = dds.Format switch
        {
            ImageFormat.Rgba32 => Image.LoadPixelData<Bgra32>(dds.Data, dds.Width, dds.Height),
            _ => throw new InvalidOperationException($"Cannot convert DDS format {dds.Format} to PNG"),
        };

        MemoryStream ms = new();
        image.SaveAsPng(ms);
        
        return ms.ToArray();
    }
}