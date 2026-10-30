# Collision: known issues and rewrite requirements

Findings from reading the current code (not yet reproduced in-game unless noted).

## Bugs in the current implementation

1. **No sliding.** `CalculateMovement` returns `HitPosition` on any hit and discards the rest of the movement. The ped sticks to walls and can't glide along them or around corners.
2. **Stops exactly on the wall plane.** On the next frame the origin lies *on* the expanded segment, and the intersection maths (`dot <= 0` rejection etc.) is numerically fragile there, so the ped can tunnel or jitter. Needs a skin/epsilon or a proper push-out.
3. **The origin cell is never checked.** `CheckWall` skips faces of the cell the origin is in (it only looks at faces facing *toward* the origin from other cells). A ped that ends up inside a wall's expanded zone, or spawns near one, is unconstrained.
4. **`Line2D.Intersection` has the wrong degenerate check.** It returns `null` when `u == 0`, which can happen for legitimate non-parallel lines, but it does not check `w == 0` (the real parallel case), so it divides by zero and returns `Infinity/NaN`.
5. **The `< 1f` hit filter is a hack.** `DistanceToLineSquared(hit) < 1f` accepts intersections up to 1 block away from the wall segment. The intersection with the *infinite* wall line is only valid if it lies within the (already radius-extended) segment, so the threshold should be about 0. As is, it can block a ped that would pass beside a wall end.
6. **Square (AABB) expansion instead of a circle.** The radius is applied as a square, so diagonal approaches to corners are off, and convex wall end-points are not treated as circles.
7. **Hit selection uses only the distance to the intersection point.** With several candidate walls the "nearest" may be wrong for diagonal moves; `HitNormal` is computed but unused.
8. **Spawn off-by-one:** `GetGroundZ(12, 2)` vs. spawn x = 11.5 (cell 11).
9. **`Map.GetGroundZ` starts at `zo = 1`** (skips the lowest block of the column) and is not a general "height at (x, y)" query.
10. **No map-bounds check.** Walking past the map edge throws in `Map.GetColumn`.
11. `IntVector3.Floor` truncates toward zero instead of flooring.
12. `Debug.WriteLine` and diagnostic allocations in the hot path.

## Missing features

- **Z axis:** ped Z is constant. No gravity/falling, no ground height that varies, no stepping up curbs, no bridges/overpasses/tunnels (choosing which layer a ped is on at a given (x, y)), no slopes (26°, 7°, 45°).
- **Block shapes:** diagonal blocks, diagonal slopes and partial blocks produce walls that are not axis-aligned or don't fill the cell; only full-cell edge `Wall` flags are considered.
- **Solidity rules:** whether a ped can enter a block from the side when no `Wall` flag is set but the block is solid is unverified against the original game.
- **Entities:** no ped-vs-ped, ped-vs-car or ped-vs-object collision.
- **Vertical extent:** a ped has no height, and walls are evaluated on a single layer (`z = (int)Position.Z`), so a ped straddling two layers (on a slope, or falling) would miss walls in the other.
- **Cars** will need oriented-box collision on top of the same map queries.

## Requirements / goals for the rewrite

A starting point, to be refined together.

1. A documented **vertical model**: what `Position.Z` means (continuous height vs. layer), how ground height is queried at `(x, y, zHint)` including slopes, and how the active surface is chosen for bridges (e.g. the highest surface at or below the ped's feet plus a step height).
2. A movement pipeline: input → desired velocity → **move-and-slide** (iterate, project the remaining motion onto the wall, cap the iterations) → gravity / ground snap → final position. Likely a mover type that owns velocity and grounded state, so `PlayerControllerComponent` only supplies intent.
3. Proper circle-vs-segment and circle-vs-corner tests with correct handling of touching / starting-inside cases.
4. Block shape support in stages: full blocks → slopes (ground height) → diagonals / partials (arbitrary wall segments).
5. Map bounds treated as solid.
6. Pure logic without MonoGame types or `DiagnosticHighlight` in the core, so it can be **unit tested** (the repo already has an xUnit + Shouldly pattern in `OpenGta2.GameData.UnitTests`).
7. Keep the debug visualisation (F1 boxes, F2 slow motion) behind an optional hook; it has been useful.

## Open questions

- Fixed-timestep physics? Today everything runs on a variable `dt` in `Update`.
- Continuous float height with ground sampling, or a snapped-layer model like the original?
- How faithful to the original GTA2 feel (fixed-point maths, fixed tick rate) do we want to be?
