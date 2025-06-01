using Refresh.Interfaces.APIv3.Endpoints.ApiTypes;

namespace RefreshTests.GameServer.Extensions;

public static class ApiResponseExtensions
{
    public static void AssertErrorIsEqual<TData>(this ApiResponse<TData> response, ApiError error) where TData : class
    {
        Assert.That(response.Error, Is.Not.Null);
        Assert.That(response.Error!.Name, Is.EqualTo(error.Name));
        Assert.That(response.Error!.StatusCode, Is.EqualTo(error.StatusCode));
        Assert.That(response.Error!.Message, Is.EqualTo(error.Message));
    }
}