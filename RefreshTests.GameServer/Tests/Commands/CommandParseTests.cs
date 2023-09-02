using Bunkum.HttpServer;
using NotEnoughLogs;
using Refresh.GameServer.Services;
using Refresh.GameServer.Types.Commands;

namespace RefreshTests.GameServer.Tests.Commands;

public class CommandParseTests : GameServerTest
{
	[Test]
	public void ParsingTest()
	{
		LoggerContainer<BunkumContext> logger = new();
		CommandService service = new(logger, new MatchService(logger));

		Assert.That(service.ParseCommand("/parse test"), Is.EqualTo(new Command("parse", "test")));
		Assert.That(service.ParseCommand("/noargs"), Is.EqualTo(new Command("noargs", null)));
		Assert.That(service.ParseCommand("/noargs "), Is.EqualTo(new Command("noargs", null)));
	}

	[Test]
	public void NoSlashThrows()
	{
		LoggerContainer<BunkumContext> logger = new();
		CommandService service = new(logger, new MatchService(logger));

		Assert.That(() => service.ParseCommand("parse test"), Throws.Exception);
	}

	[Test]
	public void BlankCommandThrows()
	{
		LoggerContainer<BunkumContext> logger = new();
		CommandService service = new(logger, new MatchService(logger));

		Assert.That(() => service.ParseCommand("/ test"), Throws.Exception);
	}
}
