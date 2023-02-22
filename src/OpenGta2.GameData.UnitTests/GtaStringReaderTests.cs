using OpenGta2.GameData;
using OpenGta2.GameData.Riff;
using Shouldly;
using Xunit;

namespace OpenGta2.Data.UnitTests;

[Trait("Category", "DataTests")]
public class GtaStringReaderTests
{
    [Fact]
    public void Read_should_succeed()
    {
        using var stream = TestGamePath.OpenFile("data/e.gxt");
        using var riff = new RiffReader(stream);

        var sut = new GtaStringReader(riff);

        var result = sut.Read();

        result.Count.ShouldBe(2595);
        
        result.ShouldContainKeyAndValue("3648", "!mI knew you were up to the job, Comrade!");
    }
}