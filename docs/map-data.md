# Map data model

Read by `MapReader` (`OpenGta2.GameData/Map/MapReader.cs`) into `Map` (a record). See also the format spec linked from [`gtadocs.md`](../gtadocs.md).

```
Map
 ├─ CompressedMap
 │   ├─ Base[y, x]     -> index into Columns
 │   ├─ Columns[]      -> ColumnInfo { Height, Offset, Blocks[] (indices into Blocks) }
 │   └─ Blocks[]       -> BlockInfo
 ├─ Objects[], Zones[], Animations[], Lights[]
```

`Map` helpers: `GetColumn(x, y)`, `GetBlock(x, y, z)` (throws if z is outside the column), `TryGetBlock`, `GetGroundZ(x, y)`.
Client extensions (`MapExtensions`): `GetBlock(IntVector3)`, `IsWall(cell, face)`.

**None of these bounds-check x/y.** A position outside the map throws `IndexOutOfRangeException`.

## ColumnInfo

`Height` is the exclusive top layer and `Offset` the number of empty blocks at the bottom, so valid blocks are
`z in [Offset, Height)` and `Blocks[z - Offset]` is the block at layer z.

## BlockInfo (12 bytes)

| Field | Meaning |
|-------|---------|
| `Left`, `Right`, `Top`, `Bottom`, `Lid` : `FaceInfo` | Per-face tile and flags |
| `Arrows` | Road direction arrows (for AI) |
| `SlopeType` : `SlopeInfo` | `GroundType` (2 bits) + `SlopeType` (6 bits) |

### FaceInfo bits

| Bits | Property | Notes |
|------|----------|-------|
| 0–9 | `TileGraphic` | 0 = no tile |
| 10 | `Wall` | side faces: blocks walking/driving |
| 11 | `BulletWall` | side faces: blocks bullets |
| 10–11 | `LightingLevel` | **lid only** (shares bits with Wall/BulletWall) |
| 12 | `Flat` | drawn as a transparent flat |
| 13 | `Flip` | |
| 14–15 | `Rotation` | 0/90/180/270 |

### GroundType
`Air, Road, Pavement, Field`. Per the format spec, `Air` means "nothing solid" and the bottom layer (z=0) is automatically water. In `bil.gmp` lid blocks with `Air` are common (11,595 of ~85,000): building roofs, slopes and decorations. Collision treats them as not there: no surface and no ceiling (see [collision-design.md](collision-design.md)). Many "building top" blocks are `Field` and do count as solid.

### SlopeType
67 values: 26° and 7° slopes in four directions (sub-steps `_1.._8`), 45° slopes, diagonal blocks, diagonal slopes,
partial blocks (left/right/top/bottom/corners/centre), `SlopeAbove`. Used by rendering (`SlopeGenerator`) and collision.
Collision derives walkable surface heights from slopes (`SlopeHeight`) and from partial, diagonal and diagonal-slope blocks (`BlockShape`); see [collision-design.md](collision-design.md).

## What "solid" means in the data

- Side faces with the `Wall` flag block movement. The flag may be set on only one of the two blocks sharing an edge,
  so the current code checks both sides (the cell's own face, and the neighbour's opposite face).
- A block's lid tile does not itself make a block "solid" for pedestrians — the player stands at the first air layer.
- Bridges, overpasses and tunnels: a column simply has several lid-bearing blocks with gaps between them; every lid block is a surface whose headroom is the gap up to the next lid block above. In `bil.gmp` about 9,300 cells have a surface with a gap of at least 0.6 above it that a character can walk through (underpasses, tunnels). Verified by walking random characters through them: they stay on the lower level and never clip into the ceiling.
- The 3-sided diagonal slope (lid tile 1023) is a wedge/spire placed on top of a diagonal block; per the spec, walking on top of diagonals is not supported, so those upper areas are unreachable by design.
