using System.Runtime.InteropServices;

namespace OpenGta2.GameData.Style;

[StructLayout(LayoutKind.Explicit)]
public struct PaletteBase
{
    [FieldOffset(0)] public ushort Tile;
    [FieldOffset(2)] public ushort Sprite;
    [FieldOffset(4)] public ushort CarRemap;
    [FieldOffset(6)] public ushort PedRemap;
    [FieldOffset(8)] public ushort CodeObjRemap;
    [FieldOffset(10)] public ushort MapObjRemap;
    [FieldOffset(12)] public ushort UserRemap;
    [FieldOffset(14)] public ushort FontRemap;

    public int TileOffset => 0;
    public int SpriteOffset => Tile;
    public int CarRemapOffset => SpriteOffset + Sprite;
    public int PedRemapOffset => CarRemapOffset + CarRemap;
    public int CodeObjRemapOffset => PedRemapOffset + CodeObjRemap;
    public int MapObjRemapOffset => CodeObjRemapOffset + CodeObjRemap;
    public int UserRemapOffset => MapObjRemapOffset + MapObjRemap;
    public int FontRemapOffset => UserRemapOffset + UserRemap;

    public int GetRemap(SpriteKind kind)
    {
        return kind switch
        {
            SpriteKind.Car => CarRemap,
            SpriteKind.Ped => PedRemap,
            SpriteKind.CodeObj => CodeObjRemap,
            SpriteKind.mapObj => MapObjRemap,
            SpriteKind.User => UserRemap,
            SpriteKind.Font => FontRemap,
            _ => throw new ArgumentOutOfRangeException(nameof(kind), kind, null)
        };
    }
    public int GetRemapOffset(SpriteKind kind)
    {
        return kind switch
        {
            SpriteKind.Car => CarRemapOffset,
            SpriteKind.Ped => PedRemapOffset,
            SpriteKind.CodeObj => CodeObjRemapOffset,
            SpriteKind.mapObj => MapObjRemapOffset,
            SpriteKind.User => UserRemapOffset,
            SpriteKind.Font => FontRemapOffset,
            _ => throw new ArgumentOutOfRangeException(nameof(kind), kind, null)
        };
    }
}