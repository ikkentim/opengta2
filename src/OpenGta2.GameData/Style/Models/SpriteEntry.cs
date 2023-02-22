using System.Runtime.InteropServices;

namespace OpenGta2.GameData.Style;

[StructLayout(LayoutKind.Explicit)]
public struct SpriteEntry
{
    [FieldOffset(0)] public uint Ptr;
    [FieldOffset(4)] public byte Width;
    [FieldOffset(5)] public byte Height;
    [FieldOffset(6)] public ushort Pad;

    public uint PageNumber => Ptr / (256 * 256);
    public uint PageY =>  (Ptr - PageNumber * 256 * 256) / 256; 
    public uint PageX =>  (Ptr - PageNumber * 256 * 256) % 256; 
}