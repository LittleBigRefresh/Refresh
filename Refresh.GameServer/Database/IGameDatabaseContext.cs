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
        where T : class, IRealmObject, ISequentialId;

    private void AddSequentialObject<T>(T obj, Action? writtenCallback) where T : class, IRealmObject, ISequentialId 
        => this.AddSequentialObject(obj, null, writtenCallback);
    
    private void AddSequentialObject<T>(T obj) where T : class, IRealmObject, ISequentialId 
        => this.AddSequentialObject(obj, null, null);

    public void Refresh();

    protected IQueryable<T> All<T>() where T : class, IRealmObject;
    protected void Write(Action func);
    protected void Add<T>(T obj, bool update = false) where T : class, IRealmObject;
    protected void AddRange<T>(IEnumerable<T> list, bool update = false) where T : class, IRealmObject;
    protected void Remove<T>(T obj) where T : class, IRealmObject;
    protected void RemoveRange<T>(IQueryable<T> list) where T : class, IRealmObject;
    protected void RemoveAll<T>() where T : class, IRealmObject;
}