using System.Runtime.InteropServices;

namespace OpenGta2.GameData.Style;

[StructLayout(LayoutKind.Explicit)]
public struct SpriteBase
{
    [FieldOffset(0)] public ushort Car;
    [FieldOffset(2)] public ushort Ped;
    [FieldOffset(4)] public ushort CodeObj;
    [FieldOffset(6)] public ushort MapObj;
    [FieldOffset(8)] public ushort User;
    [FieldOffset(10)] public ushort Font;

    public int CarOffset => 0;
    public int PedOffset => Car;
    public int CodeObjOffset => PedOffset + Ped;
    public int MapObjOffset => CodeObjOffset + CodeObj;
    public int UserOffset => MapObjOffset + MapObj;
    public int FontOffset => UserOffset + User;
}
