using System.Runtime.InteropServices;

namespace OpenGta2.GameData.Audio;

[StructLayout(LayoutKind.Sequential)]
public struct SoundEntry
{
    public int Offset;
    public int Size;
    public int SampleRate;
    public int VariationSampleRate;
    public int LoopStart;
    public int LoopEnd;
}