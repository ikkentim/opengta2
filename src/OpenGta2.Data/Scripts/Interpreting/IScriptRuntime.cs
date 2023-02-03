using OpenGta2.Data.Scripts.CommandParameters;

namespace OpenGta2.Data.Scripts.Interpreting;

public interface IScriptRuntime
{
    void SpawnCar(ushort ptrIndex, ScriptCommandType type, SpawnCarParameters parameters);
    void SpawnPlayerPed(ushort ptrIndex, SpawnPlayerPedParameters parameters);

    // TODO: This method should eventually disappear when all commands have been implemented.
    void UnknownCommand(ushort ptrIndex, ScriptCommand command);
}