# Architecture

## Projects (`src/`)

| Project | Target | Purpose |
|---------|--------|---------|
| `OpenGta2.GameData` | net10.0 | Engine-agnostic readers/models for GTA2 formats: RIFF, map (`.gmp`), style (`.sty`), audio (`.sdt`/`.raw`), scripts (`.scr`). No MonoGame dependency. |
| `OpenGta2.Client` | net10.0-windows | MonoGame (WindowsDX 3.8.4.1) game. Rendering, input, player, collision. |
| `OpenGta2.GameData.UnitTests` | net10.0 | xUnit + Shouldly tests for readers and the script parser. **No tests exist for anything in Client (collision included).** |
| `OpenGta2.DebugConsole` | net10.0 | Scratch console app for poking at game data / the script runtime. |

## Game loop and scenes

`GtaGame : Game` owns a `Controls` instance and an `AssetManager` (both registered as game services).
On the first `Update` it calls `FirstUpdate()`, which activates a `Scene`. (Activating from `LoadContent` does not work:
components would not get initialized.)

A `Scene` is a `GameComponent` whose `Initialize()` adds the real components to `Game.Components`.
`ActivateScene` disposes and clears all existing components first.

Scenes:

- `LoadingWorldScene(map, style, next)` – creates/reuses the `LevelProvider` service, loads the level, activates `next`.
- `TestWorldScene` – the playable test world. Registers `Camera` and `PedManager` as services and adds, in order:
  `AudioTestComponent`, `MapComponent`, `SpriteTestComponent`, `PlayerControllerComponent`, `PedManagerComponent`,
  `CameraComponent`, `DebuggingDrawingComponent`.
- `IntroScene` – plays the Bink intro, then activates the next scene (currently commented out in `FirstUpdate`).

Components are built through `ComponentActivator<T>` (constructor injection from `Game.Services`; the first ctor parameter is the `GtaGame`).
Update order is the add order, so **the player update runs before the camera update** (the camera follows the already-moved ped).

## Services

`LevelProvider` (map, style, textures, `CollisionMap`), `Camera`, `PedManager`, `Controls`, `AssetManager`.

## Controls

Defined in `Controls.GetKey` (hard-coded for now):

| Action | Key |
|--------|-----|
| Forward / Backward | Up / Down |
| Turn left / right | Left / Right |
| Shoot | Left Ctrl (unused) |
| Menu (exits the game) | Esc |

`IsKeyDown` = pressed this frame (edge), `IsKeyPressed` = held.

## Debug keys

F1 opens the debug menu (`DebugMenuComponent`); F2/F3 move up/down, F4 opens the selected screen, F1 goes back (and closes the menu).
Every screen is an `IDebugScreen` (`Diagnostics/Screens/`) and the keys of the open screen are drawn at the bottom left of the window.
Screens use F5 and up (F1-F4 belong to the menu).

| Screen | Keys |
|--------|------|
| World and player (`WorldDebugScreen`) | N dusk/noon lighting, S sharp bilinear filtering (`DebugSettings.SharpTexels`, crisp texels blended over one screen pixel; needs point sampling off), A flat faces alpha blend / alpha test 0.5 (`DebugSettings.FlatAlphaTest`), F5 collision boxes (`DebugSettings.ShowHighlights`), F6 slow player x0.1 (`DebugSettings.SlowPlayer`), F7/F8 tile animations slower/faster by 2 game frames per second (default 26.6, measured from the original), F9 map textures point/bilinear sampling (`DebugSettings.PointSampling`, default bilinear; see the power line notes in rendering.md), F11 clamp/wrap at tile edges (`DebugSettings.WrapSampling`, default clamp), F12 tile mapping, default SharedEdgeAll, cycles Full / SharedEdgeFlat (flat faces only) / SharedEdgeAll (`DebugSettings.TileSpan`), F10 outlines of the transparent (flat) faces in magenta (`DebugSettings.ShowFlatOutlines`), Numpad 5 teleport (works everywhere) |
| Sprites (`SpriteDebugScreen`) | grid of 8 x 8 sprites with numbers: PageUp/PageDown flip pages, Home/End first/last page; F5 grid/single sprite (PageUp/PageDown step by 1, Home/End by 10, Enter plays on); F6 kind (car, ped, code object, map object, user, font); F7 additive blending; F8 remap palette. The numbers are what `GetSpriteTexture` takes. |
| Tiles (`TileDebugScreen`) | the 64 x 64 surface textures as the map uses them (animated tiles show their current frame), transparent pixels magenta: grid of 8 x 8 with numbers, PageUp/PageDown flip pages, Home/End first/last page; F5 grid/single tile at 8x (PageUp/PageDown step by 1, Home/End by 10); F6 texel grid |
| Sounds (`SoundDebugScreen`) | lists the sound bank entries of the level (length, sample rate, pitch variation, loop start). PageUp/PageDown select, Home/End jump 10, Enter plays, K stops, L toggles looping |

To add a screen: implement `IDebugScreen` (the constructor takes services like a component) and register it in `TestWorldScene` with `debugMenu.Add<T>()`.

Other keys, always active:

| Key | Effect |
|-----|--------|
| `~` (OemTilde) | Shortcut for the dusk/noon switch (`DebugSettings.Noon`, also N in the World and player debug screen): noon = ambient 1.0, point lights off; dusk (default) is what the original shows |
| Tab | Play a random sound (309/310) |
| `+` / `-` | Override/cycle the ped animation frame number |
| Numpad 4/6/8/2 | Free-camera pan; Numpad 7/9 zoom in/out; Numpad 0 re-attach to the player |
| Numpad 5 | Teleport the player to the highest surface in the middle of the screen and re-attach the camera |

## Diagnostics

- `DiagnosticHighlight.Add(pos, scale, color)` – queue a wireframe box for this frame. `[Conditional("DEBUG")]`, so it vanishes in Release.
  Drawn only when the collision boxes are on (debug menu, World and player); the list is cleared after every draw (or every frame while they are off).
- `DiagnosticValues.Set(key, value)` – key/value lines in the on-screen debug text.
- `PerformanceCounters.Drawing` – named timing measurements shown on screen.
- The player debug text shows the cell, the blocks in the column, the physics surfaces at the player and the last fall rescue.
