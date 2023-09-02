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
		CommandService service = new(new LoggerContainer<BunkumContext>());
		
		Assert.That(service.ParseCommand("/parse test"), Is.EqualTo(new Command("parse", "test")));
		Assert.That(service.ParseCommand("/noargs"), Is.EqualTo(new Command("noargs", null)));
		Assert.That(service.ParseCommand("/noargs "), Is.EqualTo(new Command("noargs", null)));
	}

	[Test]
	public void NoSlashThrows()
	{
		CommandService service = new(new LoggerContainer<BunkumContext>());
		
		Assert.That(() => service.ParseCommand("parse test"), Throws.Exception);
	}

	[Test]
	public void BlankCommandThrows()
	{
		CommandService service = new(new LoggerContainer<BunkumContext>());
		
		Assert.That(() => service.ParseCommand("/ test"), Throws.Exception);
	}
}
