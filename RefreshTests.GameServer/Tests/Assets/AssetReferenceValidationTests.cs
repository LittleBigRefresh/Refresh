using System.Security.Cryptography;
using Refresh.Core.Helpers;
using Refresh.Core.Importing;
using Refresh.Core.Types.Assets.Validation;
using Refresh.Core.Types.Data;
using Refresh.Database.Models.Assets;
using Refresh.Database.Models.Authentication;
using Refresh.Database.Models.Users;

namespace RefreshTests.GameServer.Tests.Assets;

public class AssetReferenceValidationTests : GameServerTest
{
    private const string ValidImageGuid = "g18451"; // star sticker texture
    private const string InvalidImageGuid = "g1087"; // sackboy model

    [Test]
    [TestCase("")]
    [TestCase(" ")]
    [TestCase("  ")]
    [TestCase("0")]
    public void AcceptBlankHash(string blankHashVariation)
    {
        using TestContext context = this.GetServer();
        GameUser user = context.CreateUser();
        HttpClient client = context.GetAuthenticatedClient(TokenType.Game, TokenGame.LittleBigPlanet1, TokenPlatform.PS3, out Token token, user);
        DataContext dataContext = context.GetDataContext(token);
        AssetImporter importer = new(dataContext.Logger, context.Time);

        string newRefSetByCallback = "unset lol";
        ValidatedAssetResult result = ResourceValidationHelper.ValidateReference(new(blankHashVariation, dataContext, importer)
        {
            OnNewAssetRefCallback = delegate(string NewAssetRef) { newRefSetByCallback = NewAssetRef; },
        }, dataContext.Logger);
        Assert.That(result.Status, Is.EqualTo(OK));
        Assert.That(result.NewAssetRef, Is.EqualTo("0"));
        Assert.That(newRefSetByCallback, Is.EqualTo("0"));
        Assert.That(result.AssetInfo, Is.Null);
    }

    [Test]
    [TestCase("")]
    [TestCase(" ")]
    [TestCase("  ")]
    [TestCase("0")]
    public void RejectBlankHashIfDisallowed(string blankHashVariation)
    {
        using TestContext context = this.GetServer();
        GameUser user = context.CreateUser();
        HttpClient client = context.GetAuthenticatedClient(TokenType.Game, TokenGame.LittleBigPlanet1, TokenPlatform.PS3, out Token token, user);
        DataContext dataContext = context.GetDataContext(token);
        AssetImporter importer = new(dataContext.Logger, context.Time);

        string newRefSetByCallback = "unset lol";
        ValidatedAssetResult result = ResourceValidationHelper.ValidateReference(new(blankHashVariation, dataContext, importer)
        {
            MayBeBlank = false,
            OnNewAssetRefCallback = delegate(string NewAssetRef) { newRefSetByCallback = NewAssetRef; },
        }, dataContext.Logger);
        Assert.That(result.Status, Is.EqualTo(BadRequest));
        Assert.That(result.ErrorMessage, Is.Not.Null);
        Assert.That(result.NewAssetRef, Is.EqualTo("0"));
        Assert.That(newRefSetByCallback, Is.EqualTo("0"));
    }

    [Test]
    [TestCase(ValidImageGuid)]
    [TestCase(InvalidImageGuid)]
    public void AcceptGuidsIfNotRestricted(string guid)
    {
        using TestContext context = this.GetServer();
        GameUser user = context.CreateUser();
        HttpClient client = context.GetAuthenticatedClient(TokenType.Game, TokenGame.LittleBigPlanet1, TokenPlatform.PS3, out Token token, user);
        DataContext dataContext = context.GetDataContext(token);
        AssetImporter importer = new(dataContext.Logger, context.Time);

        string newRefSetByCallback = "unset lol";
        ValidatedAssetResult result = ResourceValidationHelper.ValidateReference(new(guid, dataContext, importer)
        {
            OnNewAssetRefCallback = delegate(string NewAssetRef) { newRefSetByCallback = NewAssetRef; },
        }, dataContext.Logger);
        Assert.That(result.Status, Is.EqualTo(OK));
        Assert.That(result.NewAssetRef, Is.EqualTo(guid));
        Assert.That(newRefSetByCallback, Is.EqualTo(guid));
        Assert.That(result.ErrorMessage, Is.Null);
    }

