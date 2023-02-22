using OpenGta2.DebugConsole;
using OpenGta2.GameData.Scripts.CommandParameters;
using OpenGta2.GameData.Scripts.Interpreting;

public class LogScriptRuntime : IScriptRuntime
{
    public void SpawnCar(ushort ptrIndex, ScriptCommandType type, SpawnCarParameters arguments)
    {
        Console.WriteLine(FormattableString.Invariant($"{type} auto{ptrIndex} = {arguments.Position:0.00} {arguments.Remap} {arguments.Rotation} {(CarModel)arguments.Model}"));
    }

    public void SpawnPlayerPed(ushort ptrIndex, SpawnPlayerPedParameters arguments)
    {
        Console.WriteLine(FormattableString.Invariant($"PLAYER_PED p{ptrIndex} = {arguments.Position:0.00} {arguments.Remap} {arguments.Rotation} "));
    }

    public void UnknownCommand(ushort ptrIndex, ScriptCommand command)
    {
        Console.WriteLine($"UNKNOWN COMMAND: {command.Type} ({command.Type:X})");
    }
}