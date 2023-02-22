namespace OpenGta2.GameData;

internal static class ThrowHelper
{
    public static void ThrowInvalidFileFormat() => throw GetInvalidFileFormat();

    public static Exception GetInvalidFileFormat() => new InvalidDataException("The specified file is not of a supported format.");

    public static void ThrowUnexpectedEndOfStream() => throw GetUnexpectedEndOfStream();

    public static Exception GetUnexpectedEndOfStream() => new InvalidDataException("Unexpected end of the input stream.");
}