    [Test]
    [TestCase(InvalidImageGuid)]
    [TestCase(ValidImageGuid)]
    public void RejectGuidIfDisallowed(string guid)
    {
        using TestContext context = this.GetServer();
        GameUser user = context.CreateUser();
        HttpClient client = context.GetAuthenticatedClient(TokenType.Game, TokenGame.LittleBigPlanet1, TokenPlatform.PS3, out Token token, user);
        DataContext dataContext = context.GetDataContext(token);
        AssetImporter importer = new(dataContext.Logger, context.Time);

        string newRefSetByCallback = "unset lol";
        ValidatedAssetResult result = ResourceValidationHelper.ValidateReference(new(guid, dataContext, importer)
        {
            MayBeGuid = false,
            OnNewAssetRefCallback = delegate(string NewAssetRef) { newRefSetByCallback = NewAssetRef; },
        }, dataContext.Logger);
        Assert.That(result.Status, Is.EqualTo(BadRequest));
        Assert.That(result.ErrorMessage, Is.Not.Null);
        Assert.That(result.NewAssetRef, Is.EqualTo("0"));
        Assert.That(newRefSetByCallback, Is.EqualTo("0"));
    }

    [Test]
    [TestCase("g")]
    [TestCase("greg")]
    [TestCase("g67676767676767676767676767676776767676767")]
    public void RejectGuidIfBadlyFormatted(string guid)
    {
        using TestContext context = this.GetServer();
        GameUser user = context.CreateUser();
        HttpClient client = context.GetAuthenticatedClient(TokenType.Game, TokenGame.LittleBigPlanet1, TokenPlatform.PS3, out Token token, user);
        DataContext dataContext = context.GetDataContext(token);
        AssetImporter importer = new(dataContext.Logger, context.Time);

        string newRefSetByCallback = "unset lol";
        ValidatedAssetResult result = ResourceValidationHelper.ValidateReference(new(guid, dataContext, importer)
        {
            OnNewAssetRefCallback = delegate(string NewAssetRef) { newRefSetByCallback = NewAssetRef; },
        }, dataContext.Logger);
        Assert.That(result.Status, Is.EqualTo(BadRequest));
        Assert.That(result.ErrorMessage, Is.Not.Null);
        Assert.That(result.NewAssetRef, Is.EqualTo("0"));
        Assert.That(newRefSetByCallback, Is.EqualTo("0"));
    }

    [Test]
    public void RejectNonTextureGuidIfOnlyTexturesAllowed()
    {
        string guid = InvalidImageGuid;
        using TestContext context = this.GetServer();
        GameUser user = context.CreateUser();
        HttpClient client = context.GetAuthenticatedClient(TokenType.Game, TokenGame.LittleBigPlanet1, TokenPlatform.PS3, out Token token, user);
        DataContext dataContext = context.GetDataContext(token);
        AssetImporter importer = new(dataContext.Logger, context.Time);

        string newRefSetByCallback = "unset lol";
        ValidatedAssetResult result = ResourceValidationHelper.ValidateReference(new(guid, dataContext, importer)
        {
            MustBeTexture = true,
            OnNewAssetRefCallback = delegate(string NewAssetRef) { newRefSetByCallback = NewAssetRef; },
        }, dataContext.Logger);
        Assert.That(result.Status, Is.EqualTo(BadRequest));
        Assert.That(result.ErrorMessage, Is.Not.Null);
        Assert.That(result.NewAssetRef, Is.EqualTo("0"));
        Assert.That(newRefSetByCallback, Is.EqualTo("0"));
    }

