using System.Runtime.InteropServices;
using OpenGta2.Data.Game;

namespace OpenGta2.Data.Scripts.CommandParameters;

[StructLayout(LayoutKind.Explicit)]
public struct SpawnCarParameters
{
    [FieldOffset(0)]
    public ushort Variable;
    [FieldOffset(4)]
    public int X;
    [FieldOffset(8)]
    public int Y;
    [FieldOffset(12)]
    public int Z;
    [FieldOffset(16)]
    public ushort Rotation;
    [FieldOffset(18)]
    public short Remap;
    [FieldOffset(20)]
    public ushort Model;
    [FieldOffset(22)]
    public ushort Trailer;

    public Vector3 Position => Vector3.FromInt(X, Y, Z);
}