using System;
using Microsoft.Xna.Framework;

namespace OpenGta2.Client;

public struct IntVector2
{
    public int X;
    public int Y;

    public IntVector2(int x, int y)
    {
        X = x;
        Y = y;
    }
    
    public static IntVector2 Floor(Vector2 vec)
    {
        return new IntVector2((int)vec.X, (int)vec.Y);
    }

    public static IntVector2 Ceiling(Vector2 vec)
    {
        vec = Vector2.Ceiling(vec);
        return new IntVector2((int)vec.X, (int)vec.Y);
    }

    public bool Equals(IntVector2 other)
    {
        return X == other.X && Y == other.Y;
    }

    public override bool Equals(object? obj)
    {
        return obj is IntVector2 other && Equals(other);
    }

    public override int GetHashCode()
    {
        return HashCode.Combine(X, Y);
    }
    
    public static bool operator ==(IntVector2 lhs, IntVector2 rhs)
    {
        return lhs.Equals(rhs);
    }

    public static bool operator !=(IntVector2 lhs, IntVector2 rhs)
    {
        return !lhs.Equals(rhs);
    }

    public static implicit operator Vector2(IntVector2 vec)
    {
        return new Vector2(vec.X, vec.Y);
    }
}