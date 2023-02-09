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
    private VertexPositionColor[] _vertices = new VertexPositionColor[4];
    private short[] _indices = { 0, 1, 2, 1, 3, 2 };

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
        indexBuffer.SetData(_indices, 0, 6);

        base.LoadContent();
    }

    public override void Draw(GameTime gameTime)
    {
        Debug.WriteLine(_camera.Position);
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

                // QuickResultsRender(ref block, x, y, z);

                _slopeGenerator.Push(ref block, z, vertices, indices);

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

    private readonly SlopeGenerator _slopeGenerator = new();

    private void QuickResultsRender(ref BlockInfo block, int x, int y, int z)
    {
        // This code is really stupid... But quick results!
        // No diagonals / slopes yet.
        if (block.Lid.TileGraphic != 0)
        {
            RenderFace(new Vector3(x, y, z), new Vector3(0, 0, 1), new Vector3(1, 0, 1), new Vector3(0, 1, 1),
                new Vector3(1, 1, 1), Color.Red, z);
        }

        if (block.Right.TileGraphic != 0)
        {
            RenderFace(new Vector3(x, y, z), new Vector3(1, 1, 1), new Vector3(1, 0, 1), new Vector3(1, 1, 0),
                new Vector3(1, 0, 0), Color.Blue, z);
        }

        if (block.Bottom.TileGraphic != 0)
        {
            RenderFace(new Vector3(x, y, z), new Vector3(0, 1, 1), new Vector3(1, 1, 1), new Vector3(0, 1, 0),
                new Vector3(1, 1, 0), Color.Yellow, z);
        }

        if (block.Top.TileGraphic != 0)
        {
            RenderFace(new Vector3(x, y, z), new Vector3(1, 0, 1), new Vector3(0, 0, 1), new Vector3(1, 0, 0),
                new Vector3(0, 0, 0), Color.Green, z);
        }

        if (block.Left.TileGraphic != 0)
        {
            RenderFace(new Vector3(x, y, z), new Vector3(0, 0, 1), new Vector3(0, 1, 1), new Vector3(0, 0, 0),
                new Vector3(0, 1, 0), Color.Magenta, z);
        }
    }

    private void RenderFace(Vector3 blockPosition, Vector3 topLeft, Vector3 topRight, Vector3 bottomLeft,
        Vector3 bottomRight, Color color, float depth)
    {
        // depth color change for testing
        var colorScale = ((depth + 2) / 7.0f);
        color = new Color((byte)MathHelper.Clamp(color.R * colorScale, 0, 255),
            (byte)MathHelper.Clamp(color.G * colorScale, 0, 255), (byte)MathHelper.Clamp(color.B * colorScale, 0, 255),
            color.A);

        _vertices[0] = new VertexPositionColor(topLeft, color);
        _vertices[1] = new VertexPositionColor(topRight, color);
        _vertices[2] = new VertexPositionColor(bottomLeft, color);
        _vertices[3] = new VertexPositionColor(bottomRight, color);

        vertexBuffer!.SetData(_vertices);

        _game.BasicEffect.World = Matrix.CreateTranslation(blockPosition);

        foreach (var pass in _game.BasicEffect.CurrentTechnique.Passes)
        {
            pass.Apply();
            _game.GraphicsDevice.DrawIndexedPrimitives(PrimitiveType.TriangleList, 0, 0, 2);
        }
    }
}