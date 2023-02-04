using System.Runtime.InteropServices;

namespace OpenGta2.Data.Map;

[StructLayout(LayoutKind.Explicit)]
public struct BlockInfo
{
    [FieldOffset(0)] public ushort Left;
    [FieldOffset(2)] public ushort Right;
    [FieldOffset(4)] public ushort Top;
    [FieldOffset(6)] public ushort Bottom;
    [FieldOffset(8)] public ushort Lid;
    [FieldOffset(10)] public Arrow Arrows;
    [FieldOffset(11)] public byte SlopeType;
}