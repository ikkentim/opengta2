using System.Runtime.InteropServices;

namespace OpenGta2.Data.Map;

[StructLayout(LayoutKind.Explicit)]
public struct Junction
{
    [FieldOffset(0)] public JunctionLink North;
    [FieldOffset(4)] public JunctionLink South;
    [FieldOffset(8)] public JunctionLink East;
    [FieldOffset(12)] public JunctionLink West;
    [FieldOffset(16)] public byte JuncType;// TODO: is this the correct data type?
    [FieldOffset(17)] public byte MinX;
    [FieldOffset(18)] public byte MinY;
    [FieldOffset(19)] public byte MaxX;
    [FieldOffset(20)] public byte MaxY;
}