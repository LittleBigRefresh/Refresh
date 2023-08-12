using NPTicket.Verification.Keys;

namespace Refresh.GameServer.Verification;

public class LbpSigningKey : PsnSigningKey
{
    public static readonly LbpSigningKey Instance = new();
    private LbpSigningKey() {}
    
    public override string CurveX => "39c62d061d4ee35c5f3f7531de0af3cf918346526edac727";
    public override string CurveY => "a5d578b55113e612bf1878d4cc939d61a41318403b5bdf86";
}