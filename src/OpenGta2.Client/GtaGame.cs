using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using OpenGta2.Client.Assets;
using OpenGta2.Client.Scenes;
using OpenGta2.Client.Utilities;
using SharpDX.MediaFoundation;

namespace OpenGta2.Client;

public class GtaGame : Game
{
    private AssetManager? _assetManager;
    private readonly Controls _controls = new();
    private bool _hasReceivedUpdate;

    public GtaGame()
    {
        Content.RootDirectory = "Assets";
        IsMouseVisible = true;

        var graphics = new GraphicsDeviceManager(this);
        graphics.GraphicsProfile = GraphicsProfile.HiDef;
        graphics.SynchronizeWithVerticalRetrace = true;
        graphics.PreparingDeviceSettings += (sender, args) =>
        {
            graphics.PreferMultiSampling = true;
        };
        graphics.ApplyChanges();
        graphics.GraphicsDevice.PresentationParameters.MultiSampleCount = 4;
        graphics.PreferredBackBufferWidth = 1920;
        graphics.PreferredBackBufferHeight = 1080;
        graphics.ApplyChanges();
    }

    public AssetManager AssetManager => _assetManager ?? throw ThrowHelper.GetContentNotLoaded();

    public Scene? ActiveScene { get; private set; }
    
    private void FirstUpdate()
    {
        // If we'd activate in LoadContent, the component won't initialize.

        // ActivateScene(new LoadingWorldScene(this, "data/wil.gmp", "data/wil.sty", new TestWorldScene(this))); // LEVEL 1
        // ActivateScene(new LoadingWorldScene(this, "data/lorne2e.gmp", "data/wil.sty", new TestWorldScene(this))); // BONUS 1a
        // ActivateScene(new LoadingWorldScene(this, "data/ste.gmp", "data/ste.sty", new TestWorldScene(this))); // LEVEL 2
        // ActivateScene(new LoadingWorldScene(this, "data/bil.gmp", "data/bil.sty", new TestWorldScene(this))); // LEVEL 3

        ActivateScene((new IntroScene(this, new LoadingWorldScene(this, "data/bil.gmp", "data/bil.sty", new TestWorldScene(this)))));
    }

    protected override void LoadContent()
    {
        _assetManager = new AssetManager();
        _assetManager.LoadContent(Content);

        Services.AddService(_controls);
        Services.AddService(_assetManager);

        base.LoadContent();
    }

    public void ActivateScene(Scene scene)
    {
        foreach (var component in Components)
        {
            if (component is IDisposable disposable) disposable.Dispose();
        }

        Components.Clear();

        Components.Add(scene);
        ActiveScene = scene;
    }

    protected override void Update(GameTime gameTime)
    {
        _controls.Update();

        if (!_hasReceivedUpdate)
        {
            _hasReceivedUpdate = true;
            FirstUpdate();
        }

        if (_controls.IsKeyDown(Control.Menu))
        {
            Exit();
        }

        base.Update(gameTime);
    }

    protected override void Draw(GameTime gameTime)
    {
        GraphicsDevice.Clear(Color.Black);
        base.Draw(gameTime);
    }
}