using System;
using Microsoft.Xna.Framework;

namespace OpenGta2.Client;

public struct IntVector3
{
    public int X;
    public int Y;
    public int Z;

    public IntVector3(int x, int y, int z)
    {
        X = x;
        Y = y;
        Z = z;
    }

    public static IntVector3 Floor(Vector3 vec)
    {
        return new IntVector3((int)vec.X, (int)vec.Y, (int)vec.Z);
    }

    public bool Equals(IntVector3 other)
    {
        return X == other.X && Y == other.Y && Z == other.Z;
    }

    public override bool Equals(object? obj)
    {
        return obj is IntVector3 other && Equals(other);
    }
    
    public override int GetHashCode()
    {
        return HashCode.Combine(X, Y, Z);
    }
    
    public static bool operator ==(IntVector3 lhs, IntVector3 rhs)
    {
        return lhs.Equals(rhs);
    }

    public static bool operator !=(IntVector3 lhs, IntVector3 rhs)
    {
        return !lhs.Equals(rhs);
    }

    public static implicit operator Vector3(IntVector3 vec)
    {
        return new Vector3(vec.X, vec.Y, vec.Z);
    }
}