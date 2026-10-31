namespace OpenGta2.Client.Diagnostics;

/// <summary>
/// Debug switches that influence the game; changed from the debug screens.
/// </summary>
public class DebugSettings
{
    /// <summary>
    /// Draw the collision debug boxes (player box, block under the player).
    /// </summary>
    public bool ShowHighlights { get; set; }

    /// <summary>
    /// Move the player at a tenth of the speed, to step through collision.
    /// </summary>
    public bool SlowPlayer { get; set; }
}
