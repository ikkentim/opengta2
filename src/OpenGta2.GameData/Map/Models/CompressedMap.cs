namespace OpenGta2.GameData.Map;

public record CompressedMap(uint[,] Base, Dictionary<uint, ColumnInfo> Columns, BlockInfo[] Blocks);