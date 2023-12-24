using ICSharpCode.SharpZipLib.Zip.Compression;
using Pfim;
using Refresh.GameServer.Importing.Gtf;
using Refresh.GameServer.Importing.Mip;
using SixLabors.ImageSharp.Formats;

namespace Refresh.GameServer.Importing;

public partial class ImageImporter // Conversions
{
    /// <summary>
    /// Converts an image of any supported ImageSharp format to PNG
    /// </summary>
    /// <param name="stream">A stream representing some image file</param>
    /// <param name="writeStream">The output stream to write the PNG to</param>
    private static void ImageToPng(Stream stream, Stream writeStream)
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

    private static void GtfToPng(Stream stream, Stream writeStream)
    {
        using Image image = new GtfDecoder().Decode(new DecoderOptions(), stream);
        image.SaveAsPng(writeStream);
    }
    
    private static void MipToPng(Stream stream, Stream writeStream)
    {
        using Image image = new MipDecoder().Decode(new DecoderOptions(), stream);
        image.SaveAsPng(writeStream);
    }

    private static readonly PfimConfig Config = new();

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