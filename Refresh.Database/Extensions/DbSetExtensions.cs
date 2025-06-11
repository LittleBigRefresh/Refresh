#if POSTGRES
namespace Refresh.Database.Extensions;

public static class DbSetExtensions
{
    public static void RemoveRange<TClass>(this DbSet<TClass> set, Func<TClass, bool> predicate) where TClass : class
    {
        set.RemoveRange(set.Where(predicate));
    }
    
    [Obsolete("Remove update parameter when postgres is removed")]
    public static void AddRange<TClass>(this DbSet<TClass> set, IEnumerable<TClass> range, bool update) where TClass : class
    {
        set.AddRange(range);
        _ = update;
    }
}

#endif