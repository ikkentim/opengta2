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


Console.WriteLine("SLOPE TESTING");

var map = map2.CompressedMap;

var maxY = map.Base.GetLength(0);
var maxX = map.Base.GetLength(1);
        
for (var x = 0; x < maxX; x++)
for (var y = 0; y < maxY; y++)
{

    // read compressed map and render column
    var columnNum = map.Base[y, x];
    var column = map.Columns[columnNum];

    for (var z = column.Offset; z < column.Height; z++)
    {
        var blockNum = column.Blocks[z - column.Offset];
        var block = map.Blocks[blockNum];

        var t = (byte)block.SlopeType.SlopeType;
        if (t > 0 && t < 4)
        {
            Console.WriteLine($@"{x},{y},{z} slope {block.SlopeType.SlopeType}");
        }
    }
}