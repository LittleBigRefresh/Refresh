using ICSharpCode.SharpZipLib.Zip.Compression;
using Pfim;

namespace Refresh.GameServer.Importing;

public partial class ImageImporter // Conversions
{
    private static void JpegToPng(Stream stream, Stream writeStream)
    {
        using Image image = Image.Load(stream);
        image.SaveAsPng(writeStream);
    }

    private static void TextureToPng(Stream stream, Stream writeStream)
    {
        using MemoryStream ms = new();
        using BinaryWriter writer = new(ms);

        TextureToDds(writer, stream);
        ms.Seek(0, SeekOrigin.Begin);

        DdsToPng(ms, writeStream);
    }

    private static void TextureToDds(BinaryWriter writer, Stream data)
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
        
        using BEBinaryReader reader = new(data);

        data.Position += 4; // Skip header, we've already determined this is a TEX

        data.Position += sizeof(short);
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

            Inflater inflater = Inflater.Value!;
            
            inflater.Reset();
            inflater.SetInput(deflated);
            inflater.Inflate(inflated);
            
            writer.Write(inflated);
        }
    }

    private static readonly PfimConfig Config = new();
    private static readonly ThreadLocal<Inflater> Inflater = new(() => new Inflater());

    private static void DdsToPng(Stream stream, Stream writeStream)
    {
        Dds dds = Dds.Create(stream, Config);
        
        // ReSharper disable once SwitchExpressionHandlesSomeKnownEnumValuesWithExceptionInDefault
        Image image = dds.Format switch
        {
            ImageFormat.Rgba32 => Image.LoadPixelData<Bgra32>(dds.Data, dds.Width, dds.Height),
            _ => throw new InvalidOperationException($"Cannot convert DDS format {dds.Format} to PNG"),
        };


        image.SaveAsPng(writeStream);
    }
}