    [Test]
    public void AcceptTextureGuidIfOnlyTexturesAllowed()
    {
        string guid = ValidImageGuid;
        using TestContext context = this.GetServer();
        GameUser user = context.CreateUser();
        HttpClient client = context.GetAuthenticatedClient(TokenType.Game, TokenGame.LittleBigPlanet1, TokenPlatform.PS3, out Token token, user);
        DataContext dataContext = context.GetDataContext(token);
        AssetImporter importer = new(dataContext.Logger, context.Time);

        string newRefSetByCallback = "unset lol";
        ValidatedAssetResult result = ResourceValidationHelper.ValidateReference(new(guid, dataContext, importer)
        {
            MustBeTexture = true,
            OnNewAssetRefCallback = delegate(string NewAssetRef) { newRefSetByCallback = NewAssetRef; },
        }, dataContext.Logger);
        Assert.That(result.Status, Is.EqualTo(OK));
        Assert.That(result.AssetInfo, Is.Null);
        Assert.That(result.NewAssetRef, Is.EqualTo(guid));
        Assert.That(newRefSetByCallback, Is.EqualTo(guid));
    }

    [Test]
    [TestCase(true, false)]
    [TestCase(false, false)]
    [TestCase(true, true)]
    [TestCase(false, true)]
    public void AcceptOrRejectHashDependingOnExistenceInDataStore(bool addToDataStore, bool mustBeInDataStore)
    {
        using TestContext context = this.GetServer();
        GameUser user = context.CreateUser();
        HttpClient client = context.GetAuthenticatedClient(TokenType.Game, TokenGame.LittleBigPlanet1, TokenPlatform.PS3, out Token token, user);
        DataContext dataContext = context.GetDataContext(token);
        AssetImporter importer = new(dataContext.Logger, context.Time);
        bool expectDataStoreFailure = !addToDataStore && mustBeInDataStore;

        ReadOnlySpan<byte> data = "LVLb"u8;
        string hash = BitConverter.ToString(SHA1.HashData(data)).Replace("-", "").ToLower();

        // add to store but not database, this way we can also test auto-import
        if (addToDataStore)
        {
            dataContext.DataStore.WriteToStore(hash, data);
        }

        string newRefSetByCallback = "unset lol";
        ValidatedAssetResult result = ResourceValidationHelper.ValidateReference(new(hash, dataContext, importer)
        {
            MustBeInDataStoreIfHash = mustBeInDataStore,
            OnNewAssetRefCallback = delegate(string NewAssetRef) { newRefSetByCallback = NewAssetRef; },
        }, dataContext.Logger);

        if (expectDataStoreFailure)
        {
            Assert.That(result.Status, Is.EqualTo(NotFound));
            Assert.That(result.AssetInfo, Is.Null);
            Assert.That(result.NewAssetRef, Is.EqualTo("0"));
            Assert.That(newRefSetByCallback, Is.EqualTo("0"));
        }
        else if (addToDataStore)
        {
            Assert.That(result.Status, Is.EqualTo(OK));
            Assert.That(result.AssetInfo, Is.Not.Null); // ensure it was auto-imported
            Assert.That(result.AssetInfo!.AssetHash, Is.EqualTo(hash));
            Assert.That(result.AssetInfo!.AssetType, Is.EqualTo(GameAssetType.Level));
            Assert.That(result.NewAssetRef, Is.EqualTo(hash));
            Assert.That(newRefSetByCallback, Is.EqualTo(hash));
        }
        else
        {
            Assert.That(result.Status, Is.EqualTo(OK));
            Assert.That(result.AssetInfo, Is.Null); // not auto-imported and also not in DB before
            Assert.That(result.NewAssetRef, Is.EqualTo(hash));
            Assert.That(newRefSetByCallback, Is.EqualTo(hash));
        }

        Assert.That(result.DisallowanceInfo, Is.Null);
        Assert.That(result.ExistsInDataStore, Is.EqualTo(addToDataStore));
    }

