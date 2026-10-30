using OpenGta2.GameData.Map;
using Shouldly;
using Xunit;

namespace OpenGta2.Data.UnitTests;

public class BlockShapeTests
{
    [Theory]
    [InlineData(SlopeType.PartialBlockLeft, 0.1f, 0.5f, true)]
    [InlineData(SlopeType.PartialBlockLeft, 0.6f, 0.5f, false)]
    [InlineData(SlopeType.PartialBlockTop, 0.5f, 0.1f, true)]
    [InlineData(SlopeType.PartialBlockTop, 0.5f, 0.6f, false)]
    [InlineData(SlopeType.PartialBlockRight, 0.9f, 0.5f, true)]
    [InlineData(SlopeType.PartialBlockRight, 0.4f, 0.5f, false)]
    [InlineData(SlopeType.PartialBlockBottom, 0.5f, 0.9f, true)]
    [InlineData(SlopeType.PartialBlockBottom, 0.5f, 0.4f, false)]
    [InlineData(SlopeType.PartialBlockTopLeftCorner, 0.1f, 0.1f, true)]
    [InlineData(SlopeType.PartialBlockTopLeftCorner, 0.9f, 0.1f, false)]
    [InlineData(SlopeType.PartialBlockTopRightCorner, 0.9f, 0.1f, true)]
    [InlineData(SlopeType.PartialBlockTopRightCorner, 0.1f, 0.9f, false)]
    [InlineData(SlopeType.PartialBlockBottomRightCorner, 0.9f, 0.9f, true)]
    [InlineData(SlopeType.PartialBlockBottomRightCorner, 0.1f, 0.1f, false)]
    [InlineData(SlopeType.PartialBlockBottomLeftCorner, 0.1f, 0.9f, true)]
    [InlineData(SlopeType.PartialBlockBottomLeftCorner, 0.9f, 0.1f, false)]
    [InlineData(SlopeType.PartialBlockCentre, 0.5f, 0.5f, true)]
    [InlineData(SlopeType.PartialBlockCentre, 0.2f, 0.5f, false)]
    // the solid half is opposite to the direction the diagonal face is facing
    [InlineData(SlopeType.DiagonalFacingUpRight, 0.1f, 0.9f, true)]
    [InlineData(SlopeType.DiagonalFacingUpRight, 0.9f, 0.1f, false)]
    [InlineData(SlopeType.DiagonalFacingDownRight, 0.1f, 0.1f, true)]
    [InlineData(SlopeType.DiagonalFacingDownRight, 0.9f, 0.9f, false)]
    [InlineData(SlopeType.DiagonalFacingDownLeft, 0.9f, 0.1f, true)]
    [InlineData(SlopeType.DiagonalFacingDownLeft, 0.1f, 0.9f, false)]
    [InlineData(SlopeType.DiagonalFacingUpLeft, 0.9f, 0.9f, true)]
    [InlineData(SlopeType.DiagonalFacingUpLeft, 0.1f, 0.1f, false)]
    public void Solid_polygon_covers_expected_area(SlopeType slope, float x, float y, bool solid)
    {
        var polygon = BlockShape.GetSolidPolygon(slope);

        polygon.Length.ShouldBeGreaterThan(0);
        BlockShape.Contains(polygon, x, y).ShouldBe(solid);
        BlockShape.GetSurfaceHeight(slope, 1, x, y).ShouldBe(solid ? 1f : (float?)null);
    }

    [Theory]
    [InlineData(SlopeType.None)]
    [InlineData(SlopeType.Up45)]
    [InlineData(SlopeType.DiagonalSlopeFacingUpRight)]
    public void Full_cell_shapes_have_no_polygon(SlopeType slope)
    {
        BlockShape.GetSolidPolygon(slope).Length.ShouldBe(0);
    }

