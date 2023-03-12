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

    public ref BlockInfo GetBlock(int x, int y, int z)
    {
        var column = GetColumn(x, y);

        if (z < 0 || z >= column.Height)
        {
            throw new ArgumentOutOfRangeException(nameof(z));
        }
        
        return ref CompressedMap.Blocks[column.Blocks[column.Offset]];
    }

    public int GetGroundZ(int x, int y)
    {
        var col = GetColumn(x, y);

        for (var zo = 1; zo < col.Height - col.Offset; zo++)
        {
            var z = zo + col.Offset;
            var block = CompressedMap.Blocks[col.Blocks[zo]];

            if (block.Lid.TileGraphic == 0)
            {
                return z;
            }
        }

        return col.Height;
    }
}
