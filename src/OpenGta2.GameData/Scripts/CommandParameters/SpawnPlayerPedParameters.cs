using System.Runtime.InteropServices;
using OpenGta2.GameData.Game;

namespace OpenGta2.GameData.Scripts.CommandParameters;

[StructLayout(LayoutKind.Explicit)]
public struct SpawnPlayerPedParameters
{
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

    public Vector3 Position => Vector3.FromInt(X, Y, Z);
}