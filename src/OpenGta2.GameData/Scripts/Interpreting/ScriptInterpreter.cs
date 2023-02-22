using System.Runtime.InteropServices;
using OpenGta2.GameData.Scripts.CommandParameters;

namespace OpenGta2.GameData.Scripts.Interpreting;

public class ScriptInterpreter
{
    private static readonly int CommandSize = Marshal.SizeOf<ScriptCommand>();

    public void Run(Script script, IScriptRuntime runtime)
    {
        foreach (var pointer in script.Pointers)
        {
            if (pointer == 0)
            {
                continue;
            }

            var functionData = script.ScriptData.AsSpan(pointer..);
            var argsData = functionData[CommandSize..];

            var command = MemoryMarshal.Read<ScriptCommand>(functionData);
            
            switch (command.Type)
            {
                case ScriptCommandType.PARKED_CAR_DATA:
                {
                    var args = MemoryMarshal.Read<SpawnCarParameters>(argsData);
                    runtime.SpawnCar(command.PtrIndex, command.Type, args);
                    break;
                }
                case ScriptCommandType.PLAYER_PED:
                {
                    var args = MemoryMarshal.Read<SpawnPlayerPedParameters>(argsData);
                    runtime.SpawnPlayerPed(command.PtrIndex, args);
                    break;
                }
                // TODO: Handle other commands
                default:
                    runtime.UnknownCommand(command.PtrIndex, command);
                    break;
            }
        }
    }
}