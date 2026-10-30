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
`Air, Road, Pavement, Field` — material of the block's top surface. Not used by game logic yet.

### SlopeType
67 values: 26° and 7° slopes in four directions (sub-steps `_1.._8`), 45° slopes, diagonal blocks, diagonal slopes,
partial blocks (left/right/top/bottom/corners/centre), `SlopeAbove`. Currently only used by rendering (`SlopeGenerator`).
Collision uses the straight slopes (`SlopeHeight`) to compute walkable surface height; diagonals and partial blocks are still treated as full blocks (see [collision-design.md](collision-design.md)).

## What "solid" means in the data

- Side faces with the `Wall` flag block movement. The flag may be set on only one of the two blocks sharing an edge,
  so the current code checks both sides (the cell's own face, and the neighbour's opposite face).
- A block's lid tile does not itself make a block "solid" for pedestrians — the player stands at the first air layer.
- Bridges, overpasses and tunnels: not yet investigated. How a column represents "air under a bridge" (blank-tile blocks
  vs. gaps) needs to be checked against real data when we implement the third axis.
