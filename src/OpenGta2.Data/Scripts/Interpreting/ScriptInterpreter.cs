using System.Diagnostics;
using System.Runtime.InteropServices;
using OpenGta2.Data.Scripts.CommandParameters;

namespace OpenGta2.Data.Scripts.Interpreting;

public class ScriptInterpreter
{
    public void Run(Script script, IScriptRuntime runtime)
    {
        foreach (var pointer in script.Pointers)
        {
            if (pointer == 0)
            {
                continue;
            }

            var functionData = script.ScriptData.AsSpan(pointer..);

            var command = MemoryMarshal.Read<ScriptCommand>(functionData);

            Debug.WriteLine($"CMD type {command.Type:X}\tflags {command.Flags}\tPTR {command.PtrIndex:X}\tNEXT {command.NextPtrIndex:X}\t\t\t{command.Type}");

            switch (command.Type)
            {
                case ScriptCommandType.PARKED_CAR_DATA:
                {
                    var par = MemoryMarshal.Read<SpawnCarParameters>(functionData[Marshal.SizeOf<ScriptCommand>()..]);
                    runtime.SpawnCar(command.PtrIndex, command.Type, par);
                    break;
                }
                case ScriptCommandType.PLAYER_PED:
                {
                    var par = MemoryMarshal.Read<SpawnPlayerPedParameters>(
                        functionData[Marshal.SizeOf<ScriptCommand>()..]);
                    runtime.SpawnPlayerPed(command.PtrIndex, par);
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