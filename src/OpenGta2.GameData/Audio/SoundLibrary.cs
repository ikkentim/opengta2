using System.Runtime.InteropServices;
using System.Text;

namespace OpenGta2.GameData.Audio;

public class SoundLibrary
{
    private readonly byte[] _data;
    private readonly SoundEntry[] _entries;

    private readonly Random _random = new();

    public SoundLibrary(byte[] data, SoundEntry[] entries)
    {
        _data = data;
        _entries = entries;
    }

    /// <summary>
    /// The sample rate of the entries that hold 8-bit samples.
    /// </summary>
    public const int EightBitSampleRate = 5500;

    /// <summary>
    /// The number of bits per sample of an entry. The sound bank is not uniform: most samples are 16 bit signed, but the
    /// 68 entries at 5500 Hz (entries 69 to 136 of the level banks, the police radio chatter) are 8 bit unsigned.
    /// This is told apart by the sample rate: the sound bank does not say. (Found by listening: read as 16 bit the speech
    /// is noise and read as 8 bit all other sounds are noise.)
    /// </summary>
    public static int GetBitsPerSample(SoundEntry entry) => entry.SampleRate == EightBitSampleRate ? 8 : 16;

    public Sound GetSound(int index)
    {
        var entry = _entries[index];

        // generate riff stream with 2 blocks: 'fmt ' and 'data'
        // TODO: no allocation
        var header = new byte[0x2c];

        var pcm = GetPcm(index);
        var bitsPerSample = pcm.BitsPerSample;
        var bytesPerSample = bitsPerSample / 8;
        var dataSize = pcm.Samples.Length;
        var sampleRate = pcm.SampleRate;

        Encoding.ASCII.GetBytes("RIFF", header.AsSpan(0, 4));
        MemoryMarshal.Cast<byte, int>(header.AsSpan(0x04, 4))[0] = dataSize + 36;
        Encoding.ASCII.GetBytes("WAVE", header.AsSpan(0x08, 4));
        Encoding.ASCII.GetBytes("fmt ", header.AsSpan(0x0c, 4));
        MemoryMarshal.Cast<byte, int>(header.AsSpan(0x10, 4))[0] = 16; // SubchunkSize
        MemoryMarshal.Cast<byte, ushort>(header.AsSpan(0x14, 2))[0] = 1; // AudioFormat = PCM
        MemoryMarshal.Cast<byte, ushort>(header.AsSpan(0x16, 2))[0] = 1; // NumChannels = mono
        MemoryMarshal.Cast<byte, int>(header.AsSpan(0x18, 4))[0] = sampleRate; // sample rate
        MemoryMarshal.Cast<byte, int>(header.AsSpan(0x1c, 4))[0] = sampleRate * bytesPerSample; // byte rate
        MemoryMarshal.Cast<byte, ushort>(header.AsSpan(0x20, 2))[0] = (ushort)bytesPerSample; // block align
        MemoryMarshal.Cast<byte, ushort>(header.AsSpan(0x22, 2))[0] = (ushort)bitsPerSample; // bits per sample
        Encoding.ASCII.GetBytes("data", header.AsSpan(0x24, 4));
        MemoryMarshal.Cast<byte, int>(header.AsSpan(0x28, 4))[0] = dataSize;

        return new Sound(new SoundStream(header, new Memory<byte>(_data, entry.Offset, dataSize)), entry);
    }

    /// <summary>
    /// Returns the samples of an entry as they are stored (16 bit signed or 8 bit unsigned, see <see cref="GetBitsPerSample"/>) together
    /// with the sample rate to play them at, including a random variation of the pitch if the entry has one.
    /// </summary>
    public SoundData GetPcm(int index)
    {
        var entry = _entries[index];
        var bitsPerSample = GetBitsPerSample(entry);
        var bytesPerSample = bitsPerSample / 8;

        // 16 bit samples need an even number of bytes (36 entries have one stray byte at the end); 8 bit samples are fine with any size
        // (XAudio2 rejects data that is not a whole number of samples)
        var dataSize = entry.Size - entry.Size % bytesPerSample;

        var sampleRate = entry.SampleRate;

        if (entry.VariationSampleRate > 0)
        {
            sampleRate += _random.Next(-entry.VariationSampleRate, entry.VariationSampleRate);
        }

        return new SoundData(new Memory<byte>(_data, entry.Offset, dataSize), sampleRate, bitsPerSample, entry);
    }

    private sealed class SoundStream : Stream
    {
        private readonly byte[] _header;
        private readonly Memory<byte> _data;

        public SoundStream(byte[] header, Memory<byte> data)
        {
            _header = header;
            _data = data;
        }

        public override void Flush() => throw new InvalidOperationException();

        public override int Read(byte[] buffer, int offset, int count)
        {
            var read = 0;
            if (Position < _header.Length)
            {
                var remainingHeader = _header.Length - (int)Position;

                var readHeader = Math.Min(count, remainingHeader);

                Array.Copy(_header, Position, buffer, offset, readHeader);
                    
                read += readHeader;
                Position += readHeader;

                offset += readHeader;
                count -= readHeader;
            }

            if (count > 0)
            {
                var readData = (int)Math.Min(Length - Position, count);
                _data[..readData].Span.CopyTo(buffer.AsSpan(offset));
                    
                read += readData;
                Position += readData;
            }

            return read;
        }

        public override long Seek(long offset, SeekOrigin origin)
        {
            switch (origin)
            {
                case SeekOrigin.Begin:
                    Position = offset;
                    break;
                case SeekOrigin.Current:
                    Position += offset;
                    break;
                case SeekOrigin.End:
                    Position = Length + offset;
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(origin), origin, null);
            }

            if(Position > Length)
                Position = Length;

            return Position;
        }

        public override void SetLength(long value) => throw new InvalidOperationException();

        public override void Write(byte[] buffer, int offset, int count) => throw new InvalidOperationException();

        public override bool CanRead => true;
        public override bool CanSeek => true;
        public override bool CanWrite => false;
        public override long Length => _data.Length + _header.Length;
        public override long Position { get; set; }
    }
}