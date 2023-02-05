using OpenGta2.Data;
using OpenGta2.Data.Map;
using OpenGta2.Data.Riff;
using OpenGta2.Data.Scripts;
using OpenGta2.Data.Scripts.Interpreting;

/*
using var stream = TestGamePath.OpenFile("data/Industrial-2P.scr");
var script = new ScriptParser().Parse(stream);
new ScriptInterpreter().Run(script, new LogScriptRuntime());
*/

using var stream = TestGamePath.OpenFile("data/bil.gmp");
var riffReader = new RiffReader(stream);
var mapreader = new MapReader(riffReader);

var map = mapreader.Read();

Console.WriteLine(map);