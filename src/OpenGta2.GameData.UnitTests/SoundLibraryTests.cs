using System.Text;
using OpenGta2.GameData.Audio;
using Shouldly;
using Xunit;

namespace OpenGta2.Data.UnitTests;

public class SoundLibraryTests
{
    private static SoundLibrary Create(params (int size, int rate, int variation)[] sounds)
    {
        var entries = new SoundEntry[sounds.Length];
        var offset = 0;
        for (var i = 0; i < sounds.Length; i++)
        {
            entries[i] = new SoundEntry { Offset = offset, Size = sounds[i].size, SampleRate = sounds[i].rate, VariationSampleRate = sounds[i].variation, LoopEnd = -1 };
            offset += sounds[i].size;
        }

        var data = new byte[offset];
        for (var i = 0; i < data.Length; i++) data[i] = (byte)(i % 251);

        return new SoundLibrary(data, entries);
    }

    private static byte[] ReadAll(Stream stream)
    {
        using var memory = new MemoryStream();
        stream.CopyTo(memory);
        return memory.ToArray();
    }

    [Fact]
    public void GetSound_creates_a_16_bit_mono_wave_for_normal_entries()
    {
        var sut = Create((100, 22050, 0));

        var wave = ReadAll(sut.GetSound(0).Stream);

        Encoding.ASCII.GetString(wave, 0, 4).ShouldBe("RIFF");
        Encoding.ASCII.GetString(wave, 8, 4).ShouldBe("WAVE");
        BitConverter.ToUInt16(wave, 0x14).ShouldBe((ushort)1); // PCM
        BitConverter.ToUInt16(wave, 0x16).ShouldBe((ushort)1); // mono
        BitConverter.ToInt32(wave, 0x18).ShouldBe(22050); // sample rate
        BitConverter.ToInt32(wave, 0x1c).ShouldBe(44100); // byte rate
        BitConverter.ToUInt16(wave, 0x20).ShouldBe((ushort)2); // block align
        BitConverter.ToUInt16(wave, 0x22).ShouldBe((ushort)16); // bits per sample
        BitConverter.ToInt32(wave, 0x28).ShouldBe(100); // data size
        wave.Length.ShouldBe(0x2c + 100);
    }

    [Fact]
    public void GetSound_creates_an_8_bit_mono_wave_for_the_5500_Hz_entries()
    {
        // the police radio chatter (entries 69 to 136 of the level banks) is 8 bit
        var sut = Create((100, SoundLibrary.EightBitSampleRate, 0));

        var wave = ReadAll(sut.GetSound(0).Stream);

        BitConverter.ToInt32(wave, 0x18).ShouldBe(5500); // sample rate
        BitConverter.ToInt32(wave, 0x1c).ShouldBe(5500); // byte rate: one byte per sample
        BitConverter.ToUInt16(wave, 0x20).ShouldBe((ushort)1); // block align
        BitConverter.ToUInt16(wave, 0x22).ShouldBe((ushort)8); // bits per sample
        BitConverter.ToInt32(wave, 0x28).ShouldBe(100); // data size
    }

    [Fact]
    public void GetSound_keeps_odd_sizes_of_8_bit_entries()
    {
        var sut = Create((2063, 5500, 0));

        var wave = ReadAll(sut.GetSound(0).Stream);

        BitConverter.ToInt32(wave, 0x28).ShouldBe(2063);
        wave.Length.ShouldBe(0x2c + 2063);
    }

    [Fact]
    public void GetSound_drops_a_stray_last_byte_of_16_bit_entries()
    {
        // 16 bit audio needs an even number of bytes, and XAudio2 rejects the sound otherwise
        var sut = Create((101, 22050, 0));

        var wave = ReadAll(sut.GetSound(0).Stream);

        BitConverter.ToInt32(wave, 0x28).ShouldBe(100);
        wave.Length.ShouldBe(0x2c + 100);
    }

    [Fact]
    public void GetSound_returns_the_samples_of_the_entry()
    {
        var sut = Create((10, 8000, 0), (6, 8000, 0));

        var wave = ReadAll(sut.GetSound(1).Stream);

        wave.Skip(0x2c).ShouldBe(new byte[] { 10, 11, 12, 13, 14, 15 });
    }

    [Fact]
    public void GetSound_varies_the_sample_rate_within_the_variation()
    {
        var sut = Create((100, 22050, 1000));

        for (var i = 0; i < 50; i++)
        {
            var rate = BitConverter.ToInt32(ReadAll(sut.GetSound(0).Stream), 0x18);
            rate.ShouldBeInRange(22050 - 1000, 22050 + 1000);
        }
    }

    [Fact]
    public void GetPcm_returns_the_samples_the_rate_and_the_format()
    {
        var sut = Create((10, 22050, 0), (7, 5500, 0), (9, 22050, 0));

        var normal = sut.GetPcm(0);
        normal.SampleRate.ShouldBe(22050);
        normal.BitsPerSample.ShouldBe(16);
        normal.Samples.ToArray().ShouldBe(new byte[] { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9 });

        var eightBit = sut.GetPcm(1);
        eightBit.BitsPerSample.ShouldBe(8);
        eightBit.Samples.Length.ShouldBe(7); // odd sizes are fine for 8 bit

        var odd = sut.GetPcm(2);
        odd.Samples.Length.ShouldBe(8); // a stray last byte of 16 bit audio is dropped
    }

    [Fact]
    public void GetPcm_applies_the_pitch_variation()
    {
        var sut = Create((100, 22050, 500));

        for (var i = 0; i < 30; i++)
        {
            sut.GetPcm(0).SampleRate.ShouldBeInRange(22050 - 500, 22050 + 500);
        }
    }

    [Theory]
    [InlineData(5500, 8)]
    [InlineData(22050, 16)]
    [InlineData(8000, 16)]
    [InlineData(11025, 16)]
    public void GetBitsPerSample_depends_on_the_sample_rate(int sampleRate, int expectedBits)
    {
        SoundLibrary.GetBitsPerSample(new SoundEntry { SampleRate = sampleRate }).ShouldBe(expectedBits);
    }
}
