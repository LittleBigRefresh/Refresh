// This class contains common classes and types that can't exist in both Realm and Postgres builds.
// Along with any other Realm compatibility steps, this will be removed when Postgres is fully complete.

// ReSharper disable once CheckNamespace
namespace Refresh.Database.Compatibility;

#if POSTGRES
using System.ComponentModel.DataAnnotations.Schema;

public interface IRealmObject;
[Obsolete("IEmbeddedObject is not supported in Postgres models")]
public interface IEmbeddedObject;

[AttributeUsage(AttributeTargets.Property)]
public class IgnoredAttribute : Attribute;

[AttributeUsage(AttributeTargets.Property)]
public class PrimaryKeyAttribute : Attribute;

[AttributeUsage(AttributeTargets.Property)]
public class IndexedAttribute : Attribute
{
    public IndexedAttribute()
    {}

    public IndexedAttribute(IndexType type)
    {
        _ = type;
    }
}

public enum IndexType
{
    FullText,
}

public static class QueryMethods
{
    public static bool FullTextSearch(string text, string query)
    {
        // TODO: this NEEDS to be actual, full, real full text search
        // the postgres library for EF doesn't support this directly, so use this hacky workaround for now
        return text.Contains(query);
    }
}

#else
[AttributeUsage(AttributeTargets.Property)]
public class KeyAttribute : Attribute;

[AttributeUsage(AttributeTargets.Property)]
public class NotMappedAttribute : Attribute;

[AttributeUsage(AttributeTargets.Class)]
public class IndexAttribute : Attribute
{
    public IndexAttribute(params string[] parameterNames)
    {
        _ = parameterNames;
    }
}
#endif