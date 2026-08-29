using Bunkum.Core;
using Bunkum.Core.Endpoints;
using Bunkum.Core.Responses;
using Bunkum.Listener.Protocol;
using Bunkum.Protocols.Http;
using Refresh.Common;
using Refresh.Core.Services;

namespace RefreshTests.GameServer.AipiServer;

public class TestAipiEndpoints : EndpointGroup
{
    [HttpEndpoint("/"), Authentication(false)]
    public string TestAipi(RequestContext context)
    {
        return "AIPI scanning service";
    }
    
    [HttpEndpoint("/eva/predict", HttpMethods.Post, ContentType.BinaryData), Authentication(false)]
    public Response ScanImage(RequestContext context, Stream body)
    {
        // check if we're secretly requesting a failure
        string? forcedFailureReason = context.RequestHeaders.Get("X-ForcedFailureReason");
        if (forcedFailureReason != null)
        {
            return new(new AipiResponse<Dictionary<string, float>>
            {
                Success = false,
                Reason = forcedFailureReason,
                Data = null,
            }, ContentType.Json, NotAcceptable);
        }
        
        string? thresholdStr = context.QueryString.Get("threshold");
        if (thresholdStr == null || !float.TryParse(thresholdStr, out float threshold))
        {
            context.Logger.LogTrace(RefreshContext.Aipi, $"Threshold not provided or unparseable, falling back to 0.0");
            threshold = 0.0f;
        }
        
        Dictionary<string, float> tags = [];
        if (threshold <= 67) tags.Add("sixSeven", 67.0f);
        if (threshold <= 123.456) tags.Add("hi", 123.456f);
        
        return new(new AipiResponse<Dictionary<string, float>>
        {
            Success = true,
            Reason = null,
            Data = tags,
        }, ContentType.Json);
    }
}