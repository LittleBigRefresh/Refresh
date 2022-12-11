using System.Net;
using System.Net.Sockets;

namespace Refresh.HttpServer;

public static class Program
{
    public static void Main(string[] args)
    {
        Socket listener = new(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
        try
        {
            listener.Bind(new IPEndPoint(IPAddress.Loopback, 10060));
            listener.Listen(2^7);

            while (true)
            {
                Socket client = listener.Accept();
                Console.WriteLine("Accepted");
            }
        }
        catch(Exception e)
        {
            Console.WriteLine("Failed to accept connection: " + e);
        }
    }
}