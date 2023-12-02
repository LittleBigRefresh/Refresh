using System.Diagnostics;
using System.Linq.Expressions;
using System.Reflection;
using Bunkum.EntityFrameworkDatabase;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;
using MongoDB.Bson;
using Realms;
using Refresh.GameServer.Authentication;
using Refresh.GameServer.Time;
using Refresh.GameServer.Types;
using Refresh.GameServer.Types.Activity;
using Refresh.GameServer.Types.Assets;
using Refresh.GameServer.Types.Comments;
using Refresh.GameServer.Types.Levels;
using Refresh.GameServer.Types.Levels.SkillRewards;
using Refresh.GameServer.Types.Notifications;
using Refresh.GameServer.Types.Photos;
using Refresh.GameServer.Types.Relations;
using Refresh.GameServer.Types.Report;
using Refresh.GameServer.Types.UserData;
using Refresh.GameServer.Types.UserData.Leaderboard;
using PrimaryKeyAttribute = Realms.PrimaryKeyAttribute;

namespace Refresh.GameServer.Database.Postgres;

#nullable disable

public class PostgresGameDatabaseContext(Action<DbContextOptionsBuilder> configureAction) : BunkumDbContext(configureAction), IGameDatabaseContext
{
    private DbSet<GameUser> GameUsers { get; set; }
    private DbSet<UserPins> UserPins { get; set; }
    private DbSet<Token> Tokens { get; set; }
    private DbSet<GameLevel> GameLevels { get; set; }
    private DbSet<GameSkillReward> GameSkillRewards { get; set; }
    private DbSet<GameComment> GameComments { get; set; }
    private DbSet<FavouriteLevelRelation> FavouriteLevelRelations { get; set; }
    private DbSet<QueueLevelRelation> QueueLevelRelations { get; set; }
    private DbSet<FavouriteUserRelation> FavouriteUserRelations { get; set; }
    private DbSet<PlayLevelRelation> PlayLevelRelations { get; set; }
    private DbSet<UniquePlayLevelRelation> UniquePlayLevelRelations { get; set; }
    private DbSet<RateLevelRelation> RateLevelRelations { get; set; }
    private DbSet<Event> Events { get; set; }
    private DbSet<GameSubmittedScore> GameSubmittedScores { get; set; }
    private DbSet<GameAsset> GameAssets { get; set; }
    private DbSet<GameNotification> GameNotifications { get; set; }
    private DbSet<GamePhoto> GamePhotos { get; set; }
    // private DbSet<GamePhotoSubject> GamePhotoSubjects { get; set; } // TODO: persist this somehow
    private DbSet<GameIpVerificationRequest> GameIpVerificationRequests { get; set; }
    private DbSet<GameAnnouncement> GameAnnouncements { get; set; }
    private DbSet<QueuedRegistration> QueuedRegistrations { get; set; }
    private DbSet<EmailVerificationCode> EmailVerificationCodes { get; set; }
    private DbSet<RequestStatistics> RequestStatistics { get; set; }
    // private DbSet<SequentialIdStorage> SequentialIdStorages { get; set; }
    // private DbSet<GameReport> GameReports { get; set; }
    // private DbSet<InfoBubble> InfoBubbles { get; set; }
    // private DbSet<Marqee> Marqees { get; set; }
    // private DbSet<Player> Players { get; set; }
    // private DbSet<Rect> Rects { get; set; }
    // private DbSet<ScreenElements> ScreenElements { get; set; }
    // private DbSet<ScreenRect> ScreenRects { get; set; }
    // private DbSet<Slot> Slots { get; set; }

    private static void AddConversion<TType, TProvider>(ModelBuilder modelBuilder,
        IReadOnlyTypeBase entity,
        Expression<Func<TType, TProvider>> convertToProviderExpression,
        Expression<Func<TProvider, TType>> convertFromProviderExpression)
    {
        IEnumerable<PropertyInfo> props = entity.ClrType.GetProperties()
            .Where(p => p.PropertyType == typeof(TType));
        
        foreach (PropertyInfo prop in props)
        {
            modelBuilder.Entity(entity.Name).Property<TType>(prop.Name)
                .HasConversion(convertToProviderExpression, convertFromProviderExpression);
        }
    }

