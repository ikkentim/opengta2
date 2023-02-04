using OpenGta2.Data;
using OpenGta2.Data.Scripts;
using OpenGta2.Data.Scripts.CommandParameters;
using OpenGta2.Data.Scripts.Interpreting;

using var stream = TestGamePath.OpenFile("data/Industrial-2P.scr");
var script = new ScriptParser().Parse(stream);

var sut = new ScriptInterpreter();

sut.Run(script, new LogScriptRuntime(script));

var rdr = new GtaStringReader(new RiffReader(TestGamePath.OpenFile("data/e.gxt")));

var strings = rdr.Read();

public static class TestGamePath
{
    public static DirectoryInfo Directory =>
        new(Environment.GetEnvironmentVariable("OPENGTA2_PATH", EnvironmentVariableTarget.User)!);

    public static Stream OpenFile(string path) => File.OpenRead(Path.Combine(Directory.FullName, path));
}

public class LogScriptRuntime : IScriptRuntime
{
    private readonly Script _script;

    public LogScriptRuntime(Script script)
    {
        _script = script;
    }

    public void SpawnCar(ushort ptrIndex, ScriptCommandType type, SpawnCarParameters parameters)
    {
        Console.WriteLine(FormattableString.Invariant($"{type} auto{ptrIndex} = {parameters.Position:0.00} {parameters.Remap} {parameters.Rotation} {(CarModel)parameters.Model}"));
    }

    public void SpawnPlayerPed(ushort ptrIndex, SpawnPlayerPedParameters parameters)
    {
        Console.WriteLine(FormattableString.Invariant($"PLAYER_PED p{ptrIndex} = {parameters.Position:0.00} {parameters.Remap} {parameters.Rotation} "));
    }

    public void UnknownCommand(ushort ptrIndex, ScriptCommand command)
    {
        Console.WriteLine($"UNKNOWN COMMAND: {command.Type} ({command.Type:X})");
    }
}