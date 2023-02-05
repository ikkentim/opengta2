namespace OpenGta2.Data.Map;

public struct TileAnimation
{
    public ushort Base;
    public byte FrameRate;
    public byte Repeat;
    public ushort[] Tiles;

    public TileAnimation(TileAnimationHeader header, ushort[] tiles)
    {
        Base = header.Base;
        FrameRate = header.FrameRate;
        Repeat = header.Repeat;
        Tiles = tiles;
    }
}