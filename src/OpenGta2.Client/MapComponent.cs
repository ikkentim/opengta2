using System;
using System.Collections;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using OpenGta2.Client.Effects;
using OpenGta2.Data.Map;
using OpenGta2.Data.Riff;
using OpenGta2.Data.Style;

namespace OpenGta2.Client;

public class MapComponent : DrawableGameComponent
{
    private readonly GtaGame _game;
    private readonly Camera _camera;
    private readonly Map _map;
    private readonly Style _style;
    private VertexBuffer? vertexBuffer;
    private IndexBuffer? indexBuffer;
    private Texture2D? _tilesTexture;
    private BlockFaceEffect? _blockFaceEffect;

    public MapComponent(GtaGame game, Camera camera, Map map, Style style) : base(game)
    {
        _game = game;
        _camera = camera;
        _map = map;
        _style = style;
    }

    protected override void LoadContent()
    {
        vertexBuffer = new VertexBuffer(GraphicsDevice, typeof(VertexPositionTile), 4 * 5, BufferUsage.WriteOnly);
        indexBuffer = new IndexBuffer(GraphicsDevice, typeof(short), 6 * 5, BufferUsage.WriteOnly);

        _blockFaceEffect = _game.Services.GetService<AssetManager>()
            .CreateBlockFaceEffect();
        
        CreateTilesTexture();
        
        _blockFaceEffect.Tiles = _tilesTexture!;
    }

    private void CreateTilesTexture()
    {
        // TODO: Move texture generation somewhere else, where we can also generate textures for the spritemaps.
        // TODO: Switch to loading each page as a texture in the texture array instead of each separate tile.
        // This would load more efficiently and makes no difference in rendering.
        var tileCount = _style.Tiles.Count;

        _tilesTexture = new Texture2D(GraphicsDevice, Tiles.TileWidth, Tiles.TileHeight, false, SurfaceFormat.Color,
            tileCount);

        // style can contain up to 992 tiles, each tile is 64x64 pixels.
        var tileData = new uint[Tiles.TileWidth * Tiles.TileHeight];

        for (ushort tileNumber = 0; tileNumber < tileCount; tileNumber++)
        {
            // don't need to add a base for virtual palette number - base for tiles is always 0.
            var physicalPaletteNumber = _style.PaletteIndex.PhysPalette[tileNumber];
            var palette = _style.PhysicsalPalette.GetPalette(physicalPaletteNumber);

            for (var y = 0; y < Tiles.TileHeight; y++)
            {
                var tileSlice = _style.Tiles.GetTileSlice(tileNumber, y);
                for (var x = 0; x < Tiles.TileWidth; x++)
                {
                    var colorEntry = tileSlice[x]; // 0 is always transparent
                    tileData[y * Tiles.TileWidth + x] = colorEntry == 0
                        ? 0
                        : palette.GetColor(colorEntry)
                            .Argb;
                }
            }
            
            _tilesTexture.SetData(0, tileNumber, null, tileData, 0, tileData.Length);
        }
    }
    
