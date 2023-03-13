namespace OpenGta2.GameData.Style;

public readonly ref struct Tile
{
    public const int Width = 64;
    public const int Height = 64;
    public const int Size = Width * Height;

    private readonly TilesPage _page;
    private readonly int _index;

    public Tile(TilesPage page, int index)
    {
        _page = page;
        _index = index;
    }

    public byte this[byte y, byte x] => GetPixel(x, y);

    public byte GetPixel(byte x, byte y)
    {
        if (x >= Width) throw new ArgumentOutOfRangeException(nameof(x));
        if (y >= Height) throw new ArgumentOutOfRangeException(nameof(y));

        var tx = _index % 4;
        var ty = _index / 4;

        var px = tx * Width + x;
        var py = ty * Height + y;

        return _page.Data[py * TilesPage.Width + px];
    }
}