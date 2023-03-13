using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Input;
using OpenGta2.GameData.Audio;

namespace OpenGta2.Client.Components;

public class AudioTestComponent : BaseComponent
{
    private readonly SoundLibrary _library;
    private readonly Controls _controls;
    private readonly Random _random = new();

    public AudioTestComponent(GtaGame game, Controls controls) : base(game)
    {
        using var sdt = TestGamePath.OpenFile("data/Audio/bil.sdt");
        var r = new SdtReader(sdt);

        var entries = r.Read();

        using var raw = TestGamePath.OpenFile("data/Audio/bil.raw");
        var rr = new RawReader(raw);
        _library = rr.Read(entries);

        _controls = controls;
    }
    
    public override void Update(GameTime gameTime)
    {
        if (_controls.IsKeyDown(Keys.Tab))
        {
            var sfx = _library.GetSound(_random.Next(0, 2) == 0 ? 309 : 310);
            var se = SoundEffect.FromStream(sfx.Stream);
            se.Play();
        }
    }
}