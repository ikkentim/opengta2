# Atmosphere audio: what we know

Status: research notes, nothing implemented yet beyond the sound bank reader (see [audio-and-intro.md](audio-and-intro.md)).
Sources: the original scripting document (see [format-notes.md](format-notes.md)), the game files, and measurements by throwaway programs.

## Files

| Path (under `data/`) | What | Notes |
|----------------------|------|-------|
| `Audio/<level>.sdt` + `.raw` (`bil`, `ste`, `wil`) | Sound bank of a level: 320 entries, about 5.7 MB of mono PCM, **mostly 16-bit signed but 8-bit unsigned for entries 69-136** (see Format below) | We can read these already (`SdtReader`, `RawReader`, `SoundLibrary`) |
| `Audio/FSTYLE.sdt` + `.raw` | Frontend (menu) sounds | Not looked at |
| `Audio/Vocals/*.wav` | 97 announcer lines ("120PeopleDown", "Busted", ...) | Speech, not ambience |
| `GTAudio/*.wav` | 22 long files (291 MB), named `1.wav`, `10.wav`, `101a.wav`, ... | Presumably the radio music; not looked at |
| `<level>.scr` | The level script | Places sound objects, see below |

## Sound objects (the ambience mechanism)

The scripting document describes atmosphere as **sound objects**: an item created at an (x, y, z) that plays a sound or sample.
"They behave like every other sound in the game: they have volume, get loud/quiet as the player approaches." Two play modes:
`PLAY_FOREVER` (keeps playing until the object is deleted) and `PLAY_INSTANT` (plays once). Commands: `SOUND` (declare),
`SOUND ... = (x, y, z) type playtype` (declare and set), `CREATE_SOUND` (mid-game), `DELETE_ITEM`.

The document lists the **sound object types**, in this order (about 52 names): WIND, SKID, NIGHT_CLUB, BAR, GENERATOR_RUMBLE, WORKSHOP,
VAT, CHURCH_SINGING, TEMPLE_CHANT, INDUSTRIAL_HIGHT, HUMAN_ABATTOIR, FUNNY_FARM, BANK_ALARM, INDUSTRIAL_LOW, PORTA_LOO, WATERFALL,
CRICKETS, PRISON, PRISON_ALARM, GANG_DUMPED, SMUG_LAUGH, PRISON_YARD, PRISON_RIOT, GANG_LOCKED_IN_BUS, SHOPPING_MALL, CLOCK_TOWER,
YEEHA_BOMB, BOWLING_ALLEY, CROWD_NOISE, FAN_NOISE, GENERATOR_LOSE_POWER, SCREAM, BOMB_TICK, BOMB_TICK_SHIT, DETECTED_MUMBLE,
KRISHNA_CHANT, CRYING, ROCKET_LAUNCH_FAIL_CLICK, ROCKET_FAIL_LAUGH, POISONED, POWER_PLANT, MUMBLE, JAZZ_CLUB, COUNTRY_CLUB, PYLON,
DISGRACELAND, BAR_2, STRIP_CLUB, TEMPLE_2, GARAGE_OPEN, GARAGE_CLOSE, LET_ME_OUT. Some are ambience (WIND, BAR, WORKSHOP, CROWD_NOISE,
JAZZ_CLUB, ...), others are mission sounds (BOMB_TICK, SCREAM, GANG_DUMPED, ...).

## What the level scripts contain (measured)

