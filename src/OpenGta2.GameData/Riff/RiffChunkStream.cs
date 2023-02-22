namespace OpenGta2.GameData.Riff;

public class RiffChunkStream : Stream
{
    private readonly RiffReader _reader;
    private Stream? _innerStream;
    private readonly long _length;
    private long _position;
    private readonly long _start;

    internal RiffChunkStream(RiffReader reader, Stream innerStream, long length)
    {
        _reader = reader;
        _innerStream = innerStream;
        _length = length;

        if (_innerStream.CanSeek)
        {
            _start = _innerStream.Position;
        }
    }

    public bool IsDisposed => _innerStream == null;

    public override bool CanRead => true;

    public override bool CanSeek => _innerStream?.CanSeek ?? throw new ObjectDisposedException(nameof(RiffChunkStream));

    public override bool CanWrite => false;

    public override long Length
    {
        get
        {
            EnsureNotDisposed();
            return _length;
        }
    }

    public override long Position
    {
        get
        {
            EnsureNotDisposed();
            return _position;
        }
        set => Seek(value, SeekOrigin.Begin);
    }

    private long RemainingLength => _length - _position;

    public override int Read(byte[] buffer, int offset, int count)
    {
        EnsureNotDisposed();

        if (offset < 0) throw new ArgumentException("Offset should not be negative", nameof(offset));
        if (count < 0) throw new ArgumentException("Count should not be negative", nameof(count));
        if (offset + count > buffer.Length) throw new ArgumentException("Insufficient buffer size", nameof(buffer));

        if (count > RemainingLength)
        {
            count = (int)RemainingLength;
        }

        var read = _innerStream!.Read(buffer, offset, count);

        _position += read;

        return read;
    }

    public override long Seek(long offset, SeekOrigin origin)
    {
        EnsureNotDisposed();

        if (!_innerStream!.CanSeek)
        {
            throw new NotSupportedException();
        }

        switch (origin)
        {
            case SeekOrigin.Begin:
                if (offset > _length || offset < 0)
                {
                    throw new ArgumentOutOfRangeException(nameof(offset), offset,
                        "Offset should be a positive number less than the length of the stream");
                }

                _position = _innerStream.Seek(_start + offset, SeekOrigin.Begin) - _start;
                break;
            case SeekOrigin.Current:
                var target = _position + offset;
                if (target < 0 || target > _length)
                    throw new ArgumentOutOfRangeException(nameof(offset), offset, "Seeking beyond stream end is not supported");

                _position = _innerStream.Seek(offset, SeekOrigin.Current) - _start;
                break;
            case SeekOrigin.End:
                if (offset > 0 || -offset > _length)
                    throw new ArgumentOutOfRangeException(nameof(offset), offset,
                        "Offset should be a negative number les than the length of the stream");

                _position = _innerStream.Seek(_start + _length, SeekOrigin.Begin) - _start;
                break;
            default:
                throw new InvalidOperationException();
        }

        return _position;
    }

    public override int Read(Span<byte> buffer)
    {
        EnsureNotDisposed();

        var remaining = RemainingLength;

        if (remaining == 0)
            return 0;

        if (buffer.Length > remaining)
        {
            buffer = buffer[..(int)remaining];
        }

        var read = _innerStream!.Read(buffer);

        _position += read;

        return read;
    }

    public override int ReadByte()
    {
        EnsureNotDisposed();

        if (_position >= _length) return -1;


        var result = _innerStream!.ReadByte();

        if (result >= 0)
        {
            _position++;
        }

        return result;
    }

    public override void Flush() => throw new NotSupportedException();

    public override void SetLength(long value) => throw new NotSupportedException();

    public override void Write(byte[] buffer, int offset, int count) => throw new NotSupportedException();

    protected override void Dispose(bool disposing)
    {
        if (IsDisposed)
        {
            return;
        }

        if (CanSeek)
        {
            Seek(0, SeekOrigin.End);
        }
        else
        {
            var remainder = RemainingLength;

            if (remainder > 0)
            {
                Span<byte> buffer = stackalloc byte[256];
                while (remainder > 0)
                {
                    var read = Read(buffer);
                    remainder -= read;

                    if (read == 0)
                    {
                        throw new Exception("invalid read");
                    }
                }
            }
        }

        _innerStream = null;
    }

    private void EnsureNotDisposed()
    {
        if (_innerStream == null) throw new ObjectDisposedException(nameof(RiffChunkStream));
    }
}