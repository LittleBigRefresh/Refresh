using SixLabors.ImageSharp.Formats;

namespace Refresh.Core.Importing.Gtf;

public class GtfFormat : IImageFormat<GtfMetadata>
{
    public GtfMetadata CreateDefaultFormatMetadata() => new();
    
    public string Name => "GTF";
    public string DefaultMimeType => "";
    public IEnumerable<string> MimeTypes => new string[] { };
    public IEnumerable<string> FileExtensions => new[] { "GTF" };

    public static IImageFormat Instance { get; } = new GtfFormat();
}