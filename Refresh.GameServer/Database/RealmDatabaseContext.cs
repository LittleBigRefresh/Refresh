using System.Diagnostics.CodeAnalysis;
using Realms;
using Bunkum.HttpServer.Database;

namespace Refresh.GameServer.Database;

[SuppressMessage("ReSharper", "InconsistentlySynchronizedField")]
public partial class RealmDatabaseContext : IDatabaseContext
{
    private readonly Realm _realm;

    internal RealmDatabaseContext(Realm realm)
    {
        this._realm = realm;
    }

    public void Dispose()
    {
        //NOTE: we dont dispose the realm here, because the same thread may use it again, so we just `Refresh()` it
        this._realm.Refresh();
    }
    
    private static readonly object IdLock = new();
    // ReSharper disable once SuggestBaseTypeForParameter
    private void AddSequentialObject<T>(T obj, IList<T>? list = null, Action? writtenCallback = null) where T : IRealmObject, ISequentialId
    {
        lock (IdLock)
        {
            this._realm.Write(() =>
            {
                int newId = this._realm.All<T>().Count() + 1;

                obj.SequentialId = newId;

                this._realm.Add(obj);
                if(list == null) writtenCallback?.Invoke();
            });
        }
        
        // Two writes are necessary here for some unexplainable reason
        // We've already set a SequentialId so we can be outside the lock at this stage
        if (list != null)
        {
            this._realm.Write(() =>
            {
                list.Add(obj);
                writtenCallback?.Invoke();
            });
        }
    }

    private void AddSequentialObject<T>(T obj, Action? writtenCallback = null) where T : IRealmObject, ISequentialId 
        => this.AddSequentialObject(obj, null, writtenCallback);
    
    private static long GetTimestampMilliseconds() => DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();
    
    private static long GetTimestampSeconds() => DateTimeOffset.UtcNow.ToUnixTimeSeconds();
}