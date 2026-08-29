using NotEnoughLogs;
using Refresh.Core.Configuration;
using Refresh.Core.Services;

namespace RefreshTests.GameServer.GameServer.Services;

public class TestAipiService : AipiService
{
    public TestAipiService(Logger logger, IntegrationConfig config, ImportService import, DiscordStaffService discord, HttpClient client) : base(logger, config, import, discord)
    {
        this._client = client;
    }
}