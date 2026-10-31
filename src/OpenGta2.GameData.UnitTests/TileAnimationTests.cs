using OpenGta2.GameData.Map;
using Shouldly;
using Xunit;

namespace OpenGta2.Data.UnitTests;

public class TileAnimationTests
{
    private static TileAnimation Create(byte frameRate, byte repeat, params ushort[] tiles) =>
        new() { Base = tiles[0], FrameRate = frameRate, Repeat = repeat, Tiles = tiles };

    [Theory]
    [InlineData(0, 0)]
    [InlineData(4, 0)] // each frame is shown for 5 game frames
    [InlineData(5, 1)]
    [InlineData(9, 1)]
    [InlineData(10, 2)]
    [InlineData(14, 2)]
    [InlineData(15, 3)]
    [InlineData(19, 3)]
    [InlineData(20, 0)] // and around again
    [InlineData(25, 1)]
    public void Frames_are_shown_for_frame_rate_game_frames(int gameFrame, int expectedIndex)
    {
        var sut = Create(5, 0, 608, 609, 610, 611);

        sut.GetFrameIndex(gameFrame).ShouldBe(expectedIndex);
    }

    [Fact]
    public void Repeat_zero_plays_forever()
    {
        var sut = Create(2, 0, 10, 20);

        sut.GetFrameIndex(2 * 2 * 1_000_000).ShouldBe(0);
        sut.GetFrameIndex(2 * 2 * 1_000_000 + 2).ShouldBe(1);
    }

    [Fact]
    public void Limited_repeat_stops_on_the_last_frame()
    {
        // 2 frames x 2 game frames x 2 times = 8 game frames
        var sut = Create(2, 2, 10, 20);

        sut.GetFrameIndex(0).ShouldBe(0);
        sut.GetFrameIndex(2).ShouldBe(1);
        sut.GetFrameIndex(4).ShouldBe(0); // second time
        sut.GetFrameIndex(7).ShouldBe(1);
        sut.GetFrameIndex(8).ShouldBe(1); // done
        sut.GetFrameIndex(1000).ShouldBe(1);
    }

    [Fact]
    public void GetTile_returns_tile_number_of_the_frame()
    {
        var sut = Create(1, 0, 21, 19);

        sut.GetTile(0).ShouldBe((ushort)21);
        sut.GetTile(1).ShouldBe((ushort)19);
        sut.GetTile(2).ShouldBe((ushort)21);
    }

    [Fact]
    public void Frame_rate_zero_is_treated_as_one()
    {
        var sut = Create(0, 0, 1, 2, 3);

        sut.GetFrameIndex(0).ShouldBe(0);
        sut.GetFrameIndex(1).ShouldBe(1);
        sut.GetFrameIndex(2).ShouldBe(2);
        sut.GetFrameIndex(3).ShouldBe(0);
    }

    [Fact]
    public void Animation_without_frames_shows_the_base_tile()
    {
        var sut = new TileAnimation { Base = 42, FrameRate = 3, Tiles = Array.Empty<ushort>() };

        sut.GetFrameIndex(10).ShouldBe(0);
        sut.GetTile(10).ShouldBe((ushort)42);
    }
}
