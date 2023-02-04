using Shouldly;
using Xunit;

namespace OpenGta2.Data.UnitTests;

public class GtaStringReaderTests
{
    [Fact]
    public void Read_should_succeed()
    {
        using var stream = TestGamePath.OpenFile("data/e.gxt");

        var sut = new GtaStringReader(new RiffReader(stream));

        var result = sut.Read();

        result.Count.ShouldBe(2595);
        
        result.ShouldContainKeyAndValue("3648", "!mI knew you were up to the job, Comrade!");
    }
}