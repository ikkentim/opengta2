namespace OpenGta2.Data.Riff;

public class RiffChunk : IDisposable
{
    private readonly RiffChunkStream _stream;

    internal RiffChunk(string name, RiffChunkStream stream)
    {
        Name = name;
        _stream = stream;
    }

    public bool IsDisposed => _stream.IsDisposed;

    public string Name { get; }
    public Stream Stream => _stream;

    public void Dispose()
    {
        Stream.Dispose();
    }
}