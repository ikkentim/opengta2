namespace OpenGta2.GameData.Style;

public readonly ref struct TilesPage
{
    public const int Width = Tile.Width * 4;
    public const int Height = Tile.Height * 4;
    public const int TilesPerPage = 4 * 4;
    public const int Size = Width * Height;

    private readonly Span<byte> _data;

    public TilesPage(Span<byte> data)
    {
        _data = data;
    }

    public Span<byte> Data => _data;

    public Tile GetTile(int index)
    {
        if(index is < 0 or >= TilesPerPage)
            throw new ArgumentOutOfRangeException(nameof(index));
        
        return new Tile(this, index);
    }
}