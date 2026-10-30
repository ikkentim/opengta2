using System.Numerics;

namespace OpenGta2.Physics;

/// <summary>
/// Moves <see cref="CharacterBody"/>s through an <see cref="ICollisionWorld"/>: slides along walls, follows
/// surfaces (ramps, bridges), and applies gravity.
/// </summary>
public sealed class CharacterMover
{
    private const int MaxSlideIterations = 4;
    private const int MaxDepenetrationIterations = 4;
    private const int MaxSubSteps = 16;

    /// <summary>
    /// Distance kept between the character and walls after a collision.
    /// </summary>
    private const float Skin = 1e-3f;

    private readonly ICollisionWorld _world;
    private readonly List<WallSegment> _walls = new();
    private readonly List<Surface> _surfaces = new();

    public CharacterMover(ICollisionWorld world)
    {
        _world = world;
    }

    /// <summary>
    /// Gravity in blocks per second squared.
    /// </summary>
    public float Gravity { get; set; } = 25f;

    /// <summary>
    /// Maximum falling speed in blocks per second.
    /// </summary>
    public float MaxFallSpeed { get; set; } = 30f;

    /// <summary>
    /// A character that falls farther than this without landing is teleported to a nearby surface (see <see cref="Rescued"/>).
    /// </summary>
    public float MaxFallDistance { get; set; } = 8f;

    /// <summary>
    /// Raised when a character is teleported after a deep fall, with the old and the new position.
    /// </summary>
    public event Action<Vector3, Vector3>? Rescued;

    private const float RescueSearchRadius = 10f;
    private const float RescueSearchStep = 0.5f;
    private const int RescueDirections = 16;

    /// <summary>
    /// Moves the body for <paramref name="deltaTime"/> seconds at the desired horizontal velocity (blocks per second).
    /// </summary>
    public void Move(ref CharacterBody body, Vector2 desiredVelocity, float deltaTime)
    {
        if (deltaTime <= 0) return;

        // sub-step fast movement so surfaces are sampled often enough
        var distance = desiredVelocity.Length() * deltaTime;
        var steps = Math.Clamp((int)MathF.Ceiling(distance / MathF.Max(body.Radius, 0.01f)), 1, MaxSubSteps);
        var stepTime = deltaTime / steps;

        for (var i = 0; i < steps; i++)
        {
            Step(ref body, desiredVelocity, stepTime);
        }
    }

    private void Step(ref CharacterBody body, Vector2 velocity, float dt)
    {
        var previous = body.Position;

        MoveHorizontally(ref body, velocity * dt);
        var blocked = !MoveVertically(ref body, previous.Z, dt);

        if (blocked)
        {
            // walked into something taller than a step that has no wall: undo the horizontal move
            body.Position = new Vector3(previous.X, previous.Y, body.Position.Z);
        }
    }

    private void MoveHorizontally(ref CharacterBody body, Vector2 delta)
    {
        var position = body.PositionXY;
        var radius = body.Radius;
        var feet = body.Position.Z;

        var bounds = new Bounds2(Vector2.Min(position, position + delta), Vector2.Max(position, position + delta)).Expand(radius + 2 * Skin);
        _walls.Clear();
        _world.GetWalls(bounds, feet, feet + body.Height, _walls);

        // low walls can be stepped over
        var stepTop = feet + body.StepUp;
        var headTop = feet + body.Height;
        _walls.RemoveAll(w => w.ZMax <= stepTop || w.ZMin >= headTop);

        position = Depenetrate(position, radius);

        var remaining = delta;
        for (var i = 0; i < MaxSlideIterations && remaining.LengthSquared() > 1e-12f; i++)
        {
            SweepHit? hit = null;
            foreach (var wall in _walls)
            {
                var candidate = SweptCircle.Sweep(position, remaining, radius, wall.A, wall.B);
                if (candidate.HasValue && (!hit.HasValue || candidate.Value.Time < hit.Value.Time))
                {
                    hit = candidate;
                }
            }

            if (!hit.HasValue)
            {
                position += remaining;
                break;
            }

            var (time, normal) = hit.Value;
            position += remaining * time + normal * Skin;
            remaining *= 1 - time;

            // slide: drop the part of the remaining movement that goes into the wall
            var into = Vector2.Dot(remaining, normal);
            if (into < 0)
            {
                remaining -= normal * into;
            }
        }

        position = Depenetrate(position, radius);
        body.PositionXY = position;
    }

    /// <summary>
    /// Pushes the circle out of any wall it overlaps.
    /// </summary>
    private Vector2 Depenetrate(Vector2 position, float radius)
    {
        for (var i = 0; i < MaxDepenetrationIterations; i++)
        {
            var moved = false;

            foreach (var wall in _walls)
            {
                var closest = SweptCircle.ClosestPointOnSegment(position, wall.A, wall.B);
                var offset = position - closest;
                var distance = offset.Length();

                if (distance >= radius - SweptCircle.Epsilon) continue;

                Vector2 direction;
                if (distance > 1e-6f)
                {
                    direction = offset / distance;
                }
                else
                {
                    // dead centre on the wall; pick a side
                    var ab = wall.B - wall.A;
                    direction = ab.LengthSquared() > 0 ? Vector2.Normalize(new Vector2(-ab.Y, ab.X)) : Vector2.UnitX;
                }

                position += direction * (radius - distance);
                moved = true;
            }

            if (!moved) break;
        }

        return position;
    }

