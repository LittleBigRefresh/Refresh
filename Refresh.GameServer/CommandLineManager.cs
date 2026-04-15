using System.Diagnostics.CodeAnalysis;
using CommandLine;
using Refresh.Database;
using Refresh.Database.Models.Assets;
using Refresh.Database.Models.Users;
using Refresh.Interfaces.APIv3.Documentation;

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
        
        [Option("flag-modded-levels", HelpText = "Go through all uploaded levels and update the modded status of each level.")]
        public bool FlagModdedLevels { get; set; }
        
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

        [Option("disallow-email", HelpText = "Disallow the email address from being used by anyone in the future. Email option is required if this is set.")]
        public bool DisallowEmailAddress { get; set; }
        
        [Option("reallow-email", HelpText = "Re-allow the email address to be used for account registration. Email option is required if this is set")]
        public bool ReallowEmailAddress { get; set; }

        [Option("disallow-email-domain", HelpText = "Disallow the email domain from being used by anyone in the future. Email option is required if this is set. If a whole Email address is given, only the substring after the last @ will be used.")]
        public bool DisallowEmailDomain { get; set; }
        
        [Option("reallow-email-domain", HelpText = "Re-allow the email domain to be used by anyone. Email option is required if this is set. If a whole Email address is given, only the substring after the last @ will be used.")]
        public bool ReallowEmailDomain { get; set; }

        [Option("disallow-asset", HelpText = "Disallow an asset by hash. While this won't delete the asset, it will prevent it from being uploaded in the future, and do other actions, such as instructing the game to censor this asset. "
                                           + "Asset option is required if this is set, and both the Type and Reason options are optional.")]
        public bool DisallowAsset { get; set; }
        
        [Option("reallow-asset", HelpText = "Re-allow an asset by hash. It may be uploaded and used in various UGC again. Asset option is required if this is set.")]
        public bool ReallowAsset { get; set; }

        [Option("asset", HelpText = "The hash of the asset to operate on.")]
        public string? AssetHash { get; set; }

        [Option("type", HelpText = "The type of the asset to use. If this isn't set, we will use the corrensponding GameAsset's type from DB instead, if it exists.")]
        public string? AssetType { get; set; }

        [Option("reason", HelpText = "The (usually optional) reason for a moderation action, such as asset disallowance.")]
        public string? Reason { get; set; }
        
        [Option("rename-user", HelpText = "Changes a user's username. (old) username or Email option is required if this is set.")]
        public string? RenameUser { get; set; }
        
        [Option("delete-user", HelpText = "Deletes a user's account, removing their data but keeping a record of their sign up.")]
        public bool DeleteUser { get; set; }
        
        [Option("fully-delete-user", HelpText = "Fully deletes a user, entirely removing the row and allowing people to register with that username once again. Not recommended.")]
        public bool FullyDeleteUser { get; set; }
        
        [Option("mark-all-reuploads", HelpText = "Marks all levels uploaded by a user as re-uploaded. Username or Email options are required if this is set.")]
        public bool MarkAllReuploads { get; set; }
        
        [Option("ask-for-verification", HelpText = "Sends a verification code to confirm a user's identity over an untrusted channel. Username or Email options are required if this is set.")]
        public bool AskUserForVerification { get; set; }
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
        else if (options.FlagModdedLevels)
        {
            this._server.FlagModdedLevels();
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
                if (!this._server.DisallowUser(options.Username, options.Reason))
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
        else if (options.DisallowEmailAddress)
        {
            if (options.EmailAddress != null)
            {
                if (!this._server.DisallowEmailAddress(options.EmailAddress, options.Reason))
                    Fail("Email address is already disallowed");
            }
            else Fail("No email address was provided");
        }
        else if (options.ReallowEmailAddress)
        {
            if (options.EmailAddress != null)
            {
                if (!this._server.ReallowEmailAddress(options.EmailAddress))
                    Fail("Email address is already allowed");
            }
            else Fail("No email address was provided");
        }
        else if (options.DisallowEmailDomain)
        {
            if (options.EmailAddress != null)
            {
                if (!this._server.DisallowEmailDomain(options.EmailAddress, options.Reason))
                    Fail("Email domain is already disallowed");
            }
            else Fail("No email domain was provided");
        }
        else if (options.ReallowEmailDomain)
        {
            if (options.EmailAddress != null)
            {
                if (!this._server.ReallowEmailDomain(options.EmailAddress))
                    Fail("Email domain is already allowed");
            }
            else Fail("No email domain was provided");
        }
        else if (options.DisallowAsset)
        {
            if (options.AssetHash != null)
            {
                GameAssetType? type = null;
                if (options.AssetType != null)
                {
                    bool parsed = Enum.TryParse(options.AssetType, true, out GameAssetType assetType);
                    if (!parsed)
                    {
                        Fail($"The asset type '{options.AssetType}' couldn't be parsed. Possible values: "
                            + string.Join(", ", Enum.GetNames(typeof(GameAssetType))));
                        
                        return;
                    }

                    type = assetType;
                }

                if (!this._server.DisallowAsset(options.AssetHash, type, options.Reason))
                    Fail("Asset is already disallowed");
            }
            else Fail("No asset hash was provided");
        }
        else if (options.ReallowAsset)
        {
            if (options.AssetHash != null)
            {
                if (!this._server.ReallowAsset(options.AssetHash))
                    Fail("Asset is already allowed");
            }
            else Fail("No asset hash was provided");
        }
        else if (options.RenameUser != null)
        {
            if(string.IsNullOrWhiteSpace(options.RenameUser))
                Fail("Email address must contain content");
            
            GameUser user = this.GetUserOrFail(options);
            this._server.RenameUser(user, options.RenameUser, options.Force);
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
        else if (options.MarkAllReuploads)
        {
            GameUser user = this.GetUserOrFail(options);
            this._server.MarkAllReuploads(user);
        }
        else if (options.AskUserForVerification)
        {
            GameUser user = this.GetUserOrFail(options);
            string code = this._server.AskUserForVerification(user);
            Console.WriteLine($"The code has been sent to {user.Username}'s notifications.");
            Console.WriteLine();
            Console.WriteLine($"\tCode: {code}");
            Console.WriteLine();
            Console.WriteLine("If the user does not reply to you with *exactly* this code, assume the worst.");
        }
    }
}