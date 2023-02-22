namespace OpenGta2.GameData.Style;

public struct PaletteIndex
{
    public PaletteIndex(ushort[] physPalette)
    {
        if (physPalette.Length != PhysPaletteLength)
        {
            throw new ArgumentOutOfRangeException(nameof(physPalette), $"Length must be {PhysPaletteLength}");
        }
        PhysPalette = physPalette;
    }

    public const int PhysPaletteLength = 16384;
    public ushort[] PhysPalette { get; }
}