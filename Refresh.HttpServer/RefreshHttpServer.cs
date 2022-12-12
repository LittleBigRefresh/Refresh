using System.Net;
using Refresh.HttpServer.Documents;
using Refresh.HttpServer.Extensions;
using Refresh.HttpServer.Responses;

namespace Refresh.HttpServer;

public class RefreshHttpServer
{
    private readonly HttpListener _listener;
    
    public RefreshHttpServer(params Uri[] listenEndpoints)
    {
        this._listener = new HttpListener();

        foreach (Uri endpoint in listenEndpoints) 
            this._listener.Prefixes.Add(endpoint.ToString());
    }

    public void Start() => this.StartAsync().Wait();
    public async Task StartAsync()
    {
        this._listener.Start();

        while(true)
        {
            HttpListenerContext context = await this._listener.GetContextAsync();

            await Task.Factory.StartNew(() =>
            {
                HandleRequest(context);
            });
        }
    }

    private static void HandleRequest(HttpListenerContext context)
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
    }
}