using System;
using System.IO;
using System.Runtime.InteropServices;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using OpenGta2.Client.Assets.Effects;
using OpenGta2.Client.Rendering;
using OpenGta2.Client.Scenes;

namespace OpenGta2.Client.Components;

public unsafe class IntroComponent : BaseDrawableComponent
{
    [UnmanagedFunctionPointer(CallingConvention.StdCall)]
    public delegate IntPtr BinkSndSysOpen(uint param);

    private readonly Scene _nextScene;
    private Bink* _bink;
    private byte[]? _buffer;
    private byte[]? _buffer2;
    private uint _currentFrame;
    private bool _deactivated;

    private ScreenspaceSpriteEffect? _effect;
    private bool _failure;
    private uint _height;
    private uint _lastFrame;
    private uint _pitch;
    private Texture2D? _texture;

    private uint _width;

    public IntroComponent(GtaGame game, Scene nextScene) : base(game)
    {
        _nextScene = nextScene;
    }

    public override void Initialize()
    {
        var bikPath = Path.Combine(TestGamePath.Directory.FullName, "data/Movie/intro.bik");

        if (!File.Exists(bikPath))
        {
            _failure = true;
        }
        else
        {
            try
            {
                LoadLibrary(Path.Combine(TestGamePath.Directory.FullName, "binkw32.dll"));

                IntPtr d8;
                DirectSoundCreate8(IntPtr.Zero, &d8, IntPtr.Zero);
                BinkSetSoundSystem(BinkOpenDirectSound, (uint)d8.ToInt32());

                _bink = BinkOpen(Path.Combine(TestGamePath.Directory.FullName, "data/Movie/intro.bik"), 0);
                BinkSetSoundOnOff(_bink, 1);
                _width = _bink->Width;
                _height = _bink->Height;
                _pitch = _width * 3;
                _currentFrame = 0;
                _lastFrame = _bink->LastFrameNum;
                _buffer = new byte[_pitch * _height];
                _buffer2 = new byte[_width * 4 * _height];
            }
            catch
            {
                _failure = true;
            }
        }

        base.Initialize();
    }

    protected override void LoadContent()
    {
        if (_failure)
        {
            return;
        }

        _texture = new Texture2D(GraphicsDevice, (int)_width, (int)_height, false, SurfaceFormat.Color);

        _effect = Game.AssetManager.CreateScreenspaceSpriteEffect();
        _effect.Texture = _texture;
    }

    protected override void UnloadContent()
    {
        _effect?.Dispose();
        _texture?.Dispose();

        base.UnloadContent();
    }

    public override void Update(GameTime gameTime)
    {
        if (_deactivated)
        {
            return;
        }


        if (_failure || _currentFrame >= _lastFrame || Keyboard.GetState().GetPressedKeyCount() > 0)
        {
            Game.ActivateScene(_nextScene);
            _deactivated = true;
        }
    }

    protected override void Dispose(bool disposing)
    {
        if (_bink != null)
        {
            BinkClose(_bink);
        }

        base.Dispose(disposing);
    }

    public override void Draw(GameTime gameTime)
    {
        if (_bink == null || _failure)
        {
            return;
        }

        try
        {
            BinkDoFrame(_bink);

            fixed (byte* ptr = _buffer)
            {
                BinkCopyToBuffer(_bink, (IntPtr)ptr, _pitch, _height, 0, 0, 0x4000000);
            }

            for (var i = 0; i < _buffer!.Length / 3; i++)
            {
                // Add alpha component
                _buffer2![i * 4 + 0] = _buffer[i * 3 + 2];
                _buffer2[i * 4 + 1] = _buffer[i * 3 + 1];
                _buffer2[i * 4 + 2] = _buffer[i * 3 + 0];
                _buffer2[i * 4 + 3] = 0xff;
            }

            _texture!.SetData(_buffer2);

            _effect.CurrentTechnique.Passes[0].Apply();

            var vp = GraphicsDevice.Viewport;
            QuadRenderer.Render(GraphicsDevice, Vector2.Zero, new Vector2(vp.Width, vp.Height));

            if (_currentFrame >= _lastFrame)
            {
                return;
            }

            if (BinkWait(_bink) == 0)
            {
                BinkNextFrame(_bink);
                _currentFrame++;
            }
        }
        catch
        {
            _failure = true;
        }
    }

    [DllImport("Kernel32.dll")]
    private static extern IntPtr LoadLibrary(string path);

    [DllImport("binkw32.dll")]
    private static extern Bink* BinkOpen(string name, uint flags);

    [DllImport("binkw32.dll")]
    private static extern void BinkClose(Bink* bink);

    [DllImport("binkw32.dll")]
    private static extern int BinkCopyToBuffer(Bink* bink, IntPtr dest, uint destPitch, uint destHeight, uint destX, uint destY, uint flags);

    [DllImport("binkw32.dll")]
    private static extern int BinkDoFrame(Bink* bink);

    [DllImport("binkw32.dll")]
    private static extern int BinkWait(Bink* bink);

    [DllImport("binkw32.dll")]
    private static extern void BinkNextFrame(Bink* bink);

    [DllImport("binkw32.dll")]
    private static extern void BinkSetSoundOnOff(Bink* bink, int onoff);

    [DllImport("binkw32.dll")]
    private static extern IntPtr BinkOpenDirectSound(uint param);

    [DllImport("binkw32.dll")]
    private static extern void BinkSetSoundSystem([MarshalAs(UnmanagedType.FunctionPtr)] BinkSndSysOpen open, uint param);

    [DllImport("Dsound.dll")]
    private static extern int DirectSoundCreate8(IntPtr lpcGuidDevice, IntPtr* ppDS8, IntPtr pUnkOuter);

    [StructLayout(LayoutKind.Sequential)]
    public struct Bink
    {
        public uint Width;
        public uint Height;
        public uint Frames;
        public uint FrameNum;
        public uint LastFrameNum;
    }
}