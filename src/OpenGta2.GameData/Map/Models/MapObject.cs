using System.Runtime.InteropServices;

namespace OpenGta2.GameData.Map;

[StructLayout(LayoutKind.Explicit)]
public struct MapObject
{
    [FieldOffset(0)] public Fixed16 X;
    [FieldOffset(2)] public Fixed16 Y;
    [FieldOffset(4)] public Ang8 Rotation;
    [FieldOffset(5)] public byte ObjectType;
}