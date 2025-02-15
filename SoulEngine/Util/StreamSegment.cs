namespace SoulEngine.Util;

/// <summary>
/// Represents a read-only view over a portion of an underlying stream.
/// </summary>
public sealed class StreamSegment : Stream
{
    private readonly Stream _stream;
    private readonly bool _leaveOpen;
    private long length;
    private long offset;

    public StreamSegment(Stream stream, bool leaveOpen = true)
    {
        _stream = stream ?? throw new ArgumentNullException(nameof(stream));
        _leaveOpen = leaveOpen;
        length = stream.Length;
    }

    public Stream BaseStream => _stream;

    public void Adjust(long offset, long length)
    {
        if (offset > _stream.Length)
            throw new ArgumentOutOfRangeException(nameof(offset));
        if (length > _stream.Length - offset)
            throw new ArgumentOutOfRangeException(nameof(length));
        
        this.offset = offset;
        this.length = length;
        _stream.Position = offset;
    }

    public override bool CanRead => _stream.CanRead;
    public override bool CanSeek => _stream.CanSeek;
    public override bool CanWrite => false;
    public override long Length => length;

    public override long Position
    {
        get => _stream.Position - offset;
        set
        {
            if (value > length)
                throw new ArgumentOutOfRangeException(nameof(value));
            _stream.Position = offset + value;
        }
    }

    private long RemainingBytes => length - Position;

    public override void Flush() => _stream.Flush();
    public override Task FlushAsync(CancellationToken cancellationToken) => _stream.FlushAsync(cancellationToken);
    public override bool CanTimeout => _stream.CanTimeout;
    public override int ReadTimeout { get => _stream.ReadTimeout; set => _stream.ReadTimeout = value; }
    public override int WriteTimeout { get => _stream.WriteTimeout; set => _stream.WriteTimeout = value; }

    public override int Read(byte[] buffer, int offset, int count)
    {
        if (buffer == null) throw new ArgumentNullException(nameof(buffer));
        if (offset < 0 || count < 0 || offset + count > buffer.Length)
            throw new ArgumentOutOfRangeException();
        
        return _stream.Read(buffer, offset, (int)Math.Min(count, RemainingBytes));
    }

    public override int Read(Span<byte> buffer) => _stream.Read(buffer.Slice(0, (int)Math.Min(buffer.Length, RemainingBytes)));
    public override Task<int> ReadAsync(byte[] buffer, int offset, int count, CancellationToken cancellationToken) =>
        _stream.ReadAsync(buffer, offset, (int)Math.Min(count, RemainingBytes), cancellationToken);
    public override ValueTask<int> ReadAsync(Memory<byte> buffer, CancellationToken cancellationToken = default) =>
        _stream.ReadAsync(buffer.Slice(0, (int)Math.Min(buffer.Length, RemainingBytes)), cancellationToken);

    public override long Seek(long offset, SeekOrigin origin)
    {
        long newPos = origin switch
        {
            SeekOrigin.Begin => offset,
            SeekOrigin.Current => Position + offset,
            SeekOrigin.End => length + offset,
            _ => throw new ArgumentOutOfRangeException(nameof(origin))
        };

        if (newPos < 0 || newPos > length)
            throw new IOException();
        
        Position = newPos;
        return newPos;
    }

    public override void SetLength(long value)
    {
        if (value > _stream.Length - offset)
            throw new ArgumentOutOfRangeException(nameof(value));
        
        length = value;
    }

    public override void Write(byte[] buffer, int offset, int count) => throw new NotSupportedException();
    public override void Write(ReadOnlySpan<byte> buffer) => throw new NotSupportedException();
    public override Task WriteAsync(byte[] buffer, int offset, int count, CancellationToken cancellationToken) =>
        Task.FromException(new NotSupportedException());
    public override ValueTask WriteAsync(ReadOnlyMemory<byte> buffer, CancellationToken cancellationToken = default) =>
        ValueTask.FromException(new NotSupportedException());

    protected override void Dispose(bool disposing)
    {
        if (disposing && !_leaveOpen)
            _stream.Dispose();
        base.Dispose(disposing);
    }

    public override async ValueTask DisposeAsync()
    {
        if (!_leaveOpen)
            await _stream.DisposeAsync().ConfigureAwait(false);
        await base.DisposeAsync().ConfigureAwait(false);
    }
}