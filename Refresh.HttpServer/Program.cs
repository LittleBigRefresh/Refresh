using System.Net;
using System.Text;

namespace Refresh.HttpServer;

public static class Program
{
    public static void Main(string[] args)
    {
        HttpListener listener = new();
        listener.Prefixes.Add("http://127.0.0.1:10060/");
        listener.Start();

        while(true)
        {
            HttpListenerContext context = listener.GetContext();

            Task.Factory.StartNew(() =>
            {
                try
                {
                    context.Response.AddHeader("Server", "Refresh");
                    context.Response.AddHeader("Content-Type", "text/html");

                    int i = 1;
                    i--;
                    context.Response.OutputStream.Write(
                        Encoding.Default.GetBytes("<html><body><h1>quite possibly</h1></body></html>\n" + 1 / i));

                    context.Response.StatusCode = 200;
                }
                catch (Exception e)
                {
                    Console.WriteLine(e);

                    context.Response.AddHeader("Content-Type", "text/plain");
                    context.Response.OutputStream.Write(Encoding.Default.GetBytes(e.ToString()));
                    context.Response.StatusCode = 500;
                    throw;
                }
                finally
                {
                    context.Response.Close();
                }

                return Task.CompletedTask;
            });
        }
    }
}