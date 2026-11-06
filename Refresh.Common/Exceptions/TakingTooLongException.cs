namespace Refresh.Common.Exceptions;

/// <summary>
/// Custom exception to throw (as a fallback) if a traversal, or another operation, is taking way too long.
/// </summary>
public class TakingTooLongException : Exception
{
    public TakingTooLongException(string message) : base(message)
    {
        
    }
}