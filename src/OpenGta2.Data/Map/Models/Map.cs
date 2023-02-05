namespace OpenGta2.Data.Map;

public record Map(CompressedMap CompressedMap, MapObject[] Objects, MapZone[] Zones, TileAnimation[] Animations);