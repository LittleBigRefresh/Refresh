#if !POSTGRES

using System.Collections;
using System.Linq.Expressions;
using JetBrains.Annotations;

namespace Refresh.Database;

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

    // TODO: update = true will need extra consideration for EF. for now just let consumers specify
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
    
    public void RemoveRange(IEnumerable<T> objs)
    {
        foreach (T obj in objs) 
            this._realm.Remove(obj);
    }

    public void RemoveRange(Expression<Func<T, bool>> predicate)
    {
        this._realm.RemoveRange(this._realm.All<T>().Where(predicate));
    }
}

#endif