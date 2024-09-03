using System.Buffers.Binary;
using System.Collections.Concurrent;
using System.Net;
using System.Net.Sockets;
using System.Runtime.InteropServices;
using System.Security.Cryptography;
using System.Text;
using NotEnoughLogs;
using Refresh.Common.Helpers;
using Refresh.PresenceServer.ApiClient;
using Refresh.PresenceServer.Server.Config;

namespace Refresh.PresenceServer.Server;

public class PresenceServer
{
    private readonly PresenceServerConfig _config;
    private readonly Logger _logger;
    private readonly RefreshPresenceApiClient _apiClient;
    private readonly int[] _key;

    private readonly HashSet<GameClient> _clients = [];
    private readonly ConcurrentDictionary<string, GameClient> _authenticatedClients = [];
    private readonly ConcurrentQueue<GameClient> _toRemove = [];

    /// <summary>
    /// The timeout in milliseconds
    /// </summary>
    private const int Timeout = 30 * 1000;

    private readonly CancellationTokenSource _stopToken = new();
    
    public PresenceServer(PresenceServerConfig config, Logger logger, RefreshPresenceApiClient apiClient)
    {
        this._config = config;
        this._logger = logger;
        this._apiClient = apiClient;

        const string expectedKeyHash = "343e7cd17cfcc476633570c0f753aa8a";
        string keyHash = HexHelper.BytesToHexString(MD5.HashData(config.Key));

        if (keyHash != expectedKeyHash)
            throw new Exception($"Key hash is invalid! Expected {expectedKeyHash}, got {keyHash}. Ensure key is 16 bytes in length and was copied correctly.");

        this._key = MemoryMarshal.Cast<byte, int>(config.Key).ToArray();

        // Endian swap the BE integers to LE
        BinaryPrimitives.ReverseEndianness(this._key, this._key);
    }

    public void Start()
    {
        this._logger.LogInfo(PresenceCategory.Startup, "Starting up presence server.");

        Task.Factory.StartNew(this.Block, TaskCreationOptions.LongRunning);
    }
    
    public async Task Block()
    {
        using TcpListener listener = new(IPAddress.Parse(this._config.ListenHost), this._config.ListenPort);

        listener.Start();

        this._logger.LogInfo(PresenceCategory.Startup, $"Presence server listening at {this._config.ListenHost}:{this._config.ListenPort}");

        while (!this._stopToken.IsCancellationRequested)
        {
            // Remove any removed clients
            while (this._toRemove.TryDequeue(out GameClient? removed))
            {   
                if(removed.AuthToken != null)
                    _ = Task.Run(async () =>
                    {
                        await this._apiClient.InformDisconnection(removed.AuthToken);
                        
                        this._authenticatedClients.TryRemove(removed.AuthToken, out _);
                    });
                        
                if(removed.TcpClient.Connected)
                    removed.TcpClient.Close();
                
                this._clients.Remove(removed);
            }
            
            // If there is a client waiting, accept their connection
            if (listener.Pending())
            {
                try
                {
                    TcpClient tcpClient = await listener.AcceptTcpClientAsync();

                    this._logger.LogInfo(PresenceCategory.Connections, "Accepted client.");

                    // Dont linger at all
                    tcpClient.LingerState = new LingerOption(false, 0);

                    // Just timeout any slow clients, these packets are max ~133 bytes, it shouldn't take this long
                    tcpClient.ReceiveTimeout = 1000;
                    tcpClient.SendTimeout = 1000;

                    // The packets are *tiny* in size, so lets cut down on the buffer size 
                    tcpClient.SendBufferSize = 256;
                    tcpClient.ReceiveBufferSize = 256;

                    GameClient gameClient = new(tcpClient);
                    this._clients.Add(gameClient);
                    this._logger.LogInfo(PresenceCategory.Connections, "Client {0} connected.", gameClient.IpAddress);
                }
                catch(Exception ex)
                {
                    this._logger.LogWarning(PresenceCategory.Connections, "Failed to accept client. Reason {0}", ex);
                }
            }

            foreach (GameClient client in this._clients)
            {
                long now = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();

                // If we aren't connected, auth took too long, or there hasn't been a ping in too long, remove the client from the list
                if (!client.TcpClient.Connected ||
                    (client.AuthToken == null && now - client.ConnectionTime > Timeout) ||
                    now - client.LastPing > Timeout)
                {
                    this._logger.LogInfo(PresenceCategory.Connections, "Client disconnected.");
                    client.TcpClient.Dispose();

                    this._toRemove.Enqueue(client);

                    continue;
                }

                client.ReceiveTask = Task.Factory.StartNew(this.ReceiveTask, client);
                if (client.SlotToSend != 0)
                {
                    int slotId = Interlocked.Exchange(ref client.SlotToSend, 0);

                    client.SendTask = Task.Run(() =>
                    {
                        try
                        {
                            Span<byte> sendData = stackalloc byte[sizeof(int) * 4];
                            
                            BinaryPrimitives.WriteInt32BigEndian(sendData, 0x01);
                            BinaryPrimitives.WriteInt32BigEndian(sendData[4..], slotId);

                            BinaryPrimitives.WriteInt32BigEndian(sendData[8..], 0x01);
                            BinaryPrimitives.WriteInt32BigEndian(sendData[12..], slotId);

                            // Encrypt the first slot once
                            ResourceHelper.XxteaEncrypt(sendData[..8], this._key);

                            // Encrypt the second slot twice
                            ResourceHelper.XxteaEncrypt(sendData[8..], this._key);
                            ResourceHelper.XxteaEncrypt(sendData[8..], this._key);
                            
                            client.TcpClient.Client.Send(sendData);
                            
                            this._logger.LogInfo(PresenceCategory.Connections, "Sending slot ID {0} to user {1}", slotId, client.IpAddress);
                        }
                        catch(Exception ex)
                        {
                            this._logger.LogWarning(PresenceCategory.Connections,
                                "Failed to send packet data to {0}, reason {1}", client.IpAddress, ex);

                            // If we get any error, just disconnect the client
                            client.TcpClient.Close();
                        }
                    });
                }
            }
                        
            IEnumerable<Task> receiveTasks = this._clients.Select(c => c.ReceiveTask).Where(t => t != null)!;
            IEnumerable<Task> sendTasks = this._clients.Select(c => c.SendTask).Where(t => t != null)!;
            
#if NET9_0_OR_GREATER
#error Please remove the ToArray call here!
#endif
            
            // Wait for all receive tasks to finish
            Task.WaitAll(receiveTasks.Concat(sendTasks).ToArray());

            // Clear out the receive task references
            foreach (GameClient client in this._clients) 
                client.ReceiveTask = null;

            // Sleep for 1 second
            await Task.Delay(1000);
        }
    }

