namespace OpenGta2.Data.Riff;

public sealed class RiffReader : IDisposable
{
    private const int TypeLength = 4;
    private const int ChunkNameLength = 4;
    private const int HeaderLength = 6;

    private readonly Stream _stream;
    private RiffChunkStream? _activeChunkStream;
    private bool _isDisposed;
    private readonly Dictionary<string, long> _chunkCache = new();
    private bool _fullCache;

    public RiffReader(Stream stream)
    {
        _stream = stream;
        Type = stream.ReadExactString(TypeLength);
        Version = stream.ReadExactWord();
    }
    
    public string Type { get; }

    public ushort Version { get; }

    private void CloseActiveChunk()
    {
        // Disposing a chunk will fast-forward the stream to the end of the chunk.
        _activeChunkStream?.Dispose();
        _activeChunkStream = null;
    }

    public RiffChunk? Next()
    {
        EnsureNotDisposed();

        CloseActiveChunk();

        var position = _stream.Position;

        var name = _stream.TryReadExactString(ChunkNameLength);

        if (name == null)
        {
            // We reached end of stream. This means we've cached all chunk offsets.
            _fullCache = true;

            return null;
        }

        var length = _stream.ReadExactDoubleWord();

        var stream = new RiffChunkStream(this, _stream, length);
        _activeChunkStream = stream;

        // Cache chunk position for GetChunk calls.
        _chunkCache[name] = position;

        return new RiffChunk(name, stream);
    }

    public RiffChunk? GetChunk(string name)
    {
        EnsureNotDisposed();

        if (!_stream.CanSeek)
        {
            throw new NotSupportedException();
        }

        CloseActiveChunk();

        // check cache 
        if (_chunkCache.TryGetValue(name, out var position))
        {
            _stream.Seek(position, SeekOrigin.Begin);
            return Next();
        }
        
        if (_fullCache)
        {
            // All chunk positions have been cached - seeking won't help.
            return null;
        }
        
        while (Next() is { } current)
        {
            if (current.Name == name)
            {
                return current;
            }
        }

        return null;
    }
    
    public void Dispose()
    {
        CloseActiveChunk();
        _isDisposed = true;
    }
    
    private void EnsureNotDisposed()
    {
        if (_isDisposed)
        {
            throw new ObjectDisposedException(nameof(RiffReader));
        }
    }
}