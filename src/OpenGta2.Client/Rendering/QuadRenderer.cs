using System.Runtime.InteropServices;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace OpenGta2.Client.Rendering;

public static class QuadRenderer
{
    private static readonly VertexPositionTexture[] _verts ={
        new(
            // top-left
            new Vector3(0,0,0),
            new Vector3(0,0,0)),
        new(
            // top-right
            new Vector3(0,0,0),
            new Vector3(1,0,0)),
        new(
            // bottom-left
            new Vector3(0,0,0),
            new Vector3(0,1,0)),
        new(
            // bottom-right
            new Vector3(0,0,0),
            new Vector3(1,1,0))
    };

    private static readonly short[] _ib = { 0, 1, 2, 2, 1, 3 };

    public static void Render(GraphicsDevice device, float texture, Vector2 v1, Vector2 v2)
    {
        Render(device, texture, v1, v2, Vector2.Zero, Vector2.One);
    }
    public static void Render(GraphicsDevice device, float texture, Vector2 v1, Vector2 v2, Vector2 u1, Vector2 u2)
    {
        _verts[0].Position.X = v1.X;
        _verts[0].Position.Y = v1.Y;
        _verts[0].TextureCoordinate.X = u1.X;
        _verts[0].TextureCoordinate.Y = u1.Y;
        _verts[0].TextureCoordinate.Z = texture;

        _verts[1].Position.X = v2.X;
        _verts[1].Position.Y = v1.Y;
        _verts[1].TextureCoordinate.X = u2.X;
        _verts[1].TextureCoordinate.Y = u1.Y;
        _verts[1].TextureCoordinate.Z = texture;

        _verts[2].Position.X = v1.X;
        _verts[2].Position.Y = v2.Y;
        _verts[2].TextureCoordinate.X = u1.X;
        _verts[2].TextureCoordinate.Y = u2.Y;
        _verts[2].TextureCoordinate.Z = texture;

        _verts[3].Position.X = v2.X;
        _verts[3].Position.Y = v2.Y;
        _verts[3].TextureCoordinate.X = u2.X;
        _verts[3].TextureCoordinate.Y = u2.Y;
        _verts[3].TextureCoordinate.Z = texture;


        device.DrawUserIndexedPrimitives(PrimitiveType.TriangleList, _verts, 0, 4, _ib, 0, 2);
    }

    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public struct VertexPositionTexture : IVertexType
    {
        public Vector3 Position;
        public Vector3 TextureCoordinate;

        public static readonly VertexDeclaration VertexDeclaration = new(
            new VertexElement(0, VertexElementFormat.Vector3, VertexElementUsage.Position, 0),
            new VertexElement(12, VertexElementFormat.Vector3, VertexElementUsage.TextureCoordinate, 0));

        public VertexPositionTexture(Vector3 position, Vector3 textureCoordinate)
        {
            Position = position;
            TextureCoordinate = textureCoordinate;
        }

        VertexDeclaration IVertexType.VertexDeclaration => VertexDeclaration;

    }
}