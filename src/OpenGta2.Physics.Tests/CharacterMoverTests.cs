using System.Numerics;
using Shouldly;
using Xunit;

namespace OpenGta2.Physics.Tests;

public class CharacterMoverTests
{
    private const float R = 0.15f;

    private static CharacterBody BodyAt(float x, float y, float z = 0) => new(new Vector3(x, y, z), R) { Grounded = true };

    private static void Run(CharacterMover mover, ref CharacterBody body, Vector2 velocity, float seconds, float dt = 1 / 60f)
    {
        for (var t = 0f; t < seconds - 1e-6f; t += dt)
        {
            mover.Move(ref body, velocity, dt);
        }
    }

    [Fact]
    public void Free_movement_reaches_the_target()
    {
        var mover = new CharacterMover(new FakeWorld());
        var body = BodyAt(0, 0);

        Run(mover, ref body, new Vector2(2, 0), 1);

        body.Position.X.ShouldBe(2f, 1e-3f);
        body.Position.Y.ShouldBe(0f, 1e-4f);
        body.Position.Z.ShouldBe(0f, 1e-4f);
    }

    [Fact]
    public void Walking_into_a_wall_stops_at_the_radius()
    {
        var world = new FakeWorld().Wall(5, -5, 5, 5);
        var mover = new CharacterMover(world);
        var body = BodyAt(0, 0);

        Run(mover, ref body, new Vector2(2, 0), 5);

        body.Position.X.ShouldBe(5 - R, 0.01f);
        body.Position.X.ShouldBeLessThanOrEqualTo(5 - R + 1e-4f);
    }

    [Fact]
    public void Glancing_a_wall_slides_along_it()
    {
        var world = new FakeWorld().Wall(5, -5, 5, 50);
        var mover = new CharacterMover(world);
        var body = BodyAt(0, 0);

        // 45 degrees into the wall
        Run(mover, ref body, new Vector2(2, 2), 3);

        body.Position.X.ShouldBe(5 - R, 0.01f);
        body.Position.Y.ShouldBeGreaterThan(3f); // kept moving along the wall
    }

    [Fact]
    public void Inside_corner_does_not_let_the_character_through()
    {
        var world = new FakeWorld().Wall(5, -5, 5, 5).Wall(-5, 5, 5, 5);
        var mover = new CharacterMover(world);
        var body = BodyAt(0, 0);

        Run(mover, ref body, new Vector2(2, 2), 6);

        body.Position.X.ShouldBeLessThan(5f);
        body.Position.Y.ShouldBeLessThan(5f);
        body.Position.X.ShouldBeGreaterThan(5 - 2 * R);
        body.Position.Y.ShouldBeGreaterThan(5 - 2 * R);
    }

    [Fact]
    public void Walking_around_the_end_of_a_wall_works()
    {
        var world = new FakeWorld().Wall(5, 0, 5, 5);
        var mover = new CharacterMover(world);
        var body = BodyAt(4, 4);

        // slide up along the wall end, then continue past it
        Run(mover, ref body, new Vector2(2, -3), 2);
        body.Position.Y.ShouldBeLessThan(0f);
        Run(mover, ref body, new Vector2(2, 0), 2);

        body.Position.X.ShouldBeGreaterThan(5f);
    }

    [Fact]
    public void Fast_movement_does_not_tunnel_through_walls()
    {
        var world = new FakeWorld().Wall(5, -5, 5, 5);
        var mover = new CharacterMover(world);
        var body = BodyAt(0, 0);

        mover.Move(ref body, new Vector2(1000, 0), 1);

        body.Position.X.ShouldBeLessThan(5f);
    }

    [Fact]
    public void Character_starting_inside_a_wall_is_pushed_out()
    {
        var world = new FakeWorld().Wall(5, -5, 5, 5);
        var mover = new CharacterMover(world);
        var body = BodyAt(5.02f, 0);

        mover.Move(ref body, Vector2.Zero, 1 / 60f);
        mover.Move(ref body, new Vector2(0.001f, 0), 1 / 60f);

        MathF.Abs(body.Position.X - 5).ShouldBeGreaterThanOrEqualTo(R - 1e-3f);
        body.Position.X.ShouldBeGreaterThan(5f); // stays on its own side
    }

    [Fact]
    public void Walls_below_step_height_can_be_walked_over()
    {
        var world = new FakeWorld().Wall(5, -5, 5, 5, zMin: 0, zMax: 0.2f);
        var mover = new CharacterMover(world);
        var body = BodyAt(0, 0);

        Run(mover, ref body, new Vector2(2, 0), 4);

        body.Position.X.ShouldBeGreaterThan(5f);
    }

    [Fact]
    public void Walls_above_the_character_do_not_block()
    {
        var world = new FakeWorld().Wall(5, -5, 5, 5, zMin: 3, zMax: 4);
        var mover = new CharacterMover(world);
        var body = BodyAt(0, 0);

        Run(mover, ref body, new Vector2(2, 0), 4);

        body.Position.X.ShouldBeGreaterThan(5f);
    }

