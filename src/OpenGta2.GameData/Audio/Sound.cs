namespace OpenGta2.GameData.Audio;

/// <summary>
/// A sound as a wave stream (with a generated header), ready to be played.
/// </summary>
public record struct Sound(Stream Stream, SoundEntry Entry);

/// <summary>
/// The samples of a sound.
/// </summary>
/// <param name="Samples">The samples: 16 bit signed or 8 bit unsigned little endian mono PCM.</param>
/// <param name="SampleRate">The sample rate to play at.</param>
/// <param name="BitsPerSample">8 or 16.</param>
/// <param name="Entry">The entry in the sound bank, with the loop start (in bytes).</param>
public readonly record struct SoundData(ReadOnlyMemory<byte> Samples, int SampleRate, int BitsPerSample, SoundEntry Entry);
