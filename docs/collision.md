# Old collision (`CollisionMap.cs`) — REPLACED

> **This describes the previous implementation, which has been deleted** (`CollisionMap`, `Line2D`, `LineSegment2D`).
> The replacement is the `OpenGta2.Physics` project plus `Levels/MapCollisionWorld.cs`; see [collision-design.md](collision-design.md).
> Kept as a record of what was wrong and why it was rewritten.

Entry point: `CollisionMap.CalculateMovement(Vector3 point, float radius, Vector2 delta) : Vector3`.
Created in `LevelProvider.LoadLevel`, used only by `PlayerControllerComponent`.

## Summary of the algorithm

A "swept point vs. expanded wall segment" test, one pass, no iteration:

1. `z = (int)point.Z`; everything is evaluated on that single layer.
2. Build a cell range covering the swept AABB `[min(p, p+delta) - r, max(p, p+delta) + r]`, expanded to at least ±1 cell around the origin cell.
3. For every cell in the range, `CheckWall(cell, point, r, delta)`:
   - Only checks faces *facing the origin cell*: `cell.X < origin.X` → `Right` face; `cell.X > origin.X` → `Left`; `cell.Y < origin.Y` → `Bottom`; `cell.Y > origin.Y` → `Top`.
     The origin cell's own faces are never checked.
   - A face is solid if `IsWall(cell, face)` **or** `IsWall(neighbour, opposite(face))`. `IsWall` returns false when `z` is outside the column.
   - Builds the wall segment pushed outward by `radius` on all sides (`GetWallSegmentForCircleCollision`), which is a Minkowski sum of the wall with an axis-aligned square, with the segment ends extended by `radius` too.
   - Intersects the infinite line through `origin → origin+delta` with the infinite line through that segment (`Line2D.Intersection`).
   - Rejects the hit if the intersection is behind the origin (`dot(delta, hit - origin) <= 0`), farther than `|delta|`, or more than 1.0 (squared distance `< 1f`) from the segment.
   - Otherwise records `WallCollision(HitPosition, Normal, DistanceSquared)`.
4. The nearest collision wins (`SetNearest`).
5. **On a hit, the result is the hit position**: the ped stops exactly on the expanded wall. No sliding, no handling of the remaining delta (the commented-out block shows an earlier attempt with a `depth` counter).
6. No hit → `point + delta`. Z is returned unchanged.

## Helpers

- `Line2D` – infinite line through two points; `Intersection` via homogeneous coordinates.
- `LineSegment2D` – `DistanceToLineSquared(point)`.
- `IntVector2/3` – integer cell coordinates with `Floor`/`Ceiling`.
- `MapExtensions.IsWall`, `BlockFaceExtensions.GetFace`.

## Debug output

With F1 on (Debug builds): purple box = every examined cell, red = intersection behind the origin,
green = accepted hit, yellow = intersection beyond the delta. Blue box = player. Hits also write to `Debug.WriteLine`.

## What it does not do

Slopes, diagonal blocks, partial blocks, the vertical axis, other entities, map bounds, sliding. See [collision-issues.md](collision-issues.md).
