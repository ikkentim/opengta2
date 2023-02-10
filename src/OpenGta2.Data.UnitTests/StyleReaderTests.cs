using OpenGta2.Data.Style;
using Shouldly;
using Xunit;

namespace OpenGta2.Data.UnitTests;

[Trait("Category", "DataTests")]
public class StyleReaderTests : RiffFileTestBase<StyleReader>
{
    public StyleReaderTests() : base("data/bil.sty", riff => new StyleReader(riff))
    {
    }
    
    [Fact]
    public void Read_should_read_tiles()
    {
        var result = Sut.Read();
        
        result.Tiles.Count.ShouldBe(992);
    }
    
    [Fact]
    public void Read_should_read_palette_base()
    {
        var result = Sut.Read();

        result.PaletteBase.Sprite.ShouldBe<ushort>(2089);
    }
    
    [Fact]
    public void Read_should_read_palette_index()
    {
        var result = Sut.Read();
        
        result.PaletteIndex.PhysPalette[500].ShouldBe<ushort>(15);
        result.PaletteIndex.PhysPalette[5].ShouldBe<ushort>(0);
    }

    [Fact]
    public void Read_should_read_physical_palette()
    {
        var result = Sut.Read();

        result.PhysicsalPalette.GetPalette(0).GetColor(0).Argb.ShouldBe(0u);
        result.PhysicsalPalette.GetPalette(0).GetColor(1).Argb.ShouldBe(4279242760u);
    }

}