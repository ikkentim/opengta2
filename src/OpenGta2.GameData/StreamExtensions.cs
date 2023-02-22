using System.Runtime.InteropServices;
using System.Text;

namespace OpenGta2.GameData;

internal static class StreamExtensions
{
    public static string ReadExactString(this Stream stream, byte length)
    {
        return TryReadExactString(stream, length) ?? throw ThrowHelper.GetUnexpectedEndOfStream();
    }

    public static string? TryReadExactString(this Stream stream, byte length)
    {
        Span<byte> buffer = stackalloc byte[length];
        var read = stream.Read(buffer);

        if (read != length)
        {
            return null;
        }

        var nullTerminatorIndex = buffer.IndexOf((byte)0);

        if (nullTerminatorIndex < 0)
        {
            return Encoding.ASCII.GetString(buffer);
        }
        else
        {
            var textBuffer = buffer[..nullTerminatorIndex];
            return Encoding.ASCII.GetString(textBuffer);
        }
    }

    public static unsafe ushort ReadExactWord(this Stream stream)
    {
        ushort d = 0;
        var p = (byte*)&d;
        var length = stream.Read(new Span<byte>(p, 2));

        if (length != 2)
        {
            ThrowHelper.ThrowUnexpectedEndOfStream();
        }
        return d;
    }

    public static unsafe uint ReadExactDoubleWord(this Stream stream)
    {
        uint d = 0;
        var p = (byte*)&d;
        var length = stream.Read(new Span<byte>(p, 4));

        if (length != 4)
        {
            ThrowHelper.ThrowUnexpectedEndOfStream();
        }

        return d;
    }

    public static int ReadExact(this Stream stream, Span<byte> span)
    {
        var read = stream.Read(span);

        if (read != span.Length)
        {
            ThrowHelper.ThrowUnexpectedEndOfStream();
        }

        return read;
    }

    public static int ReadExact<T>(this Stream stream, Span<T> span) where T : struct
    {
        var buffer = MemoryMarshal.Cast<T, byte>(span);
        return ReadExact(stream, buffer) / Marshal.SizeOf<T>();
    }
    
    public static void ReadExact<T>(this Stream stream, out T value) where T : struct
    {
        value = new T();
        var span = MemoryMarshal.CreateSpan(ref value, 1);
        ReadExact(stream, span);
    }

    public static byte ReadExactByte(this Stream stream)
    {
        var b = stream.ReadByte();

        if (b < 0)
        {
            ThrowHelper.ThrowUnexpectedEndOfStream();
        }

        return (byte)b;
    }
}
