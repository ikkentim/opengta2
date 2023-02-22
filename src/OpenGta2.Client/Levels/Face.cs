using System;

namespace OpenGta2.Client.Levels;

[Flags]
public enum Face : byte
{
    None = 0,
    Top = 1,
    Bottom = 2,
    Left = 4,
    Right = 8,
    Lid = 16
}