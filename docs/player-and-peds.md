# Player and peds

## Ped (`Peds/Ped.cs`)

Plain data object: `Position` (Vector3), `Rotation` (radians), `Remap` (palette remap index), plus animation state
(`Idle` / `Walking`; base sprite 53 / 8, 12 / 8 frames, 0.2s / 0.06s per frame). `PedManager` is just a `List<Ped>`.

There is no velocity, no physics state, no grounded flag, and no collision shape beyond the radius the caller passes in.

## PlayerControllerComponent

Per update:

1. Read input: left/right = turn, forward/backward = move.
2. `Rotation += lr * 2π * dt` (one full turn per second).
3. `heading = (sin(-rot), cos(-rot))`, `delta = heading * fd * dt * 2` (2 blocks/s; x0.1 in F2 slow mode).
4. `Position = CollisionMap.CalculateMovement(Position, radius: 0.1, delta)`.
5. Animation = Walking if `fd != 0`, else Idle.

The player radius is `0.1` blocks. The commented-out `18/64 ≈ 0.28` is closer to the sprite width; it was lowered while debugging collision.
Movement is purely kinematic and 2D. There is no strafe, jump, fall or vertical movement.

## What the rewrite will need from here

- A place for per-ped movement state (velocity, grounded, current z, step height).
- Movement applied through a collision-aware mover rather than the player controller assigning `Position`.
- Z driven by the map (ground height under the ped, slopes), not by a spawn-time constant.
