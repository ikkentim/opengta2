using OpenGta2.GameData.Style;
using Shouldly;
using Xunit;

namespace OpenGta2.Data.UnitTests;

public class PaletteBaseTests
{
    // the values of bil.sty, ste.sty and wil.sty
    private static PaletteBase GetPaletteBase() => new()
    {
        Tile = 992,
        Sprite = 2089,
        CarRemap = 37,
        PedRemap = 53,
        CodeObjRemap = 6,
        MapObjRemap = 1,
        UserRemap = 15,
        FontRemap = 9
    };

    [Fact]
    public void Remap_areas_follow_each_other_in_order()
    {
        var sut = GetPaletteBase();

        sut.SpriteOffset.ShouldBe(992);
        sut.CarRemapOffset.ShouldBe(992 + 2089);
        sut.PedRemapOffset.ShouldBe(sut.CarRemapOffset + 37);
        sut.CodeObjRemapOffset.ShouldBe(sut.PedRemapOffset + 53);
        sut.MapObjRemapOffset.ShouldBe(sut.CodeObjRemapOffset + 6);
        sut.UserRemapOffset.ShouldBe(sut.MapObjRemapOffset + 1);
        sut.FontRemapOffset.ShouldBe(sut.UserRemapOffset + 15);
    }

    [Fact]
    public void Remap_offsets_of_the_real_styles()
    {
        var sut = GetPaletteBase();

        sut.PedRemapOffset.ShouldBe(3118);
        sut.CodeObjRemapOffset.ShouldBe(3171);
        sut.MapObjRemapOffset.ShouldBe(3177);
        sut.UserRemapOffset.ShouldBe(3178);
        sut.FontRemapOffset.ShouldBe(3193);
    }
}
