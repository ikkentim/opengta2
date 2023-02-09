using OpenGta2.Data.Map;
using OpenGta2.Data.Riff;
using Shouldly;
using Xunit;

namespace OpenGta2.Data.UnitTests;

public class MapReaderTests
{
    [Fact]
    public void Read_should_read_compressed_map()
    {
        using var stream = TestGamePath.OpenFile("data/bil.gmp");
        using var riff = new RiffReader(stream);

        var sut = new MapReader(riff);

        var result = sut.Read();
        
        result.Width.ShouldBe(256);
        result.Height.ShouldBe(256);

        result.CompressedMap.Base[100, 100].ShouldBe<uint>(41903);
        result.CompressedMap.Columns[41903].Offset.ShouldBe<byte>(2);
        result.CompressedMap.Columns[41903].Height.ShouldBe<byte>(5);
        result.CompressedMap.Columns[41903].Blocks.Length.ShouldBe(3);
    }
    
    [Fact]
    public void Read_should_read_objects()
    {
        using var stream = TestGamePath.OpenFile("data/bil.gmp");
        using var riff = new RiffReader(stream);

        var sut = new MapReader(riff);

        var result = sut.Read();
        
        result.Objects.Length.ShouldBe(0);
    }
    
    [Fact]
    public void Read_should_read_animations()
    {
        using var stream = TestGamePath.OpenFile("data/bil.gmp");
        using var riff = new RiffReader(stream);

        var sut = new MapReader(riff);

        var result = sut.Read();
        
        result.Animations.Length.ShouldBe(16);
        result.Animations[1].Base.ShouldBe<ushort>(243);
        result.Animations[1].FrameRate.ShouldBe<byte>(1);
        result.Animations[1].Repeat.ShouldBe<byte>(0);
    }

    [Fact]
    public void Read_should_read_zones()
    {
        using var stream = TestGamePath.OpenFile("data/bil.gmp");
        using var riff = new RiffReader(stream);

        var sut = new MapReader(riff);

        var result = sut.Read();
        
        result.Zones.Length.ShouldBe(189);
        result.Zones[1].Name.ShouldBe("busstop2");
        result.Zones[1].Type.ShouldBe(ZoneType.BusStop);
    }
}