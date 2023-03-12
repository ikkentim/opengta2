using System;
using Microsoft.Xna.Framework.Input;

namespace OpenGta2.Client;

public class Controls
{
    private KeyboardState _current;
    private KeyboardState _previous;
    
    /// <summary>
    /// Is key down since current frame.
    /// </summary>
    public bool IsKeyDown(Keys key)
    {
        return _current.IsKeyDown(key) && _previous.IsKeyUp(key);
    }

    /// <summary>
    /// Is key up since current frame.
    /// </summary>
    public bool IsKeyUp(Keys key)
    {
        return _current.IsKeyUp(key) && _previous.IsKeyDown(key);
    }
    
    /// <summary>
    /// Is key down.
    /// </summary>
    public bool IsKeyPressed(Keys key)
    {
        return _current.IsKeyDown(key);
    }

    /// <summary>
    /// Is key down since current frame.
    /// </summary>
    public bool IsKeyDown(Control key)
    {
        return IsKeyDown(GetKey(key));
    }

    /// <summary>
    /// Is key up since current frame.
    /// </summary>
    public bool IsKeyUp(Control key)
    {
        return IsKeyUp(GetKey(key));
    }

    /// <summary>
    /// Is key down.
    /// </summary>
    public bool IsKeyPressed(Control key)
    {
        return IsKeyPressed(GetKey(key));
    }

    private Keys GetKey(Control control)
    {
        // should be configurable at some point
        return control switch
        {
            Control.Forward => Keys.Up,
            Control.Backward => Keys.Down,
            Control.Left => Keys.Left,
            Control.Right => Keys.Right,
            Control.Shoot => Keys.LeftControl,
            Control.Menu => Keys.Escape,
            _ => throw new ArgumentOutOfRangeException(nameof(control), control, null)
        };
    }
    
    public void Update()
    {
        _previous = _current;
        _current = Keyboard.GetState();
    }
}