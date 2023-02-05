using System.Runtime.InteropServices;

namespace OpenGta2.Data.Map;

[StructLayout(LayoutKind.Explicit)]
public struct Fixed16
{
    [FieldOffset(0)]
    public ushort _data;
}