using System;

namespace OpenGta2.Client.Utilities
{
    internal static class ThrowHelper
    {
        public static Exception GetContentNotLoaded() => new InvalidOperationException("The content of this component has not yet been loaded.");
        public static Exception GetLevelNotLoaded() => new InvalidOperationException("No level is currently loaded.");
    }
}
