namespace OpenGta2.GameData.Audio;

public class RawReader
{
    private readonly Stream _stream;

    public RawReader(Stream stream)
    {
        _stream = stream;
    }

    public SoundLibrary Read(SoundEntry[] entries)
    {
        var bytes = new byte[_stream.Length];
        var n = _stream.Read(bytes);

        if (n != bytes.Length)
        {
            ThrowHelper.ThrowUnexpectedEndOfStream();
        }

        return new SoundLibrary(bytes, entries);
    }
}