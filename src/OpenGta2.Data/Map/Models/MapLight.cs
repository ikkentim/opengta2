using System.Runtime.InteropServices;

namespace OpenGta2.Data.Map;

[StructLayout(LayoutKind.Explicit)]
public struct MapLight
{
    [FieldOffset(0)] public ColorArgb ARGB;
    [FieldOffset(4)] public Fixed16 X;
    [FieldOffset(6)] public Fixed16 Y;
    [FieldOffset(8)] public Fixed16 Z;
    [FieldOffset(10)] public Fixed16 Radius;
    [FieldOffset(12)] public byte Intensity;
    [FieldOffset(13)] public byte Shape;
    [FieldOffset(14)] public byte OnTime;
    [FieldOffset(15)] public byte OffTime;
}