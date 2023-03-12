using System;
using Microsoft.VisualBasic.CompilerServices;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using OpenGta2.Client.Content;
using OpenGta2.Client.Scenes;
using OpenGta2.Client.Utilities;

namespace OpenGta2.Client;

public class GtaGame : Game
{
    private AssetManager? _assetManager;
    private readonly Controls _controls = new();
    private bool _hasReceivedUpdate;

    public GtaGame()
    {
        Content.RootDirectory = "Content";
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
        ActivateScene(new LoadingWorldScene(this, "data/bil.gmp", "data/bil.sty", new TestWorldScene(this))); // LEVEL 3
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

public struct IntVector2
{
    public int X;
    public int Y;

    public IntVector2(int x, int y)
    {
        X = x;
        Y = y;
    }
}

public struct IntVector3
{
    public int X;
    public int Y;
    public int Z;

    public IntVector3(int x, int y, int z)
    {
        X = x;
        Y = y;
        Z = z;
    }

    public static IntVector3 Floor(Vector3 vec)
    {
        return new IntVector3((int)vec.X, (int)vec.Y, (int)vec.Z);
    }

    public bool Equals(IntVector3 other)
    {
        return X == other.X && Y == other.Y && Z == other.Z;
    }

    public override bool Equals(object? obj)
    {
        return obj is IntVector3 other && Equals(other);
    }

    public override int GetHashCode()
    {
        return HashCode.Combine(X, Y, Z);
    }
    
    public static bool operator ==(IntVector3 lhs, IntVector3 rhs)
    {
        return lhs.Equals(rhs);
    }

    public static bool operator !=(IntVector3 lhs, IntVector3 rhs)
    {
        return !lhs.Equals(rhs);
    }

    public static implicit operator Vector3(IntVector3 vec)
    {
        return new Vector3(vec.X, vec.Y, vec.Z);
    }
}