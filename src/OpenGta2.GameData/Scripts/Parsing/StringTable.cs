using System.Collections;

namespace OpenGta2.GameData.Scripts;

public class StringTable : IEnumerable<StringValue>
{
    public StringTable(IReadOnlyDictionary<uint, StringValue> values)
    {
        Values = values;
    }

    public IReadOnlyDictionary<uint, StringValue> Values { get; }
    public IEnumerator<StringValue> GetEnumerator() => Values.Values.GetEnumerator();
    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
}