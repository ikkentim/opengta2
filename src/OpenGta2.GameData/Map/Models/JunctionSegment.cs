using System.Runtime.InteropServices;

namespace OpenGta2.GameData.Map;

[StructLayout(LayoutKind.Explicit)]
public struct JunctionSegment
{
    [FieldOffset(0)] public ushort JunctionNum1;
    [FieldOffset(2)] public ushort JunctionNum2;
    [FieldOffset(4)] public byte MinX;
    [FieldOffset(5)] public byte MinY;
    [FieldOffset(6)] public byte MaxX;
    [FieldOffset(7)] public byte MaxY;
}