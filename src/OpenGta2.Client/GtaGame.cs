using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Graphics;
using System.Drawing;
using Color = Microsoft.Xna.Framework.Color;

namespace OpenGta2.Client;

public class GtaGame : Game
{
    private GraphicsDeviceManager _graphics;
    private Scene? _activeScene;
    private BasicEffect? _basicEffect;
    
    public GtaGame()
    {
        _graphics = new GraphicsDeviceManager(this);
        Content.RootDirectory = "Content";
        IsMouseVisible = true;
    }
    
    public Matrix Projection => GetProj();

    public Matrix ProjectionLhs => Matrix.CreatePerspectiveFieldOfView(
        MathHelper.PiOver4, // 90 fov
        (Window.ClientBounds.Width / (float)Window.ClientBounds.Height),
        0.1f,
        9000);
    
    private Matrix GetProj()
    {
        var p = ProjectionLhs;

        // Invert matrix because DirectX is LHS and MonoGame is RHS
        p.M11 = -p.M11;
        p.M13 = -p.M13;

        return p;
    }

    public BasicEffect BasicEffect => _basicEffect!;

    protected override void Initialize()
    {
        ActivateScene(new TestWorldScene(this));

        base.Initialize();
    }

    protected override void LoadContent()
    {
        _basicEffect = new BasicEffect(GraphicsDevice);
        
        base.LoadContent();
    }

    public void ActivateScene(Scene scene)
    {
        // Components.Clear();
        // TODO: maybe should dispose all active components?

        if (_activeScene != null)
        {
            _activeScene.Dispose();
        }
        
        Components.Add(scene);
        _activeScene = scene;
    }
    
    protected override void Update(GameTime gameTime)
    {
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