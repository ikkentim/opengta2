namespace OpenGta2.GameData.Style;

public readonly ref struct PalettePage
{
    private readonly Span<BgraColor> _data;

    public PalettePage(Span<BgraColor> data)
    {
        _data = data;
    }

    public BgraColor GetColor(byte paletteNumber, byte entry) => _data[paletteNumber + entry * PhysicalPalette.PalettesPerPage];
}