    [Test]
    public void RejectHashIfReadingFromDataStoreFails()
    {
        using TestContext context = this.GetServer();
        GameUser user = context.CreateUser();
        HttpClient client = context.GetAuthenticatedClient(TokenType.Game, TokenGame.LittleBigPlanet1, TokenPlatform.PS3, out Token token, user);

        // read-failing will unconditionally return true on key lookup, but will fail trying to read (null bytes/false return), which is what we want
        DataContext dataContext = context.GetDataContext(token, new ReadFailingDataStore());
        AssetImporter importer = new(dataContext.Logger, context.Time);

        ReadOnlySpan<byte> data = "LVLb"u8;
        string hash = BitConverter.ToString(SHA1.HashData(data)).Replace("-", "").ToLower();

        string newRefSetByCallback = "unset lol";
        ValidatedAssetResult result = ResourceValidationHelper.ValidateReference(new(hash, dataContext, importer)
        {
            MustBeInDataStoreIfHash = true,
            OnNewAssetRefCallback = delegate(string NewAssetRef) { newRefSetByCallback = NewAssetRef; },
        }, dataContext.Logger);

        Assert.That(result.Status, Is.EqualTo(InternalServerError));
        Assert.That(result.AssetInfo, Is.Null);
        Assert.That(result.DisallowanceInfo, Is.Null);
        Assert.That(result.ExistsInDataStore, Is.True);
        Assert.That(result.NewAssetRef, Is.EqualTo("0"));
        Assert.That(newRefSetByCallback, Is.EqualTo("0"));
    }

    [Test]
    public void RejectHashIfImportingFromDataStoreFails()
    {
        using TestContext context = this.GetServer();
        GameUser user = context.CreateUser();
        HttpClient client = context.GetAuthenticatedClient(TokenType.Game, TokenGame.LittleBigPlanet1, TokenPlatform.PS3, out Token token, user);
        DataContext dataContext = context.GetDataContext(token);
        AssetImporter importer = new(dataContext.Logger, context.Time);

        ReadOnlySpan<byte> data = "totallyalevel"u8;
        // importing, for now, only really fails if the given hash doesn't match the actual hash
        string fakeHash = BitConverter.ToString(SHA1.HashData("veryreallevel"u8)).Replace("-", "").ToLower();
        dataContext.DataStore.WriteToStore(fakeHash, data);

        string newRefSetByCallback = "unset lol";
        ValidatedAssetResult result = ResourceValidationHelper.ValidateReference(new(fakeHash, dataContext, importer)
        {
            MustBeInDataStoreIfHash = true,
            OnNewAssetRefCallback = delegate(string NewAssetRef) { newRefSetByCallback = NewAssetRef; },
        }, dataContext.Logger);

        Assert.That(result.Status, Is.EqualTo(BadRequest));
        Assert.That(result.AssetInfo, Is.Null);
        Assert.That(result.DisallowanceInfo, Is.Null);
        Assert.That(result.ExistsInDataStore, Is.True);
        Assert.That(result.ErrorMessage, Is.Not.Null);
        Assert.That(result.NewAssetRef, Is.EqualTo("0"));
        Assert.That(newRefSetByCallback, Is.EqualTo("0"));
    }

