namespace OpenGta2.Data.Style;

public readonly ref struct Palette
{
    private readonly PalettePage _page;
    private readonly byte _paletteNumber;

    public Palette(PalettePage page, byte paletteNumber)
    {
        _page = page;
        _paletteNumber = paletteNumber;
    }

    public BgraColor GetColor(byte entry) => _page.GetColor(_paletteNumber, entry);
}