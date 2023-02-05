using System.Runtime.InteropServices;

namespace OpenGta2.Data.Map;

[StructLayout(LayoutKind.Explicit)]
public struct FaceInfo
{
    [FieldOffset(0)] private readonly ushort _data;

    public ushort TileGraphic => (ushort)(_data & 0b111_111_111);
    
    public bool Wall => (_data & (1 << 10)) == 1 << 10;
    public bool BulletWall => (_data & (1 << 11)) == 1 << 11;
    public bool Flat => (_data & (1 << 12)) == 1 << 12;
    public bool Flip => (_data & (1 << 13)) == 1 << 13;
    public Rotation Rotation => (Rotation)(byte)(_data >> 13);
}