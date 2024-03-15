using Refresh.GameServer.Importing.Gtf;
using SixLabors.ImageSharp.Formats;

namespace Refresh.GameServer.Importing.Mip;

public class MipFormat : IImageFormat<MipMetadata>
{
    public MipMetadata CreateDefaultFormatMetadata() => new();
    
    public string Name => "MIP";
    public string DefaultMimeType => "";
    public IEnumerable<string> MimeTypes => new string[] { };
    public IEnumerable<string> FileExtensions => new[] { "MIP" };

    public static IImageFormat Instance { get; } = new MipFormat();
}