# Notes from the original format documents

Facts taken from the original GTA2 (GBH) documents "GTA2 Map Format", "GTA2 Style Format" and "GTA2 Scripting"
(kept outside the repo, in `D:\stack\projects\project_gta2\docs`). This page only records what matters for our code.
Older web versions of the same specs: see [`/gtadocs.md`](../gtadocs.md).

## Map format

- **Water:** the floor of the map (z=0) can have a lid but no sides, and cannot be a slope. Those blocks are automatically
  treated as water: the player drowns if he lands on one. Nothing else makes water. Water-looking tiles on higher layers
  (we found about 500 on `ste.gmp`) are ordinary blocks as far as the map format is concerned.
- **Ground type** (slope byte bits 0-1): 0 air, 1 road, 2 pavement, 3 field. Arrows may be placed on any ground type except air.
  The block under a slope has slope code 63 and ground type 0 (air).
- **Block 0** is always all zero: an air block without graphics.
- **Map objects** can't be placed where there is only air or water, or on top of blocks without proper collision such as diagonals.
- **Tile animation** (chunk `ANIM`): `base` tile marks an animating tile; `frame_rate` is the number of **game frames** each
  animation frame is shown for (so it is a delay, not a speed); `repeat` is how many times it plays (0 = forever);
  `anim_length` tiles follow. Example from `ste.gmp`: water, base 608, 12 frames (608-619), frame_rate 5. If the game runs
  at 30 game frames per second that is 6 water frames per second (the documents don't state the game frame rate).
  Our renderer does not play tile animations yet.
- **Lights** use `on_time`/`off_time` in game cycles, with a random `shape` variance.

## Style format

- Tile numbers 0-991 are real tiles; **992-1023 are used internally by the game** ("for asynchronous animation").
- **Code objects** (the document names blood and skidmarks as examples) are only placed by code, so the style file stores no
  information about them: no list of which sprite is what, no frame counts. The meaning of code object sprite numbers (for
  example the burst sprites around 327-345 that look like a water splash) can only be found out by looking at the original game.
- Map objects (bins, hot dog stands, ...) have a model number and a sprite count (`object_info`).
- Sprites are 256 colour, at most 128x64, at least 2x2, even sizes. Deltas modify sprites (car dents, doors, lights).
- The sprite number of a game item is its relative number plus the cumulative base for its kind (car, ped, code object, map object, user, font).

## Scripting

- `IS_CHAR_IN_AIR` ("is char falling?"): true while the character is doing the **falling animation**. So peds have a dedicated
  falling animation; it is probably among the ped sprites below 158 (arms-out tumbling frames, see [ped-sprites.md](ped-sprites.md)).
- `HAS_CHAR_JUST_SUNK` ("is the character sinking in water"): the game tracks a sinking state for characters; `HAS_CAR_JUST_SUNK`
  does the same for cars. Our `CharacterBody.InWater` / `CharacterMover.EnteredWater` is the equivalent.
- `DISPLAY MESSAGE` shows a message in the large bold font in the middle of the screen; this is how the "WASTED!" message is shown.
  Messages must be upper case.
- `IS_CAR_IN_AIR`: true when a car has left the ground (also triggered by hills).

## Open questions these documents raise

- What kills a character falling from a height onto solid ground (the "fake water" seen in the original game)? The documents
  don't mention fall damage; test in the original game.
- Which sprites are the splash? Not documented (see above).
