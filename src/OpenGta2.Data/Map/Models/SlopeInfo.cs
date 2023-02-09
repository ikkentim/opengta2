using System.Runtime.InteropServices;

namespace OpenGta2.Data.Map;

[StructLayout(LayoutKind.Explicit)]
public readonly struct SlopeInfo
{

    [FieldOffset(0)] private readonly byte _data;

    public GroundType GroundType => (GroundType)(_data & 0x3);

    public SlopeType SlopeType => (SlopeType)(byte)(_data >> 2);
}