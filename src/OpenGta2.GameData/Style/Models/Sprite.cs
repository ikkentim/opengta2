namespace OpenGta2.GameData.Style;

public readonly struct Sprite
{
    private readonly byte[] _pageData;
    private readonly SpriteEntry _entry;

    public Sprite(byte[] pageData, SpriteEntry entry, ushort number)
    {
        Number = number;
        _pageData = pageData;
        _entry = entry;
    }

    public byte this[byte y, byte x] => GetPixel(x, y);

    public ushort Number { get; }
    public byte Width => _entry.Width;
    public byte Height => _entry.Height;

    public byte GetPixel(byte x, byte y)
    {
        if (x >= _entry.Width)
            throw new ArgumentOutOfRangeException(nameof(x));
        if (y >= _entry.Height)
            throw new ArgumentOutOfRangeException(nameof(y));

        var py = _entry.PageY + y;
        var px = _entry.PageX + x;
        return _pageData[py * 256 + px];
    }
}