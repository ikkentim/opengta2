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
    private short[] _indices = { 2, 1, 0, 2, 3, 1, /* reversed because i messed up. quick hacks: */0, 1, 2, 1, 3, 2 };
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
        indexBuffer = new IndexBuffer(GraphicsDevice, typeof(short), 12, BufferUsage.WriteOnly);
        indexBuffer.SetData(_indices);

        base.LoadContent();
    }

    private const float BlockSize = 10;

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
            var colMin = new Vector2(colX * BlockSize, colY * BlockSize);
            var colMax = colMin + new Vector2(BlockSize, BlockSize);
            if (!_camera.Frustum.Intersects(new BoundingBox(new Vector3(colMin, -10), new Vector3(colMax, 10))))
            {
                continue;
            }

            // read compressed map and render column
            var columnNum = map.Base[colX, colY];
            var column = map.Columns[columnNum];
                
            var blockY = BlockSize * colY;
            var blockX = BlockSize * colX;

            for (var colZ = column.Offset; colZ < column.Height; colZ++)
            {
                var blockZ = BlockSize * colZ;

                var blockNum = column.Blocks[colZ - column.Offset];

                var block = map.Blocks[blockNum];

                // TODO: this can be greatly optimized by preloading common face variations in the vertex buffer only only rewriting the buffer for slopes
                // This code is really stupid... But quick results!
                // No diagonals / slopes yet.
                if (block.Lid.TileGraphic != 0)
                {
                    RenderFace(new Vector3(blockX, blockY, blockZ), 
                        new Vector3(0, 0, BlockSize),
                        new Vector3(BlockSize, 0, BlockSize), 
                        new Vector3(0, BlockSize, BlockSize),
                        new Vector3(BlockSize, BlockSize, BlockSize), 
                        Color.Red, colZ, true);
                }
                
                if (block.Right.TileGraphic != 0)
                {
                    RenderFace(new Vector3(blockX, blockY, blockZ), 
                        new Vector3(BlockSize, BlockSize, BlockSize),
                        new Vector3(BlockSize, 0, BlockSize), 
                        new Vector3(BlockSize, BlockSize, 0),
                        new Vector3(BlockSize, 0, 0), 
                        Color.Blue, colZ, true);
                }
                if (block.Bottom.TileGraphic != 0)
                {
                    RenderFace(new Vector3(blockX, blockY, blockZ), 
                        new Vector3(0, BlockSize, BlockSize),
                        new Vector3(BlockSize, BlockSize, BlockSize),
                        new Vector3(0, BlockSize, 0),
                        new Vector3(BlockSize, BlockSize, 0),
                        Color.Yellow, colZ, true);
                }
                if (block.Top.TileGraphic != 0)
                {
                    RenderFace(new Vector3(blockX, blockY, blockZ), 
                        new Vector3(BlockSize, 0, BlockSize),
                        new Vector3(0, 0, BlockSize), 
                        new Vector3(BlockSize, 0, 0),
                        new Vector3(0, 0, 0), 
                        Color.Green, colZ, true);
                }
                if (block.Left.TileGraphic != 0)
                {
                    RenderFace(new Vector3(blockX, blockY, blockZ), 
                        new Vector3(0, 0, BlockSize),
                        new Vector3(0, BlockSize, BlockSize), 
                        new Vector3(0, 0, 0),
                        new Vector3(0, BlockSize, 0), 
                        Color.Magenta, colZ, true);
                }
            }
        }

        base.Draw(gameTime);
    }

    private void RenderFace(Vector3 blockPosition, Vector3 topLeft, Vector3 topRight, Vector3 bottomLeft,
        Vector3 bottomRight, Color color, float depth, bool reverse)
    {
        // depth color change for testing
        color = new Color((byte)MathHelper.Clamp(color.R - depth * 15, 0, 255), (byte)MathHelper.Clamp(color.G - depth * 10, 0, 255), (byte)MathHelper.Clamp(color.B - depth * 10, 0, 255), color.A);

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
            _game.GraphicsDevice.DrawIndexedPrimitives(PrimitiveType.TriangleList, 0, reverse ? 6 : 0, 2);
        }
    }

    public static class GtaVector
    {
        private static readonly Vector3 _left = -Vector3.UnitX;
        private static readonly Vector3 _right = Vector3.UnitX;
        private static readonly Vector3 _up = -Vector3.UnitY;
        private static readonly Vector3 _down = Vector3.UnitY;
        private static readonly Vector3 _sky = Vector3.UnitZ;
        /// <summary>
        /// (-1, 0, 0)
        /// </summary>
        public static Vector3 Left => _left;
        /// <summary>
        /// (1, 0, 0)
        /// </summary>
        public static Vector3 Right => _right;
        /// <summary>
        /// (0, -1, 0)
        /// </summary>
        public static Vector3 Up => _up;
        /// <summary>
        /// (0, 1, 0)
        /// </summary>
        public static Vector3 Down => _down;

        /// <summary>
        /// (0, 0, 1)
        /// </summary>
        public static Vector3 Skywards => _sky;
    }
}