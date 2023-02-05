namespace OpenGta2.Data.Riff;

public class RiffReader
{
    private const int TypeLength = 4;
    private const int ChunkNameLength = 4;
    private const int HeaderLength = 6;

    private readonly Stream _stream;
    private RiffChunkStream? _activeChunkStream;

    public RiffReader(Stream stream)
    {
        _stream = stream;
        Type = stream.ReadExactString(TypeLength);
        Version = stream.ReadExactWord();
    }

    public string Type { get; }

    public ushort Version { get; }

    public RiffChunk? Next()
    {
        CloseChunkStream(true);

        var name = _stream.TryReadExactString(ChunkNameLength);

        if (name == null) return null;

        var length = _stream.ReadExactDoubleWord();

        var stream = new RiffChunkStream(this, _stream, length);
        _activeChunkStream = stream;

        return new RiffChunk(name, stream);
    }

    public RiffChunk? GetChunk(string name)
    {
        if (!_stream.CanSeek)
        {
            throw new NotSupportedException();
        }

        CloseChunkStream(false);

        _stream.Seek(HeaderLength, SeekOrigin.Begin);

        while (Next() is { } current)
        {
            if (current.Name == name)
            {
                return current;
            }
        }

        return null;
    }

    private void CloseChunkStream(bool moveToEnd)
    {
        if (_activeChunkStream == null)
        {
            return;
        }

        if (moveToEnd)
        {
            if (_activeChunkStream.CanSeek)
            {
                _activeChunkStream.Seek(0, SeekOrigin.End);
            }
            else
            {
                var remainder = _activeChunkStream.Length - _activeChunkStream.Position;

                if (remainder > 0)
                {
                    Span<byte> buffer = stackalloc byte[256];
                    while (remainder > 0)
                    {
                        remainder -= _activeChunkStream.ReadExact(buffer);
                    }
                }
            }
        }

        _activeChunkStream.Dispose();
    }

    internal void CloseChunkStream(RiffChunkStream chunkStream)
    {
        if (chunkStream.IsDisposed)
        {
            return;
        }

        if (_activeChunkStream != chunkStream)
        {
            // We have multiple open streams. This should not happen
            throw new InvalidOperationException();
        }

        _activeChunkStream = null;
    }
}