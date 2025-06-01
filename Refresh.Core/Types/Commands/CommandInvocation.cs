namespace Refresh.Core.Types.Commands;

public ref struct CommandInvocation
{
    public CommandInvocation(ReadOnlySpan<char> name, ReadOnlySpan<char> arguments)
    {
        this.Name = name;
        this.Arguments = arguments;
    }

    public ReadOnlySpan<char> Name { get; init; }
    public ReadOnlySpan<char> Arguments { get; init; }
}
