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

Plan for the game, when we get to it:
1. Expose "landed on a non-standable layer-0 surface" from `CharacterMover` (for example a `LandedInWater` event).
2. Play ped sprites 148–150 at the water position, then the usual death handling (the player gets "wasted").
3. Verify the 148–150 guess in the game before relying on it; the numbers may be different in other style files
   (`wil.sty`, `ste.sty`), so check those too.

## How to re-create the contact sheets

The sheets were made with a throwaway console app that read the style with `StyleReader`, looked up each sprite with
`style.SpriteEntries[SpriteBases.PedOffset + n]`, mapped its pixels through the sprite's default palette
(`PaletteIndex.PhysPalette[sprite.Number + PaletteBase.SpriteOffset]`, `PhysicalPalette.GetPalette`) and drew them
scaled 4x with the index printed above each frame. The logic is the same as `StyleTextureSet.CreateSpriteTexture`.
