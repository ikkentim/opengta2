using OpenGta2.GameData.Map;
using Shouldly;
using Xunit;

namespace OpenGta2.Data.UnitTests;

public class SlopeHeightTests
{
    [Theory]
    [InlineData(SlopeType.None, 0.5f, 0.5f, 1f)]
    [InlineData(SlopeType.Up45, 0.5f, 1f, 0f)] // low edge at the bottom of the screen
    [InlineData(SlopeType.Up45, 0.5f, 0f, 1f)]
    [InlineData(SlopeType.Down45, 0.5f, 0f, 0f)]
    [InlineData(SlopeType.Down45, 0.5f, 1f, 1f)]
    [InlineData(SlopeType.Left45, 0f, 0.5f, 1f)]
    [InlineData(SlopeType.Left45, 1f, 0.5f, 0f)]
    [InlineData(SlopeType.Right45, 1f, 0.5f, 1f)]
    [InlineData(SlopeType.Right45, 0f, 0.5f, 0f)]
    [InlineData(SlopeType.Up26_1, 0.5f, 1f, 0f)]
    [InlineData(SlopeType.Up26_1, 0.5f, 0f, 0.5f)]
    [InlineData(SlopeType.Up26_2, 0.5f, 1f, 0.5f)]
    [InlineData(SlopeType.Up26_2, 0.5f, 0f, 1f)]
    [InlineData(SlopeType.Right7_3, 0f, 0.5f, 0.25f)]
    [InlineData(SlopeType.Right7_3, 1f, 0.5f, 0.375f)]
    [InlineData(SlopeType.Down7_8, 0.5f, 1f, 1f)]
    [InlineData(SlopeType.Left7_1, 1f, 0.5f, 0f)]
    public void Get_returns_height_of_slope(SlopeType slope, float x, float y, float expected)
    {
        SlopeHeight.Get(slope, x, y).ShouldBe(expected, 1e-5f);
    }

    [Fact]
    public void Gentle_slope_sections_connect_end_to_start()
    {
        // walking up a 7 degree slope along x: the high edge of section n is the low edge of section n + 1
        for (var i = 0; i < 7; i++)
        {
            var current = (SlopeType)((int)SlopeType.Right7_1 + i);
            var next = (SlopeType)((int)SlopeType.Right7_1 + i + 1);

            SlopeHeight.Get(current, 1, 0.5f).ShouldBe(SlopeHeight.Get(next, 0, 0.5f), 1e-5f);
        }
    }
}
