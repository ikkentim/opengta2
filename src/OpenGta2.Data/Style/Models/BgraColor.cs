using System.Runtime.InteropServices;

namespace OpenGta2.Data.Style;

[StructLayout(LayoutKind.Explicit)]
public struct BgraColor
{
    [FieldOffset(0)] public byte B;
    [FieldOffset(1)] public byte G;
    [FieldOffset(2)] public byte R;
    [FieldOffset(3)] public byte A;

    public uint Argb
    {
        get
        {
            if ((R | G | B) == 0)
            {
                return 0;
            }

            // alpha channel is unused
            // return (uint)(A << 24 | (B << 16 | G << 8 | R));
            return (uint)(0xff000000u | (B << 16 | G << 8 | R));
        }
    }
}