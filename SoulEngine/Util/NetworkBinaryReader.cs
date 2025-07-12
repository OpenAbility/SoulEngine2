using System.Buffers.Binary;
using System.Diagnostics;
using System.Net;
using System.Text;

namespace SoulEngine.Util;

public class NetworkBinaryReader : BinaryReader
{

    /// <inheritdoc />
    public NetworkBinaryReader(Stream stream) : base(stream)
    {
    }
    
    /// <inheritdoc />
    public NetworkBinaryReader(Stream stream, Encoding encoding) : base(stream, encoding)
    {
    }
    
    /// <inheritdoc />
    public NetworkBinaryReader(Stream stream, Encoding encoding, bool leaveOpen) : base(stream, encoding, leaveOpen)
    {
    }

    [StackTraceHidden]
    [DebuggerHidden]
    private Span<byte> InternalRead(Span<byte> span)
    {

        Span<byte> original = span;
        
        int read = 0;
        while ((read = base.Read(span)) != 0)
        {
            span = span.Slice(read);
        }

        if (span.Length != 0)
            throw new EndOfStreamException("Reached EOF!");
        
        
        return original;
    }

    public override short ReadInt16() => BinaryPrimitives.ReadInt16BigEndian(InternalRead(stackalloc byte[sizeof(short)]));
    public override ushort ReadUInt16() => BinaryPrimitives.ReadUInt16BigEndian(InternalRead(stackalloc byte[sizeof(ushort)]));
    public override int ReadInt32() => BinaryPrimitives.ReadInt32BigEndian(InternalRead(stackalloc byte[sizeof(int)]));
    public override uint ReadUInt32() => BinaryPrimitives.ReadUInt32BigEndian(InternalRead(stackalloc byte[sizeof(uint)]));
    public override long ReadInt64() => BinaryPrimitives.ReadInt64BigEndian(InternalRead(stackalloc byte[sizeof(long)]));
    public override ulong ReadUInt64() => BinaryPrimitives.ReadUInt64BigEndian(InternalRead(stackalloc byte[sizeof(ulong)]));
    public override unsafe Half ReadHalf() => BinaryPrimitives.ReadHalfBigEndian(InternalRead(stackalloc byte[sizeof(Half)]));
    public override unsafe float ReadSingle() => BinaryPrimitives.ReadSingleBigEndian(InternalRead(stackalloc byte[sizeof(float)]));
    public override unsafe double ReadDouble() => BinaryPrimitives.ReadDoubleBigEndian(InternalRead(stackalloc byte[sizeof(double)]));
}