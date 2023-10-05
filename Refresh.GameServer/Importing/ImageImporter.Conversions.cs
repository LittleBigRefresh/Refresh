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

    /// <summary>
    /// Converts a TEX file to PNG
    /// </summary>
    /// <param name="stream">The TEX stream</param>
    /// <param name="writeStream">The output PNG stream</param>
    private static void TextureToPng(Stream stream, Stream writeStream)
    {
        DdsToPng(new TexStream(stream), writeStream);
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