namespace OpenGta2.Data.Scripts;

public class Script
{
    public Script(ushort[] pointers, byte[] scriptData, StringTable strings)
    {
        Pointers = pointers;
        ScriptData = scriptData;
        Strings = strings;
    }

    public ushort[] Pointers { get; }
    public byte[] ScriptData { get; }
    public StringTable Strings { get; }
}