using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using OpenGta2.Client.Content;
using OpenGta2.Client.Scenes;
using OpenGta2.Client.Utilities;

namespace OpenGta2.Client;

public class GtaGame : Game
{
    private readonly GraphicsDeviceManager _graphics;
    private AssetManager? _assetManager;

    private bool _hasReceivedUpdate;

    public GtaGame()
    {
        Content.RootDirectory = "Content";
        IsMouseVisible = true;

        _graphics = new GraphicsDeviceManager(this);
        _graphics.PreparingDeviceSettings += Graphics_PreparingDeviceSettings;
        _graphics.GraphicsProfile = GraphicsProfile.HiDef;
        _graphics.ApplyChanges();
        _graphics.PreferredBackBufferWidth = 1920;
        _graphics.PreferredBackBufferHeight = 1080;
        _graphics.ApplyChanges();
    }

    public AssetManager AssetManager => _assetManager ?? throw ThrowHelper.GetContentNotLoaded();

    public Scene? ActiveScene { get; private set; }

    private void Graphics_PreparingDeviceSettings(object? sender, PreparingDeviceSettingsEventArgs e)
    {
        _graphics.PreferMultiSampling = true;
    }

    protected override void Initialize()
    {
        _graphics.GraphicsDevice.PresentationParameters.MultiSampleCount = 1;
        _graphics.ApplyChanges();

        base.Initialize();
    }

    private void FirstUpdate()
    {
        // If we'd activate in LoadContent, the component won't initialize.

        // ActivateScene(new LoadingWorldScene(this, "data/wil.gmp", "data/wil.sty", new TestWorldScene(this))); // LEVEL 1
        // ActivateScene(new LoadingWorldScene(this, "data/lorne2e.gmp", "data/wil.sty", new TestWorldScene(this))); // BONUS 1a
        // ActivateScene(new LoadingWorldScene(this, "data/ste.gmp", "data/ste.sty", new TestWorldScene(this))); // LEVEL 2
        ActivateScene(new LoadingWorldScene(this, "data/bil.gmp", "data/bil.sty", new TestWorldScene(this))); // LEVEL 3
    }

    protected override void LoadContent()
    {
        _assetManager = new AssetManager();
        _assetManager.LoadContent(Content);

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
        if (!_hasReceivedUpdate)
        {
            _hasReceivedUpdate = true;
            FirstUpdate();
        }

        if (Keyboard.GetState()
            .IsKeyDown(Keys.Escape))
            Exit();

        base.Update(gameTime);
    }

    protected override void Draw(GameTime gameTime)
    {
        GraphicsDevice.Clear(Color.Black);

        base.Draw(gameTime);
    }
}