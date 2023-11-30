using Bunkum.RealmDatabase;
using Realms;
using Refresh.GameServer.Time;
using Refresh.GameServer.Types;

namespace Refresh.GameServer.Database.Realm;

public class RealmGameDatabaseContext : RealmDatabaseContext, IGameDatabaseContext
{
    private static readonly object IdLock = new();
    public IDateTimeProvider Time { get; }

    private int GetOrCreateSequentialId<T>() where T : IRealmObject, ISequentialId
    {
        string name = typeof(T).Name;

        SequentialIdStorage? storage = this.All<SequentialIdStorage>()
            .FirstOrDefault(s => s.TypeName == name);

        if (storage != null)
        {
            storage.SequentialId += 1;
            return storage.SequentialId;
        }
        
        storage = new SequentialIdStorage
        {
            TypeName = name,
            SequentialId = this.All<T>().Count() * 2, // skip over a bunch of ids incase table is broken
        };

        // no need to do write block, this should only be called in a write transaction
        this._realm.Add(storage);

        return storage.SequentialId;
    }
    
    public void AddSequentialObject<T>(T obj, IList<T>? list, Action? writtenCallback = null) where T : IRealmObject, ISequentialId
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

    public IQueryable<T> All<T>() where T : IRealmObject => this._realm.All<T>();
    public void Write(Action func) => this._realm.Write(func);
    public void Add<T>(T obj, bool update = false) where T : IRealmObject => this._realm.Add(obj, update);
    public void AddRange<T>(IEnumerable<T> list, bool update = false) where T : IRealmObject => this._realm.Add(list, update);
    public void Remove<T>(T obj) where T : IRealmObject => this._realm.Remove(obj);
    public void RemoveRange<T>(IQueryable<T> list) where T : IRealmObject => this._realm.RemoveRange(list);
    public void RemoveAll<T>() where T : IRealmObject => this._realm.RemoveAll<T>();

    internal RealmGameDatabaseContext(IDateTimeProvider time)
    {
        this.Time = time;
    }
}