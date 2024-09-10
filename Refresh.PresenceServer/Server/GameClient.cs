using System.Net.Sockets;

namespace Refresh.PresenceServer.Server;

public class GameClient
{
    public GameClient(TcpClient tcpClient)
    {
        this.TcpClient = tcpClient;

        this.IpAddress = this.TcpClient.Client.RemoteEndPoint!.Serialize().ToString();
    }

    /// <summary>
    /// The TCP client the user is using
    /// </summary>
    public TcpClient TcpClient;

    /// <summary>
    /// A per-client read buffer
    /// </summary>
    public readonly byte[] ReceiveBuffer = new byte[512];
    
    /// <summary>
    /// The last ping the user sent to the server
    /// </summary>
    public long LastPing = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();

    /// <summary>
    /// The auth token the client has sent to the server
    /// </summary>
    public string? AuthToken = null;

    /// <summary>
    /// The time the client connected to the server
    /// </summary>
    public long ConnectionTime = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();

    public readonly string IpAddress;
    
    public Task? ReceiveTask = null;
    public Task? SendTask = null;
    
    /// <summary>
    /// The slot to send to the client at the next opportunity
    /// </summary>
    public int SlotToSend = 0;
}