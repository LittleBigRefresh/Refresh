using Pfim;
using Refresh.Core.Importing.Gtf;
using Refresh.Core.Importing.Mip;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Formats;
using SixLabors.ImageSharp.PixelFormats;

namespace Refresh.Core.Importing;

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

    private static Image<Rgba32> LoadDds(Stream stream)
    {
        Dds dds = Dds.Create(stream, Config);
        
        // ReSharper disable once SwitchExpressionHandlesSomeKnownEnumValuesWithExceptionInDefault
        Image<Rgba32> image = dds.Format switch
        {
            ImageFormat.Rgba32 => Image.LoadPixelData<Bgra32>(dds.Data, dds.Width, dds.Height).CloneAs<Rgba32>(),
            _ => throw new InvalidOperationException($"Cannot convert DDS format {dds.Format} to PNG"),
        };

        return image;
    }

    //Loads a Tex file into an ImageSharp image.
    public static Image<Rgba32> LoadTex(Stream stream)
    {
        using Stream loadStream = new TexStream(stream);
        
        return LoadDds(loadStream);
    }
    
    //Loads a GTF file into an ImageSharp image.
    public static Image<Rgba32> LoadGtf(Stream stream)
    {
        return new GtfDecoder().Decode<Rgba32>(new DecoderOptions(), stream);
    }
    
    //Loads a MIP file into an ImageSharp image.
    public static Image<Rgba32> LoadMip(Stream stream)
    {
        return new MipDecoder().Decode<Rgba32>(new DecoderOptions(), stream);
    }

    private static void DdsToPng(Stream stream, Stream writeStream)
    {
        LoadDds(stream).SaveAsPng(writeStream);
    }
}