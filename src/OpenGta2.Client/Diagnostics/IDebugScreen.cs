using System.Collections.Generic;
using Microsoft.Xna.Framework;

namespace OpenGta2.Client.Diagnostics;

/// <summary>
/// A key a debug screen reacts to, shown in the key help on screen.
/// </summary>
public readonly record struct DebugKey(string Key, string Description);

/// <summary>
/// A debug tool that can be chosen from the debug menu (<see cref="DebugMenuComponent"/>).
/// </summary>
/// <remarks>
/// Add a screen by implementing this and registering it in the scene with <c>DebugMenuComponent.Add&lt;T&gt;()</c>; the constructor
/// can take the same services as a component. F1 to F4 belong to the menu (F1 back/close, F2/F3 up/down, F4 select): screens use F5 and up
/// or other keys. <see cref="Update"/> and <see cref="Draw"/> are only called while the screen is open.
/// </remarks>
public interface IDebugScreen
{
    /// <summary>
    /// The name in the menu.
    /// </summary>
    string Name { get; }

    /// <summary>
    /// The keys of this screen, drawn on screen while it is open.
    /// </summary>
    IReadOnlyList<DebugKey> HelpKeys { get; }

    /// <summary>
    /// Called once when the graphics are ready.
    /// </summary>
    void LoadContent();

    /// <summary>
    /// Called when the screen is opened from the menu.
    /// </summary>
    void Opened();

    /// <summary>
    /// Called when the screen is closed (back to the menu).
    /// </summary>
    void Closed();

    void Update(GameTime gameTime);

    void Draw(GameTime gameTime);
}
