# Audio and intro

## Sound library

`AudioTestComponent` loads `data/Audio/bil.sdt` (the index: `SdtReader` → `SoundEntry[]`) and `data/Audio/bil.raw`
(the samples: `RawReader.Read(entries)` → `SoundLibrary`).
`SoundLibrary.GetSound(index)` returns a `Sound` whose `Stream` puts a synthesized WAV header in front of the raw PCM
(`SoundStream`); that stream is fed to MonoGame's `SoundEffect.FromStream`. Pressing Tab plays sound 309 or 310 (test sounds).
There is no mixer, no 3D positioning, and no per-level bank selection yet; the bank is hard-coded to `bil`.

## Intro movie

`IntroComponent` plays `data/Movie/intro.bik` through the original `binkw32.dll` (P/Invoke, `unsafe`, DirectSound via
`DirectSoundCreate8`) and uploads each frame to a texture drawn full-screen with `ScreenspaceSpriteEffect`. When the movie ends
(or if the files are missing, `_failure`) it activates the next scene. `IntroScene` wraps it; its use is commented out in `GtaGame.FirstUpdate`.
`binkw32.dll` is a 32-bit DLL, so the process may need to run as x86 for this to work.
