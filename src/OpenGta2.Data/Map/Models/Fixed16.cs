using System.Runtime.InteropServices;

namespace OpenGta2.Data.Map;

[StructLayout(LayoutKind.Explicit)]
public struct Fixed16
{
    [FieldOffset(0)]
    public short _data;

    public static implicit operator float(Fixed16 value)
    {
        int x = value._data;
        const int e = 7;
        var c = Math.Abs(x);
        var sign = 1;
        if (x < 0) //convert back from two's complement
        {
            c = x - 1;
            c = ~c;
            sign = -1;
        }

        var f = (1.0 * c) / Math.Pow(2, e);
        f *= sign;

        return (float)f;
    }
}