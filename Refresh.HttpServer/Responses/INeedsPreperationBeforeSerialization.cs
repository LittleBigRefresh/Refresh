namespace Refresh.HttpServer.Responses;

public interface INeedsPreparationBeforeSerialization
{
    public void PrepareForSerialization();
}