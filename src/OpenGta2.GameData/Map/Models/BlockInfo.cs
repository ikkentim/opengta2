using System.Runtime.InteropServices;

namespace OpenGta2.GameData.Map;

[StructLayout(LayoutKind.Explicit)]
public struct BlockInfo
{
    [FieldOffset(0)] public FaceInfo Left;
    [FieldOffset(2)] public FaceInfo Right;
    [FieldOffset(4)] public FaceInfo Top;
    [FieldOffset(6)] public FaceInfo Bottom;
    [FieldOffset(8)] public FaceInfo Lid;
    [FieldOffset(10)] public Arrow Arrows;
    [FieldOffset(11)] public SlopeInfo SlopeType;
}