    [Fact]
    public void Walking_up_a_ramp_follows_its_height()
    {
        var world = new FakeWorld
        {
            // ramp from x = 2 (height 0) to x = 4 (height 1)
            SurfaceFunc = (x, _) => new[] { new Surface(Math.Clamp((x - 2) / 2, 0, 1), float.PositiveInfinity) }
        };
        var mover = new CharacterMover(world);
        var body = BodyAt(0, 0);

        Run(mover, ref body, new Vector2(1, 0), 5);

        body.Position.X.ShouldBeGreaterThan(4f);
        body.Position.Z.ShouldBe(1f, 1e-3f);
        body.Grounded.ShouldBeTrue();
    }

    [Fact]
    public void Walking_off_a_ledge_makes_the_character_fall_and_land()
    {
        var world = new FakeWorld
        {
            // plateau at height 3 for x < 2, floor at 0 beyond
            SurfaceFunc = (x, _) => new[] { new Surface(x < 2 ? 3 : 0, float.PositiveInfinity) }
        };
        var mover = new CharacterMover(world);
        var body = BodyAt(0, 0, 3);

        Run(mover, ref body, new Vector2(1, 0), 2.2f);
        body.Grounded.ShouldBeFalse();
        body.Position.Z.ShouldBeLessThan(3f);

        Run(mover, ref body, Vector2.Zero, 2);
        body.Grounded.ShouldBeTrue();
        body.Position.Z.ShouldBe(0f, 1e-4f);
    }

    [Fact]
    public void Spawning_above_the_ground_falls_onto_it()
    {
        var mover = new CharacterMover(new FakeWorld());
        var body = new CharacterBody(new Vector3(0, 0, 2), R);

        Run(mover, ref body, Vector2.Zero, 2);

        body.Grounded.ShouldBeTrue();
        body.Position.Z.ShouldBe(0f, 1e-4f);
    }

    // road at 0 with a bridge deck at 2 (underside at 1.5) covering x >= 3
    private static FakeWorld BridgeWorld() => new()
    {
        SurfaceFunc = (x, _) => x >= 3
            ? new[] { new Surface(0, 1.5f), new Surface(2, float.PositiveInfinity) }
            : new[] { new Surface(0, float.PositiveInfinity) }
    };

    [Fact]
    public void Character_on_the_road_walks_under_a_bridge()
    {
        var mover = new CharacterMover(BridgeWorld());
        var body = BodyAt(0, 0, 0);

        Run(mover, ref body, new Vector2(2, 0), 3);

        body.Position.X.ShouldBeGreaterThan(5f);
        body.Position.Z.ShouldBe(0f, 1e-4f);
    }

    [Fact]
    public void Character_on_the_deck_stays_on_the_deck()
    {
        var mover = new CharacterMover(BridgeWorld());
        var body = BodyAt(3.5f, 0, 2);

        Run(mover, ref body, new Vector2(1, 0), 2);

        body.Position.X.ShouldBeGreaterThan(5f);
        body.Position.Z.ShouldBe(2f, 1e-4f);
    }

    [Fact]
    public void Character_falling_off_the_deck_lands_on_the_road()
    {
        var mover = new CharacterMover(BridgeWorld());
        var body = BodyAt(3.5f, 0, 2);

        Run(mover, ref body, new Vector2(-2, 0), 3);

        body.Position.X.ShouldBeLessThan(3f);
        body.Grounded.ShouldBeTrue();
        body.Position.Z.ShouldBe(0f, 1e-4f);
    }

    [Fact]
    public void Falling_onto_a_deck_lands_on_the_deck_not_the_road()
    {
        var mover = new CharacterMover(BridgeWorld());
        var body = new CharacterBody(new Vector3(4, 0, 4), R);

        Run(mover, ref body, Vector2.Zero, 2);

        body.Grounded.ShouldBeTrue();
        body.Position.Z.ShouldBe(2f, 1e-4f);
    }

    [Fact]
    public void Ramp_onto_a_deck_leads_up_to_the_deck()
    {
        var world = new FakeWorld
        {
            // ramp rises from height 0 at x = 0 to the deck (height 2) at x = 4; road below stays at 0 under the ramp
            SurfaceFunc = (x, _) => x < 4
                ? new[] { new Surface(Math.Clamp(x / 2, 0, 2), float.PositiveInfinity) }
                : new[] { new Surface(2, float.PositiveInfinity) }
        };
        var mover = new CharacterMover(world);
        var body = BodyAt(0, 0, 0);

        Run(mover, ref body, new Vector2(1, 0), 6);

        body.Position.X.ShouldBeGreaterThan(4f);
        body.Position.Z.ShouldBe(2f, 1e-3f);
    }

