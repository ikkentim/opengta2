using Microsoft.Xna.Framework;

namespace OpenGta2.Client.Effects;

public record struct Light(Vector3 Position, Color Color, float Radius, float Intensity);