    public override void Draw(GameTime gameTime)
    {
        _blockFaceEffect!.View = _camera.ViewMatrix;
        _blockFaceEffect.Projection = _camera.Projection;
        _game.GraphicsDevice.Indices = indexBuffer;
        _game.GraphicsDevice.SetVertexBuffer(vertexBuffer);
        // _game.GraphicsDevice.BlendState = BlendState.AlphaBlend; // TODO: handle alpha in a shader

        var map = _map.CompressedMap;

        var maxX = _map.Width;
        var maxY = _map.Height;

        var vertices = new List<VertexPositionTile>();
        var indices = new List<short>();

        // TODO: Separate the map into segments. When a segment enters the view frustum, load the segment into vram
        // This would allow the map to stay in memory, and we wouldn't have to regenerate the vertices each draw call.
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

                SlopeGenerator.Push(ref block, vertices, indices);

                if (indices.Count > 0)
                {
                    // TODO: don't use list so we have access to inner array
                    vertexBuffer!.SetData(vertices.ToArray());
                    indexBuffer!.SetData(indices.ToArray());

                    _blockFaceEffect.World = Matrix.CreateTranslation(new Vector3(x, y, z));

                    // TODO: Should render flat tiles in a separate render pass to allow for transparency (instead of clipping)
                    foreach (var pass in _blockFaceEffect.CurrentTechnique.Passes)
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

public class LevelProvider
{
    private readonly Vector3[] _cameraCornersBuffer = new Vector3[8];

    private const int ChunkSize = 8;
    private const int MaxChunksX = 256 / ChunkSize;
    private const int MaxChunksY = 256 / ChunkSize;

    private readonly GraphicsDevice _graphicsDevice;

    public LevelProvider(GraphicsDevice graphicsDevice)
    {
        _graphicsDevice = graphicsDevice;
    }

    public Map? Map { get; private set; }
    public Style? Style { get; private set; }

    public bool IsMapLoaded => Map != null && Style != null;
    
    public void LoadLevel(string mapFile, string styleFile)
    {
        using var mapStream = TestGamePath.OpenFile(mapFile);
        using var mapRiffReader = new RiffReader(mapStream);
        var mapreader = new MapReader(mapRiffReader);

        using var styleStream = TestGamePath.OpenFile(styleFile);
        using var styleRiffReader = new RiffReader(styleStream);
        var styleReader = new StyleReader(styleRiffReader);

        Map = mapreader.Read();
        Style = styleReader.Read();
    }

    private Rectangle _chunkBounds;

    private Dictionary<Point,RenderableMapChunk> _chunks = new();

    public void Update(Camera camera)
    {
        // find visible chunks
        camera.Frustum.GetCorners(_cameraCornersBuffer);
        var fovBounds = BoundingBox.CreateFromPoints(_cameraCornersBuffer);
        var minX = (int)MathF.Floor(fovBounds.Min.X);
        var maxX = MathF.Ceiling(fovBounds.Max.X);
        var minY = (int)MathF.Floor(fovBounds.Min.Y);
        var maxY = MathF.Ceiling(fovBounds.Max.Y);
        
        // align with chunk bounds
        var chunkMinX = minX / ChunkSize * ChunkSize;
        var chunkMinY = minY / ChunkSize * ChunkSize;
        var chunkMaxX = (int)MathF.Ceiling(maxX / ChunkSize);
        var chunkMaxY = (int)MathF.Ceiling(maxY / ChunkSize);

        chunkMinX = Math.Clamp(chunkMinX, 0, MaxChunksX);
        chunkMaxX = Math.Clamp(chunkMaxX, 0, MaxChunksX);
        chunkMinY = Math.Clamp(chunkMinY, 0, MaxChunksY);
        chunkMaxY = Math.Clamp(chunkMaxY, 0, MaxChunksY);

        var chunkBounds = new Rectangle(chunkMinX, chunkMaxY, chunkMaxX - chunkMinX, chunkMaxY - chunkMinY);

        // unload invisible chunks
        for (var x = _chunkBounds.Left; x < _chunkBounds.Right; x++)
        for (var y = _chunkBounds.Top; y < _chunkBounds.Bottom; y++)
        {
            if (chunkBounds.Contains(x, y) || !_chunks.TryGetValue(new Point(x, y), out var chunk))
                continue;

            UnloadChunk(chunk);
        }

        // load new visible chunks
        for (var x = chunkBounds.Left; x < chunkBounds.Right; x++)
        for (var y = chunkBounds.Top; y < chunkBounds.Bottom; y++)
        {
            LoadChunk(x, y);
        }

        _chunkBounds = chunkBounds;
    }
    
    private readonly BufferArray<short> _indices = new();
    private readonly BufferArray<VertexPositionTile> _vertices = new();

    private void LoadChunk(int chunkX, int chunkY)
    {
        if (_chunks.ContainsKey(new Point(chunkX, chunkY)))
        {
            // already loaded
            return;
        }
        
        var minX = chunkX * ChunkSize;
        var maxX = minX + ChunkSize;
        var minY = chunkY * ChunkSize;
        var maxY = minY + ChunkSize;
        
        var map = Map.CompressedMap;
        
        for (var x = minX; x < maxX; x++)
        for (var y = minY; y < maxY; y++)
        {
            // read compressed map and render column
            var column = Map.GetColumn(x, y);

            for (var z = column.Offset; z < column.Height; z++)
            {
                var blockNum = column.Blocks[z - column.Offset];
                ref var block = ref map.Blocks[blockNum];

                SlopeGenerator.Push(ref block, _vertices, _indices);
            }
        }
        
        var vert = new VertexBuffer(_graphicsDevice, typeof(VertexPositionTile), _vertices.Length,
            BufferUsage.WriteOnly);
        var idx = new IndexBuffer(_graphicsDevice, typeof(short), _indices.Length, BufferUsage.WriteOnly);
        vert.SetData(_vertices.GetArray(), 0, _vertices.Length);
        idx.SetData(_indices.GetArray(), 0, _indices.Length);

        _chunks[new Point(chunkX, chunkY)] = new RenderableMapChunk(new Point(chunkX, chunkY), vert, idx);

        _vertices.Clear();
        _indices.Clear();
    }

    private void UnloadChunk(RenderableMapChunk chunk)
    {
        chunk.Indices.Dispose();
        chunk.Vertices.Dispose();

        _chunks.Remove(chunk.ChunkLocation);
    }
}

public class BufferArray<T> : ICollection<T>
{
    // TODO: Remove ICollection interface

    private T[] _buffer = new T[16];

    private int _length;

    public int Length => _length;

    public void Reset(bool clear = false)
    {
        _length = 0;
        if (clear)
        {
            Array.Clear(_buffer);
        }
    }

    private void Resize()
    {
        var buffer = new T[_buffer.Length * 2];
        Array.Copy(_buffer, 0, buffer, 0, _length);
        _buffer = buffer;
    }

    public void Add(T value)
    {
        if (_buffer.Length == _length)
        {
            Resize();
        }

        _buffer[_length++] = value;
    }

    public void Clear()
    {
        throw new NotImplementedException();
    }

    public bool Contains(T item) => throw new NotImplementedException();

    public void CopyTo(T[] array, int arrayIndex)
    {
        throw new NotImplementedException();
    }

    public bool Remove(T item) => throw new NotImplementedException();

    public int Count => _buffer.Length;
    public bool IsReadOnly => false;

    public T[] GetArray() => _buffer;

    public Span<T> AsSpan() => _buffer.AsSpan(0, _length);

    public IEnumerator<T> GetEnumerator() => throw new NotImplementedException();

    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
}


public record RenderableMapChunk(Point ChunkLocation, VertexBuffer Vertices, IndexBuffer Indices);