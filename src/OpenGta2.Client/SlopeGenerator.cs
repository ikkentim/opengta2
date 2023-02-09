using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using OpenGta2.Data.Map;

namespace OpenGta2.Client;

public class SlopeGenerator
{
    private static readonly Matrix MatrixUp = Matrix.Identity;
    private static readonly Matrix MatrixRight = Matrix.CreateRotationZ(MathF.PI / 2) * Matrix.CreateTranslation(1, 0, 0);
    private static readonly Matrix MatrixDown = Matrix.CreateRotationZ(MathF.PI) * Matrix.CreateTranslation(1, 1, 0);
    private static readonly Matrix MatrixLeft = Matrix.CreateRotationZ(-MathF.PI / 2) * Matrix.CreateTranslation(0, 1, 0);

    private static Color TestColor(Color color, int heightForTest)
    {
        var colorScale = ((heightForTest + 2) / 7.0f);
        return new Color((byte)MathHelper.Clamp(color.R * colorScale, 0, 255),
            (byte)MathHelper.Clamp(color.G * colorScale, 0, 255), (byte)MathHelper.Clamp(color.B * colorScale, 0, 255),
            color.A);
    }

    private static Matrix GetTranslation(Rotation rotation)
    {
        return rotation switch
        {
            Rotation.Rotate90 => MatrixRight,
            Rotation.Rotate180 => MatrixDown,
            Rotation.Rotate270 => MatrixLeft,
            Rotation.Rotate0 => MatrixUp,
            _ => MatrixUp
        };
    }

    private static ref FaceInfo GetFace(ref BlockInfo block, Face face, Rotation rotation)
    {
        switch (rotation)
        {
            case Rotation.Rotate0:
                switch (face)
                {
                    case Face.Left: return ref block.Left;
                    case Face.Right: return ref block.Right;
                    case Face.Top: return ref block.Top;
                    default: return ref block.Bottom;
                }
            case Rotation.Rotate90:
                switch (face)
                {
                    case Face.Left: return ref block.Top;
                    case Face.Right: return ref block.Bottom;
                    case Face.Top: return ref block.Right;
                    default: return ref block.Left;
                }
            case Rotation.Rotate180:
                switch (face)
                {
                    case Face.Left: return ref block.Right;
                    case Face.Right: return ref block.Left;
                    case Face.Top: return ref block.Bottom;
                    default: return ref block.Top;
                }
            case Rotation.Rotate270:
                switch (face)
                {
                    case Face.Left: return ref block.Bottom;
                    case Face.Right: return ref block.Top;
                    case Face.Top: return ref block.Left;
                    default: return ref block.Right;
                }
            default: throw new InvalidOperationException();
        }
    }

