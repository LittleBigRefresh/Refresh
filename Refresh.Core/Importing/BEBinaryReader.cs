using System.Buffers.Binary;

namespace Refresh.Core.Importing;

public class BEBinaryReader : BinaryReader
{
    public BEBinaryReader(Stream input) : base(input)
    {}
    
    public override short ReadInt16()
    {
        Span<byte> buf = stackalloc byte[sizeof(short)];
        this.BaseStream.ReadExactly(buf);
        return BinaryPrimitives.ReadInt16BigEndian(buf);
    }
    public override int ReadInt32()
    {
        Span<byte> buf = stackalloc byte[sizeof(int)];
        this.BaseStream.ReadExactly(buf);
        return BinaryPrimitives.ReadInt32BigEndian(buf);
    }
    public override long ReadInt64()
    {
        Span<byte> buf = stackalloc byte[sizeof(long)];
        this.BaseStream.ReadExactly(buf);
        return BinaryPrimitives.ReadInt64BigEndian(buf);
    }
    
    public override ushort ReadUInt16()
    {
        Span<byte> buf = stackalloc byte[sizeof(ushort)];
        this.BaseStream.ReadExactly(buf);
        return BinaryPrimitives.ReadUInt16BigEndian(buf);
    }
    public override uint ReadUInt32()
    {
        Span<byte> buf = stackalloc byte[sizeof(uint)];
        this.BaseStream.ReadExactly(buf);
        return BinaryPrimitives.ReadUInt32BigEndian(buf);
    }
    public override ulong ReadUInt64()
    {
        Span<byte> buf = stackalloc byte[sizeof(ulong)];
        this.BaseStream.ReadExactly(buf);
        return BinaryPrimitives.ReadUInt64BigEndian(buf);
    }
}