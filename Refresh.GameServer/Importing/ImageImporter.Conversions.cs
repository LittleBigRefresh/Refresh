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
}