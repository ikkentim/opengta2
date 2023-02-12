using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace OpenGta2.Client;

public record RenderableMapChunk(Point ChunkLocation, VertexBuffer Vertices, IndexBuffer Indices, int PrimitiveCount, Matrix Translation);