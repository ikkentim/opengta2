using Microsoft.Xna.Framework;

namespace OpenGta2.Client.Utilities;

public static class GameTimeExtensions
{
    public static float GetDelta(this GameTime gameTime)
    {
        return (float)gameTime.ElapsedGameTime.TotalSeconds;
    }
}