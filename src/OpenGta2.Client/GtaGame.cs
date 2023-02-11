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
        _graphics = new GraphicsDeviceManager(this);
        Content.RootDirectory = "Content";
        IsMouseVisible = true;
    }
    
    public Matrix Projection => GetProjection();

    public Matrix ProjectionLhs => Matrix.CreatePerspectiveFieldOfView(
        MathHelper.PiOver4, // 90 fov
        (Window.ClientBounds.Width / (float)Window.ClientBounds.Height),
        0.1f,
        9000);
    
    private Matrix GetProjection()
    {
        var p = ProjectionLhs;

        // Invert matrix because DirectX is LHS and MonoGame is RHS
        p.M11 = -p.M11;
        p.M13 = -p.M13;

        return p;
    }
    
    protected override void Initialize()
    {
        _graphics.GraphicsProfile = GraphicsProfile.HiDef;
        _graphics.ApplyChanges();

        _graphics.PreferredBackBufferWidth = 1920;
        _graphics.PreferredBackBufferHeight = 1080;
        _graphics.ApplyChanges();

        base.Initialize();
    }

    private void FirstUpdate()
    {
        // If we'd activate in LoadContent, the component won't initialize.
        ActivateScene(new TestWorldScene(this));
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
        GraphicsDevice.Clear(Color.CornflowerBlue);
        
        base.Draw(gameTime);
    }
}