    private static void SlopeNone(ref BlockInfo block, Rotation rotation, List<VertexPositionColor> vertices,
        List<short> indices, int heightForTest)
    {
        var translation = GetTranslation(rotation);

        if (block.Lid.TileGraphic != 0)
        {
            var color = TestColor(Color.Red, heightForTest);
            var start = vertices.Count;
            vertices.Add(new VertexPositionColor(Vector3.Transform(new Vector3(0, 0, 1), translation), color));
            vertices.Add(new VertexPositionColor(Vector3.Transform(new Vector3(1, 0, 1), translation), color));
            vertices.Add(new VertexPositionColor(Vector3.Transform(new Vector3(0, 1, 1), translation), color));
            vertices.Add(new VertexPositionColor(Vector3.Transform(new Vector3(1, 1, 1), translation), color));

            indices.Add((short)(start + 0));
            indices.Add((short)(start + 1));
            indices.Add((short)(start + 2));
            indices.Add((short)(start + 1));
            indices.Add((short)(start + 3));
            indices.Add((short)(start + 2));
        }

        if (GetFace(ref block, Face.Left, rotation)
                .TileGraphic != 0)
        {
            var color = TestColor(Color.Magenta, heightForTest);
            var start = vertices.Count;

            vertices.Add(new VertexPositionColor(Vector3.Transform(new Vector3(0, 0, 1), translation), color));
            vertices.Add(new VertexPositionColor(Vector3.Transform(new Vector3(0, 1, 1), translation), color));
            vertices.Add(new VertexPositionColor(Vector3.Transform(new Vector3(0, 0, 0), translation), color));
            vertices.Add(new VertexPositionColor(Vector3.Transform(new Vector3(0, 1, 0), translation), color));

            indices.Add((short)(start + 0));
            indices.Add((short)(start + 1));
            indices.Add((short)(start + 2));
            indices.Add((short)(start + 1));
            indices.Add((short)(start + 3));
            indices.Add((short)(start + 2));
        }

        if (GetFace(ref block, Face.Right, rotation)
                .TileGraphic != 0)
        {
            var color = TestColor(Color.Blue, heightForTest);
            var start = vertices.Count;

            vertices.Add(new VertexPositionColor(Vector3.Transform(new Vector3(1, 1, 1), translation), color));
            vertices.Add(new VertexPositionColor(Vector3.Transform(new Vector3(1, 0, 1), translation), color));
            vertices.Add(new VertexPositionColor(Vector3.Transform(new Vector3(1, 1, 0), translation), color));
            vertices.Add(new VertexPositionColor(Vector3.Transform(new Vector3(1, 0, 0), translation), color));

            indices.Add((short)(start + 0));
            indices.Add((short)(start + 1));
            indices.Add((short)(start + 2));
            indices.Add((short)(start + 1));
            indices.Add((short)(start + 3));
            indices.Add((short)(start + 2));
        }

        if (GetFace(ref block, Face.Top, rotation)
                .TileGraphic != 0)
        {
            var color = TestColor(Color.Green, heightForTest);
            var start = vertices.Count;

            vertices.Add(new VertexPositionColor(Vector3.Transform(new Vector3(1, 0, 1), translation), color));
            vertices.Add(new VertexPositionColor(Vector3.Transform(new Vector3(0, 0, 1), translation), color));
            vertices.Add(new VertexPositionColor(Vector3.Transform(new Vector3(1, 0, 0), translation), color));
            vertices.Add(new VertexPositionColor(Vector3.Transform(new Vector3(0, 0, 0), translation), color));

            indices.Add((short)(start + 0));
            indices.Add((short)(start + 1));
            indices.Add((short)(start + 2));
            indices.Add((short)(start + 1));
            indices.Add((short)(start + 3));
            indices.Add((short)(start + 2));
        }

        if (GetFace(ref block, Face.Bottom, rotation)
                .TileGraphic != 0)
        {
            var color = TestColor(Color.Yellow, heightForTest);
            var start = vertices.Count;

            vertices.Add(new VertexPositionColor(Vector3.Transform(new Vector3(0, 1, 1), translation), color));
            vertices.Add(new VertexPositionColor(Vector3.Transform(new Vector3(1, 1, 1), translation), color));
            vertices.Add(new VertexPositionColor(Vector3.Transform(new Vector3(0, 1, 0), translation), color));
            vertices.Add(new VertexPositionColor(Vector3.Transform(new Vector3(1, 1, 0), translation), color));

            indices.Add((short)(start + 0));
            indices.Add((short)(start + 1));
            indices.Add((short)(start + 2));
            indices.Add((short)(start + 1));
            indices.Add((short)(start + 3));
            indices.Add((short)(start + 2));
        }
    }

