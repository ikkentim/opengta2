namespace OpenGta2.Data.Riff;

public class RiffReader
{
    private const int TypeLength = 4;
    private const int ChunkNameLength = 4;
    private const int HeaderLength = 6;

    private readonly Stream _stream;
    private RiffChunkStream? _activeChunkStream;
    private bool _isResettingForSearch;

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
        _activeChunkStream?.Dispose();

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

        _isResettingForSearch = true;
        _activeChunkStream?.Dispose();
        _isResettingForSearch = false;

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
    
    internal bool ShouldSeekEnd(RiffChunkStream chunkStream) => chunkStream == _activeChunkStream && !_isResettingForSearch;
}