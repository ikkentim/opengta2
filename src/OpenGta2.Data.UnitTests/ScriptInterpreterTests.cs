using Xunit;

namespace OpenGta2.Data.UnitTests;

public class ScriptInterpreterTests
{
    [Fact]
    public void Testing()
    {
        using var stream = TestGamePath.OpenFile("data/Industrial-2P.scr");
        var script = new ScriptParser().Parse(stream);

        var sut = new ScriptInterpreter();

        sut.Run(script);
    }
}