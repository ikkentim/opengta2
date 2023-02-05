using System.Runtime.InteropServices;

namespace OpenGta2.Data.Map;

[StructLayout(LayoutKind.Explicit)]
public struct MapZoneHeader
{
    [FieldOffset(0)] public ZoneType Type;
    [FieldOffset(1)] public byte X;
    [FieldOffset(2)] public byte Y;
    [FieldOffset(3)] public byte Width;
    [FieldOffset(4)] public byte Height;
    [FieldOffset(5)] public byte NameLength;
    // followed by name.
}