    private static void AddBacklinks(ModelBuilder modelBuilder, Type entityType, PropertyInfo entityProperty)
    {
        BacklinkAttribute backlink = entityProperty.GetCustomAttribute<BacklinkAttribute>();
        if (backlink == null) return;
            
        string backlinkPropertyName = (string)backlink.GetType().GetProperty("Property", BindingFlags.NonPublic | BindingFlags.Instance)!.GetValue(backlink)!;
        Type backlinkType = entityProperty.PropertyType.GenericTypeArguments[0];
        PropertyInfo backlinkProperty = backlinkType.GetProperty(backlinkPropertyName)!;

        Console.WriteLine($"Found backlink: {entityType.Name}.{entityProperty.Name} <- {backlinkType.Name}.{backlinkProperty.Name}");

        modelBuilder.Entity(entityType)
            .HasMany(backlinkType)
            .WithOne(backlinkPropertyName);
    }

    private static void DontMapIgnoredProperties(ModelBuilder modelBuilder, IMutableEntityType efEntityType, PropertyInfo entityProperty)
    {
        IgnoredAttribute ignored = entityProperty.GetCustomAttribute<IgnoredAttribute>();
        if (ignored == null) return;

        Console.WriteLine($"Ignoring {efEntityType.Name}->{entityProperty.Name}");
        modelBuilder.Entity(efEntityType.Name).Ignore(entityProperty.Name);
    }

    private static void FindPrimaryKey(IMutableEntityType efEntityType, PropertyInfo entityProperty, IMutableProperty efEntityProperty)
    {
        PrimaryKeyAttribute primaryKey = entityProperty.GetCustomAttribute<PrimaryKeyAttribute>();
        if (primaryKey == null) return;

        efEntityType.SetPrimaryKey(efEntityProperty);
    }
    
    /// <summary>
    /// Generate an Entity Framework model using hints from pre-existing Realm types.
    /// </summary>
    /// <param name="modelBuilder">The model to build.</param>
    /// <seealso cref="IgnoredAttribute"/>
    /// <seealso cref="BacklinkAttribute"/>
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        foreach (IMutableEntityType efEntityType in modelBuilder.Model.GetEntityTypes())
        {
            AddConversion<ObjectId, string>(modelBuilder, efEntityType, v => v.ToString(), v => new ObjectId(v));
            AddConversion<ObjectId?, string>(modelBuilder, efEntityType, v => v.ToString(), v => new ObjectId(v));
            
            // TODO: actually convert GameLocation
            AddConversion<GameLocation, string>(modelBuilder, efEntityType, v => v.ToString(), v => new GameLocation());
            
            Type entityType = Assembly.GetExecutingAssembly().GetTypes().First(t => t.FullName == efEntityType.Name);

            foreach (PropertyInfo entityProperty in entityType.GetProperties())
            {
                AddBacklinks(modelBuilder, entityType, entityProperty);
                DontMapIgnoredProperties(modelBuilder, efEntityType, entityProperty);
                
                IMutableProperty efEntityProperty = null;
                try
                {
                    efEntityProperty = efEntityType.GetProperty(entityProperty.Name);
                }
                catch (InvalidOperationException)
                {
                    // ignored
                }

                if (efEntityProperty != null)
                {
                    FindPrimaryKey(efEntityType, entityProperty, efEntityProperty);
                }
            }
        }
    }

    private DbSet<T> GetSetFromType<T>() where T : class
    {
        PropertyInfo property = this.GetType().GetProperties(BindingFlags.Instance | BindingFlags.NonPublic)
            .Where(p => p.CanRead)
            .First(p => p.PropertyType == typeof(DbSet<T>));

        return (DbSet<T>)property.GetValue(this);
    }
    
    public IDateTimeProvider Time { get; }
    #nullable enable
    public void AddSequentialObject<T>(T obj, IList<T>? list, Action? writtenCallback = null) where T : class, IRealmObject, ISequentialId
    {
        throw new NotImplementedException();
    }
    #nullable disable

    public void Refresh()
    {
        throw new NotImplementedException();
    }

    public IQueryable<T> All<T>() where T : class, IRealmObject
    {
        return this.GetSetFromType<T>().AsQueryable();
    }

    public void Write(Action func)
    {
        throw new NotImplementedException();
    }

    public void Add<T>(T obj, bool update = false) where T : class, IRealmObject
    {
        throw new NotImplementedException();
    }

    public void AddRange<T>(IEnumerable<T> list, bool update = false) where T : class, IRealmObject
    {
        throw new NotImplementedException();
    }

    public new void Remove<T>(T obj) where T : class, IRealmObject
    {
        throw new NotImplementedException();
    }

    public void RemoveRange<T>(IQueryable<T> list) where T : class, IRealmObject
    {
        throw new NotImplementedException();
    }

    public void RemoveAll<T>() where T : class, IRealmObject
    {
        throw new NotImplementedException();
    }
}