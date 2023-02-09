namespace OpenGta2.Data.Map;

public record CompressedMap(uint[,] Base, Dictionary<uint, ColumnInfo> Columns, BlockInfo[] Blocks);