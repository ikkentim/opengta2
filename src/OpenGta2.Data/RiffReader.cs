namespace OpenGta2.Data;

public class RiffReader
{
    private readonly Stream _stream;

    public RiffReader(Stream stream)
    {
        _stream = stream;
        Type = stream.ReadExactString(4);
        Version = stream.ReadExactWord();
    }

    public string Type { get; }

    public ushort Version { get; }

    public IEnumerable<RiffChunk> ReadChunks()
    {
        while (_stream.TryReadExactString(4) is { } name)
        {
            var length = _stream.ReadExactDoubleWord();

            var data = new byte[length];

            if (_stream.Read(data) != length)
            {
                throw new Exception("bad read");
            }

            yield return new RiffChunk(name, data);
        }
    }
}