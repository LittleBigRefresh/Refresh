using System.Diagnostics.CodeAnalysis;
using Realms;
using Bunkum.RealmDatabase;
using Refresh.GameServer.Time;

namespace Refresh.GameServer.Database;

[SuppressMessage("ReSharper", "InconsistentlySynchronizedField")]
public partial class GameDatabaseContext : RealmDatabaseContext
{
    private static readonly object IdLock = new();

    private readonly IDateTimeProvider _time;

    public GameDatabaseContext()
    {
        this._time = new SystemDateTimeProvider();
    }

    public GameDatabaseContext(IDateTimeProvider time)
    {
        this._time = time;
    }

    // ReSharper disable once SuggestBaseTypeForParameter
    private void AddSequentialObject<T>(T obj, IList<T>? list, Action? writtenCallback = null) where T : IRealmObject, ISequentialId
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

    private void AddSequentialObject<T>(T obj, Action? writtenCallback) where T : IRealmObject, ISequentialId 
        => this.AddSequentialObject(obj, null, writtenCallback);
    
    private void AddSequentialObject<T>(T obj) where T : IRealmObject, ISequentialId 
        => this.AddSequentialObject(obj, null, null);
}