namespace OpenGta2.GameData.Game;

public struct Vector3 : IFormattable
{
    public float X;
    public float Y;
    public float Z;

    public Vector3(float x, float y, float z)
    {
        X = x;
        Y = y;
        Z = z;
    }

    public static Vector3 FromInt(int x, int y, int z) => new(x / 16384.0f, y / 16384.0f, z / 16384.0f);
    
    public override string ToString() => $"({X}, {Y}, {Z})";
    public string ToString(string? format, IFormatProvider? formatProvider) => $"({X.ToString(format, formatProvider)}, {Y.ToString(format, formatProvider)}, {Z.ToString(format, formatProvider)})";
    public string ToString(string? format) => $"({X.ToString(format)}, {Y.ToString(format)}, {Z.ToString(format)})";
}