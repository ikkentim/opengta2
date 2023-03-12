using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using OpenGta2.GameData.Riff;

namespace OpenGta2.GameData.Audio
{
    public class SdtReader
    {
        private readonly Stream _stream;

        public SdtReader(Stream stream)
        {
            _stream = stream;
        }

        public SoundEntry[] Read()
        {
            var entries = new SoundEntry[_stream.Length / Marshal.SizeOf<SoundEntry>()];

            _stream.ReadExact(entries.AsSpan());
            return entries;
        }

    }
}