`SOUND` is command `0x0147` (`ScriptCommandType.SOUND`; the interpreter doesn't decode it yet). Layout of the arguments after the
8-byte command header: word 0 unknown (always 0 or garbage from the next command), words 1-3 the position as fixed point
(divide by 16384, the same as cars; all z are 6.0), word 4 the sound object **type number**. The play type was not found in these
five words (probably in the command flags). The type numbers used:

| Script | SOUND commands | Type numbers (count) |
|--------|----------------|----------------------|
| `bil.scr` (level 3) | 78 | 7 (x11), 8, 9 (x4), 11 (x4), 15, 16, 17 (x6), 30 (x4), 41 (x10), 45 (x32), 49 (x4) |
| `wil.scr` (level 1) | 22 | 3, 4, 5 (x3), 8 (x2), 12 (x6), 14, 26 (x2), 30 (x2), 42 (x4) |
| `ste.scr` (level 2) | **0** | none: the script places no sound objects |

If the numbers are indices into the document's list (0 = WIND), `wil` reads sensibly: 12 = BANK_ALARM x6, 42 = JAZZ_CLUB x4,
5 = WORKSHOP x3. For `bil` it is less clear (45 would be DISGRACELAND x32 which is odd, 44 = PYLON x32 would fit an industrial level,
so the numbering may be off by one). **This mapping is unverified.**

## The sound bank (measured)

- `bil.sdt`, `ste.sdt` and `wil.sdt` each have 320 entries. They are identical in size and sample rate except for entries 122-136 (15 short
  sounds of 0.2-0.4 s, level specific: probably pedestrian voices) and one more (179 or 185).
- Sample rates range from 4.5 kHz to 22.05 kHz; 114 entries are 8 kHz and 52 are 22.05 kHz. `VariationSampleRate` adds a random pitch
  variation (the library already applies it).
- The longest entries are 5-8.6 s (176: 8.6 s, 168: 6.2 s, 170: 5.5 s, 147: 5.0 s), so ambience would be short loops. (Lengths: size in bytes / 2 / sample rate for the 16-bit entries, size / sample rate for the 8-bit ones.)
- **Format: the bank is not uniform.** Most entries are 16-bit signed mono. The 68 entries at 5500 Hz (entries 69-136 in all three level banks: the police radio chatter, 15 of them level specific) are 8-bit unsigned. `SoundLibrary.GetBitsPerSample` tells them apart by the sample rate, because the bank does not say. This was found by listening in the sound viewer: read as 16-bit the speech is noise, read as 8-bit everything else is noise. (Statistics on the bytes did not separate them: a first attempt concluded the whole bank was 8-bit, which was wrong and made all non-speech sounds noise; the "smoothness" of the data does not tell speech from noise at 5500 Hz either.) The 36 odd-sized entries are all 8-bit entries; for the 16-bit entries the (unseen) stray last byte is dropped, because XAudio2 rejects data that is not a whole number of samples. Before this was fixed the library wrote 16-bit headers for everything, which played the speech as noise and crashed XAudio2 on the 36 odd-sized entries.
- **Loop fields (the viewer now uses them):** in the viewer, L plays entries with a loop start once and then repeats from there to the end (`SoundEffect` with a loop region; the start is in bytes, so half of it in 16-bit samples; MonoGame only accepts 8000 Hz and up for sounds made from samples, so slower ones such as entry 60 have every sample repeated a whole number of times). Unverified against the original: whether the game really loops "from the start to the end" this way and whether entries without a loop start loop the whole sample.
- **Loop fields:** `LoopEnd` is `-1` in every entry (to the end of the sample). Nine entries have `LoopStart > 0` (17, 18, 20, 22, 36, 58, 60, 193, 318)
  and loop from there to the end; the rest have `LoopStart = 0` (whole sample loops, or does not loop, decided by the game).
  The earlier assumption "entries without a loop point" was wrong.

## What we do not know

1. **Which bank entry belongs to which sound object type.** Not in any document; it is probably hard-coded in the executable. It has to be
   found by listening, the same way as for sprites (a sound viewer would help: step through the 320 entries and play them).
2. The play type encoding in the `SOUND` command and whether the position is the centre of a radius (and what the radius is).
3. How volume falls off with distance, whether sounds are panned, and how many play at once.
4. Whether ambience also comes from somewhere else: zones (no zone type in the map format is about sound), or the radio stations
   (placed by scripts with `RADIO_STATION`, ranges preset, types STATION_ZAIBATSU ... STATION_DOWNTOWN).
5. How `GTAudio/*.wav` relate to the radio stations.

## Sound viewer

The Sounds screen of the debug menu (F1, `SoundDebugScreen`) is the sound viewer: all 320 entries of the level bank with length, sample rate, pitch variation and loop start; Enter plays the selected one (L loops it, from the loop start where the entry has one; K stops). Use it with the original game to find which entry is which sound object type.

## Suggested next steps

1. Decode the `SOUND` command in the script interpreter (position, type, play type) and add `IScriptRuntime.CreateSound`, with tests on `bil.scr` and `wil.scr`.
2. Build a sound viewer in the client (like the sprite viewer): step through the bank, play an entry, show index, length, rate and loop start.
3. Use it, together with the original game (stand next to a known sound object, e.g. the banks in `wil`), to find the type-to-entry mapping.
4. Implement sound objects: a listener at the camera or player, distance attenuation (guess, then tune against the original), looping through `SoundEffectInstance`.
