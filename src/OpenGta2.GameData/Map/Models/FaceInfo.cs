using System.Diagnostics;
using System.Runtime.InteropServices;

namespace OpenGta2.GameData.Map;

[StructLayout(LayoutKind.Explicit)]
[DebuggerDisplay("TileGraphic = {TileGraphic}, Rotation = {Rotation}")]
public readonly struct FaceInfo
{
    [FieldOffset(0)] private readonly ushort _data;
    
    public FaceInfo(ushort data) => _data = data;

    public ushort TileGraphic => (ushort)(_data & 0b11_1111_1111);

    /// <summary>
    /// not on lid
    /// </summary>
    public bool Wall => (_data & (1 << 10)) == 1 << 10;
    /// <summary>
    /// not on lid
    /// </summary>
    public bool BulletWall => (_data & (1 << 11)) == 1 << 11;
    
    /// <summary>
    /// only on lid
    /// </summary>
    public byte LightingLevel => (byte)((_data >> 10) & 3);

    public bool Flat => (_data & (1 << 12)) == 1 << 12;
    public bool Flip => (_data & (1 << 13)) == 1 << 13;
    public Rotation Rotation => (Rotation)(byte)(_data >> 14);

}