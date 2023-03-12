using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace OpenGta2.Client.Rendering;

public static class QuadRenderer
{
    private static readonly VertexPositionSprite[] _verts ={
        new(
            // top-left
            new Vector3(0,0,0),
            new Vector2(0,0)),
        new(
            // top-right
            new Vector3(0,0,0),
            new Vector2(1,0)),
        new(
            // bottom-left
            new Vector3(0,0,0),
            new Vector2(0,1)),
        new(
            // bottom-right
            new Vector3(0,0,0),
            new Vector2(1,1))
    };

    private static readonly short[] _ib = { 0, 1, 2, 2, 1, 3 };
    
    public static void Render(GraphicsDevice device, Vector2 v1, Vector2 v2)
    {
        _verts[0].Position.X = v1.X;
        _verts[0].Position.Y = v1.Y;

        _verts[1].Position.X = v2.X;
        _verts[1].Position.Y = v1.Y;

        _verts[2].Position.X = v1.X;
        _verts[2].Position.Y = v2.Y;

        _verts[3].Position.X = v2.X;
        _verts[3].Position.Y = v2.Y;


        device.DrawUserIndexedPrimitives(PrimitiveType.TriangleList, _verts, 0, 4, _ib, 0, 2);
    }
}