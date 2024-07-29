using System.Diagnostics.CodeAnalysis;
using CommandLine;
using NotEnoughLogs;
using Refresh.GameServer.Database;
using Refresh.GameServer.Documentation;
using Refresh.GameServer.Types.Roles;
using Refresh.GameServer.Types.UserData;

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
        
        [Option("set-trusted", HelpText = "Gives the user the Trusted role. Username or Email options are required if this is set.")]
        public bool SetTrusted { get; set; }
        
        [Option("set-curator", HelpText = "Gives the user the Curator role. Username or Email options are required if this is set.")]
        public bool SetCurator { get; set; }
        
        [Option("set-default", HelpText = "Gives the user the default role. Username or Email options are required if this is set.")]
        public bool SetDefault { get; set; }
        
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
        
        [Option("delete-user", HelpText = "Deletes a user's account, removing their data but keeping a record of their sign up.")]
        public bool DeleteUser { get; set; }
        
        [Option("fully-delete-user", HelpText = "Fully deletes a user, entirely removing the row and allowing people to register with that username once again. Not recommended.")]
        public bool FullyDeleteUser { get; set; }
    }

    internal void StartWithArgs(string[] args)
    {
        try
        {
            Parser.Default.ParseArguments<Options>(args)
                .WithParsed(this.StartWithOptions);
        }
        catch (Exception e)
        {
            Fail($"An internal error occurred: {e}", 139);
        }
        
        Console.WriteLine("The operation completed successfully.");
    }

    [DoesNotReturn]
    private static void Fail(string reason, int code = 1)
    {
        ConsoleColor oldColor = Console.ForegroundColor;
        
        Console.ForegroundColor = ConsoleColor.Red;
        Console.WriteLine(reason);
        Console.WriteLine($"The operation failed with exit code {code}");
        Console.ForegroundColor = oldColor;
        
        Environment.Exit(code);
    }
    
    private GameUser GetUserOrFail(Options options)
    {
        if (options.Username == null && options.EmailAddress == null) Fail("No username or email was provided");
        
        using GameDatabaseContext context = this._server.GetContext();

        GameUser? user = null;
        if (options.Username != null) user = context.GetUserByUsername(options.Username);
        if (options.EmailAddress != null) user ??= context.GetUserByEmailAddress(options.EmailAddress);
        
        if (user == null) Fail($"Cannot find the user by username '{options.Username}' or by email '{options.EmailAddress}'");
        return user;
    }

    [SuppressMessage("ReSharper", "InvertIf")]
    private void StartWithOptions(Options options)
    {
        if (options.ImportAssets)
        {
            this._server.ImportAssets(options.Force);
        }
        else if (options.ImportImages)
        {
            this._server.ImportImages();
        }
        else if (options.GenerateDocumentation)
        {
            DocumentationService service = new(this._server.Logger);
            service.Initialize();
            
            string json = JsonConvert.SerializeObject(service.Documentation, Formatting.Indented);
            File.WriteAllText(Path.Combine(Environment.CurrentDirectory, "apiDocumentation.json"), json);
        }
        else if (options.CreateUser)
        {
            if (options.Username == null || options.EmailAddress == null)
            {
                Fail("Both the email and username are required to create a user");
            }
            
            this._server.CreateUser(options.Username, options.EmailAddress);
        }
        else if (options.SetAdmin)
        {
            GameUser user = this.GetUserOrFail(options);
            this._server.SetUserAsRole(user, GameUserRole.Admin);
        }
        else if (options.SetTrusted)
        {
            GameUser user = this.GetUserOrFail(options);
            this._server.SetUserAsRole(user, GameUserRole.Trusted);
        }
        else if (options.SetCurator)
        {
            GameUser user = this.GetUserOrFail(options);
            this._server.SetUserAsRole(user, GameUserRole.Curator);
        }
        else if (options.SetDefault)
        {
            GameUser user = this.GetUserOrFail(options);
            this._server.SetUserAsRole(user, GameUserRole.User);
        }
        else if (options.DisallowUser)
        {
            if (options.Username != null)
            {
                if (!this._server.DisallowUser(options.Username))
                    Fail("User is already disallowed");
            }
            else Fail("No username was provided");
        }
        else if (options.ReallowUser)
        {
            if (options.Username != null)
            {
                if (!this._server.ReallowUser(options.Username))
                    Fail("User is already allowed");
            }
            else Fail("No username was provided");
        }
        else if (options.RenameUser != null)
        {
            if(string.IsNullOrWhiteSpace(options.RenameUser))
                Fail("Username must contain content");
            
            GameUser user = this.GetUserOrFail(options);
            this._server.RenameUser(user, options.RenameUser);
        }
        else if (options.DeleteUser)
        {
            GameUser user = this.GetUserOrFail(options);
            this._server.DeleteUser(user);
        }
        else if (options.FullyDeleteUser)
        {
            GameUser user = this.GetUserOrFail(options);
            this._server.FullyDeleteUser(user);
        }
    }
}