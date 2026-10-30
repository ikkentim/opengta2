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

| Key | Effect |
|-----|--------|
| F1 | Toggle diagnostic highlights (collision debug boxes) – `DebuggingDrawingComponent` |
| F2 | Toggle slow player movement (x0.1) – useful for stepping through collision |
| `~` (OemTilde) | Toggle "noon" lighting (ambient 1.0, point lights off) |
| Tab | Play a random sound (309/310) |
| `+` / `-` | Override/cycle the ped animation frame number |
| Numpad 4/6/8/2 | Free-camera pan; Numpad 7/9 zoom in/out; Numpad 0 re-attach to the player |

## Diagnostics

- `DiagnosticHighlight.Add(pos, scale, color)` – queue a wireframe box for this frame. `[Conditional("DEBUG")]`, so it vanishes in Release.
  Drawn only when F1 is on; the list is cleared after every draw (or every frame while F1 is off).
- `DiagnosticValues.Set(key, value)` – key/value lines in the on-screen debug text.
- `PerformanceCounters.Drawing` – named timing measurements shown on screen.
- Collision currently also spams `Debug.WriteLine` on hits.
