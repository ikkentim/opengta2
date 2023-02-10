namespace OpenGta2.Data.Riff;

public static class RiffReaderExtensions
{
    public static RiffChunk GetRequiredChunk(this RiffReader reader, string name) =>
        reader.GetChunk(name) ?? throw new RiffChunkNotFoundException(name);

    public static RiffChunk GetRequiredChunk(this RiffReader reader, string name, long length)
    {
        var chunk = GetRequiredChunk(reader, name);

        if (chunk.Stream.Length != length)
        {
            ThrowHelper.ThrowInvalidFileFormat();
        }

        return chunk;
    }
}