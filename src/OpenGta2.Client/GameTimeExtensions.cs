using Microsoft.Xna.Framework;

namespace OpenGta2.Client;

public static class GameTimeExtensions
{
    public static float GetDelta(this GameTime gameTime) => (float)gameTime.ElapsedGameTime.TotalSeconds;
}