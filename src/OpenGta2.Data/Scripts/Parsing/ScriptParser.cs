using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Text;

namespace OpenGta2.Data.Scripts
{
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

        private ushort[] ReadPointers(Stream stream)
        {
            var pointers = new ushort[6000];
            var bPointers = MemoryMarshal.Cast<ushort, byte>(pointers.AsSpan());

            Debug.Assert(bPointers.Length == 12000);

            var length = stream.Read(bPointers);

            if (length != bPointers.Length)
            {
                throw new Exception("bad read");
            }

            return pointers;
        }

        private byte[] ReadScript(Stream stream)
        {
            var scriptData = new byte[65536];
            var length = stream.Read(scriptData.AsSpan());

            if (length != scriptData.Length)
            {
                throw new Exception("bad read");
            }

            return scriptData;
        }

        private StringTable ReadStrings(Stream stream)
        {
            var tableLength = ReadWord(stream);
            var read = 0;
            var strings = new Dictionary<uint, StringValue>();

            while (read < tableLength)
            {
                var id = ReadDoubleWord(stream);
                var type = (StringType)ReadDoubleWord(stream);
                var length = ReadByte(stream);
                var value = ReadString(stream, length);

                read += 9 + length;

                strings[id] = new StringValue(type, length, value);
            }

            return new StringTable(strings);
        }

        private static string ReadString(Stream stream, byte length)
        {
            Span<byte> buffer = stackalloc byte[length];
            var read = stream.Read(buffer);

            if (read != length)
            {
                throw new Exception("bad read");
            }

            var nullTerminatorIndex = buffer.IndexOf((byte)0);
            var textBuffer = buffer[..nullTerminatorIndex];

            return Encoding.ASCII.GetString(textBuffer);
        }

        private static unsafe ushort ReadWord(Stream stream)
        {
            ushort d = 0;
            var p = (byte*)&d;
            var length = stream.Read(new Span<byte>(p, 2));

            if (length != 2)
            {
                throw new Exception("bad read");
            }
            return d;
        }

        private static unsafe uint ReadDoubleWord(Stream stream)
        {
            uint d = 0;
            var p = (byte*)&d;
            var length = stream.Read(new Span<byte>(p, 4));

            if (length != 4)
            {
                throw new Exception("bad read");
            }
            return d;
        }

        private static byte ReadByte(Stream stream)
        {
            var b = stream.ReadByte();

            if (b < 0)
            {
                throw new Exception("bad read");
            }

            return (byte)b;
        }
    }
}