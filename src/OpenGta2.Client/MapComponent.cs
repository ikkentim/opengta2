using System.Collections.Generic;
using System.Diagnostics;
using System.Transactions;
using Accessibility;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using OpenGta2.Data.Map;
using SharpDX.DXGI;

namespace OpenGta2.Client;

public class MapComponent : DrawableGameComponent
{
    private readonly GtaGame _game;
    private readonly Camera _camera;
    private readonly Map _map;
    private VertexBuffer? vertexBuffer;
    private IndexBuffer? indexBuffer;

    public MapComponent(GtaGame game, Camera camera, Map map) : base(game)
    {
        _game = game;
        _camera = camera;
        _map = map;
    }

    protected override void LoadContent()
    {
        // for now simple buffers for drawing a single face. will optimize this later.
        vertexBuffer = new VertexBuffer(GraphicsDevice, typeof(VertexPositionColor), 4 * 5, BufferUsage.WriteOnly);
        indexBuffer = new IndexBuffer(GraphicsDevice, typeof(short), 6 * 5, BufferUsage.WriteOnly);

        base.LoadContent();
    }

    public override void Draw(GameTime gameTime)
    {
        _game.BasicEffect.VertexColorEnabled = true;
        _game.BasicEffect.View = _camera.ViewMatrix;
        _game.BasicEffect.Projection = _game.Projection;
        _game.GraphicsDevice.Indices = indexBuffer;
        _game.GraphicsDevice.SetVertexBuffer(vertexBuffer);


        var map = _map.CompressedMap;

        var maxX = _map.Width;
        var maxY = _map.Height;

        var vertices = new List<VertexPositionColor>();
        var indices = new List<short>();

        for (var x = 0; x < maxX; x++)
        for (var y = 0; y < maxY; y++)
        {
            // simple column culling
            var colMin = new Vector2(x, y);
            var colMax = colMin + Vector2.One;
            if (!_camera.Frustum.Intersects(new BoundingBox(new Vector3(colMin, 0), new Vector3(colMax, 5))))
            {
                continue;
            }

            // read compressed map and render column
            var column = _map.GetColumn(x, y);

            for (var z = column.Offset; z < column.Height; z++)
            {
                vertices.Clear();
                indices.Clear();

                var blockNum = column.Blocks[z - column.Offset];
                ref var block = ref map.Blocks[blockNum];

                SlopeGenerator.Push(ref block, z, vertices, indices);

                if (indices.Count > 0)
                {
                    // TODO: don't use list so we have access to inner array
                    vertexBuffer!.SetData(vertices.ToArray());
                    indexBuffer!.SetData(indices.ToArray());

                    _game.BasicEffect.World = Matrix.CreateTranslation(new Vector3(x, y, z));

                    foreach (var pass in _game.BasicEffect.CurrentTechnique.Passes)
                    {
                        pass.Apply();
                        _game.GraphicsDevice.DrawIndexedPrimitives(PrimitiveType.TriangleList, 0, 0, indices.Count / 3);
                    }
                }
            }
        }

        base.Draw(gameTime);
    }
}