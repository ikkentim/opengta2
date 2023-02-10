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
using var riffReader = new RiffReader(stream);
var mapreader = new MapReader(riffReader);

var map2 = mapreader.Read();

Console.WriteLine(map2);


var map = map2.CompressedMap;
var col = map.Columns[map.Base[1, 4]];

var bNum = col.Blocks.Last();
var block = map.Blocks[bNum];
Console.WriteLine(block.Lid.TileGraphic);
Console.WriteLine("test");
