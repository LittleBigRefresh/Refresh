using System.Buffers.Binary;

namespace Refresh.GameServer.Importing;

public class BEBinaryReader : BinaryReader
{
    public BEBinaryReader(Stream input) : base(input)
    {}
    
    public override short ReadInt16() => BinaryPrimitives.ReadInt16BigEndian(this.ReadBytes(2));
    public override int ReadInt32() => BinaryPrimitives.ReadInt32BigEndian(this.ReadBytes(4));
    public override long ReadInt64() => BinaryPrimitives.ReadInt64BigEndian(this.ReadBytes(8));

    public override ushort ReadUInt16() => BinaryPrimitives.ReadUInt16BigEndian(this.ReadBytes(2));
    public override uint ReadUInt32() => BinaryPrimitives.ReadUInt32BigEndian(this.ReadBytes(4));
    public override ulong ReadUInt64() => BinaryPrimitives.ReadUInt64BigEndian(this.ReadBytes(8));
}