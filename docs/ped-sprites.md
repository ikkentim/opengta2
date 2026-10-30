# Ped sprites

Findings from rendering every ped sprite of `bil.sty` (474 sprites, default palette). The format spec
(<https://gtamp.com/gta2/gta2-style-sty-graphics-file-format/>) says nothing about ped frame layout, so everything
below is read off the images. **Items marked (guess) are unverified.**

Sprite numbers here are relative to the ped sprite base (`SpriteBase.PedOffset`), the same numbers
`StyleTextureSet.GetSpriteTexture(SpriteKind.Ped, number, remap)` takes.

## Layout

| Sprites | What they look like | Status |
|---------|---------------------|--------|
| 158 and up | The normal ped animations. The game already uses `158 + AnimationBase + frame` for walking (base 8) and idle (base 53), see `Ped.cs`. | confirmed by use |
| 0–139 | Special animations: body flat on the ground, falling or tumbling with arms out, legs up in the air, curled up. Mostly knocked-down, falling and hit reactions. | (guess) |
| 140–147 | Flattened body seen from above, getting wider and flatter. | (guess) run over / squashed |
| 148–150 | A ped shrinking to nothing: a small blob with hands, then three dots. | (guess) sinking, i.e. drowning |
| 151 | Arms-out ped in the air. | (guess) |
| 152–153 | Yellow-outlined skeleton. | (guess) electrocution |
| 154–155 | Grey skeleton. | (guess) electrocution (x-ray) |
| 156–157 | Upside-down ped. | (guess) |

Sprite sizes are 12 to 32 pixels wide and 10 to 32 tall (a block is 64 pixels).

## Drowning

GTA2 water is deadly, not swimmable. The format spec says the bottom layer (z=0) is automatically water and that a player who
lands on one drowns. Splash and sound are not in the ped frames; a splash is probably an object or effect sprite
(not looked for yet).

How it works now:
1. `MapCollisionWorld` marks surfaces on layer 0 as `Surface.Water` (and not `Standable`). They are solid, so the character lands on the water.
2. `CharacterMover` raises `EnteredWater` once when a character settles onto a water surface and sets `CharacterBody.InWater`.
3. `PlayerControllerComponent` reacts by calling `Ped.Drown()`: the player loses control, the ped plays sprites 148–150
   (`PedAnimation.Drowning`, 0.25 s per frame, plays once), disappears after 1 s, and after 1.5 s (`Ped.DrowningDuration`)
   respawns at the start (`Respawn`). The debug text shows `LastDrowning` with the position.
4. Not done: splash effect, splash/drowning sound, player death handling beyond the respawn ("wasted"), cars in water.

The sprite numbers 148–150 are still a guess; if the animation looks wrong in the game, change `Ped.AnimationBase` for
`PedAnimation.Drowning`. Other style files (`wil.sty`, `ste.sty`) need to be checked too.


## How to re-create the contact sheets

The sheets were made with a throwaway console app that read the style with `StyleReader`, looked up each sprite with
`style.SpriteEntries[SpriteBases.PedOffset + n]`, mapped its pixels through the sprite's default palette
(`PaletteIndex.PhysPalette[sprite.Number + PaletteBase.SpriteOffset]`, `PhysicalPalette.GetPalette`) and drew them
scaled 4x with the index printed above each frame. The logic is the same as `StyleTextureSet.CreateSpriteTexture`.

## Effect sprites and remaps (splash search)

- The code object sprites (552) and the ped sprites are **identical in `bil.sty`, `ste.sty` and `wil.sty`** (same pixels, same
  palettes), so the splash is not a level-specific sprite.
- The format documents say code objects are only placed by code, so nothing in the style file labels them
  (see [format-notes.md](format-notes.md)).
- The style has 6 **remap palettes for code objects**. Fixed a bug found on the way: `PaletteBase.CodeObjRemapOffset` added
  `CodeObjRemap` instead of `PedRemap`, so the remap palettes of code objects, map objects, user sprites and fonts were looked up
  47 palettes too early (3124 instead of 3171 for code objects). With the fix, the burst sprites 327-345 look green (remap 1),
  blue (2) and cyan-white (3) instead of only the default blue-white; remaps 4-6 look garbled.
- **Conclusion so far:** every sprite of the style file was paged through in the sprite viewer (F3) against the original game on `ste`,
  and none looks like the water splash, not even in the other remaps. No other file in the game folder holds sprites
  (`data/frontend` is menu screens, `nyc.gci` vehicle data). The splash is therefore most likely an effect that the game
  executable draws itself (the format document mentions tiles 992-1023 as used internally by the game). We have no data for it.
  The burst sprites 327-345 are the explosion fireball, not a splash.
