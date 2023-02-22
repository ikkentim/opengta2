using System.Runtime.InteropServices;

namespace OpenGta2.GameData.Scripts;

public class ScriptParser
{
    public Script Parse(Stream stream)
    {
        // GTA2 script file format:
        // - POINTERS: 6000 word values which point to functions within the script
        // - SCRIPT: 65536 bytes of script binary data
        // - STRING TABLE:
        //   - LENGTH: a word which indicates the length of the string table data
        //   - STRINGS:
        //     - TYPE: dword the type of the string
        //     - LENGTH: a byte which contains the length of the string
        //     - VALUE: a null terminated string value

        var pointers = ReadPointers(stream);
        var scriptData = ReadScript(stream);
        var strings = ReadStrings(stream);

        return new Script(pointers, scriptData, strings);
    }

    private static ushort[] ReadPointers(Stream stream)
    {
        var pointers = new ushort[6000];
        stream.ReadExact(pointers.AsSpan());
        return pointers;
    }

    private static byte[] ReadScript(Stream stream)
    {
        var scriptData = new byte[65536];
        stream.ReadExact(scriptData.AsSpan());
        return scriptData;
    }

    private static StringTable ReadStrings(Stream stream)
    {
        var tableLength = stream.ReadExactWord();
        var read = 0;
        var strings = new Dictionary<uint, StringValue>();

        while (read < tableLength)
        {
            stream.ReadExact(out StringHeader header);
            var value = stream.ReadExactString(header.Length);

            strings[header.Id] = new StringValue(header.Type, header.Length, value);

            read += 9 + header.Length;
        }

        return new StringTable(strings);
    }

    [StructLayout(LayoutKind.Explicit)]
    private readonly struct StringHeader
    {
        [FieldOffset(0)] public readonly uint Id;
        [FieldOffset(4)] public readonly StringType Type;
        [FieldOffset(8)] public readonly byte Length;
    }
}