using System.Runtime.InteropServices;

namespace OpenGta2.Data.Map;

[StructLayout(LayoutKind.Explicit)]
public readonly struct SlopeType
{

    [FieldOffset(0)] private readonly byte _data;

    public GroundType GroundType => (GroundType)(_data & 0x3);

    public byte Type => (byte)(_data >> 2);

    /*
     * Type:
     *   0 = none
     *   1- 2 = up 26 low, high
     *   3 – 4 = down 26 low, high5 – 6 = left 26 low, high
     *   7 – 8 = right 26 low, high
     *   9 – 16 = up 7 low – high17 – 24 = down 7 low – high
     *   25 – 32 = left 7 low – high
     *   33 – 40 = right 7 low – high
     *
     *   41 – 44 = 45up,down,left,right
     *
     *   45 = diagonal, facing up left
     *
     *   46 = diagonal, facing up right
     *
     *   47 = diagonal, facing down left
     *
     *   48 = diagonal, facing down right
     *
     *   49 = 3 or 4-sided diagonal slope, facing up left
     *   50 = 3 or 4-sided diagonal slope, facing up right
     *   51 = 3 or 4-sided diagonal slope, facing down left
     *   52 = 3 or 4-sided diagonal slope, facing down right
     *
     *   53 = partial block left
     *
     *   54 = partial block right
     *
     *   55 = partial block top
     *
     *   56 = partial block bottom
     *
     *   57 = partial block top left corner
     *
     *   58 = partial block top right corner
     *
     *   59 = partial block bottom right corner
     *
     *   60 = partial block bottom left corner
     *
     *   61 = partial centre block 16×16
     *
     *   62 = reserved for future use
     *   63 = indicates slope in block above
     */
}