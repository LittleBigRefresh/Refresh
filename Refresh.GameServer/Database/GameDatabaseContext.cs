using System.Diagnostics.CodeAnalysis;
using Realms;
using Bunkum.RealmDatabase;
using Refresh.GameServer.Time;
using Refresh.GameServer.Types;

namespace Refresh.GameServer.Database;

[SuppressMessage("ReSharper", "InconsistentlySynchronizedField")]
public partial class GameDatabaseContext : RealmDatabaseContext
{
    private static readonly object IdLock = new();

    private readonly IDateTimeProvider _time;

    internal GameDatabaseContext(IDateTimeProvider time)
    {
        this._time = time;
    }

    private int GetOrCreateSequentialId<T>() where T : IRealmObject, ISequentialId
    {
        string name = typeof(T).Name;

        SequentialIdStorage? storage = this._realm.All<SequentialIdStorage>()
            .FirstOrDefault(s => s.TypeName == name);
        
        if (storage != null) return storage.SequentialId++;
        
        storage = new SequentialIdStorage
        {
            TypeName = name,
            SequentialId = this._realm.All<T>().Count() * 2, // skip over a bunch of ids incase table is broken
        };

        // no need to do write block, this should only be called in a write transaction
        this._realm.Add(storage);

        return storage.SequentialId;
    }

    // ReSharper disable once SuggestBaseTypeForParameter
    private void AddSequentialObject<T>(T obj, IList<T>? list, Action? writtenCallback = null) where T : IRealmObject, ISequentialId
    {
        lock (IdLock)
        {
            this._realm.Write(() =>
            {
                int newId = this.GetOrCreateSequentialId<T>() + 1;

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