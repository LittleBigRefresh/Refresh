using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using MongoDB.Bson;

#if POSTGRES

namespace Refresh.Database.Compatibility;

public class ObjectIdConverter : ValueConverter<ObjectId, string>
{
    [UsedImplicitly]
    public ObjectIdConverter() : base(
        v => v.ToString(),
        v => ObjectId.Parse(v))
    {}
}
#endif