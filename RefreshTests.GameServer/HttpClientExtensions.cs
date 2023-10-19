namespace RefreshTests.GameServer;

public static class HttpClientExtensions
{
    public static T WaitResult<T>(this Task<T> message)
    {
        message.Wait();
        return message.Result;
    }
}