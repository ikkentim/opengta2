namespace OpenGta2.GameData.Style;

public readonly struct Tiles
{
    private readonly byte[] _data;

    public Tiles(byte[] data)
    {
        _data = data;
    }

    public int TileCount => _data.Length / Tile.Size;

    public int PageCount => _data.Length / TilesPage.Size;

    public TilesPage GetPage(int index)
    {
        if (index < 0 || index >= PageCount) throw new ArgumentOutOfRangeException(nameof(index));

        var page = _data.AsSpan(index * TilesPage.Size, TilesPage.Size);
        return new TilesPage(page);
    }

    public Tile GetTile(int index)
    {
        if (index < 0 || index >= TileCount) throw new ArgumentOutOfRangeException(nameof(index));

        var page = index / TilesPage.TilesPerPage;
        var indexOnPage = index - page * TilesPage.TilesPerPage;

        return GetPage(page).GetTile(indexOnPage);
    }
}