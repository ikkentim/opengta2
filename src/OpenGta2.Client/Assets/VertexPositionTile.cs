using System.Runtime.InteropServices;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace OpenGta2.Client.Effects;

[StructLayout(LayoutKind.Sequential, Pack = 1)]
public struct VertexPositionTile : IVertexType
{
    private static readonly VertexDeclaration _declaration = new(
        new VertexElement(0, VertexElementFormat.Vector3, VertexElementUsage.Position, 0),
        new VertexElement(4 * 3, VertexElementFormat.Vector3, VertexElementUsage.TextureCoordinate, 0)
    );

    VertexDeclaration IVertexType.VertexDeclaration => _declaration;

    public Vector3 Position;
    public Vector3 TextureCoordinate;

    public VertexPositionTile(Vector3 position, Vector3 textureCoordinate)
    {
        Position = position;
        TextureCoordinate = textureCoordinate;
    }
}
