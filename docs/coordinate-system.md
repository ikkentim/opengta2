# Coordinate system

This is the most confusing part of the codebase. Everything below is taken from the code.

## Axes (world space)

World space is in **block units**: 1.0 = one map block. A ped sprite is scaled `texture.Width / 64`, so one block = 64 sprite pixels.

| Axis | Positive direction | Notes |
|------|--------------------|-------|
| +X | right / east | `GtaVector.Right` |
| +Y | **down the screen** / south | `GtaVector.Down`. Map row index `y` grows downwards. |
| +Z | up, towards the camera ("skywards") | `GtaVector.Skywards`. Block layer index `z`, 0..7 |

`GtaVector` names these directions (`Left (-1,0,0)`, `Right`, `Up (0,-1,0)`, `Down (0,1,0)`, `Skywards (0,0,1)`).
`Up` is **−Y**, not +Z.

The camera sits at `target + Skywards * 8` looking straight down (`CreateLookAt(pos, pos - Skywards, Up)`).
The projection matrix has M11/M13 negated because MonoGame is RHS and the map is effectively LHS.

## Map addressing

`Map.GetColumn(x, y)` reads `CompressedMap.Base[y, x]` — **row = y, column = x**. The map is up to 256 x 256 columns with up to 8 layers.
Positions are `float`; the containing cell is `floor(pos)` (`IntVector2.Floor`).
`IntVector3.Floor` truncates via `(int)`, which differs from `floor` for negative values.

## Z / layers

A column (`ColumnInfo`) stores `Offset` and `Height`; blocks exist for `z in [Offset, Height)`. Everything outside is air.
A ped standing on the ground has `Position.Z` equal to the z-layer of the **first air block above the ground**, i.e. the
integer z whose floor is the top of the highest solid block. `Map.GetGroundZ(x, y)` computes that
(first block above index 0 whose lid tile is 0; falls back to `col.Height`).

The player spawns at `(11.5, 2.5, GetGroundZ(12, 2))`. This uses cell (12, 2) while x = 11.5 is in cell 11, which looks like an off-by-one in the spawn code.

`Position.Z` is constant for a ped today: nothing ever changes it.

## Faces

`Face` (flags): `Top = 1, Bottom = 2, Left = 4, Right = 8, Lid = 16`.
Faces are named by screen direction:

| Face | Lies on | Outward normal |
|------|---------|----------------|
| `Left` | x = cell.X | (−1, 0) |
| `Right` | x = cell.X + 1 | (+1, 0) |
| `Top` | y = cell.Y (north edge, since −Y is up) | (0, −1) |
| `Bottom` | y = cell.Y + 1 | (0, +1) |
| `Lid` | z = cell.Z + 1 | +Z |

## Rotation

`Ped.Rotation` is in radians. The heading vector in `PlayerControllerComponent` is:

```csharp
heading = (sin(-rotation), cos(-rotation))   // rotation 0 => (0, +1), i.e. facing +Y (down the screen)
```

Left/Right keys add/subtract `2π * dt` to the rotation. The sprite is drawn with `Matrix.CreateRotationZ(ped.Rotation)`.
The handedness of turning vs. the on-screen result has not been verified; re-derive it when movement is rewritten.
