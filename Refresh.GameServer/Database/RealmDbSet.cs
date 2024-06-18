using System.Collections;
using System.Linq.Expressions;
using JetBrains.Annotations;
using Realms;

namespace Refresh.GameServer.Database;

public class RealmDbSet<T> : IQueryable<T> where T : IRealmObject
{
    private readonly Realm _realm;
    private IQueryable<T> Queryable => this._realm.All<T>();

    public RealmDbSet(Realm realm)
    {
        this._realm = realm;
    }

    [MustDisposeResource] public IEnumerator<T> GetEnumerator() => this.Queryable.GetEnumerator();
    [MustDisposeResource] IEnumerator IEnumerable.GetEnumerator() => this.GetEnumerator();

    public Type ElementType => typeof(T);
    public Expression Expression => this.Queryable.Expression;
    public IQueryProvider Provider => this.Queryable.Provider;
    
    // functions for compatibility with EF
    public void Add(T obj)
    {
        this._realm.Add(obj);
    }

    public void AddRange(IEnumerable<T> objs, bool update = false)
    {
        this._realm.Add(objs, update);
    }

    public void Remove(T obj)
    {
        this._realm.Remove(obj);
    }
    
    public void RemoveRange(IQueryable<T> objs)
    {
        this._realm.RemoveRange(objs);
    }
}