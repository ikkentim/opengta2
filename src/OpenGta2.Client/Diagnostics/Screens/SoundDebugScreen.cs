using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using OpenGta2.Client.Levels;
using OpenGta2.GameData.Audio;

namespace OpenGta2.Client.Diagnostics.Screens;

/// <summary>
/// Debug tool for finding out what the sounds in the sound bank of the level are: lists the entries and plays them.
/// </summary>
/// <remarks>
/// PageUp/PageDown select the next/previous entry, Home/End jump 10 entries, Enter plays the selected entry (again: restarts it),
/// L toggles looping (entries with a loop start play once and then repeat from there; the others repeat the whole sample), K stops.
/// The list shows the entry number, length, sample rate, the random pitch variation and the loop start: the numbers are the
/// indices <see cref="SoundLibrary.GetSound"/> takes.
/// </remarks>
public class SoundDebugScreen : IDebugScreen
{
    private const int VisibleRows = 24;

    /// <summary>
    /// The lowest sample rate MonoGame accepts for a sound created from samples.
    /// </summary>
    private const int MinimumSampleRate = 8000;

    private readonly GtaGame _game;
    private readonly Controls _controls;
    private readonly LevelProvider _levelProvider;
    private SpriteBatch? _spriteBatch;
    private SpriteFont? _font;
    private Texture2D? _pixel;

    private SoundEntry[] _entries = Array.Empty<SoundEntry>();
    private SoundLibrary? _library;
    private string _loadedLevel = "";
    private string _error = "";

    private bool _loop;
    private int _selected;
    private int _playing = -1;
    private SoundEffect? _effect;
    private SoundEffectInstance? _instance;

    public SoundDebugScreen(GtaGame game, Controls controls, LevelProvider levelProvider)
    {
        _game = game;
        _controls = controls;
        _levelProvider = levelProvider;
    }

    public string Name => "Sounds";

    public IReadOnlyList<DebugKey> HelpKeys { get; } = new DebugKey[]
    {
        new("PgUp / PgDn", "select next / previous entry"),
        new("Home / End", "select +-10 entries"),
        new("Enter", "play"),
        new("L", "loop (from the loop start where there is one)"),
        new("K", "stop"),
    };

    private GraphicsDevice GraphicsDevice => _game.GraphicsDevice;

    public void LoadContent()
    {
        _spriteBatch = new SpriteBatch(GraphicsDevice);
        _font = _game.AssetManager.GetDebugFont();
        _pixel = new Texture2D(GraphicsDevice, 1, 1);
        _pixel.SetData(new[] { Color.White });
    }

    public void Opened()
    {
    }

    public void Closed() => Stop();

    private void LoadBank()
    {
        var name = _levelProvider.LevelName;

        try
        {
            using var sdt = TestGamePath.OpenFile($"data/Audio/{name}.sdt");
            _entries = new SdtReader(sdt).Read();

            using var raw = TestGamePath.OpenFile($"data/Audio/{name}.raw");
            _library = new RawReader(raw).Read(_entries);

            _error = "";
        }
        catch (Exception exception)
        {
            _entries = Array.Empty<SoundEntry>();
            _library = null;
            _error = exception.Message;
        }

        _loadedLevel = name;
        _selected = Math.Clamp(_selected, 0, Math.Max(0, _entries.Length - 1));
    }

    public void Update(GameTime gameTime)
    {
        if (_loadedLevel != _levelProvider.LevelName)
        {
            LoadBank();
        }

        if (_entries.Length == 0) return;

        var step = 0;
        if (_controls.IsKeyDown(Keys.PageUp)) step = 1;
        if (_controls.IsKeyDown(Keys.PageDown)) step = -1;
        if (_controls.IsKeyDown(Keys.Home)) step = 10;
        if (_controls.IsKeyDown(Keys.End)) step = -10;
        _selected = Math.Clamp(_selected + step, 0, _entries.Length - 1);

        if (_controls.IsKeyDown(Keys.L))
        {
            _loop = !_loop;

            // restart what is playing so the new mode applies (loop from the loop start needs a different sound effect)
            if (_playing >= 0) Play(_playing);
        }

        if (_controls.IsKeyDown(Keys.K)) Stop();
        if (_controls.IsKeyDown(Keys.Enter)) Play(_selected);

        if (_instance != null && _instance.State == SoundState.Stopped)
        {
            _playing = -1;
        }
    }

