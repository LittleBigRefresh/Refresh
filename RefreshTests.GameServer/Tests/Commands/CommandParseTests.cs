using NotEnoughLogs;
using Refresh.Core.Configuration;
using Refresh.Core.Services;
using Refresh.Core.Types.Commands;
using RefreshTests.GameServer.Logging;

namespace RefreshTests.GameServer.Tests.Commands;

public class CommandParseTests : GameServerTest
{
#pragma warning disable NUnit2045
	private void ParseTest(CommandService service, ReadOnlySpan<char> input, ReadOnlySpan<char> expectedName, ReadOnlySpan<char> expectedArguments)
	{
		CommandInvocation command = service.ParseCommand(input);
		Assert.That(command.Name.SequenceEqual(expectedName), Is.True, $"Expected '{command.Name.ToString()}' to equal '{expectedName.ToString()}'");
		Assert.That(command.Arguments.SequenceEqual(expectedArguments), Is.True, $"Expected '{command.Arguments.ToString()}' to equal '{expectedArguments.ToString()}'");
	}
#pragma warning restore NUnit2045
	
	[Test]
	public void ParsingTest()
	{
		using Logger logger = new([new NUnitSink()]);
		CommandService service = new(logger, new MatchService(logger), new PlayNowService(logger, new PresenceService(logger, new IntegrationConfig())));
        
		ParseTest(service, "/parse test", "parse", "test");
		ParseTest(service, "/noargs", "noargs", "");
		ParseTest(service, "/noargs ", "noargs", "");
	}

	[Test]
	public void NoSlashThrows()
	{
		using Logger logger = new([new NUnitSink()]);
		CommandService service = new(logger, new MatchService(logger), new PlayNowService(logger, new PresenceService(logger, new IntegrationConfig())));

		Assert.That(() => _ = service.ParseCommand("parse test"), Throws.InstanceOf<FormatException>());
	}

	[Test]
	public void BlankCommandThrows()
	{
		using Logger logger = new([new NUnitSink()]);
		CommandService service = new(logger, new MatchService(logger), new PlayNowService(logger, new PresenceService(logger, new IntegrationConfig())));

		Assert.That(() => _ = service.ParseCommand("/ test"), Throws.InstanceOf<FormatException>());
	}
}
