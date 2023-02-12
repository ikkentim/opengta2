using System;
using System.Collections.Generic;
using System.Diagnostics;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using OpenGta2.Client.Effects;
using OpenGta2.Data.Map;
using OpenGta2.Data.Riff;
using OpenGta2.Data.Style;

namespace OpenGta2.Client;

public class LevelProvider
{
    private readonly Vector3[] _cameraCornersBuffer = new Vector3[8];

    private const int ChunkSize = 16;
    private int MaxChunksX;
    private int MaxChunksY;

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

        MaxChunksX = (int)Math.Ceiling(Map.Width / (float)ChunkSize);
        MaxChunksY = (int)Math.Ceiling(Map.Height / (float)ChunkSize);
    }

    private Rectangle _chunkBounds;

    private Dictionary<Point,RenderableMapChunk> _chunks = new();

    public IEnumerable<RenderableMapChunk> GetRenderableChunks() => _chunks.Values;

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
        var chunkMinX = minX / ChunkSize;
        var chunkMaxX = (int)MathF.Ceiling(maxX / ChunkSize);
        var chunkMinY = minY / ChunkSize;
        var chunkMaxY = (int)MathF.Ceiling(maxY / ChunkSize);

        chunkMinX = Math.Clamp(chunkMinX, 0, MaxChunksX);
        chunkMaxX = Math.Clamp(chunkMaxX, 0, MaxChunksX);
        chunkMinY = Math.Clamp(chunkMinY, 0, MaxChunksY);
        chunkMaxY = Math.Clamp(chunkMaxY, 0, MaxChunksY);
        
        var chunkBounds = new Rectangle(chunkMinX, chunkMinY, chunkMaxX - chunkMinX, chunkMaxY - chunkMinY);

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
            if (_chunks.ContainsKey(new Point(x, y)))
            {
                // already loaded
                continue;
            }

            LoadChunk(x, y);
        }

        _chunkBounds = chunkBounds;
    }
    
    private readonly BufferArray<short> _indices = new();
    private readonly BufferArray<VertexPositionTile> _vertices = new();

    private void LoadChunk(int chunkX, int chunkY)
    {
        var point = new Point(chunkX, chunkY);

        var minX = chunkX * ChunkSize;
        var maxX = minX + ChunkSize;
        var minY = chunkY * ChunkSize;
        var maxY = minY + ChunkSize;
        
        var map = Map.CompressedMap;
        
        for (var x = minX; x < maxX; x++)
        for (var y = minY; y < maxY; y++)
        {
            var column = Map.GetColumn(x, y);

            for (var z = column.Offset; z < column.Height; z++)
            {
                var offset = new Vector3(x - minX, y - minY, z);

                var blockNum = column.Blocks[z - column.Offset];
                ref var block = ref map.Blocks[blockNum];

                SlopeGenerator.Push(ref block, offset, _vertices, _indices);
            }
        }
        
        var vert = new VertexBuffer(_graphicsDevice, typeof(VertexPositionTile), _vertices.Length,
            BufferUsage.WriteOnly);
        var idx = new IndexBuffer(_graphicsDevice, typeof(short), _indices.Length, BufferUsage.WriteOnly);
        vert.SetData(_vertices.GetArray(), 0, _vertices.Length);
        idx.SetData(_indices.GetArray(), 0, _indices.Length);

        _chunks[point] = new RenderableMapChunk(point, vert, idx, _indices.Length / 3, Matrix.CreateTranslation(chunkX * ChunkSize, chunkY * ChunkSize, 0));
        
        _vertices.Reset();
        _indices.Reset();
    }

    private void UnloadChunk(RenderableMapChunk chunk)
    {
        chunk.Indices.Dispose();
        chunk.Vertices.Dispose();

        _chunks.Remove(chunk.ChunkLocation);
    }
}