    [Theory]
    [InlineData(SlopeType.DiagonalSlopeFacingUpRight, 0f, 1f, 1f)] // tip of the wedge: the bottom-left corner
    [InlineData(SlopeType.DiagonalSlopeFacingUpRight, 0f, 0f, 0f)] // diagonal is the low edge
    [InlineData(SlopeType.DiagonalSlopeFacingUpRight, 1f, 1f, 0f)]
    [InlineData(SlopeType.DiagonalSlopeFacingUpRight, 0.25f, 0.75f, 0.5f)]
    [InlineData(SlopeType.DiagonalSlopeFacingDownRight, 0f, 0f, 1f)] // rotated: tip at the top-left corner
    [InlineData(SlopeType.DiagonalSlopeFacingDownLeft, 1f, 0f, 1f)] // tip at the top-right corner
    [InlineData(SlopeType.DiagonalSlopeFacingUpLeft, 1f, 1f, 1f)] // tip at the bottom-right corner
    public void Wedge_slope_rises_to_its_corner(SlopeType slope, float x, float y, float expected)
    {
        BlockShape.GetSurfaceHeight(slope, BlockShape.WedgeLidTile, x, y)!.Value.ShouldBe(expected, 1e-5f);
    }

    [Theory]
    [InlineData(SlopeType.DiagonalSlopeFacingUpRight, 0.9f, 0.1f)] // the corner opposite to the tip is open air
    [InlineData(SlopeType.DiagonalSlopeFacingDownRight, 0.9f, 0.9f)]
    [InlineData(SlopeType.DiagonalSlopeFacingDownLeft, 0.1f, 0.9f)]
    [InlineData(SlopeType.DiagonalSlopeFacingUpLeft, 0.1f, 0.1f)]
    public void Wedge_slope_is_open_on_the_far_side(SlopeType slope, float x, float y)
    {
        BlockShape.GetSurfaceHeight(slope, BlockShape.WedgeLidTile, x, y).ShouldBeNull();
    }

    [Theory]
    [InlineData(SlopeType.DiagonalSlopeFacingUpRight, 0.1f, 0.9f, 1f)] // plateau
    [InlineData(SlopeType.DiagonalSlopeFacingUpRight, 1f, 0f, 0f)] // low corner
    [InlineData(SlopeType.DiagonalSlopeFacingUpRight, 0.5f, 0.5f, 1f)] // diagonal
    [InlineData(SlopeType.DiagonalSlopeFacingUpRight, 0.75f, 0.25f, 0.5f)]
    [InlineData(SlopeType.DiagonalSlopeFacingDownRight, 1f, 1f, 0f)] // rotated: low corner at the bottom-right
    [InlineData(SlopeType.DiagonalSlopeFacingDownLeft, 0f, 1f, 0f)] // bottom-left
    [InlineData(SlopeType.DiagonalSlopeFacingUpLeft, 0f, 0f, 0f)] // top-left
    public void Four_sided_diagonal_slope_has_plateau_and_ramp(SlopeType slope, float x, float y, float expected)
    {
        BlockShape.GetSurfaceHeight(slope, 5, x, y)!.Value.ShouldBe(expected, 1e-5f);
    }

    [Fact]
    public void Diagonal_slope_heights_are_continuous_across_the_diagonal()
    {
        foreach (var slope in new[] { SlopeType.DiagonalSlopeFacingUpRight, SlopeType.DiagonalSlopeFacingDownRight, SlopeType.DiagonalSlopeFacingDownLeft, SlopeType.DiagonalSlopeFacingUpLeft })
        {
            for (var t = 0.1f; t < 1; t += 0.2f)
            {
                // sample both sides of the main diagonal (x == y) and the anti-diagonal in all orientations
                foreach (var (x, y) in new[] { (t, t), (t, 1 - t) })
                {
                    var a = BlockShape.GetSurfaceHeight(slope, 5, x - 0.001f, y - 0.001f);
                    var b = BlockShape.GetSurfaceHeight(slope, 5, x + 0.001f, y + 0.001f);

                    MathF.Abs(a!.Value - b!.Value).ShouldBeLessThan(0.01f);
                }
            }
        }
    }
}
