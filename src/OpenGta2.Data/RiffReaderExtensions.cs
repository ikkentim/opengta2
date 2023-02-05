namespace OpenGta2.Data;

public static class RiffReaderExtensions
{
    public static List<RiffChunk> ReadAllChunks(this RiffReader reader) =>
        reader.ReadChunks()
            .ToList();
}