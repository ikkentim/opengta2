using System.Numerics;
using Shouldly;
using Xunit;

namespace OpenGta2.Physics.Tests;

public class SweptCircleTests
{
    private static readonly Vector2 A = new(0, 0);
    private static readonly Vector2 B = new(10, 0);

    [Fact]
    public void Head_on_hit_reports_time_and_normal()
    {
        var hit = SweptCircle.Sweep(new Vector2(5, -3), new Vector2(0, 4), 0.5f, A, B);

        hit.ShouldNotBeNull();
        hit.Value.Time.ShouldBe(0.625f, 1e-5f); // 2.5 of 4 units
        hit.Value.Normal.ShouldBe(new Vector2(0, -1));
    }

    [Fact]
    public void Hit_from_other_side_has_flipped_normal()
    {
        var hit = SweptCircle.Sweep(new Vector2(5, 3), new Vector2(0, -4), 0.5f, A, B);

        hit.ShouldNotBeNull();
        hit.Value.Normal.ShouldBe(new Vector2(0, 1));
    }

    [Fact]
    public void Stopping_short_of_the_wall_is_no_hit()
    {
        SweptCircle.Sweep(new Vector2(5, -3), new Vector2(0, 2), 0.5f, A, B).ShouldBeNull();
    }

    [Fact]
    public void Moving_parallel_is_no_hit()
    {
        SweptCircle.Sweep(new Vector2(0, -1), new Vector2(10, 0), 0.5f, A, B).ShouldBeNull();
    }

    [Fact]
    public void Moving_away_while_touching_is_no_hit()
    {
        SweptCircle.Sweep(new Vector2(5, -0.5f), new Vector2(0, -1), 0.5f, A, B).ShouldBeNull();
    }

    [Fact]
    public void Moving_into_while_touching_hits_at_zero()
    {
        var hit = SweptCircle.Sweep(new Vector2(5, -0.5f), new Vector2(0, 1), 0.5f, A, B);

        hit.ShouldNotBeNull();
        hit.Value.Time.ShouldBe(0f);
    }

    [Fact]
    public void Passing_beyond_the_end_is_no_hit()
    {
        SweptCircle.Sweep(new Vector2(11, -3), new Vector2(0, 6), 0.5f, A, B).ShouldBeNull();
    }

    [Fact]
    public void Hitting_the_end_cap_gives_a_diagonal_normal()
    {
        // aim at the segment end from the side so only the rounded end is touched
        var hit = SweptCircle.Sweep(new Vector2(10.3f, -3), new Vector2(0, 6), 0.5f, A, B);

        hit.ShouldNotBeNull();
        hit.Value.Normal.X.ShouldBeGreaterThan(0);
        hit.Value.Normal.Y.ShouldBeLessThan(0);
        hit.Value.Normal.Length().ShouldBe(1f, 1e-5f);
    }

    [Fact]
    public void Axis_aligned_movement_that_the_old_intersection_code_rejected_still_hits()
    {
        // regression for Line2D.Intersection returning null when u == 0
        var hit = SweptCircle.Sweep(new Vector2(0.5f, 0.5f), new Vector2(0, 1), 0.1f, new Vector2(0, 1), new Vector2(1, 1));

        hit.ShouldNotBeNull();
    }
}
