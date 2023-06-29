using CommandLine;

namespace Refresh.GameServer;

internal class CliHelper
{
    private readonly RefreshGameServer _server;

    internal CliHelper(RefreshGameServer server)
    {
        this._server = server;
    }

    [Serializable]
    private class Options
    {
        [Option('i', "import_assets", Required = false, 
            HelpText = "Re-import all assets from the datastore into the database. This is a destructive action, only use when upgrading to <=v1.5.0.")]
        public bool ImportAssets { get; set; }
        
        [Option('I', "import_images", Required = false, HelpText = "Convert all images in the database to .PNGs. Otherwise, images will be converted as they are used.")]
        public bool ImportImages { get; set; }
        
        [Option('f', "force", Required = false, HelpText = "Force all operations to happen, skipping user consent.")]
        public bool Force { get; set; }
    }

    internal void StartWithArgs(string[] args)
    {
        Parser.Default.ParseArguments<Options>(args)
            .WithParsed(this.StartWithOptions);
    }

    private void StartWithOptions(Options options)
    {
        if (options.ImportAssets)
        {
            this._server.ImportAssets(options.Force);
            return;
        }

        if (options.ImportImages)
        {
            this._server.ImportImages();
            return;
        }
    }
}