    [Fact]
    public void Gap_that_is_too_low_blocks_entry()
    {
        var world = new FakeWorld
        {
            // a low gap (headroom 0.3) between the floor and the block above at x >= 3
            SurfaceFunc = (x, _) => x >= 3
                ? new[] { new Surface(0, 0.3f), new Surface(1, float.PositiveInfinity) }
                : new[] { new Surface(0, float.PositiveInfinity) }
        };
        var mover = new CharacterMover(world);
        var body = BodyAt(0, 0);

        Run(mover, ref body, new Vector2(2, 0), 3);

        body.Position.X.ShouldBeLessThan(3f);
        body.Position.Z.ShouldBe(0f, 1e-4f);
    }

    [Fact]
    public void Falling_character_does_not_drift_into_a_solid_block_without_walls()
    {
        var world = new FakeWorld
        {
            // block top at 3 for x >= 3, floor at 0 elsewhere
            SurfaceFunc = (x, _) => new[] { new Surface(x >= 3 ? 3 : 0, float.PositiveInfinity) }
        };
        var mover = new CharacterMover(world);
        var body = new CharacterBody(new Vector3(0, 0, 2), R);

        Run(mover, ref body, new Vector2(2, 0), 3);

        body.Position.X.ShouldBeLessThan(3f);
        body.Grounded.ShouldBeTrue();
        body.Position.Z.ShouldBe(0f, 1e-4f);
    }

    [Fact]
    public void Endless_fall_teleports_to_the_nearest_surface()
    {
        var world = new FakeWorld
        {
            // a pit without any surface for x < 5
            SurfaceFunc = (x, _) => x < 5 ? Array.Empty<Surface>() : new[] { new Surface(2, float.PositiveInfinity) }
        };
        var mover = new CharacterMover(world);
        Vector3? from = null, to = null;
        mover.Rescued += (a, b) => (from, to) = (a, b);
        var body = new CharacterBody(new Vector3(3, 0, 2), R);

        Run(mover, ref body, Vector2.Zero, 3);

        to.ShouldNotBeNull();
        from!.Value.Z.ShouldBeLessThan(0f); // rescued once below the world
        body.Grounded.ShouldBeTrue();
        body.Position.X.ShouldBeGreaterThanOrEqualTo(5f);
        body.Position.X.ShouldBeLessThan(6f); // nearest edge, not somewhere far away
        body.Position.Z.ShouldBe(2f, 1e-4f);
    }

    [Fact]
    public void Rescue_skips_surfaces_that_are_not_standable()
    {
        var world = new FakeWorld
        {
            // a roof (not standable) right next to the pit, solid ground further away
            SurfaceFunc = (x, _) => x < 5
                ? Array.Empty<Surface>()
                : x < 8 ? new[] { new Surface(2, float.PositiveInfinity, Standable: false) } : new[] { new Surface(2, float.PositiveInfinity) }
        };
        var mover = new CharacterMover(world);
        var body = new CharacterBody(new Vector3(3, 0, 2), R);

        Run(mover, ref body, Vector2.Zero, 3);

        body.Grounded.ShouldBeTrue();
        body.Position.X.ShouldBeGreaterThanOrEqualTo(8f);
    }

    [Fact]
    public void Rescue_avoids_spots_inside_walls()
    {
        var world = new FakeWorld
        {
            SurfaceFunc = (x, _) => x < 5 ? Array.Empty<Surface>() : new[] { new Surface(0, float.PositiveInfinity) }
        }.Wall(5.1f, -50, 5.1f, 50);
        var mover = new CharacterMover(world);
        var body = new CharacterBody(new Vector3(3, 0, 0), R);

        Run(mover, ref body, Vector2.Zero, 3);

        body.Grounded.ShouldBeTrue();
        MathF.Abs(body.Position.X - 5.1f).ShouldBeGreaterThanOrEqualTo(R);
    }

    [Fact]
    public void Normal_falls_are_not_rescued()
    {
        var mover = new CharacterMover(new FakeWorld());
        var rescued = false;
        mover.Rescued += (_, _) => rescued = true;
        var body = new CharacterBody(new Vector3(0, 0, 7), R);

        Run(mover, ref body, Vector2.Zero, 2);

        rescued.ShouldBeFalse();
        body.Position.Z.ShouldBe(0f, 1e-4f);
    }

    [Fact]
    public void Tall_step_without_a_wall_blocks_instead_of_swallowing_the_character()
    {
        var world = new FakeWorld
        {
            SurfaceFunc = (x, _) => new[] { new Surface(x >= 3 ? 1 : 0, float.PositiveInfinity) }
        };
        var mover = new CharacterMover(world);
        var body = BodyAt(0, 0);

        Run(mover, ref body, new Vector2(2, 0), 3);

        body.Position.X.ShouldBeLessThan(3f);
        body.Grounded.ShouldBeTrue();
        body.Position.Z.ShouldBe(0f, 1e-4f);
    }
}
