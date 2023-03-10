namespace OpenGta2.GameData.Style;

public readonly struct SpritePage
{
    private readonly byte[] _data;

    public SpritePage(byte[] data)
    {
        _data = data;
    }

    public Sprite GetSprite(SpriteEntry entry, ushort number)
    {
        return new Sprite(_data, entry, number);
    }
}