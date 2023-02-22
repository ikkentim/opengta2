using OpenGta2.GameData.Riff;

namespace OpenGta2.Data.UnitTests;

public abstract class RiffFileTestBase<T> : IDisposable
{
    private readonly Stream _stream;
    private readonly RiffReader _riffReader;


    protected RiffFileTestBase(string path, Func<RiffReader, T> factory)
    {
        _stream = TestGamePath.OpenFile(path);
        _riffReader = new RiffReader(_stream);
        
        Sut = factory(_riffReader);
    }

    protected T Sut { get; }
    
    public virtual void Dispose()
    {
        _stream.Dispose();
        _riffReader.Dispose();
    }
}