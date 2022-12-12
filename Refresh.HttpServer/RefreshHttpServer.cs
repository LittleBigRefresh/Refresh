using System.Diagnostics;
using System.Net;
using System.Reflection;
using Refresh.HttpServer.Endpoints;
using Refresh.HttpServer.Extensions;
using Refresh.HttpServer.Responses;

namespace Refresh.HttpServer;

public class RefreshHttpServer
{
    private readonly HttpListener _listener;
    private readonly List<Endpoint> _endpoints = new();
    
    public RefreshHttpServer(params Uri[] listenEndpoints)
    {
        this._listener = new HttpListener();

        foreach (Uri endpoint in listenEndpoints) 
            this._listener.Prefixes.Add(endpoint.ToString());
    }

    public void Start()
    {
        this._listener.Start();
        Task.Factory.StartNew(async () => await this.Block());
    }
    
    public async Task StartAndBlockAsync()
    {
        this._listener.Start();
        await this.Block();
    }

    private async Task Block()
    {
        while (true)
        {
            HttpListenerContext context = await this._listener.GetContextAsync();

            await Task.Factory.StartNew(() => { this.HandleRequest(context); });
        }
    }

    public void HandleRequest(HttpListenerContext context)
    {
        try
        {
            context.Response.AddHeader("Server", "Refresh");

            string? path = context.Request.Url?.AbsolutePath;
            
            Response? resp = this._endpoints
                .FirstOrDefault(d => d.Route == path)?
                .GetResponse(context);

            if (resp == null)
            {
                context.Response.AddHeader("Content-Type", ContentType.Plaintext.GetName());
                context.Response.StatusCode = (int)HttpStatusCode.NotFound;
                context.Response.WriteString("Not found: " + path);
            }
            else
            {
                context.Response.AddHeader("Content-Type", resp.Value.ContentType.GetName());
                context.Response.StatusCode = (int)resp.Value.StatusCode;
                context.Response.OutputStream.Write(resp.Value.Data);
            }
        }
        catch (Exception e)
        {
            Console.WriteLine(e);

            context.Response.AddHeader("Content-Type", ContentType.Plaintext.GetName());
            context.Response.StatusCode = 500;
            
#if DEBUG
            context.Response.WriteString(e.ToString());
#else
            context.Response.WriteString("Internal Server Error");
#endif
            throw;
        }
        finally
        {
            context.Response.Close();
        }
    }

    public void AddEndpoint<TDoc>() where TDoc : Endpoint
    {
        Type type = typeof(TDoc);
        Endpoint? doc = (Endpoint?)Activator.CreateInstance(type);
        Debug.Assert(doc != null);
        
        this._endpoints.Add(doc);
    }

    public void DiscoverEndpointsFromAssembly(Assembly assembly)
    {
        throw new NotImplementedException();
    }
}