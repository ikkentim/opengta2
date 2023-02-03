using Shouldly;
using System.IO;
using Xunit;

namespace OpenGta2.Data.UnitTests
{
    public class ScriptParserTests
    {
        [Theory]
        [InlineData("data/mike1m.SCR")]
        [InlineData("data/wil.SCR")]
        public void Parse_should_succeed_with_strings(string path)
        {
            var sut = new ScriptParser();
            
            using var stream = TestGamePath.OpenFile(path);
            var script = sut.Parse(stream);

            script.Pointers.Length.ShouldBe(6000);
            script.ScriptData.Length.ShouldBe(ushort.MaxValue + 1);
            script.Strings.Values.Count.ShouldBeGreaterThan(0);
        }

        [Theory]
        [InlineData("data/downtown-2p.SCR")]
        [InlineData("data/MP2-2P.SCR")]
        public void Parse_should_succeed_with_no_strings(string path)
        {
            var sut = new ScriptParser();

            using var stream = TestGamePath.OpenFile(path);
            var script = sut.Parse(stream);

            script.Pointers.Length.ShouldBe(6000);
            script.ScriptData.Length.ShouldBe(ushort.MaxValue + 1);
            script.Strings.Values.Count.ShouldBe(0);
        }
    }
}