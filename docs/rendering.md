# Rendering

## Map

`LevelProvider` builds renderable chunks of `8 x 8` columns (`ChunkSize = 8`).

- `Update(camera)`: computes the camera frustum's bounding box, converts it to chunk bounds, unloads chunks that left the box and loads new ones (`LoadChunk`).
- `LoadChunk`: for every block in the chunk's columns calls `SlopeGenerator.Push(ref block, offset, vertices, indices, flatIndices)`,
  which emits quads (position + tile data via `VertexPositionTile`) for faces, slopes, diagonals and partial blocks.
  Flat (transparent) faces are collected separately, **sorted by draw order**, and appended after the opaque indices in the same index buffer.
- `MapComponent.Draw` does two passes per chunk with `BlockFaceEffect`: `Opaque`, then `Flat`. Point lights from the map are collected per chunk
  (linear scan over all lights, no spatial structure) and are disabled when camera Z > 40 or in "noon" mode.
- Tile textures come from `StyleTextureSet`, built from the `.sty` file; `GetSpriteTexture(kind, number, remap)` produces sprite textures with palette remaps.

Rendering reads block data directly. It shares only the `Map` and the `Face` enum with collision.

## Peds

`PedManagerComponent` draws each ped as a quad (`WorldSpriteEffect`): sprite `158 + animBase + frame` of `SpriteKind.Ped`, with the ped's remap.
It draws a shadow first (black, 0.6 alpha, offset `4/64, 4/64`, z + 0.1), then the sprite (z + 0.2) at the ped's position.
Sprite size is `texture size / 64` blocks.

## Camera

`Camera` holds position, view/projection matrices and a frustum. `CameraComponent` in `AttachedToPed` mode sets the position to `ped.Position + (0, 0, 8)`.
Moving the free camera with the numpad detaches it. FOV is `PiOver4`; the far plane is `Position.Z + 1`.

## Text

`FontRenderer` draws GTA2 style-font sprites in screen space. The debug text uses a MonoGame `SpriteFont`.
