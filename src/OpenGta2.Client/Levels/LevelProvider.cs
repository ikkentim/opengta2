using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using OpenGta2.Client.Data;
using OpenGta2.Client.Rendering;
using OpenGta2.Client.Utilities;
using OpenGta2.GameData.Map;
using OpenGta2.GameData.Riff;
using OpenGta2.GameData.Style;

namespace OpenGta2.Client.Levels;

public class LevelProvider
{
    public const int ChunkSize = 8;
    private readonly Vector3[] _cameraCornersBuffer = new Vector3[8];
    private readonly Dictionary<Point, RenderableMapChunk> _chunks = new();
    private readonly BufferArray<(float drawOrder, short index)> _flatIndices = new();


    private readonly GraphicsDevice _graphicsDevice;

    private readonly BufferArray<short> _indices = new();
    private readonly BufferArray<VertexPositionTile> _vertices = new();
    private Rectangle _chunkBounds;
    private int _maxChunksX;
    private int _maxChunksY;
    private Map? _map;
    private StyleTextureSet? _textures;
    private Style? _style;

    public LevelProvider(GraphicsDevice graphicsDevice)
    {
        _graphicsDevice = graphicsDevice;
    }

    public Map Map => _map ?? throw ThrowHelper.GetLevelNotLoaded();


    public Style Style => _style ?? throw ThrowHelper.GetLevelNotLoaded();


    public StyleTextureSet Textures => _textures ?? throw ThrowHelper.GetLevelNotLoaded();


    public bool IsMapLoaded => _map != null && _style != null && _textures != null;

    public void LoadLevel(string mapFile, string styleFile)
    {
        try
        {
            using var mapStream = TestGamePath.OpenFile(mapFile);
            using var mapRiffReader = new RiffReader(mapStream);
            var mapreader = new MapReader(mapRiffReader);

            using var styleStream = TestGamePath.OpenFile(styleFile);
            using var styleRiffReader = new RiffReader(styleStream);
            var styleReader = new StyleReader(styleRiffReader);

            _map = mapreader.Read();
            _style = styleReader.Read();
            _textures = StyleTextureSet.Create(_style, _graphicsDevice);

            _maxChunksX = (int)Math.Ceiling(Map.Width / (float)ChunkSize);
            _maxChunksY = (int)Math.Ceiling(Map.Height / (float)ChunkSize);
        }
        catch
        {
            UnloadLevel();
            throw;
        }
    }

    public void UnloadLevel()
    {
        _map = null;
        _style = null;
        _textures = null;
    }

    public IEnumerable<RenderableMapChunk> GetRenderableChunks()
    {
        return _chunks.Values;
    }

    public void Update(Camera camera)
    {
        if (!IsMapLoaded)
            return;

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

        chunkMinX = Math.Clamp(chunkMinX, 0, _maxChunksX);
        chunkMaxX = Math.Clamp(chunkMaxX, 0, _maxChunksX);
        chunkMinY = Math.Clamp(chunkMinY, 0, _maxChunksY);
        chunkMaxY = Math.Clamp(chunkMaxY, 0, _maxChunksY);

        var chunkBounds = new Rectangle(chunkMinX, chunkMinY, chunkMaxX - chunkMinX, chunkMaxY - chunkMinY);

        // unload invisible chunks
        for (var x = _chunkBounds.Left; x < _chunkBounds.Right; x++)
        {
            for (var y = _chunkBounds.Top; y < _chunkBounds.Bottom; y++)
            {
                if (chunkBounds.Contains(x, y) || !_chunks.TryGetValue(new Point(x, y), out var chunk))
                    continue;

                UnloadChunk(chunk);
            }
        }

        // load new visible chunks
        for (var x = chunkBounds.Left; x < chunkBounds.Right; x++)
        {
            for (var y = chunkBounds.Top; y < chunkBounds.Bottom; y++)
            {
                if (_chunks.ContainsKey(new Point(x, y)))
                    // already loaded
                    continue;

                LoadChunk(x, y);
            }
        }

        _chunkBounds = chunkBounds;
    }

    private void LoadChunk(int chunkX, int chunkY)
    {
        var point = new Point(chunkX, chunkY);

        var minX = chunkX * ChunkSize;
        var maxX = minX + ChunkSize;
        var minY = chunkY * ChunkSize;
        var maxY = minY + ChunkSize;

        var map = Map.CompressedMap;

        for (var x = minX; x < maxX; x++)
        {
            for (var y = minY; y < maxY; y++)
            {
                var column = Map.GetColumn(x, y);

                for (var z = column.Offset; z < column.Height; z++)
                {
                    var offset = new Vector3(x - minX, y - minY, z);

                    var blockNum = column.Blocks[z - column.Offset];
                    ref var block = ref map.Blocks[blockNum];

                    SlopeGenerator.Push(ref block, offset, _vertices, _indices, _flatIndices);
                }
            }
        }

        var flats = _flatIndices.GetArray()
            .Take(_flatIndices.Length)
            .OrderBy(x => x.drawOrder)
            .Select(x => x.index)
            .ToArray();

        var vert = new VertexBuffer(_graphicsDevice, typeof(VertexPositionTile), _vertices.Length, BufferUsage.WriteOnly);
        var idx = new IndexBuffer(_graphicsDevice, typeof(short), _indices.Length + flats.Length, BufferUsage.WriteOnly);
        vert.SetData(_vertices.GetArray(), 0, _vertices.Length);
        idx.SetData(_indices.GetArray(), 0, _indices.Length);

        idx.SetData(_indices.Length * 2, flats, 0, flats.Length);

        _chunks[point] = new RenderableMapChunk(point, vert, idx, _indices.Length / 3, _indices.Length, flats.Length / 3,
            Matrix.CreateTranslation(chunkX * ChunkSize, chunkY * ChunkSize, 0));

        _vertices.Reset();
        _indices.Reset();
        _flatIndices.Reset();
    }

    private void UnloadChunk(RenderableMapChunk chunk)
    {
        chunk.Indices.Dispose();
        chunk.Vertices.Dispose();

        _chunks.Remove(chunk.ChunkLocation);
    }

    public Span<Light> CollectLights(Span<Light> buffer, Point chunkLocation)
    {
        var minX = chunkLocation.X * ChunkSize;
        var minY = chunkLocation.Y * ChunkSize;
        var maxX = minX + ChunkSize;
        var maxY = minY + ChunkSize;

        // TODO: Performance is terrible. Lights should be in a quadtree for optimization.
        var index = 0;
        foreach (var light in Map.Lights)
        {
            if (index == buffer.Length)
            {
                Debug.WriteLine($"Hit lights limit {buffer.Length} at chunk {chunkLocation}");
                return buffer; // hit limit of lights for this chunk
            }
            if (!IsInRadius(minX, maxX, light.Radius, light.X) || !IsInRadius(minY, maxY, light.Radius, light.Y))
                continue;

            var point = new Vector3(light.X, light.Y, light.Z);
            var color = new Color(light.ARGB.R, light.ARGB.G, light.ARGB.B, light.ARGB.A);

            buffer[index] = new Light(point, color, light.Radius, light.Intensity / 256f);

            index++;
        }

        return buffer[..index];
    }
    
    private static bool IsInRadius(float min, float max, float radius, float value)
    {
        return min - radius <= value && max + radius >= value;
    }
}