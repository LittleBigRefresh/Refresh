namespace Refresh.HttpServer.Authentication.Dummy;

public class DummyUser : IUser
{
    public ulong UserId { get; set; } = 1;
    public string Username { get; set; } = "Dummy";
}