    private static void SlopeDiagonal(ref BlockInfo block, Rotation rotation, List<VertexPositionColor> vertices,
        List<short> indices, int heightForTest)
    {
        var translation = GetTranslation(rotation);

        // based on facing top-right

        if (block.Lid.TileGraphic != 0)
        {
            var color = TestColor(Color.Purple, heightForTest);
            var start = vertices.Count;
            vertices.Add(new VertexPositionColor(Vector3.Transform(new Vector3(0, 0, 1), translation), color));
            vertices.Add(new VertexPositionColor(Vector3.Transform(new Vector3(1, 1, 1), translation), color));
            vertices.Add(new VertexPositionColor(Vector3.Transform(new Vector3(0, 1, 1), translation), color));

            indices.Add((short)(start + 0));
            indices.Add((short)(start + 1));
            indices.Add((short)(start + 2));
        }

        if (GetFace(ref block, Face.Left, rotation)
                .TileGraphic != 0)
        {
            var color = TestColor(Color.Magenta, heightForTest);
            var start = vertices.Count;

            vertices.Add(new VertexPositionColor(Vector3.Transform(new Vector3(0, 0, 1), translation), color));
            vertices.Add(new VertexPositionColor(Vector3.Transform(new Vector3(0, 1, 1), translation), color));
            vertices.Add(new VertexPositionColor(Vector3.Transform(new Vector3(0, 0, 0), translation), color));
            vertices.Add(new VertexPositionColor(Vector3.Transform(new Vector3(0, 1, 0), translation), color));

            indices.Add((short)(start + 0));
            indices.Add((short)(start + 1));
            indices.Add((short)(start + 2));
            indices.Add((short)(start + 1));
            indices.Add((short)(start + 3));
            indices.Add((short)(start + 2));
        }

        if (GetFace(ref block, Face.Right, rotation)
                .TileGraphic != 0 || GetFace(ref block, Face.Top, rotation)
                .TileGraphic != 0)
        {
            var color = TestColor(Color.Blue, heightForTest);
            var start = vertices.Count;

            vertices.Add(new VertexPositionColor(Vector3.Transform(new Vector3(1, 1, 1), translation), color));
            vertices.Add(new VertexPositionColor(Vector3.Transform(new Vector3(0, 0, 1), translation), color));
            vertices.Add(new VertexPositionColor(Vector3.Transform(new Vector3(1, 1, 0), translation), color));
            vertices.Add(new VertexPositionColor(Vector3.Transform(new Vector3(0, 0, 0), translation), color));

            indices.Add((short)(start + 0));
            indices.Add((short)(start + 1));
            indices.Add((short)(start + 2));
            indices.Add((short)(start + 1));
            indices.Add((short)(start + 3));
            indices.Add((short)(start + 2));
        }

        if (GetFace(ref block, Face.Bottom, rotation)
                .TileGraphic != 0)
        {
            var color = TestColor(Color.Yellow, heightForTest);
            var start = vertices.Count;

            vertices.Add(new VertexPositionColor(Vector3.Transform(new Vector3(0, 1, 1), translation), color));
            vertices.Add(new VertexPositionColor(Vector3.Transform(new Vector3(1, 1, 1), translation), color));
            vertices.Add(new VertexPositionColor(Vector3.Transform(new Vector3(0, 1, 0), translation), color));
            vertices.Add(new VertexPositionColor(Vector3.Transform(new Vector3(1, 1, 0), translation), color));

            indices.Add((short)(start + 0));
            indices.Add((short)(start + 1));
            indices.Add((short)(start + 2));
            indices.Add((short)(start + 1));
            indices.Add((short)(start + 3));
            indices.Add((short)(start + 2));
        }
    }

