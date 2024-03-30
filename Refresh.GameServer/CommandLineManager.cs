using System.Diagnostics.CodeAnalysis;
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
        [Option("import-assets", HelpText = "Re-import all assets from the datastore into the database.")]
        public bool ImportAssets { get; set; }
        
        [Option("convert-images", HelpText = "Convert all images in the database to .PNGs. Otherwise, images will be converted as they are used.")]
        public bool ImportImages { get; set; }
        
        [Option("generate-docs", HelpText = "Generates documentation for API V3 endpoints.")]
        public bool GenerateDocumentation { get; set; }
        
        [Option('a', "set-admin", HelpText = "Gives the user the Admin role. Username or Email options are required if this is set.")]
        public bool SetAdmin { get; set; }
        
        [Option('n', "new-user", HelpText = "Creates a user. Username *and* Email options are required if this is set.")]
        public bool CreateUser { get; set; }
        
        [Option('u', "username", HelpText = "The user to operate on/create.")]
        public string? Username { get; set; }
        
        [Option('e', "email", HelpText = "The user's email to operate on/create.")]
        public string? EmailAddress { get; set; }
        
        [Option('f', "force", HelpText = "Force all operations to happen, skipping user consent.")]
        public bool Force { get; set; }
        
        [Option("disallow-user", HelpText = "Disallow a user from registering. Username option is required if this is set.")]
        public bool DisallowUser { get; set; }
        
        [Option("reallow-user", HelpText = "Re-allow a user to register. Username option is required if this is set.")]
        public bool ReallowUser { get; set; }
        
        [Option("rename-user", HelpText = "Changes a user's username. (old) username or Email option is required if this is set.")]
        public string? RenameUser { get; set; }
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
            DocumentationService service = new(this._server.Logger);
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
        
        if (options.DisallowUser)
        {
            if (options.Username != null)
            {
                if (!this._server.DisallowUser(options.Username))
                {
                    Console.WriteLine("User is already disallowed");
                    Environment.Exit(1);
                }
            }
            else
            {
                Console.WriteLine("No user was provided, cannot continue.");
                Environment.Exit(1);
            }
        }
        
        if (options.ReallowUser)
        {
            if (options.Username != null)
            {
                if (!this._server.ReallowUser(options.Username))
                {
                    Console.WriteLine("User is already allowed");
                    Environment.Exit(1);
                }
            }
            else
            {
                Console.WriteLine("No user was provided, cannot continue.");
                Environment.Exit(1);
            }
        }

        if (options.RenameUser != null)
        {
            if (options.Username != null)
            {
                this._server.RenameUserFromUsername(options.Username, options.RenameUser);
            } else if (options.EmailAddress != null)
            {
                this._server.RenameUserFromEmailAddress(options.EmailAddress, options.RenameUser);
            }
            else
            {
                Console.WriteLine("No user/email was provided, cannot continue.");
                Environment.Exit(1);
            }
        }
    }
}