    private void Play(int index)
    {
        if (_library == null) return;

        Stop();

        try
        {
            var entry = _entries[index];
            if (_loop && entry.LoopStart > 0 && SoundLibrary.GetBitsPerSample(entry) == 16)
            {
                // the sample plays once from the start and then repeats from the loop start to the end. The loop start is in bytes;
                // MonoGame wants samples (16 bit: two bytes) and the length of the loop.
                var pcm = _library.GetPcm(index);
                var samples = pcm.Samples.ToArray();
                var sampleRate = pcm.SampleRate;
                var loopStart = entry.LoopStart / 2;

                // MonoGame only accepts sample rates from 8000 Hz when creating a sound from samples: repeat every sample a whole
                // number of times for slower ones, which sounds the same
                if (sampleRate < MinimumSampleRate)
                {
                    var factor = (MinimumSampleRate + sampleRate - 1) / sampleRate;
                    samples = Repeat16BitSamples(samples, factor);
                    sampleRate *= factor;
                    loopStart *= factor;
                }

                var sampleCount = samples.Length / 2;
                loopStart = Math.Min(loopStart, sampleCount - 1);
                _effect = new SoundEffect(samples, 0, samples.Length, sampleRate, AudioChannels.Mono, loopStart, sampleCount - loopStart);
            }
            else
            {
                // GetSound builds a wave stream with the sample rate of the entry, including its random pitch variation.
                // Looping this repeats the whole sample.
                var sound = _library.GetSound(index);
                _effect = SoundEffect.FromStream(sound.Stream);
            }

            _instance = _effect.CreateInstance();
            _instance.IsLooped = _loop;
            _instance.Play();
            _playing = index;
            _error = "";
        }
        catch (Exception exception)
        {
            // a debug tool should not take the game down with it
            Stop();
            _error = $"Could not play entry {index}: {exception.GetType().Name}: {exception.Message}";
        }
    }

    private static byte[] Repeat16BitSamples(byte[] samples, int factor)
    {
        var result = new byte[samples.Length * factor];
        for (var i = 0; i + 1 < samples.Length; i += 2)
        {
            for (var k = 0; k < factor; k++)
            {
                result[(i / 2 * factor + k) * 2] = samples[i];
                result[(i / 2 * factor + k) * 2 + 1] = samples[i + 1];
            }
        }

        return result;
    }

    private void Stop()
    {
        _instance?.Stop();
        _instance?.Dispose();
        _effect?.Dispose();
        _instance = null;
        _effect = null;
        _playing = -1;
    }

    public void Draw(GameTime gameTime)
    {
        var viewport = GraphicsDevice.Viewport;
        var lineHeight = (int)_font!.MeasureString("A").Y + 2;
        var width = 760;
        var height = (VisibleRows + 5) * lineHeight;
        var x = viewport.Width - width - 10;
        var y = 10;

        _spriteBatch!.Begin(blendState: BlendState.NonPremultiplied);
        _spriteBatch.Draw(_pixel, new Rectangle(x - 6, y - 6, width + 12, height + 12), new Color(0, 0, 0, 215));

        var text = new StringBuilder();
        text.Append(CultureInfo.InvariantCulture, $"Sound bank '{_levelProvider.LevelName}' ({_entries.Length} entries)    loop: ");
        text.Append(_loop ? "ON (from the loop start where an entry has one)" : "off");
        _spriteBatch.DrawString(_font, text.ToString(), new Vector2(x, y), Color.White);
        if (_entries.Length > 0 && !string.IsNullOrEmpty(_error))
        {
            _spriteBatch.DrawString(_font, _error, new Vector2(x, y + lineHeight * 2), Color.OrangeRed);
        }

        y += lineHeight * 3;

        if (_entries.Length == 0)
        {
            _spriteBatch.DrawString(_font, string.IsNullOrEmpty(_error) ? "No sound bank loaded." : _error, new Vector2(x, y), Color.OrangeRed);
        }
        else
        {
            _spriteBatch.DrawString(_font, "  #     length   rate     bits  variation  loop from", new Vector2(x, y), Color.LightGray);
            y += lineHeight;

            var first = Math.Clamp(_selected - VisibleRows / 2, 0, Math.Max(0, _entries.Length - VisibleRows));
            for (var i = first; i < Math.Min(first + VisibleRows, _entries.Length); i++)
            {
                var entry = _entries[i];
                var seconds = entry.Size / (double)(Math.Max(1, entry.SampleRate) * (SoundLibrary.GetBitsPerSample(entry) / 8));
                var marker = i == _selected ? ">" : " ";
                var line = string.Create(CultureInfo.InvariantCulture,
                    $"{marker} {i,-5} {seconds,6:F2}s  {entry.SampleRate,6}Hz  {SoundLibrary.GetBitsPerSample(entry),2}   {(entry.VariationSampleRate > 0 ? "+-" + entry.VariationSampleRate : "-"),-9}  {(entry.LoopStart > 0 ? entry.LoopStart.ToString(CultureInfo.InvariantCulture) : "-")}{(i == _playing ? "    << playing" : "")}");

                var color = i == _selected ? Color.Yellow : i == _playing ? Color.LightGreen : Color.White;
                _spriteBatch.DrawString(_font, line, new Vector2(x, y), color);
                y += lineHeight;
            }
        }

        _spriteBatch.End();

        // reset states changed by the sprite batch
        GraphicsDevice.DepthStencilState = DepthStencilState.Default;
    }
}
