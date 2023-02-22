using System.Runtime.InteropServices;

namespace OpenGta2.GameData.Map;

[StructLayout(LayoutKind.Explicit)]
public readonly struct ColorArgb
{
    [FieldOffset(0)]
    private readonly uint _data;

    public byte A => (byte)((_data >> 24) & 0xff);
    public byte R => (byte)((_data >> 16) & 0xff);
    public byte G => (byte)((_data >> 8) & 0xff);
    public byte B => (byte)(_data & 0xff);
}