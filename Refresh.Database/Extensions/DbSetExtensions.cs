#if POSTGRES
namespace Refresh.Database.Extensions;

public static class DbSetExtensions
{
    public static void RemoveRange<TClass>(this DbSet<TClass> set, Func<TClass, bool> predicate) where TClass : class
    {
        set.RemoveRange(set.Where(predicate));
    }
    
    public static void AddRange<TClass>(this DbSet<TClass> set, IEnumerable<TClass> range, bool update) where TClass : class
    {
        if(!update)
            set.AddRange(range);
        else
            set.UpdateRange(range);
    }
}

#endif