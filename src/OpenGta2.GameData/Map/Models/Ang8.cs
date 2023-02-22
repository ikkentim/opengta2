using System.Runtime.InteropServices;

namespace OpenGta2.GameData.Map;

[StructLayout(LayoutKind.Explicit)]
public struct Ang8
{
    [FieldOffset(0)]
    public byte _data;
}