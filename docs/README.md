# OpenGTA2 docs

Documentation of the current state of the project. These docs describe what the code *does today*, including known
problems, so we have a baseline for the upcoming collision / movement / vertical-axis rewrite.

| Doc | Contents |
|-----|----------|
| [architecture.md](architecture.md) | Projects, scene/component structure, game loop, services, controls, debug keys |
| [coordinate-system.md](coordinate-system.md) | Axes, units, block/column/Z conventions, rotation. **Read this first** before touching collision |
| [map-data.md](map-data.md) | How `.gmp` map data is represented in memory (columns, blocks, faces, slopes) |
| [rendering.md](rendering.md) | Chunked map rendering, slope generator, sprites, camera |
| [player-and-peds.md](player-and-peds.md) | Ped, player controller, camera follow, animation |
| [format-notes.md](format-notes.md) | Facts from the original map, style and scripting documents that matter for our code |
| [ped-sprites.md](ped-sprites.md) | Layout of the ped sprites in the style file (partly guesses), drowning plan |
| [collision.md](collision.md) | Current collision implementation, step by step |
| [collision-issues.md](collision-issues.md) | Catalogue of bugs/limitations in the current collision, and requirements for the rewrite |
| [audio-ambience.md](audio-ambience.md) | Research notes on atmosphere audio: sound objects, the sound bank, what is still unknown |
| [audio-and-intro.md](audio-and-intro.md) | Sound library loading, the Bink intro |

Older notes: [`/gtadocs.md`](../gtadocs.md) (file types and format links).

## Running

The client reads game files from the directory in the **user-level** environment variable `OPENGTA2_PATH`
(see `TestGamePath`). That directory is a GTA2 install containing `data/*.gmp`, `data/*.sty`, `data/Audio/*`,
`data/Movie/intro.bik` and `binkw32.dll`. The game files are not part of this repo.

The level to load is hard-coded in `GtaGame.FirstUpdate()` (currently `bil.gmp` / `bil.sty`, level 3).
