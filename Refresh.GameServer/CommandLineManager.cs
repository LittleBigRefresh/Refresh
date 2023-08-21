using System.Diagnostics.CodeAnalysis;
using Bunkum.HttpServer;
using CommandLine;
using NotEnoughLogs;
using Refresh.GameServer.Documentation;

namespace Refresh.GameServer;

internal class CommandLineManager
{
    private readonly RefreshGameServer _server;

    internal CommandLineManager(RefreshGameServer server)
    {
        this._server = server;
    }

    [Serializable]
    private class Options
    {
        [Option('i', "import_assets", Required = false, 
            HelpText = "Re-import all assets from the datastore into the database. This is a destructive action, only use when upgrading to <=v1.5.0")]
        public bool ImportAssets { get; set; }
        
        [Option('I', "import_images", Required = false, HelpText = "Convert all images in the database to .PNGs. Otherwise, images will be converted as they are used")]
        public bool ImportImages { get; set; }
        
        [Option('d', "generate_docs", Required = false, HelpText = "Generate API V3 Documentation")]
        public bool GenerateDocumentation { get; set; }
        
        [Option('a', "set_admin", Required = false, HelpText = "Give the user the Admin role.  Username or Email option is required if this is set.")]
        public bool SetAdmin { get; set; }
        
        [Option('n', "create_user", Required = false, HelpText = "Creates a user. Username and Email option is required if this is set.")]
        public bool CreateUser { get; set; }
        
        [Option('u', "username", Required = false, HelpText = "The user to operate on/create.")]
        public string? Username { get; set; }
        
        [Option('e', "email", Required = false, HelpText = "The user's email to operate on/create.")]
        public string? EmailAddress { get; set; }
        
        [Option('f', "force", Required = false, HelpText = "Force all operations to happen, skipping user consent")]
        public bool Force { get; set; }
    }

    internal void StartWithArgs(string[] args)
    {
        Parser.Default.ParseArguments<Options>(args)
            .WithParsed(this.StartWithOptions);
    }

    [SuppressMessage("ReSharper", "InvertIf")]
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

        if (options.GenerateDocumentation)
        {
            DocumentationService service = new(new LoggerContainer<BunkumContext>());
            service.Initialize();
            
            string json = JsonConvert.SerializeObject(service.Documentation, Formatting.Indented);
            File.WriteAllText(Path.Combine(Environment.CurrentDirectory, "apiDocumentation.json"), json);
        }

        if (options.CreateUser)
        {
            if (options.Username == null || options.EmailAddress == null)
            {
                Console.WriteLine("Both the email and username are required to create a user, cannot continue.");
                Environment.Exit(1);
            }
            
            this._server.CreateUser(options.Username, options.EmailAddress);
        }

        if (options.SetAdmin)
        {
            if (options.Username != null)
            {
                this._server.SetAdminFromUsername(options.Username);
            } else if (options.EmailAddress != null)
            {
                this._server.SetAdminFromEmailAddress(options.EmailAddress);
            }
            else
            {
                Console.WriteLine("No user/email was provided, cannot continue.");
                Environment.Exit(1);
            }
        }
    }
}