using System.Runtime.InteropServices;

namespace OpenGta2.Data.Map;

[StructLayout(LayoutKind.Explicit)]
public struct TileAnimation
{
    [FieldOffset(0)] public ushort Base;
    [FieldOffset(2)] public byte FrameRate;
    [FieldOffset(3)] public byte Repeat;
    [FieldOffset(4)] public byte AnimLength;
    [FieldOffset(5)] public byte Unused;
    // followed by ushorts of the tiles (AnimLength)
}