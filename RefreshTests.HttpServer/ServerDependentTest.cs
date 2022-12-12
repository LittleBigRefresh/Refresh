using System.Net;
using System.Net.NetworkInformation;
using JetBrains.Annotations;
using Refresh.HttpServer;

namespace RefreshTests.HttpServer;

public class ServerDependentTest
{
    private static int _lowestPort = 32768;
    
    [Pure]
    private protected (RefreshHttpServer, HttpClient) Setup()
    {
        // find free port to use for listener
        // we should probably find a way to send requests directly to the HttpListener, but this works for now
        // adapted from https://stackoverflow.com/a/53936819
        IPGlobalProperties properties = IPGlobalProperties.GetIPGlobalProperties();
        IPEndPoint[] listeners = properties.GetActiveTcpListeners();
        
        foreach (int port in Enumerable.Range(_lowestPort, 65536))
        {
            int? openPort = listeners.Select(item => item.Port).FirstOrDefault(p => p != port);
            
            if (openPort != null) continue;
            _lowestPort = openPort!.Value;
            break;
        }

        Uri uri = new($"http://127.0.0.1:{_lowestPort}/");
        _lowestPort++;

        RefreshHttpServer server = new(uri);
        server.Start();

        HttpClient client = new();
        client.BaseAddress = uri;

        return (server, client);
    }
}