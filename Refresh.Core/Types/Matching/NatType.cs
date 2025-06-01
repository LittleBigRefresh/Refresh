namespace Refresh.Core.Types.Matching;

public enum NatType : byte
{
    /// <summary>
    /// The client is connected directly to the internet.
    /// </summary>
    Open = 1,
    /// <summary>
    /// The client is connected behind a NAT, through a permissive router.
    /// </summary>
    Moderate = 2,
    /// <summary>
    /// The client is unable to properly communicate through their network. This user can only connect to Open clients.
    /// </summary>
    Strict = 3,
}