using System.Net;
using System.Text;
using Refresh.HttpServer.Documents;
using Refresh.HttpServer.Extensions;
using Refresh.HttpServer.Responses;

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

                    TestDocument document = new();
                    Response resp = document.GetResponse(context);
                    
                    context.Response.AddHeader("Content-Type", resp.ContentType.GetName());
                    context.Response.OutputStream.Write(resp.Data);
                    context.Response.StatusCode = (int)resp.StatusCode;
                }
                catch (Exception e)
                {
                    Console.WriteLine(e);

                    context.Response.AddHeader("Content-Type", ContentType.Plaintext.GetName());
                    #if DEBUG
                    context.Response.WriteString(e.ToString());
                    #else
                    context.Response.WriteString("Internal Server Error");
                    #endif
                    
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