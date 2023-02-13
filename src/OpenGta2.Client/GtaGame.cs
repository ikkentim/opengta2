using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

namespace OpenGta2.Client;

public class GtaGame : Game
{
    private readonly GraphicsDeviceManager _graphics;
    private Scene? _activeScene;

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

    private void Graphics_PreparingDeviceSettings(object? sender, PreparingDeviceSettingsEventArgs e)
    {
        _graphics.PreferMultiSampling = true;
    }

    protected override void Initialize()
    {
        _graphics.GraphicsDevice.PresentationParameters.MultiSampleCount = 4;
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
        var assetManager = new AssetManager();
        assetManager.LoadContent(Content);

        Services.AddService(assetManager);

        base.LoadContent();
    }

    public void ActivateScene(Scene scene)
    {
        // Components.Clear();
        // TODO: maybe should dispose all active components?

        if (_activeScene != null)
        {
            _activeScene.Dispose();
            Components.Remove(_activeScene);
        }
        
        Components.Add(scene);
        _activeScene = scene;
    }

    private bool _hasReceivedUpdate;

    protected override void Update(GameTime gameTime)
    {
        if (!_hasReceivedUpdate)
        {
            _hasReceivedUpdate = true;
            FirstUpdate();
        }

        if (Keyboard.GetState().IsKeyDown(Keys.Escape))
            Exit();
        
        base.Update(gameTime);
    }

    protected override void Draw(GameTime gameTime)
    {
        GraphicsDevice.Clear(Color.Black);
        
        base.Draw(gameTime);
    }
}