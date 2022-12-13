namespace Refresh.HttpServer.Authentication;

#nullable disable

public interface IUser
{
    public ulong UserId { get; set; }
    public string Username { get; set; }
}