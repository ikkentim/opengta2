using System.Globalization;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using OpenGta2.Client.Assets.Effects;
using OpenGta2.Client.Diagnostics;
using OpenGta2.Client.Levels;
using OpenGta2.Client.Peds;
using OpenGta2.Client.Rendering;
using OpenGta2.Client.Utilities;
using OpenGta2.GameData.Style;

namespace OpenGta2.Client.Components;

public class PedManagerComponent : BaseDrawableComponent
{
    private readonly Controls _controls;
    private readonly Camera _camera;
    private readonly PedManager _pedManager;
    private IndexBuffer? _indices;
    private VertexBuffer? _vertices;
    private WorldSpriteEffect? _spriteEfect;
    private readonly LevelProvider _levelProvider;

    public PedManagerComponent(GtaGame game, Camera camera, PedManager pedManager, Controls controls, LevelProvider levelProvider) : base(game)
    {
        _controls = controls;
        _camera = camera;
        _pedManager = pedManager;
        _levelProvider = levelProvider;
    }
    
    protected override void LoadContent()
    {
        _spriteEfect = Game.AssetManager.CreateWorldSpriteEffect();

        _indices = new IndexBuffer(GraphicsDevice, IndexElementSize.SixteenBits, 6, BufferUsage.WriteOnly);
        _vertices = new VertexBuffer(GraphicsDevice, typeof(VertexPositionSprite), 4, BufferUsage.WriteOnly);
        _vertices.SetData(new VertexPositionSprite[]
        {
            new(
                // top-left
                new Vector3(-0.5f, -0.5f, 0), new Vector2(0, 0)),
            new(
                // top-right
                new Vector3(0.5f, -0.5f, 0), new Vector2(1, 0)),
            new(
                // bottom-left
                new Vector3(-0.5f, 0.5f, 0), new Vector2(0, 1)),
            new(
                // bottom-right
                new Vector3(0.5f, 0.5f, 0), new Vector2(1, 1))
        });
        _indices.SetData(new short[] { 0, 1, 2, 2, 1, 3 });

        base.LoadContent();
    }
    
    public override void Update(GameTime gameTime)
    {
        if (_controls.IsKeyDown(Keys.OemPlus))
        {
            _pedAnimNum++;
            _pedAnimOveride = true;
        }

        if (_controls.IsKeyDown(Keys.OemMinus))
        {
            _pedAnimNum--;
            _pedAnimOveride = true;
        }
        
        foreach (var ped in _pedManager.Peds)
        {
            ped.UpdateAnimation(gameTime.GetDelta());
        }

        DiagnosticValues.Values["PedAnim"] = _pedAnimNum.ToString(CultureInfo.InvariantCulture);
    }

    private bool _pedAnimOveride;
    private int _pedAnimNum = 53;

    public override void Draw(GameTime gameTime)
    {
        GraphicsDevice.SetVertexBuffer(_vertices);
        GraphicsDevice.Indices = _indices;

        foreach (var ped in _pedManager.Peds)
        {
            if (!_pedAnimOveride)
            {
                _pedAnimNum = ped.AnimationFrame + ped.AnimationBase;
            }

            _spriteEfect!.Texture = _levelProvider.Textures.GetSpriteTexture(SpriteKind.Ped, (ushort)(158 + _pedAnimNum), ped.Remap);
            
            var world = Matrix.CreateScale(_spriteEfect!.Texture.Width / 64f, _spriteEfect!.Texture.Height / 64f, 1)  * Matrix.CreateRotationZ(ped.Rotation) * Matrix.CreateTranslation(ped.Position); 

            // shadow
            _spriteEfect.Color = new Color(0, 0, 0, 0.6f);
            _spriteEfect.TransformMatrix = world * Matrix.CreateTranslation(new Vector3(4f/64, 4f/64, 0.1f)) * _camera.ViewMatrix * _camera.Projection;
            _spriteEfect.CurrentTechnique.Passes[0].Apply();
            
            GraphicsDevice.DrawIndexedPrimitives(PrimitiveType.TriangleList, 0, 0, 2);

            // colors
            _spriteEfect.Color = Color.White;
            _spriteEfect.TransformMatrix = world * Matrix.CreateTranslation(new Vector3(0, 0, 0.2f)) * _camera.ViewMatrix * _camera.Projection;
            _spriteEfect.CurrentTechnique.Passes[0].Apply();
            
            GraphicsDevice.DrawIndexedPrimitives(PrimitiveType.TriangleList, 0, 0, 2);
        }


        base.Draw(gameTime);
    }
}