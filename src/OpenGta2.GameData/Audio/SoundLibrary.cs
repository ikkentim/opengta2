using System.Runtime.InteropServices;
using System.Text;

namespace OpenGta2.GameData.Audio;

public class SoundLibrary
{
    private readonly byte[] _data;
    private readonly SoundEntry[] _entries;

    private Random _random = new();

    public SoundLibrary(byte[] data, SoundEntry[] entries)
    {
        _data = data;
        _entries = entries;
    }

    public Sound GetSound(int index)
    {
        var entry = _entries[index];

        // generate riff stream with 2 blocks: 'fmt ' and 'data'
        // TODO: no allocation
        var header = new byte[0x2c];

        var sampleRate = entry.SampleRate;

        if (entry.VariationSampleRate > 0)
        {
            sampleRate += _random.Next(-entry.VariationSampleRate,  entry.VariationSampleRate);
        }

        Encoding.ASCII.GetBytes("RIFF", header.AsSpan(0, 4));
        MemoryMarshal.Cast<byte, int>(header.AsSpan(0x04, 4))[0] = entry.Size + 36;
        Encoding.ASCII.GetBytes("WAVE", header.AsSpan(0x08, 4));
        Encoding.ASCII.GetBytes("fmt ", header.AsSpan(0x0c, 4));
        MemoryMarshal.Cast<byte, int>(header.AsSpan(0x10, 4))[0] = 16; // SubchunkSize
        MemoryMarshal.Cast<byte, ushort>(header.AsSpan(0x14, 2))[0] = 1; // AudioFormat = PCM
        MemoryMarshal.Cast<byte, ushort>(header.AsSpan(0x16, 2))[0] = 1; // NumChannels = mono
        MemoryMarshal.Cast<byte, int>(header.AsSpan(0x18, 4))[0] = sampleRate; // sample rate
        MemoryMarshal.Cast<byte, int>(header.AsSpan(0x1c, 4))[0] = sampleRate * 2; // byte rate
        MemoryMarshal.Cast<byte, ushort>(header.AsSpan(0x20, 2))[0] = 2; // block align
        MemoryMarshal.Cast<byte, ushort>(header.AsSpan(0x22, 2))[0] = 16; // bits per sample
        Encoding.ASCII.GetBytes("data", header.AsSpan(0x24, 4));
        MemoryMarshal.Cast<byte, int>(header.AsSpan(0x28, 4))[0] = entry.Size;

        return new Sound(new SoundStream(header, new Memory<byte>(_data, entry.Offset, entry.Size)), entry);
    }

    private class SoundStream : Stream
    {
        private readonly byte[] _header;
        private Memory<byte> _data;

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