using System.Diagnostics.CodeAnalysis;
using Bunkum.Core.Database;
using Realms;
using Bunkum.RealmDatabase;
using Refresh.GameServer.Time;
using Refresh.GameServer.Types;

namespace Refresh.GameServer.Database;

[SuppressMessage("ReSharper", "InconsistentlySynchronizedField")]
public partial interface IGameDatabaseContext : IDatabaseContext
{
    protected IDateTimeProvider Time { get; }

    // ReSharper disable once SuggestBaseTypeForParameter
    protected void AddSequentialObject<T>(T obj, IList<T>? list, Action? writtenCallback = null)
        where T : IRealmObject, ISequentialId;

    private void AddSequentialObject<T>(T obj, Action? writtenCallback) where T : IRealmObject, ISequentialId 
        => this.AddSequentialObject(obj, null, writtenCallback);
    
    private void AddSequentialObject<T>(T obj) where T : IRealmObject, ISequentialId 
        => this.AddSequentialObject(obj, null, null);

    protected IQueryable<T> All<T>() where T : IRealmObject;
    protected void Write(Action func);
    protected void Add<T>(T obj, bool update = false) where T : IRealmObject;
    protected void AddRange<T>(IEnumerable<T> list, bool update = false) where T : IRealmObject;
    protected void Remove<T>(T obj) where T : IRealmObject;
    protected void RemoveRange<T>(IQueryable<T> list) where T : IRealmObject;
    protected void RemoveAll<T>() where T : IRealmObject;
}