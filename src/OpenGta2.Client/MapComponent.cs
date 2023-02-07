using System.Diagnostics;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using OpenGta2.Data.Map;

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
        vertexBuffer = new VertexBuffer(GraphicsDevice, typeof(VertexPositionColor), 4, BufferUsage.WriteOnly);
        indexBuffer = new IndexBuffer(GraphicsDevice, typeof(short), 6, BufferUsage.WriteOnly);
        indexBuffer.SetData(_indices);

        base.LoadContent();
    }
    
    public override void Draw(GameTime gameTime)
    {
        Debug.WriteLine(_camera.Position);
        _game.BasicEffect.VertexColorEnabled = true;
        _game.BasicEffect.View = _camera.ViewMatrix;
        _game.BasicEffect.Projection = _game.Projection;
        _game.GraphicsDevice.Indices = indexBuffer;

        var map = _map.CompressedMap;

        var maxColY = map.Base.GetLength(0);
        var maxColX = map.Base.GetLength(1);
        
        for (var colX = 0; colX < maxColX; colX++)
        for (var colY = 0; colY < maxColY; colY++)
        {

            // simple column culling
            var colMin = new Vector2(colX, colY);
            var colMax = colMin + Vector2.One;
            if (!_camera.Frustum.Intersects(new BoundingBox(new Vector3(colMin, 0), new Vector3(colMax, 5))))
            {
                continue;
            }

            // read compressed map and render column
            var columnNum = map.Base[colX, colY];
            var column = map.Columns[columnNum];
                
            var blockY = colY;
            var blockX = colX;

            for (var colZ = column.Offset; colZ < column.Height; colZ++)
            {
                var blockZ = colZ;

                var blockNum = column.Blocks[colZ - column.Offset];

                var block = map.Blocks[blockNum];

                // TODO: this can be greatly optimized by preloading common face variations in the vertex buffer only only rewriting the buffer for slopes
                // This code is really stupid... But quick results!
                // No diagonals / slopes yet.
                if (block.Lid.TileGraphic != 0)
                {
                    RenderFace(new Vector3(blockX, blockY, blockZ), 
                        new Vector3(0, 0, 1),
                        new Vector3(1, 0, 1), 
                        new Vector3(0, 1, 1),
                        new Vector3(1, 1, 1), 
                        Color.Red, colZ);
                }
                
                if (block.Right.TileGraphic != 0)
                {
                    RenderFace(new Vector3(blockX, blockY, blockZ), 
                        new Vector3(1, 1, 1),
                        new Vector3(1, 0, 1), 
                        new Vector3(1, 1, 0),
                        new Vector3(1, 0, 0), 
                        Color.Blue, colZ);
                }
                if (block.Bottom.TileGraphic != 0)
                {
                    RenderFace(new Vector3(blockX, blockY, blockZ), 
                        new Vector3(0, 1, 1),
                        new Vector3(1, 1, 1),
                        new Vector3(0, 1, 0),
                        new Vector3(1, 1, 0),
                        Color.Yellow, colZ);
                }
                if (block.Top.TileGraphic != 0)
                {
                    RenderFace(new Vector3(blockX, blockY, blockZ), 
                        new Vector3(1, 0, 1),
                        new Vector3(0, 0, 1), 
                        new Vector3(1, 0, 0),
                        new Vector3(0, 0, 0), 
                        Color.Green, colZ);
                }
                if (block.Left.TileGraphic != 0)
                {
                    RenderFace(new Vector3(blockX, blockY, blockZ), 
                        new Vector3(0, 0, 1),
                        new Vector3(0, 1, 1), 
                        new Vector3(0, 0, 0),
                        new Vector3(0, 1, 0), 
                        Color.Magenta, colZ);
                }
            }
        }

        base.Draw(gameTime);
    }

    private void RenderFace(Vector3 blockPosition, Vector3 topLeft, Vector3 topRight, Vector3 bottomLeft,
        Vector3 bottomRight, Color color, float depth)
    {
        // depth color change for testing
        var colorScale = ((depth + 2) / 7.0f);
        color = new Color((byte)MathHelper.Clamp(color.R * colorScale, 0, 255), (byte)MathHelper.Clamp(color.G * colorScale, 0, 255), (byte)MathHelper.Clamp(color.B * colorScale, 0, 255), color.A);

        _vertices[0] = new VertexPositionColor(topLeft, color);
        _vertices[1] = new VertexPositionColor(topRight, color);
        _vertices[2] = new VertexPositionColor(bottomLeft, color);
        _vertices[3] = new VertexPositionColor(bottomRight, color);

        vertexBuffer!.SetData(_vertices);

        _game.GraphicsDevice.SetVertexBuffer(vertexBuffer);

        _game.BasicEffect.World = Matrix.CreateTranslation(blockPosition);
        
        foreach (var pass in _game.BasicEffect.CurrentTechnique.Passes)
        {
            pass.Apply();
            _game.GraphicsDevice.DrawIndexedPrimitives(PrimitiveType.TriangleList, 0, 0, 2);
        }
    }
}