    private int EncryptSlotId(int slotId)
    {
        Span<byte> span = stackalloc byte[sizeof(int)];

        BinaryPrimitives.WriteInt32BigEndian(span, slotId);

        ResourceHelper.XxteaEncrypt(span, this._key);
        
        return BinaryPrimitives.ReadInt32BigEndian(span);
    }
    
    private async Task ReceiveTask(object? state)
    {
        GameClient gameClient = (GameClient)state!;
        
        try
        {
            if (gameClient.TcpClient.Available == 0)
                return;

#if NET9_0_OR_GREATER
#error Please clean this mess to use Span<T>!!!
#endif

            int readAmount = await gameClient.TcpClient.Client.ReceiveAsync(gameClient.ReceiveBuffer);
            if (readAmount == 0)
                return;

            byte[] read = gameClient.ReceiveBuffer[..readAmount];

            switch (read[0])
            {
                // login packet
                case 0x4c when read[1] == 0x0d && read[2] == 0x0a:
                {
                    // Decrypt the body of the packet
                    ResourceHelper.XxteaDecrypt(read.AsSpan()[3..][..128], this._key);

                    // Convert the auth token back into a string
                    string authToken = Encoding.UTF8.GetString(read.AsSpan()[3..][..128]["MM_AUTH=".Length..]).TrimEnd('\0');

                    this._logger.LogInfo(PresenceCategory.Authentication, "{0} logged in", gameClient.IpAddress,
                        authToken);

                    // Set the user's auth token
                    gameClient.AuthToken = authToken;

                    this._authenticatedClients[authToken] = gameClient;

                    _ = Task.Run(async () =>
                    {
                        bool success = await this._apiClient.InformConnection(authToken);
                        
                        if(!success)
                            this._toRemove.Enqueue(gameClient);
                    });
                    
                    break;
                }
                // keepalive packet
                case 0x0d when read[1] == 0x0a && read.Length == 2:
                    this._logger.LogDebug(PresenceCategory.Connections, "Keepalive from {0}", gameClient.IpAddress);

                    break;
                default:
                    this._logger.LogWarning(PresenceCategory.Connections,
                        "Unknown packet from {0}, treating as basic keepalive", gameClient.IpAddress);

                    break;
            }

            // Update the last ping
            gameClient.LastPing = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();
        }
        catch (Exception ex)
        {
            this._logger.LogWarning(PresenceCategory.Connections,
                "Failed to receive packet data from {0}, reason {1}", gameClient.IpAddress, ex);

            // If we get any error, just disconnect the client
            gameClient.TcpClient.Close();
        }
    }

    public void Stop()
    {
        this.StopAsync().Wait();
    }
    
    public async Task StopAsync()
    {
        await this._stopToken.CancelAsync();
    }

    /// <summary>
    /// Tells the client to play a level
    /// </summary>
    /// <param name="token">The client's token</param>
    /// <param name="id">The level ID to tell them to play</param>
    /// <returns></returns>
    public bool PlayLevel(string token, int id)
    {
        if (!this._authenticatedClients.TryGetValue(token, out GameClient? client))
        {
            this._logger.LogWarning(PresenceCategory.Connections, "Couldn't find client from server");
            return false;
        }
        
        client.SlotToSend = id;

        return true;
    }
}