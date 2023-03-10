using Microsoft.Xna.Framework;

namespace OpenGta2.Client.Components;

public class Ped
{
    public Ped(Vector3 position, float rotation, int remap)
    {
        Position = position;
        Rotation = rotation;
        Remap = remap;
    }

    public Vector3 Position { get; set; }
    public float Rotation { get; set; }
    public int Remap { get; }
}