    [Test]
    public void RejectHashIfInvalidHash()
    {
        using TestContext context = this.GetServer();
        GameUser user = context.CreateUser();
        HttpClient client = context.GetAuthenticatedClient(TokenType.Game, TokenGame.LittleBigPlanet1, TokenPlatform.PS3, out Token token, user);
        DataContext dataContext = context.GetDataContext(token);
        AssetImporter importer = new(dataContext.Logger, context.Time);

        string newRefSetByCallback = "unset lol";
        ValidatedAssetResult result = ResourceValidationHelper.ValidateReference(new("lololol", dataContext, importer)
        {
            MustBeInDataStoreIfHash = true,
            OnNewAssetRefCallback = delegate(string NewAssetRef) { newRefSetByCallback = NewAssetRef; },
        }, dataContext.Logger);

        Assert.That(result.Status, Is.EqualTo(BadRequest));
        Assert.That(result.AssetInfo, Is.Null);
        Assert.That(result.DisallowanceInfo, Is.Null);
        Assert.That(result.ExistsInDataStore, Is.False); // Cancelled before actually wasting time looking it up
        Assert.That(result.NewAssetRef, Is.EqualTo("0"));
        Assert.That(newRefSetByCallback, Is.EqualTo("0"));
    }

    [Test]
    public void RejectHashIfAssetDisallowed()
    {
        using TestContext context = this.GetServer();
        GameUser user = context.CreateUser();
        HttpClient client = context.GetAuthenticatedClient(TokenType.Game, TokenGame.LittleBigPlanet1, TokenPlatform.PS3, out Token token, user);
        DataContext dataContext = context.GetDataContext(token);
        AssetImporter importer = new(dataContext.Logger, context.Time);

        ReadOnlySpan<byte> data = "LVLb"u8;
        string hash = BitConverter.ToString(SHA1.HashData(data)).Replace("-", "").ToLower();
        context.Database.DisallowAsset(hash, GameAssetType.Level, "youre evel was so shit that we had to ban it");

        context.Database.Refresh();

        string newRefSetByCallback = "unset lol";
        ValidatedAssetResult result = ResourceValidationHelper.ValidateReference(new(hash, dataContext, importer)
        {
            MustBeInDataStoreIfHash = true, // Ensure disallowance is checked before the data store check
            OnNewAssetRefCallback = delegate(string NewAssetRef) { newRefSetByCallback = NewAssetRef; },
        }, dataContext.Logger);

        Assert.That(result.Status, Is.EqualTo(Unauthorized));
        Assert.That(result.ErrorMessage, Is.Null);
        Assert.That(result.AssetInfo, Is.Null);
        Assert.That(result.DisallowanceInfo, Is.Not.Null);
        Assert.That(result.NewAssetRef, Is.EqualTo("0"));
        Assert.That(newRefSetByCallback, Is.EqualTo("0"));
    }

    [Test]
    public void RejectHashIfHashesDisallowed()
    {
        using TestContext context = this.GetServer();
        GameUser user = context.CreateUser();
        HttpClient client = context.GetAuthenticatedClient(TokenType.Game, TokenGame.LittleBigPlanet1, TokenPlatform.PS3, out Token token, user);
        DataContext dataContext = context.GetDataContext(token);
        AssetImporter importer = new(dataContext.Logger, context.Time);

        ReadOnlySpan<byte> data = "LVLb"u8;
        string hash = BitConverter.ToString(SHA1.HashData(data)).Replace("-", "").ToLower();

        string newRefSetByCallback = "unset lol";
        ValidatedAssetResult result = ResourceValidationHelper.ValidateReference(new(hash, dataContext, importer)
        {
            MayBeHash = false,
            OnNewAssetRefCallback = delegate(string NewAssetRef) { newRefSetByCallback = NewAssetRef; },
        }, dataContext.Logger);

        Assert.That(result.Status, Is.EqualTo(BadRequest));
        Assert.That(result.ErrorMessage, Is.Not.Null);
        Assert.That(result.AssetInfo, Is.Null);
        Assert.That(result.DisallowanceInfo, Is.Null);
        Assert.That(result.NewAssetRef, Is.EqualTo("0"));
        Assert.That(newRefSetByCallback, Is.EqualTo("0"));
    }

