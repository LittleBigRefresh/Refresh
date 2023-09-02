namespace Refresh.GameServer.Types.Commands;

public ref struct Command
{
    public Command(ReadOnlySpan<char> name, ReadOnlySpan<char> arguments)
    {
        this.Name = name;
        this.Arguments = arguments;
    }

    public ReadOnlySpan<char> Name { get; init; }
    public ReadOnlySpan<char> Arguments { get; init; }
}
