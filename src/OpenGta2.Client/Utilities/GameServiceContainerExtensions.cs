using Microsoft.Xna.Framework;

namespace OpenGta2.Client.Utilities;

public static class GameServiceContainerExtensions
{
    public static void ReplaceService<T>(this GameServiceContainer container, T service)
    {
        container.RemoveService(typeof(T));
        container.AddService(service);
    }
}