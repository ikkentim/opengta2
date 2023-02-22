using Microsoft.Xna.Framework;

namespace OpenGta2.Client.Rendering;

public record struct Light(Vector3 Position, Color Color, float Radius, float Intensity);