    private static void SlopeN(ref BlockInfo block, Rotation rotation, List<VertexPositionColor> vertices, List<short> indices,
        float slopeFrom, float slopeTo, int heightForTest, Color colTest = default)
    {
        var translation = GetTranslation(rotation);

        if (colTest == default) colTest = Color.Red;
        // based on slope up

        if (block.Lid.TileGraphic != 0)
        {
            var color = TestColor(colTest, heightForTest); //Red
            var start = vertices.Count;
            vertices.Add(new VertexPositionColor(Vector3.Transform(new Vector3(0, 0, slopeTo), translation), color));
            vertices.Add(new VertexPositionColor(Vector3.Transform(new Vector3(1, 0, slopeTo), translation), color));
            vertices.Add(new VertexPositionColor(Vector3.Transform(new Vector3(0, 1, slopeFrom), translation), color));
            vertices.Add(new VertexPositionColor(Vector3.Transform(new Vector3(1, 1, slopeFrom), translation), color));

            indices.Add((short)(start + 0));
            indices.Add((short)(start + 1));
            indices.Add((short)(start + 2));
            indices.Add((short)(start + 1));
            indices.Add((short)(start + 3));
            indices.Add((short)(start + 2));
        }


        if (GetFace(ref block, Face.Left, rotation)
                .TileGraphic != 0)
        {
            var color = TestColor(Color.Magenta, heightForTest);
            var start = vertices.Count;

            vertices.Add(new VertexPositionColor(Vector3.Transform(new Vector3(0, 0, slopeTo), translation), color));
            vertices.Add(new VertexPositionColor(Vector3.Transform(new Vector3(0, 1, slopeFrom), translation), color));
            vertices.Add(new VertexPositionColor(Vector3.Transform(new Vector3(0, 0, 0), translation), color));

            indices.Add((short)(start + 0));
            indices.Add((short)(start + 1));
            indices.Add((short)(start + 2));

            if (slopeFrom != 0)
            {
                vertices.Add(new VertexPositionColor(Vector3.Transform(new Vector3(0, 1, 0), translation), color));
                indices.Add((short)(start + 1));
                indices.Add((short)(start + 3));
                indices.Add((short)(start + 2));
            }
        }

        if (GetFace(ref block, Face.Right, rotation)
                .TileGraphic != 0)
        {
            var color = TestColor(Color.Blue, heightForTest);
            var start = vertices.Count;

            if (slopeFrom != 0)
            {
                vertices.Add(new VertexPositionColor(Vector3.Transform(new Vector3(1, 1, slopeFrom), translation),
                    color));
            }

            vertices.Add(new VertexPositionColor(Vector3.Transform(new Vector3(1, 0, slopeTo), translation), color));
            vertices.Add(new VertexPositionColor(Vector3.Transform(new Vector3(1, 1, 0), translation), color));
            vertices.Add(new VertexPositionColor(Vector3.Transform(new Vector3(1, 0, 0), translation), color));

            indices.Add((short)(start + 0));
            indices.Add((short)(start + 1));
            indices.Add((short)(start + 2));

            if (slopeFrom != 0)
            {
                indices.Add((short)(start + 1));
                indices.Add((short)(start + 3));
                indices.Add((short)(start + 2));
            }
        }

        if (GetFace(ref block, Face.Top, rotation)
                .TileGraphic != 0)
        {
            var color = TestColor(Color.Green, heightForTest);
            var start = vertices.Count;

            vertices.Add(new VertexPositionColor(Vector3.Transform(new Vector3(1, 0, slopeTo), translation), color));
            vertices.Add(new VertexPositionColor(Vector3.Transform(new Vector3(0, 0, slopeTo), translation), color));
            vertices.Add(new VertexPositionColor(Vector3.Transform(new Vector3(1, 0, 0), translation), color));
            vertices.Add(new VertexPositionColor(Vector3.Transform(new Vector3(0, 0, 0), translation), color));

            indices.Add((short)(start + 0));
            indices.Add((short)(start + 1));
            indices.Add((short)(start + 2));
            indices.Add((short)(start + 1));
            indices.Add((short)(start + 3));
            indices.Add((short)(start + 2));
        }

        if (slopeFrom != 0 && GetFace(ref block, Face.Bottom, rotation)
                .TileGraphic != 0)
        {
            var color = TestColor(Color.Yellow, heightForTest);
            var start = vertices.Count;

            vertices.Add(new VertexPositionColor(Vector3.Transform(new Vector3(0, 1, slopeFrom), translation), color));
            vertices.Add(new VertexPositionColor(Vector3.Transform(new Vector3(1, 1, slopeFrom), translation), color));
            vertices.Add(new VertexPositionColor(Vector3.Transform(new Vector3(0, 1, 0), translation), color));
            vertices.Add(new VertexPositionColor(Vector3.Transform(new Vector3(1, 1, 0), translation), color));

            indices.Add((short)(start + 0));
            indices.Add((short)(start + 1));
            indices.Add((short)(start + 2));
            indices.Add((short)(start + 1));
            indices.Add((short)(start + 3));
            indices.Add((short)(start + 2));
        }
    }

