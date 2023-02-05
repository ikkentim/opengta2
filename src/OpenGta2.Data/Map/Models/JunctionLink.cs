using System.Runtime.InteropServices;

namespace OpenGta2.Data.Map;

[StructLayout(LayoutKind.Explicit)]
public struct JunctionLink
{
    [FieldOffset(0)]
    public uint _data;
}