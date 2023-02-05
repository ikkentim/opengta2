using System.Runtime.InteropServices;

namespace OpenGta2.Data.Map;

[StructLayout(LayoutKind.Explicit)]
public struct ColorArgb
{
    [FieldOffset(0)]
    private uint _data;
}