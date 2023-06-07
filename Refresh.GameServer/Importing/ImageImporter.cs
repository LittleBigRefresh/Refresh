using Bunkum.HttpServer;
using NotEnoughLogs;

namespace Refresh.GameServer.Importing;

public class ImageImporter : Importer
{
    public ImageImporter(LoggerContainer<BunkumContext>? logger = null) : base(logger)
    {
    }
}