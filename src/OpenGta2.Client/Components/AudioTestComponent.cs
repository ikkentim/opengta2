using System.IO;
using System.Runtime.InteropServices;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Input;
using OpenGta2.GameData.Audio;
using OpenGta2.GameData.Riff;

namespace OpenGta2.Client.Components;

public class AudioTestComponent : GtaComponent
{
    private SoundLibrary _library;
    private Controls _controls;

    public AudioTestComponent(GtaGame game) : base(game)
    {
        using var sdt = TestGamePath.OpenFile("data/Audio/bil.sdt");
        var r = new SdtReader(sdt);

        var entries = r.Read();

        using var raw = TestGamePath.OpenFile("data/Audio/bil.raw");
        var rr = new RawReader(raw);
        _library = rr.Read(entries);

        _controls = Game.Services.GetService<Controls>();
    }
    
    public override void Update(GameTime gameTime)
    {
        if (_controls.IsKeyDown(Keys.Tab))
        {
            var sfx = _library.GetSound(309);
            var se = SoundEffect.FromStream(sfx.Stream);
            se.Play();
        }
    }

    [StructLayout(LayoutKind.Sequential)]
    public struct SoundEntry
    {
        public int Offset;
        public int Size;
        public int SamplesPerSecond;
        public int Padding;
        public int LoopStart;
        public int LoopEnd;
    }
}