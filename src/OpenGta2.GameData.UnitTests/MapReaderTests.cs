using OpenGta2.GameData.Map;
using Shouldly;
using Xunit;

namespace OpenGta2.Data.UnitTests;

[Trait("Category", "DataTests")]
public class MapReaderTests : RiffFileTestBase<MapReader>
{
    public MapReaderTests() : base("data/bil.gmp", riff => new MapReader(riff))
    {
    }

    [Fact]
    public void Read_should_read_compressed_map()
    {
        var result = Sut.Read();
        
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
        var result = Sut.Read();
        
        result.Objects.Length.ShouldBe(0);
    }
    
    [Fact]
    public void Read_should_read_animations()
    {
        var result = Sut.Read();
        
        result.Animations.Length.ShouldBe(16);
        result.Animations[1].Base.ShouldBe<ushort>(243);
        result.Animations[1].FrameRate.ShouldBe<byte>(1);
        result.Animations[1].Repeat.ShouldBe<byte>(0);
    }

    [Fact]
    public void Read_should_read_zones()
    {
        var result = Sut.Read();
        
        result.Zones.Length.ShouldBe(189);
        result.Zones[1].Name.ShouldBe("busstop2");
        result.Zones[1].Type.ShouldBe(ZoneType.BusStop);
    }
}