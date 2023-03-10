using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace OpenGta2.Client.Rendering;

public struct VertexPositionSprite : IVertexType
{
    private static readonly VertexDeclaration _declaration = new(
        new VertexElement(0, VertexElementFormat.Vector3, VertexElementUsage.Position, 0),
        new VertexElement(4 * 3, VertexElementFormat.Vector2, VertexElementUsage.TextureCoordinate, 0)
    );

    VertexDeclaration IVertexType.VertexDeclaration => _declaration;

    public Vector3 Position;
    public Vector2 Uv;

    public VertexPositionSprite(Vector3 position, Vector2 uv)
    {
        
        Position = position;
        Uv = uv;
    }
}