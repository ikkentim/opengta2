namespace OpenGta2.GameData.Map;

public record Map(CompressedMap CompressedMap, MapObject[] Objects, MapZone[] Zones, TileAnimation[] Animations, MapLight[] Lights)
{
    public int Width { get; } = CompressedMap.Base.GetLength(1);
    public int Height { get; } = CompressedMap.Base.GetLength(0);

    public ColumnInfo GetColumn(int x, int y)
    {
        var num = CompressedMap.Base[y, x];
        var column = CompressedMap.Columns[num];
        return column;
    }
}