    /// <summary>
    /// Updates the height of the body. Returns false if the character cannot be at the new horizontal position.
    /// </summary>
    private bool MoveVertically(ref CharacterBody body, float previousFeet, float dt)
    {
        _surfaces.Clear();
        _world.GetSurfaces(body.Position.X, body.Position.Y, _surfaces);

        if (body.Grounded)
        {
            if (TryFindGroundedSurface(in body, previousFeet, out var height))
            {
                body.Position = new Vector3(body.Position.X, body.Position.Y, height);
                body.VelocityZ = 0;
                return true;
            }

            if (IsBlockedFromSide(in body, previousFeet))
            {
                return false;
            }

            body.Grounded = false;
            body.VelocityZ = 0;
            body.FallStartZ = previousFeet;
        }

        // airborne: don't drift sideways into solid ground that has no wall
        if (!body.Grounded && IsBlockedFromSide(in body, previousFeet))
        {
            return false;
        }

        body.VelocityZ = MathF.Max(body.VelocityZ - Gravity * dt, -MaxFallSpeed);
        var newFeet = previousFeet + body.VelocityZ * dt;

        if (body.VelocityZ <= 0 && TryFindLandingSurface(in body, previousFeet, newFeet, out var landing))
        {
            body.Position = new Vector3(body.Position.X, body.Position.Y, landing);
            body.VelocityZ = 0;
            body.Grounded = true;
        }
        else
        {
            body.Position = new Vector3(body.Position.X, body.Position.Y, newFeet);

            if (body.FallStartZ - newFeet > MaxFallDistance || newFeet < -1)
            {
                Rescue(ref body);
            }
        }

        return true;
    }

    /// <summary>
    /// Teleports a character that fell too far (through the world, or into a pit it can't land in) to the nearest
    /// standable surface, preferring surfaces close to the height it fell from.
    /// </summary>
    private void Rescue(ref CharacterBody body)
    {
        var origin = body.PositionXY;
        var from = body.Position;

        for (var distance = 0f; distance <= RescueSearchRadius; distance += RescueSearchStep)
        {
            var points = distance == 0 ? 1 : RescueDirections;
            var found = false;
            var best = default(Vector3);

            for (var i = 0; i < points; i++)
            {
                var angle = i * MathF.Tau / points;
                var candidate = origin + new Vector2(MathF.Cos(angle), MathF.Sin(angle)) * distance;

                _surfaces.Clear();
                _world.GetSurfaces(candidate.X, candidate.Y, _surfaces);

                foreach (var surface in _surfaces)
                {
                    if (surface.Headroom < body.Height) continue;
                    if (found && MathF.Abs(surface.Height - body.FallStartZ) >= MathF.Abs(best.Z - body.FallStartZ)) continue;
                    if (!HasClearance(candidate, surface.Height, in body)) continue;

                    best = new Vector3(candidate, surface.Height);
                    found = true;
                }
            }

            if (found)
            {
                body.Position = best;
                body.VelocityZ = 0;
                body.Grounded = true;
                Rescued?.Invoke(from, best);
                return;
            }
        }
    }

    /// <summary>
    /// Whether a character fits at the specified location without touching walls.
    /// </summary>
    private bool HasClearance(Vector2 position, float feet, in CharacterBody body)
    {
        _walls.Clear();
        _world.GetWalls(new Bounds2(position, position).Expand(body.Radius + 2 * Skin), feet, feet + body.Height, _walls);

        foreach (var wall in _walls)
        {
            if (wall.ZMax <= feet + body.StepUp || wall.ZMin >= feet + body.Height) continue;

            var closest = SweptCircle.ClosestPointOnSegment(position, wall.A, wall.B);
            if (Vector2.Distance(position, closest) < body.Radius) return false;
        }

        return true;
    }

    /// <summary>
    /// Finds the usable surface closest to the feet within step range.
    /// </summary>
    private bool TryFindGroundedSurface(in CharacterBody body, float feet, out float height)
    {
        var found = false;
        height = 0;

        foreach (var surface in _surfaces)
        {
            if (surface.Headroom < body.Height) continue;
            if (surface.Height > feet + body.StepUp || surface.Height < feet - body.StepDown) continue;

            if (!found || MathF.Abs(surface.Height - feet) < MathF.Abs(height - feet))
            {
                height = surface.Height;
                found = true;
            }
        }

        return found;
    }

    /// <summary>
    /// A grounded character that finds no surface in step range is blocked (rather than walking off a ledge)
    /// if there are surfaces above it but nothing to fall on below it.
    /// </summary>
    private bool IsBlockedFromSide(in CharacterBody body, float feet)
    {
        var anyAbove = false;
        foreach (var surface in _surfaces)
        {
            if (surface.Height <= feet + body.StepUp)
            {
                if (surface.Headroom >= body.Height)
                {
                    return false; // can fall onto this one
                }
            }
            else
            {
                anyAbove = true;
            }
        }

        return anyAbove;
    }

    /// <summary>
    /// Finds the highest usable surface the feet passed through while falling from <paramref name="oldFeet"/> to <paramref name="newFeet"/>.
    /// </summary>
    private bool TryFindLandingSurface(in CharacterBody body, float oldFeet, float newFeet, out float height)
    {
        var found = false;
        height = 0;

        foreach (var surface in _surfaces)
        {
            if (surface.Headroom < body.Height) continue;
            if (surface.Height > oldFeet + SweptCircle.Epsilon || surface.Height < newFeet) continue;

            if (!found || surface.Height > height)
            {
                height = surface.Height;
                found = true;
            }
        }

        return found;
    }
}
