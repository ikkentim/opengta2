namespace OpenGta2.Data.Map;

public struct MapZone
{
    public ZoneType Type { get; }
    public byte X { get; }
    public byte Y { get; }
    public byte Width { get; }
    public byte Height { get; }
    public string Name { get; }

    public MapZone(MapZoneHeader header, string name)
    {
        Type = header.Type;
        X = header.X;
        Y = header.Y;
        Width = header.Width;
        Height = header.Height;
        Name = name;
    }
}