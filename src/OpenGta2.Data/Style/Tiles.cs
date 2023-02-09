namespace OpenGta2.Data.Style;

public readonly struct Tiles
{
    // Tiles are stored in pages. Each page contains 4x4 tiles, each tile contains 64x64 pixels of 256-colors data.
    public const int TileWidth = 64;
    public const int TileHeight = 64;
    public const int TilePageCount = 92;

    public const int TilesPerPageX = 4;
    public const int TilesPerPageY = 4;
    public const int TilesPerPage = TilesPerPageX * TilesPerPageY;

    public const int TilePageWidth = TilesPerPageX * TileWidth;
    public const int TilePageHeight = TilesPerPageY * TileHeight;

    private const int TileLength = TileWidth * TileHeight;
    private const int TilePageLength = TilePageWidth * TilePageHeight;

    private readonly byte[] _data;

    public Tiles(byte[] data)
    {
        _data = data;
    }

    public ushort Count => (ushort)(_data.Length / TileLength);

    public Span<byte> GetTileSlice(ushort number, int y)
    {
        if (number >= Count)
        {
            throw new ArgumentOutOfRangeException(nameof(number));
        }
            
        var pageIndex = number / TilesPerPage;

        var page = _data.AsSpan(pageIndex * TilePageLength, TilePageLength);
            
        var tileIndex = number - (pageIndex * TilesPerPage);
        var tileRow = tileIndex / TilesPerPageX;
        var tileCol = tileIndex % TilesPerPageX;


        var pageY = tileRow * TileHeight + y;
        var pageX = tileCol * TileWidth;

        var start = pageY * TilePageWidth + pageX;
            
        return page[start..(start+TileWidth)];
    }
}