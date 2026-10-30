# Collision / movement rewrite: design proposal

Status: **stages 1–5 implemented** (full blocks, straight slopes, partial blocks, diagonal blocks, diagonal slopes). Remaining work is polish, see the known gaps below.
Motivation and old bugs: [collision-issues.md](collision-issues.md).

What exists:
- `src/OpenGta2.Physics`: `CharacterMover`, `CharacterBody`, `SweptCircle`, `ICollisionWorld` (no MonoGame dependency).
- `src/OpenGta2.Physics.Tests`: 29 tests against a `FakeWorld` (walls, slides, corners, tunneling, ramps, ledges, bridges, low gaps).
- `src/OpenGta2.Client/Levels/MapCollisionWorld.cs`: map-backed `ICollisionWorld`. Walls = `Wall`-flagged side faces (either side of an edge), map border solid.
  Surfaces = tops of lid-bearing blocks; headroom = gap to the underside of the next lid-bearing block above.
- `Ped.Body` (`CharacterBody`) is moved by `CharacterMover` from `PlayerControllerComponent`. Player radius 0.15, height 0.6, step 0.35.

Implementation notes / deviations from the proposal below:
- A character that finds no usable surface in step range is *blocked* (horizontal move reverted, no sliding) when surfaces exist above and none below it. This stops solid blocks without wall flags from swallowing the player, grounded or falling.
- Walls are filtered by the mover: lower than `feet + StepUp` are ignored, above the head are ignored.
- **Slopes:** `GameData/Map/Models/SlopeHeight.cs` gives the surface height of 45°/26°/7° slopes (mirrors `SlopeGenerator`: the lid rises from a low edge to a high edge in the named direction; 26° blocks cover half and 7° blocks an eighth of the height each, chained along the slope). `MapCollisionWorld.GetSurfaces` samples it at the exact position, so the player's Z follows ramps. The underside of a block above a slope is treated as flat.
- **Partial and diagonal blocks (stage 5):** `GameData/Map/Models/BlockShape.cs` describes them, mirroring `SlopeGenerator`.
  - *Partial blocks* (halves and corners 24/64 wide, centre post 16/64) and *diagonal blocks* (solid triangle, solid on the side opposite to the direction the diagonal face is facing) are full-height solid polygons. `MapCollisionWorld` emits the polygon outline as wall segments (including the diagonal hypotenuse) and gives the block a surface only inside the polygon. The wall flags on the cell's own faces are ignored for these blocks; the neighbour's flags still count.
  - *Diagonal slopes*: the 3-sided kind (lid tile 1023) is a wedge that rises from the diagonal towards one corner and is open air on the other side; the 4-sided kind is a flat plateau on one half with a ramp down to the opposite corner. Both are surfaces only (no extra walls; the legs rely on the wall flags).
  - Verified on `bil.gmp` (scratch harness): charging each of 400 sampled partial/diagonal blocks from six directions never ends inside the block (834 charges), and 500 random walkers never end inside one.
- **Deep falls:** `CharacterMover` teleports a character that has fallen more than `MaxFallDistance` (8) without landing, or dropped below Z = −1, to the nearest standable surface (spiral search up to 10 blocks, must have headroom and wall clearance; among surfaces at the same distance, the one closest to the height it fell from). Raises `CharacterMover.Rescued(from, to)`.
- **Ground type `Air` is not solid.** Per the format spec `Air` means nothing solid, and the data agrees: for example the ramp at (10,10) leads onto a walkway at height 5 along x=10 that has `Air`-type lid blocks directly above it at (10,5) and (10,6). Treating those as ceilings dropped the player through the walkway (found while play-testing). So `MapCollisionWorld.GetSurfaces` ignores `Air` blocks completely: no surface, and no ceiling for what is below. A consequence: falling characters pass through roofs and `Air`-only columns; columns that contain nothing but `Air` (found near the map edge) have no ground at all, so the deep-fall rescue teleports characters out of them.
- **Water:** `Surface.Standable` is false for layer 0 (the format spec says bottom-layer blocks are water). It stays solid, so nothing falls through it, but the fall rescue never teleports a character onto one.
- **Water:** `MapCollisionWorld` marks layer-0 surfaces `Surface.Water`; `CharacterMover` raises `EnteredWater` and sets `CharacterBody.InWater` when a character settles on one. The player then drowns (see [ped-sprites.md](ped-sprites.md)) and respawns. Verified on `bil.gmp`: of 200 sampled shore walks, every one that ended in water raised the event exactly once.
- Known gaps: no splash/sound, no real death handling beyond the respawn, cars.
- Verified on `bil.gmp` with 500 random walkers (scratch harness, not in repo): no out-of-bounds/NaN, no fall-throughs, no rescues needed, largest grounded height change per frame 0.33 (< step height 0.35).

