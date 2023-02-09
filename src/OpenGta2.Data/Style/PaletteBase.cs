using System.Runtime.InteropServices;

namespace OpenGta2.Data.Style;

[StructLayout(LayoutKind.Explicit)]
public struct PaletteBase
{
    [FieldOffset(0)]public ushort Tile;
    [FieldOffset(2)]public ushort Sprite;
    [FieldOffset(4)]public ushort CarRemap;
    [FieldOffset(6)]public ushort PedRemap;
    [FieldOffset(8)]public ushort CodeObjRemap;
    [FieldOffset(10)]public ushort MapObjRemap;
    [FieldOffset(12)]public ushort UserRemap;
    [FieldOffset(14)]public ushort FontRemap;

    public int TileOffset => 0;
    public int SpriteOffset => Tile;
    public int CarRemapOffset => SpriteOffset + Sprite;
    public int PedRemapOffset => CarRemapOffset + CarRemap;
    public int CodeObjRemapOffset => PedRemapOffset + CodeObjRemap;
    public int MapObjRemapOffset => CodeObjRemapOffset + CodeObjRemap;
    public int UserRemapOffset => MapObjRemapOffset + MapObjRemap;
    public int FontRemapOffset => UserRemapOffset + UserRemap;
}