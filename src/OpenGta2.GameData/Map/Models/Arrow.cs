namespace OpenGta2.GameData.Map;

[Flags]
public enum Arrow : byte
{
    GreenLeft = 1 << 0,
    GreenRight = 1 << 1,
    GreenUp = 1 << 2,
    GreenDown = 1 << 3,
    RedLeft = 1 << 4,
    RedRight = 1 << 5,
    RedUp = 1 << 6,
    RedDown = 1 << 7,
}