    public void Push(ref BlockInfo block, int z, List<VertexPositionColor> vertices, List<short> indices)
    {
        switch (block.SlopeType.SlopeType)
        {
            case SlopeType.Up45:
                SlopeN(ref block, Rotation.Rotate0, vertices, indices, 0, 1, z);
                break;
            case SlopeType.Right45:
                SlopeN(ref block, Rotation.Rotate90, vertices, indices, 0, 1, z);
                break;
            case SlopeType.Down45:
                SlopeN(ref block, Rotation.Rotate180, vertices, indices, 0, 1, z);
                break;
            case SlopeType.Left45:
                SlopeN(ref block, Rotation.Rotate270, vertices, indices, 0, 1, z);
                break;
            case SlopeType.DiagonalFacingUpRight:
                SlopeDiagonal(ref block, Rotation.Rotate0, vertices, indices, z);
                break;
            case SlopeType.DiagonalFacingDownRight:
                SlopeDiagonal(ref block, Rotation.Rotate90, vertices, indices, z);
                break;
            case SlopeType.DiagonalFacingDownLeft:
                SlopeDiagonal(ref block, Rotation.Rotate180, vertices, indices, z);
                break;
            case SlopeType.DiagonalFacingUpLeft:
                SlopeDiagonal(ref block, Rotation.Rotate270, vertices, indices, z);
                break;
            case SlopeType.Up26_1:
                SlopeN(ref block, Rotation.Rotate0, vertices, indices, 0.5f * (1 - 1), 0.5f * 1, z);
                break;
            case SlopeType.Up26_2:
                SlopeN(ref block, Rotation.Rotate0, vertices, indices, 0.5f * (2 - 1), 0.5f * 2, z);
                break;
            case SlopeType.Down26_1:
                SlopeN(ref block, Rotation.Rotate180, vertices, indices, 0.5f * (1 - 1), 0.5f * 1, z);
                break;
            case SlopeType.Down26_2:
                SlopeN(ref block, Rotation.Rotate180, vertices, indices, 0.5f * (2 - 1), 0.5f * 2, z);
                break;
            case SlopeType.Left26_1:
                SlopeN(ref block, Rotation.Rotate270, vertices, indices, 0.5f * (1 - 1), 0.5f * 1, z);
                break;
            case SlopeType.Left26_2:
                SlopeN(ref block, Rotation.Rotate270, vertices, indices, 0.5f * (2 - 1), 0.5f * 2, z);
                break;
            case SlopeType.Right26_1:
                SlopeN(ref block, Rotation.Rotate90, vertices, indices, 0.5f * (1 - 1), 0.5f * 1, z);
                break;
            case SlopeType.Right26_2:
                SlopeN(ref block, Rotation.Rotate90, vertices, indices, 0.5f * (2 - 1), 0.5f * 2, z);
                break;
            case SlopeType.Up7_1:
            case SlopeType.Up7_2:
            case SlopeType.Up7_3:
            case SlopeType.Up7_4:
            case SlopeType.Up7_5:
            case SlopeType.Up7_6:
            case SlopeType.Up7_7:
            case SlopeType.Up7_8:
            {
                var num = ((byte)block.SlopeType.SlopeType - (byte)SlopeType.Up7_1) + 1;
                SlopeN(ref block, Rotation.Rotate0, vertices, indices, 0.125f * (num - 1), 0.125f * num, z,
                    Color.Yellow);
                break;
            }
            case SlopeType.Down7_1:
            case SlopeType.Down7_2:
            case SlopeType.Down7_3:
            case SlopeType.Down7_4:
            case SlopeType.Down7_5:
            case SlopeType.Down7_6:
            case SlopeType.Down7_7:
            case SlopeType.Down7_8:
            {
                var num = ((byte)block.SlopeType.SlopeType - (byte)SlopeType.Down7_1) + 1;
                SlopeN(ref block, Rotation.Rotate180, vertices, indices, 0.125f * (num - 1), 0.125f * num, z,
                    Color.Yellow);
                break;
            }
            case SlopeType.Left7_1:
            case SlopeType.Left7_2:
            case SlopeType.Left7_3:
            case SlopeType.Left7_4:
            case SlopeType.Left7_5:
            case SlopeType.Left7_6:
            case SlopeType.Left7_7:
            case SlopeType.Left7_8:
            {
                var num = ((byte)block.SlopeType.SlopeType - (byte)SlopeType.Left7_1) + 1;
                SlopeN(ref block, Rotation.Rotate270, vertices, indices, 0.125f * (num - 1), 0.125f * num, z,
                    Color.Yellow);
                break;
            }
            case SlopeType.Right7_1:
            case SlopeType.Right7_2:
            case SlopeType.Right7_3:
            case SlopeType.Right7_4:
            case SlopeType.Right7_5:
            case SlopeType.Right7_6:
            case SlopeType.Right7_7:
            case SlopeType.Right7_8:
            {
                var num = ((byte)block.SlopeType.SlopeType - (byte)SlopeType.Right7_1) + 1;
                SlopeN(ref block, Rotation.Rotate90, vertices, indices, 0.125f * (num - 1), 0.125f * num, z,
                    Color.Yellow);
                break;
            }
            case SlopeType.DiagonalSlopeFacingUpLeft:
            case SlopeType.DiagonalSlopeFacingUpRight:
            case SlopeType.DiagonalSlopeFacingDownLeft:
            case SlopeType.DiagonalSlopeFacingDownRight:
            case SlopeType.PartialBlockLeft:
            case SlopeType.PartialBlockRight:
            case SlopeType.PartialBlockTop:
            case SlopeType.PartialBlockBottom:
            case SlopeType.PartialBlockTopLeftCorner:
            case SlopeType.PartialBlockTopRightCorner:
            case SlopeType.PartialBlockBottomLeftCorner:
            case SlopeType.PartialBlockBottomRightCorner:
            case SlopeType.PartialBlockCentre:

            case SlopeType.Reserved:
            case SlopeType.SlopeAbove:
            default:
            case SlopeType.None:
                SlopeNone(ref block, Rotation.Rotate0, vertices, indices, z);
                break;
        }
    }
}