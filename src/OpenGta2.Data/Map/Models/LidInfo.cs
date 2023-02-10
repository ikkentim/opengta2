using System.Diagnostics;
using System.Runtime.InteropServices;

namespace OpenGta2.Data.Map;

[StructLayout(LayoutKind.Explicit)]
[DebuggerDisplay("TileGraphic = {TileGraphic}, Rotation = {Rotation}")]
public struct LidInfo
{
    [FieldOffset(0)] private readonly ushort _data;

    public ushort TileGraphic => (ushort)(_data & 0b11_1111_1111);
    public byte LightingLevel => (byte)((_data >> 10) & 3);
    public bool Flat => (_data & (1 << 12)) == 1 << 12;
    public bool Flip => (_data & (1 << 13)) == 1 << 13;
    public Rotation Rotation => (Rotation)(byte)(_data >> 14);
}