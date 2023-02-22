namespace OpenGta2.GameData.Style;

public record struct FontBase(ushort[] Base)
{
    public int FontCount => Base.Length;

    public int GetFontOffset(int index)
    {
        if (index < 0 || index >= FontCount)
        {
            throw new ArgumentOutOfRangeException(nameof(index));
        }
        
        var offset = 0;
        for (var i = 0; i < index; i++)
        {
            offset += Base[i];
        }

        return offset;
    }
}