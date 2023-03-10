using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using OpenGta2.Client.Diagnostics;
using OpenGta2.Client.Levels;
using OpenGta2.Client.Rendering;
using OpenGta2.Client.Rendering.Effects;
using OpenGta2.Client.Utilities;
using OpenGta2.GameData.Style;

namespace OpenGta2.Client.Components;

public class PedManagerComponent : DrawableGameComponent
{
    private readonly Camera _camera;
    private IndexBuffer? _indices;
    private VertexBuffer? _vertices;
    private WorldSpriteEffect? _spriteEfect;
    private new GtaGame Game => (GtaGame)base.Game;
    private LevelProvider _levelProvider;

    public PedManagerComponent(GtaGame game, Camera camera) : base(game)
    {
        _camera = camera;
        _levelProvider = Game.Services.GetService<LevelProvider>();

        Peds.Add(new Ped(new Vector3(11.5f, 2.5f, GetZ(12, 2)), 0, 25));
    }

    private int GetZ(int x, int y)
    {
        var b = _levelProvider.Map.CompressedMap.Base[y, x];
        var col = _levelProvider.Map.CompressedMap.Columns[b];

        for (var z = col.Height - col.Offset - 1; z > 0; z--)
        {
            var block = _levelProvider.Map.CompressedMap.Blocks[col.Blocks[z]];

            if (block.Lid.TileGraphic != 0 &&
                (block.Top.TileGraphic != 0 || block.Right.TileGraphic != 0 || block.Bottom.TileGraphic != 0 || block.Left.TileGraphic != 0))
            {
                return col.Offset + y + 1;
            }
        }
        return col.Offset + 1;
    }

    private List<Ped> Peds { get; } = new();

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

    private KeyboardState _lastState;
    public override void Update(GameTime gameTime)
    {
        var kb = Keyboard.GetState();
        var fd = 0f;
        var lr = 0f;


        if (kb.IsKeyDown(Keys.Right))
            lr++;

        if (kb.IsKeyDown(Keys.Left))
            lr--;

        if (kb.IsKeyDown(Keys.Up))
            fd++;

        if (kb.IsKeyDown(Keys.Down))
            fd--;

        if (Peds.Count > 0)
        {
            var player = Peds[0];

            player.Rotation += lr * MathHelper.TwoPi * gameTime.GetDelta();
            
            var heading = new Vector2(MathF.Sin(-player.Rotation), MathF.Cos(-player.Rotation));
            player.Position += new Vector3(heading * fd * gameTime.GetDelta(), 0) * 2;

            _camera.Position = player.Position + GtaVector.Skywards * 5;

            player.Animation = fd != 0 ? PedAnimation.Walking  : PedAnimation.Idle;
        }
        
        if (kb.IsKeyDown(Keys.OemPlus) && !_lastState.IsKeyDown(Keys.OemPlus))
        {
            _pedAnimNum++;
            _pedAnimOveride = true;
        }

        if (kb.IsKeyDown(Keys.OemMinus) && !_lastState.IsKeyDown(Keys.OemMinus))
        {
            _pedAnimNum--;
            _pedAnimOveride = true;
        }
        
        foreach (var ped in Peds)
        {
            ped.UpdateAnimation(gameTime.GetDelta());
        }

        DiagnosticValues.Values["PedAnim"] = _pedAnimNum.ToString(CultureInfo.InvariantCulture);

        _lastState = kb;


        base.Update(gameTime);
    }

    private bool _pedAnimOveride = false;
    private int _pedAnimNum = 53;

    public override void Draw(GameTime gameTime)
    {
        GraphicsDevice.SetVertexBuffer(_vertices);
        GraphicsDevice.Indices = _indices;

        foreach (var ped in Peds)
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