    [Test]
    [TestCase(true)]
    [TestCase(false)]
    public void AcceptTextureHashIfOnlyTexturesAllowed(bool addToDataStore)
    {
        using TestContext context = this.GetServer();
        GameUser user = context.CreateUser();
        HttpClient client = context.GetAuthenticatedClient(TokenType.Game, TokenGame.LittleBigPlanet1, TokenPlatform.PS3, out Token token, user);
        DataContext dataContext = context.GetDataContext(token);
        AssetImporter importer = new(dataContext.Logger, context.Time);

        ReadOnlySpan<byte> data = "TEX "u8;
        string hash = BitConverter.ToString(SHA1.HashData(data)).Replace("-", "").ToLower();

        if (addToDataStore) dataContext.DataStore.WriteToStore(hash, data);
        else context.Database.AddAssetToDatabase(new() // not adding to store: test getting from just DB instead
        {
            AssetHash = hash,
            AssetType = GameAssetType.Texture,
        });

        context.Database.Refresh();

        string newRefSetByCallback = "unset lol";
        ValidatedAssetResult result = ResourceValidationHelper.ValidateReference(new(hash, dataContext, importer)
        {
            MustBeTexture = true,
            MustBeInDataStoreIfHash = false, // not tested in this one
            OnNewAssetRefCallback = delegate(string NewAssetRef) { newRefSetByCallback = NewAssetRef; },
        }, dataContext.Logger);

        Assert.That(result.Status, Is.EqualTo(OK));
        Assert.That(result.NewAssetRef, Is.EqualTo(hash));
        Assert.That(newRefSetByCallback, Is.EqualTo(hash));
        Assert.That(result.AssetInfo, Is.Not.Null);
        Assert.That(result.AssetInfo!.AssetHash, Is.EqualTo(hash));
        Assert.That(result.AssetInfo!.AssetType, Is.EqualTo(GameAssetType.Texture));
        Assert.That(result.DisallowanceInfo, Is.Null);
        Assert.That(result.ExistsInDataStore, Is.EqualTo(addToDataStore));
    }

    [Test]
    [TestCase(true)]
    [TestCase(false)]
    public void RejectNonTextureHashIfOnlyTexturesAllowed(bool addToDataStore)
    {
        using TestContext context = this.GetServer();
        GameUser user = context.CreateUser();
        HttpClient client = context.GetAuthenticatedClient(TokenType.Game, TokenGame.LittleBigPlanet1, TokenPlatform.PS3, out Token token, user);
        DataContext dataContext = context.GetDataContext(token);
        AssetImporter importer = new(dataContext.Logger, context.Time);

        ReadOnlySpan<byte> data = "LVLb"u8;
        string hash = BitConverter.ToString(SHA1.HashData(data)).Replace("-", "").ToLower();

        if (addToDataStore) dataContext.DataStore.WriteToStore(hash, data);
        else context.Database.AddAssetToDatabase(new() // not adding to store: test getting from just DB instead
        {
            AssetHash = hash,
            AssetType = GameAssetType.Level,
        });

        context.Database.Refresh();

        string newRefSetByCallback = "unset lol";
        ValidatedAssetResult result = ResourceValidationHelper.ValidateReference(new(hash, dataContext, importer)
        {
            MustBeTexture = true,
            MustBeInDataStoreIfHash = false, // not tested in this one
            OnNewAssetRefCallback = delegate(string NewAssetRef) { newRefSetByCallback = NewAssetRef; },
        }, dataContext.Logger);

        Assert.That(result.Status, Is.EqualTo(BadRequest));
        Assert.That(result.NewAssetRef, Is.EqualTo("0"));
        Assert.That(newRefSetByCallback, Is.EqualTo("0"));
        Assert.That(result.AssetInfo, Is.Not.Null);
        Assert.That(result.AssetInfo!.AssetHash, Is.EqualTo(hash));
        Assert.That(result.AssetInfo!.AssetType, Is.EqualTo(GameAssetType.Level));
        Assert.That(result.DisallowanceInfo, Is.Null);
        Assert.That(result.ExistsInDataStore, Is.EqualTo(addToDataStore));
    }

