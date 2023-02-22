using System.Runtime.InteropServices;

namespace OpenGta2.GameData.Map;

[StructLayout(LayoutKind.Explicit)]
public struct JunctionLink
{
    [FieldOffset(0)]
    public uint _data;
}