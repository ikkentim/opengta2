using System.Runtime.Serialization;

namespace OpenGta2.Data.Riff;

[Serializable]
public class RiffChunkNotFoundException : Exception
{
    public RiffChunkNotFoundException(string chunkName) : base($"The chunk '{chunkName}' could not be found in the specified file.")
    {
    }
    
    protected RiffChunkNotFoundException(SerializationInfo info,
        StreamingContext context) : base(info, context)
    {
    }
}