    // TODO: test AIPI using a fake test server

    [Test]
    public void AcceptsIfMustBeTextureButUnreadablePSPAsset()
    {
        using TestContext context = this.GetServer();
        GameUser user = context.CreateUser();
        HttpClient client = context.GetAuthenticatedClient(TokenType.Game, TokenGame.LittleBigPlanetPSP, TokenPlatform.PSP, out Token token, user);
        DataContext dataContext = context.GetDataContext(token);
        AssetImporter importer = new(dataContext.Logger, context.Time);

        ReadOnlySpan<byte> data = "bbbbbbbbbbb"u8;
        string hash = BitConverter.ToString(SHA1.HashData(data)).Replace("-", "").ToLower();
        dataContext.DataStore.WriteToStore($"psp/{hash}", data);

        context.Database.Refresh();

        string newRefSetByCallback = "unset lol";
        ValidatedAssetResult result = ResourceValidationHelper.ValidateReference(new(hash, dataContext, importer)
        {
            MustBeTexture = true,
            MustBeInDataStoreIfHash = true,
            OnNewAssetRefCallback = delegate(string NewAssetRef) { newRefSetByCallback = NewAssetRef; },
        }, dataContext.Logger);

        Assert.That(result.Status, Is.EqualTo(OK));
        Assert.That(result.NewAssetRef, Is.EqualTo(hash));
        Assert.That(newRefSetByCallback, Is.EqualTo(hash));
        Assert.That(result.AssetInfo, Is.Not.Null);
        Assert.That(result.AssetInfo!.AssetHash, Is.EqualTo(hash));
        Assert.That(result.AssetInfo!.AssetType, Is.EqualTo(GameAssetType.Unknown));
        Assert.That(result.AssetInfo!.IsPSP, Is.True);
        Assert.That(result.DisallowanceInfo, Is.Null);
        Assert.That(result.ExistsInDataStore, Is.True);
    }

    [Test]
    public void RejectsIfMustBeTextureAndUnreadableNonPSPAsset()
    {
        using TestContext context = this.GetServer();
        GameUser user = context.CreateUser();
        HttpClient client = context.GetAuthenticatedClient(TokenType.Game, TokenGame.LittleBigPlanet1, TokenPlatform.PS3, out Token token, user);
        DataContext dataContext = context.GetDataContext(token);
        AssetImporter importer = new(dataContext.Logger, context.Time);

        ReadOnlySpan<byte> data = "bbbbbbbbbbb"u8;
        string hash = BitConverter.ToString(SHA1.HashData(data)).Replace("-", "").ToLower();
        dataContext.DataStore.WriteToStore(hash, data);

        context.Database.Refresh();

        string newRefSetByCallback = "unset lol";
        ValidatedAssetResult result = ResourceValidationHelper.ValidateReference(new(hash, dataContext, importer)
        {
            MustBeTexture = true,
            MustBeInDataStoreIfHash = true,
            OnNewAssetRefCallback = delegate(string NewAssetRef) { newRefSetByCallback = NewAssetRef; },
        }, dataContext.Logger);

        Assert.That(result.Status, Is.EqualTo(BadRequest));
        Assert.That(result.NewAssetRef, Is.EqualTo("0"));
        Assert.That(newRefSetByCallback, Is.EqualTo("0"));
        Assert.That(result.AssetInfo, Is.Not.Null);
        Assert.That(result.AssetInfo!.AssetHash, Is.EqualTo(hash));
        Assert.That(result.AssetInfo!.AssetType, Is.EqualTo(GameAssetType.Unknown));
        Assert.That(result.AssetInfo!.IsPSP, Is.False);
        Assert.That(result.DisallowanceInfo, Is.Null);
        Assert.That(result.ExistsInDataStore, Is.True);
    }
}