## 1. Vertical model

- `Ped.Position.Z` is a **continuous feet height** in block units (z = 3.0 means standing on top of layer 2).
- A ped has a `Height` (about 0.6 blocks; tune later) and a `StepHeight` (about 0.35: curbs and the first steps of a slope are walkable, a full block is not).
- A ped is **grounded** (standing on a surface) or **airborne** (has `VelocityZ`, gravity applies). Falling off a bridge edge needs nothing special: the ground below is further than `StepHeight`, so the ped goes airborne.

### Surface query

Scan the column at `floor(x), floor(y)` from the top layer down. For each block compute its walkable surface height at the sub-cell position `(x - floor(x), y - floor(y))`:

| Block kind | Surface height |
|------------|----------------|
| Full block with a lid tile | `z + 1` |
| Slope (26°, 7°, 45°) | `z + slopeHeight(slopeType, fx, fy)` (table derived from the same data `SlopeGenerator` uses) |
| Air (no lid) | none, keep scanning down |
| Diagonal / partial blocks | stage 4 |

**A column can have several walkable surfaces at once** (road under a bridge, a deck on top, a tunnel floor, stacked overpasses). The world therefore exposes *all* surfaces at a point, not just one:

```csharp
struct Surface { float Height; float Headroom; }   // Headroom = distance up to the next solid thing above (next surface's underside / ceiling / sky = infinity)
void GetSurfaces(float x, float y, List<Surface> results);   // ordered bottom to top
```

