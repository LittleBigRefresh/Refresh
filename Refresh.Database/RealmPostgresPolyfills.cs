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