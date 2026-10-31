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

## Animated tiles

The map lists tile animations (water, neon signs, ...): a base tile, the tiles to show in turn, and how many game frames each is shown for
(see [format-notes.md](format-notes.md)). `TileAnimator` (owned by `LevelProvider`, updated from `MapComponent.Update`) plays them:
every game frame it works out which frame of each animation is due (`TileAnimation.GetFrameIndex`) and, when it changes, copies the
pixels of that frame's tile into the slot of the base tile in the tile texture array (`StyleTextureSet.CopyTile`). The map geometry is
never touched; blocks keep using the base tile number. A game frame is 1/26.6 s by default (`TileAnimator.GameFramesPerSecond`, tunable in the game with F8/F9). That rate was measured from a recording of the original game on `bil` (the 12-frame water, 3 game frames per frame, advanced every 112.8 ms over 197 steps). Fan reports say the
game runs at about 30 frames per second, 50 in the "fast" mode of some versions). Animations with a limited repeat count stop on their
last frame. `bil` has 16 animations, `ste` 14 and `wil` 11.

## Power line kinks (open)

The overhead power lines on layer 5 of `bil` (tiles 830/831, flat lids) show 2-3 px kinks at every tile join in our renderer, while
the original game draws them as one straight line. What was established (measured on screenshots at the same spot):

- The tile art itself has a one texel offset between tiles 830 and 831 when joined with the flips/rotations the map specifies
  (line centre on the diagonal at -2960.49 for 830 versus -2961.51 for 831). None of the 8 flip/rotate combinations removes it, so
  it is not a wrong flip or rotation direction in `SlopeGenerator.MapUv`.
- Wrap sampling bled the opposite tile edge (fixed by clamping); bilinear filtering is flat for the last half texel at a tile edge
  (bends the line a little); point sampling shows the one texel jog fully. The debug menu (World and player, F9) switches between the two.
- The original's line is smooth and soft (residual of a straight fit within about 0.5 px), so it hides or avoids the jog in a way we do not know yet.

Update: the tile art of 830 and 831 has one duplicated row where the tiles meet vertically (830's top row and 831's bottom row both have
the line at columns 46-47), which is the pixel that has to be cut off to make them line up. Wrap/clamp (F11) changes nothing for the
power lines (it only adds seams on solid surfaces). Sampling 63 of 64 texels per tile brings a simulated chain closest to a straight line
(residual range 2.6 -> 1.7 texels) but not all the way; it is available as an experiment (F12 cycles the tile mapping modes: normal, shared edge on flat faces, shared edge on all faces). The original loads one texture per tile
(see gta2-texture-loader) and uses UVs up to size - 0.0001, so it is not known why it shows no dwell.