A surface is **usable** by a ped if `Headroom >= ped.Height` (a ped can't walk into a gap under a deck that is too low).

Selection is by **continuity with the ped's current feet**, in `CharacterMover`, not in the map code:
- Grounded: pick the usable surface closest to `feetZ` with `Height <= feetZ + StepHeight` and `Height >= feetZ - StepDown` (small snap-down distance, so ramps and curbs stay glued but a bridge edge lets go).
- Airborne: pick the highest usable surface with `Height <= feetZ` (land on the first thing you pass through).
- If nothing qualifies while grounded, the ped goes airborne.

Consequences, all without explicit layer bookkeeping:
- Under a bridge: feet are at road height, the deck is above `feetZ + StepHeight`, so the road is chosen. On the deck: the deck is chosen and the road below is ignored. Both can be walked at the same (x, y) simultaneously.
- Entering a ramp that leads onto a bridge: the ramp surface is continuous with the feet, so the ped follows it up and arrives on the deck; the road under the ramp is `> StepDown` below and ignored.
- Walls are filtered by the ped's `[feetZ, feetZ + Height]` span, so a wall on the deck doesn't block a ped on the road beneath it, and vice versa.
- Open verification item: confirm in real data how bridge, tunnel and "slope above" columns look (this is also where `SlopeType.SlopeAbove` matters).

### Wall query

`GetWalls(bounds, zMin, zMax, results)` yields 2D segments (with outward normal) for every `Wall`-flagged face, checking both the cell's own face and the neighbour's opposite face as today. Layers considered are those overlapping `[feetZ + epsilon, feetZ + Height]`; a wall in the layer the ped's feet are standing on top of does not count (the ped is above it). Out-of-map is treated as solid. Diagonal blocks later emit non-axis-aligned segments through the same interface, so the solver doesn't change.

## 2. Movement pipeline

```
intent (turn, move)  ->  desired velocity (XY)
                      ->  MoveAndSlide (XY, circle vs segments)
                      ->  vertical step: ground snap OR gravity
                      ->  write Position / grounded
```

**MoveAndSlide** (all in 2D, circle of radius `r`):
1. Gather segments in the swept bounds (cached for the whole iteration).
2. Find the earliest time of impact `t` of the circle against each segment's body and its two end-point circles (standard swept circle vs. segment).
3. Move to `t` minus a small skin (about 0.001), remove the velocity component into the contact normal, and continue with the remaining time.
4. At most 4 iterations; this slides along walls and around corners.
5. A final depenetration pass pushes the circle out of any segment it overlaps (handles spawn inside geometry and float drift). This replaces the "stop exactly on the plane" behaviour.

**Vertical step** after XY:
- Grounded: `GetSurfaces(newX, newY)`, pick by the grounded rule above. If one is found, snap `feetZ` to it; otherwise become airborne.
- Airborne: `vz -= gravity * dt`, `feetZ += vz * dt`; land on the surface chosen by the airborne rule if the feet crossed it this step. Clamp fall speed.
- Walls are queried with the *post-snap* height so a ped climbing a slope sees the right layers.

## 3. Code structure

New project **`OpenGta2.Physics`** (net10.0, `System.Numerics`, no MonoGame) so it is unit testable:

```csharp
interface ICollisionWorld {
    void GetWalls(Bounds2 bounds, float zMin, float zMax, List<WallSegment> results);
    void GetSurfaces(float x, float y, List<Surface> results);   // all walkable surfaces at (x, y), bottom to top
}
struct CharacterBody { Vector3 Position; Vector3 Velocity; float Radius, Height, StepHeight; bool Grounded; }
static class CharacterMover { static void Move(ref CharacterBody body, Vector2 desiredVelocity, float dt, ICollisionWorld world); }
```

- `MapCollisionWorld : ICollisionWorld` lives in the Client (or Physics referencing GameData) and wraps `Map`. It replaces `CollisionMap`.
- `Ped` gets a `CharacterBody` (or the fields directly); `PlayerControllerComponent` only supplies `desiredVelocity` and rotation.
- Tests: a `FakeWorld` built from a few hand-placed segments and ground heights. Cases: head-on wall, glancing wall (slides), inside corner, outside corner end-point, start touching a wall, start inside a wall, tunneling at high speed, walk up a ramp, walk off a ledge and land, walk under a bridge, walk on the deck above the same (x, y), ramp onto a deck, fall from a deck onto the road, a too-low gap that must block entry.
- Debug draw: `CharacterMover` takes an optional `ICollisionDebug` sink; the Client implements it with `DiagnosticHighlight`.

## 4. Staging

1. `Physics` project, `CharacterMover` XY only, with the `FakeWorld` tests. **No game integration yet.**
2. `MapCollisionWorld` for full blocks (walls + flat ground), bounds as solid, fix spawn. Replace `CollisionMap`; delete `Line2D`/`LineSegment2D` if unused.
3. Vertical: ground snap, gravity, bridges. Fix `GetGroundZ`.
4. Slopes (ground-height table).
5. Diagonal and partial blocks (extra wall segments).
6. Later: fixed timestep, ped-vs-ped, cars (oriented boxes on the same segment queries).

## 5. Decisions to confirm

- **Continuous float height** (above) vs. the original's snapped-layer model: I recommend continuous, since slopes and falling are far simpler.
- **Variable `dt` for now.** The mover is written as `Move(dt)` so a fixed timestep can be dropped in later. Substep if `speed * dt` exceeds the radius.
- **Project layout**: new `OpenGta2.Physics` project (recommended) vs. a folder inside Client.
- **Ped size**: radius about 0.15 and height about 0.6 as a start; the original sprite is about 18/64 wide.
