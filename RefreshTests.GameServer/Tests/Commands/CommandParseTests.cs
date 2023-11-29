using NotEnoughLogs;
using Refresh.GameServer.Services;
using Refresh.GameServer.Types.Commands;
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
		using Logger logger = new(new []{ new NUnitSink() });
		CommandService service = new(logger, new MatchService(logger), new LevelListOverrideService(logger));
        
		ParseTest(service, "/parse test", "parse", "test");
		ParseTest(service, "/noargs", "noargs", "");
		ParseTest(service, "/noargs ", "noargs", "");
	}

	[Test]
	public void NoSlashThrows()
	{
		using Logger logger = new(new []{ new NUnitSink() });
		CommandService service = new(logger, new MatchService(logger), new LevelListOverrideService(logger));

		Assert.That(() => _ = service.ParseCommand("parse test"), Throws.InstanceOf<FormatException>());
	}

	[Test]
	public void BlankCommandThrows()
	{
		using Logger logger = new(new []{ new NUnitSink() });
		CommandService service = new(logger, new MatchService(logger), new LevelListOverrideService(logger));

		Assert.That(() => _ = service.ParseCommand("/ test"), Throws.InstanceOf<FormatException>());
	}
}
