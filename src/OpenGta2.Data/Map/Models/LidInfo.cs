using System.Runtime.InteropServices;

namespace OpenGta2.Data.Map;

[StructLayout(LayoutKind.Explicit)]
public struct LidInfo
{
    [FieldOffset(0)] private readonly ushort _data;

    public ushort TileGraphic => (ushort)(_data & 0b111_111_111);
    public byte LightingLevel => (byte)((_data >> 10) & 3);
    public bool Flat => (_data & (1 << 12)) == 1 << 12;
    public bool Flip => (_data & (1 << 13)) == 1 << 13;
    public Rotation Rotation => (Rotation)(byte)(_data >> 13);
}