using System.Runtime.InteropServices;

namespace OpenGta2.Data.Scripts.Interpreting;

[StructLayout(LayoutKind.Explicit)]
public struct ScriptCommand
{
    [FieldOffset(0)] public ushort PtrIndex;
    [FieldOffset(2)] public ScriptCommandType Type;
    [FieldOffset(4)] public ushort NextPtrIndex;
    [FieldOffset(6)] public ScriptCommandFlags Flags;
}