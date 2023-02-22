using Moq;
using OpenGta2.GameData.Scripts;
using OpenGta2.GameData.Scripts.CommandParameters;
using OpenGta2.GameData.Scripts.Interpreting;
using Xunit;

namespace OpenGta2.Data.UnitTests;

[Trait("Category", "DataTests")]
public class ScriptInterpreterTests
{
    [Fact]
    public void Run_should_call_runtime_SpawnCar()
    {
        using var stream = TestGamePath.OpenFile("data/Industrial-2P.scr");
        var script = new ScriptParser().Parse(stream);
        
        var runtime = new Mock<IScriptRuntime>();

        var sut = new ScriptInterpreter();

        sut.Run(script, runtime.Object);

        runtime.Verify(x => x.SpawnCar(It.IsAny<ushort>(), ScriptCommandType.PARKED_CAR_DATA, It.IsAny<SpawnCarParameters>()),
            Times.Exactly(70));
    }
}