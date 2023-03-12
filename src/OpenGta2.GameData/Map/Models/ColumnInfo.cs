namespace OpenGta2.GameData.Map;

/// <summary>
/// </summary>
/// <param name="Height">The height of the column (0 to 7)</param>
/// <param name="Offset">The number of empty blocks at the bottom (0 to <see cref="Height"/>).</param>
/// <param name="Blocks">Block numbers for each block.</param>
public record ColumnInfo(byte Height, byte Offset, uint[] Blocks);