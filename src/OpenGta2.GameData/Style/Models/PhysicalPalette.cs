namespace OpenGta2.GameData.Style;

public readonly struct PhysicalPalette
{
    public const int PaletteLength = 256;
    public const int PalettesPerPage = 64;
    public const int PalettePageLength = PaletteLength * PalettesPerPage;

    private readonly BgraColor[] _palette;

    public PhysicalPalette(BgraColor[] palette)
    {
        _palette = palette;
    }

    public int Count => _palette.Length / PaletteLength;
    public int PageCount => Count / PalettesPerPage;

    public PalettePage GetPage(ushort number)
    {
        if (number >= PageCount)
        {
            throw new ArgumentOutOfRangeException(nameof(number));
        }

        var idx = number * PalettePageLength;

        return new PalettePage(_palette.AsSpan(idx, PalettePageLength));
    }

    public Palette GetPalette(ushort number)
    {

        if (number >= Count)
        {
            throw new ArgumentOutOfRangeException(nameof(number));
        }

        var pageNumber = number / PalettesPerPage;
        var paletteNumber = number - pageNumber * PalettesPerPage;

        var page = GetPage((ushort)pageNumber);
        return new Palette(page, (byte)paletteNumber);
    }
}