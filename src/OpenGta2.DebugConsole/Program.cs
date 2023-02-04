using OpenGta2.Data.Scripts;
using OpenGta2.Data.Scripts.Interpreting;

using var stream = TestGamePath.OpenFile("data/Industrial-2P.scr");
var script = new ScriptParser().Parse(stream);
new ScriptInterpreter().Run(script, new LogScriptRuntime());