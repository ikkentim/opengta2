using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace OpenGta2.Client.Levels;

public record RenderableMapChunk(Point ChunkLocation, VertexBuffer Vertices, IndexBuffer Indices, int OpaquePrimitiveCount, int FlatIndexOffset, int FlatPrimitiveCount, Matrix Translation);