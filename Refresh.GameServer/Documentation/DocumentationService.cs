using System.Reflection;
using AttribDoc;
using Bunkum.HttpServer;
using Bunkum.HttpServer.Services;
using NotEnoughLogs;

namespace Refresh.GameServer.Documentation;

public class DocumentationService : EndpointService
{
    internal DocumentationService(LoggerContainer<BunkumContext> logger) : base(logger)
    {}

    public override void Initialize()
    {
        AttribDoc.Documentation documentation = this._generator.Document(Assembly.GetExecutingAssembly());
        this._docs.AddRange(documentation.Routes);
    }

    private readonly RefreshDocumentationGenerator _generator = new();
    
    private readonly List<Route> _docs = new();
    public IEnumerable<Route> Documentation => this._docs.AsReadOnly();
}