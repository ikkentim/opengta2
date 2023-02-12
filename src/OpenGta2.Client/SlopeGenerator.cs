using System;
using System.Transactions;
using Microsoft.Xna.Framework;
using OpenGta2.Client.Effects;
using OpenGta2.Data.Map;

namespace OpenGta2.Client;

public static class SlopeGenerator
{
    // rotation of cube
    private static readonly Matrix MatrixUp = Matrix.Identity;
    private static readonly Matrix MatrixRight = Matrix.CreateRotationZ(MathF.PI / 2) * Matrix.CreateTranslation(1, 0, 0);
    private static readonly Matrix MatrixDown = Matrix.CreateRotationZ(MathF.PI) * Matrix.CreateTranslation(1, 1, 0);
    private static readonly Matrix MatrixLeft = Matrix.CreateRotationZ(-MathF.PI / 2) * Matrix.CreateTranslation(0, 1, 0);
    
    // point on face to face
    private static readonly Matrix PofLeft = Matrix.CreateRotationX(-MathHelper.PiOver2) *
                                             Matrix.CreateRotationZ(MathHelper.PiOver2) *
                                             Matrix.CreateTranslation(new Vector3(0, 0, 1));
    
    private static readonly Matrix PofRight = Matrix.CreateRotationX(-MathHelper.PiOver2) *
                                              Matrix.CreateRotationZ(-MathHelper.PiOver2) *
                                              Matrix.CreateTranslation(new Vector3(1, 1, 1));

    private static readonly Matrix PofTop = Matrix.CreateRotationX(-MathHelper.PiOver2) *
                                            Matrix.CreateRotationZ(MathHelper.Pi) *
                                            Matrix.CreateTranslation(new Vector3(1, 0, 1));

    private static readonly Matrix PofBottom = Matrix.CreateRotationX(-MathHelper.PiOver2) *
                                               Matrix.CreateTranslation(new Vector3(0, 1, 1));

    private static readonly Matrix PofLid = Matrix.CreateTranslation(new Vector3(0, 0, 1));


    private static Matrix GetRotationMatrix(Rotation rotation)
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
                    case Face.Bottom: return ref block.Bottom;
                    default: return ref block.Lid;
                }
            case Rotation.Rotate90:
                switch (face)
                {
                    case Face.Left: return ref block.Top;
                    case Face.Right: return ref block.Bottom;
                    case Face.Top: return ref block.Right;
                    case Face.Bottom: return ref block.Left;
                    default: return ref block.Lid;
                }
            case Rotation.Rotate180:
                switch (face)
                {
                    case Face.Left: return ref block.Right;
                    case Face.Right: return ref block.Left;
                    case Face.Top: return ref block.Bottom;
                    case Face.Bottom: return ref block.Top;
                    default: return ref block.Lid;
                }
            case Rotation.Rotate270:
                switch (face)
                {
                    case Face.Left: return ref block.Bottom;
                    case Face.Right: return ref block.Top;
                    case Face.Top: return ref block.Left;
                    case Face.Bottom: return ref block.Right;
                    default: return ref block.Lid;
                }
            default: throw new InvalidOperationException();
        }
    }

    private static void RemapFaces(ref BlockInfo block, Rotation rotation, out FaceInfo top, out FaceInfo bottom, out FaceInfo left, out FaceInfo right)
    {
        top = GetFace(ref block, Face.Top, rotation);
        bottom = GetFace(ref block, Face.Bottom, rotation);
        left = GetFace(ref block, Face.Left, rotation);
        right = GetFace(ref block, Face.Right, rotation);
    }

    private static Rotation Subtract(Rotation lhs, Rotation rhs)
    {
        var tmpRot = (int)lhs;
        tmpRot -= (int)rhs;
        if (tmpRot < 0)
        {
            tmpRot += 4;
        }

        return (Rotation)tmpRot;
    }

    private static Vector3 MapUv(ref FaceInfo face, Rotation rotationBlock, float x, float y) => MapUv(face.TileGraphic, face.Rotation, rotationBlock, face.Flip, x, y);

    private static Vector3 MapUv(ushort tileGraphic, Rotation rotation, Rotation rotationBlock, bool flip, float x, float y)
    {
        if (flip)
        {
            if (rotationBlock is Rotation.Rotate90 or Rotation.Rotate270)
            {
                y = 1 - y;
            }
            else
            {
                x = 1 - x;
            }
        }

        rotation = Subtract(rotation, rotationBlock);
        
        float tmp;
        switch (rotation)
        {
            case Rotation.Rotate90:
                tmp = 1 - x;
                x = y;
                y = tmp;
                break;
            case Rotation.Rotate180:
                x = 1 - x;
                y = 1 - y;
                break;
            case Rotation.Rotate270:
                tmp = 1 - y;
                y = x;
                x = tmp;
                break;
        }
        
        return new Vector3(x, y, tileGraphic);
    }
    
    private static Matrix GetPointOnFaceMatrix(Face face)
    {
        return face switch
        {
            Face.Left => PofLeft,
            Face.Right => PofRight,
            Face.Top => PofTop,
            Face.Bottom => PofBottom,
            Face.Lid => PofLid,
            _ => throw new ArgumentOutOfRangeException(nameof(face), face, null)
        };
    }

    private record struct Buffers(BufferArray<VertexPositionTile> Vertices, BufferArray<short> Indices);

    private static VertexPositionTile GetFaceVertex(Vector3 point, Vector2 uv, ref FaceInfo face, Face direction, Matrix translation, Rotation uvRotation = Rotation.Rotate0, bool flat = false)
    {
        var matrix = GetPointOnFaceMatrix(direction);

        matrix *= translation;

        return new VertexPositionTile(Vector3.Transform(point, matrix), MapUv(ref face, uvRotation, uv.X, uv.Y), face.Flat || flat);
    }

    private static VertexPositionTile GetVertex(Vector3 point, Vector2 uv, ref FaceInfo face, Matrix translation, Rotation uvRotation = Rotation.Rotate0) =>
        new(Vector3.Transform(point, translation), MapUv(ref face, uvRotation, uv.X, uv.Y), face.Flat);

    private static void AddSimpleFace(ref FaceInfo face, Face normal, Buffers buffers, Matrix translation, bool oppositeFlat = false)
    {
        if (face.TileGraphic == 0)
            return;

        var start = buffers.Vertices.Length;

        // oppositeFlat = false;//test
        var z = oppositeFlat ? -0.999f : 0;

        if (face.Flat && oppositeFlat)
        {
            // render on both sides
            buffers.Vertices.Add(GetFaceVertex(new Vector3(0, 0, 0), new Vector2(0, 0), ref face, normal, translation));
            buffers.Vertices.Add(GetFaceVertex(new Vector3(1, 0, 0), new Vector2(1, 0), ref face, normal, translation));
            buffers.Vertices.Add(GetFaceVertex(new Vector3(0, 1, 0), new Vector2(0, 1), ref face, normal, translation));
            buffers.Vertices.Add(GetFaceVertex(new Vector3(1, 1, 0), new Vector2(1, 1), ref face, normal, translation));
            
            buffers.Indices.Add((short)(start + 0));
            buffers.Indices.Add((short)(start + 1));
            buffers.Indices.Add((short)(start + 2));
            buffers.Indices.Add((short)(start + 1));
            buffers.Indices.Add((short)(start + 3));
            buffers.Indices.Add((short)(start + 2));

            start = buffers.Vertices.Length;
        }

        buffers.Vertices.Add(GetFaceVertex(new Vector3(0, 0, z), new Vector2(0, 0), ref face, normal, translation, flat: oppositeFlat));
        buffers.Vertices.Add(GetFaceVertex(new Vector3(1, 0, z), new Vector2(1, 0), ref face, normal, translation, flat: oppositeFlat));
        buffers.Vertices.Add(GetFaceVertex(new Vector3(0, 1, z), new Vector2(0, 1), ref face, normal, translation, flat: oppositeFlat));
        buffers.Vertices.Add(GetFaceVertex(new Vector3(1, 1, z), new Vector2(1, 1), ref face, normal, translation, flat: oppositeFlat));

        buffers.Indices.Add((short)(start + 0));
        buffers.Indices.Add((short)(start + 1));
        buffers.Indices.Add((short)(start + 2));
        buffers.Indices.Add((short)(start + 1));
        buffers.Indices.Add((short)(start + 3));
        buffers.Indices.Add((short)(start + 2));
    }
    
    private static void SlopeNone(ref BlockInfo block, Matrix translationMatrix, Buffers buffers)
    {
        AddSimpleFace(ref block.Lid, Face.Lid, buffers, translationMatrix);
        AddSimpleFace(ref block.Left, Face.Left, buffers, translationMatrix, block.Right.Flat);
        AddSimpleFace(ref block.Right, Face.Right, buffers, translationMatrix, block.Left.Flat);
        AddSimpleFace(ref block.Top, Face.Top, buffers, translationMatrix, block.Bottom.Flat);
        AddSimpleFace(ref block.Bottom, Face.Bottom, buffers, translationMatrix, block.Top.Flat);
    }

    private static void SlopeDiagonal(ref BlockInfo block, Rotation blockRotation, Matrix translationMatrix, Buffers buffers)
    {
        // based on facing up-right
        var translation = GetRotationMatrix(blockRotation) * translationMatrix;
        RemapFaces(ref block, blockRotation, out var top, out var bottom, out var left, out var right);
        
        AddSimpleFace(ref left, Face.Left, buffers, translation);
        AddSimpleFace(ref bottom, Face.Bottom, buffers, translation);
        
        if (block.Lid.TileGraphic != 0)
        {
            var start = buffers.Vertices.Length;
            buffers.Vertices.Add(GetVertex(new Vector3(0, 0, 1), new Vector2(0, 0), ref block.Lid, translation, blockRotation));
            buffers.Vertices.Add(GetVertex(new Vector3(1, 1, 1), new Vector2(1, 1), ref block.Lid, translation, blockRotation));
            buffers.Vertices.Add(GetVertex(new Vector3(0, 1, 1), new Vector2(0, 1), ref block.Lid, translation, blockRotation));

            buffers.Indices.Add((short)(start + 0));
            buffers.Indices.Add((short)(start + 1));
            buffers.Indices.Add((short)(start + 2));
        }
        
        // diagonal is the right face. due to block rotation this might be the top face.
        ref var diag = ref right;
        if (blockRotation is Rotation.Rotate90 or Rotation.Rotate270)
        {
            diag = ref top;
        }
        
        if (diag.TileGraphic != 0)
        {
            var start = buffers.Vertices.Length;
            
            buffers.Vertices.Add(GetVertex(new Vector3(1, 1, 1), new Vector2(0, 0), ref diag, translation));
            buffers.Vertices.Add(GetVertex(new Vector3(0, 0, 1), new Vector2(1, 0), ref diag, translation));
            buffers.Vertices.Add(GetVertex(new Vector3(1, 1, 0), new Vector2(0, 1), ref diag, translation));
            buffers.Vertices.Add(GetVertex(new Vector3(0, 0, 0), new Vector2(1, 1), ref diag, translation));
            
            buffers.Indices.Add((short)(start + 0));
            buffers.Indices.Add((short)(start + 1));
            buffers.Indices.Add((short)(start + 2));
            buffers.Indices.Add((short)(start + 1));
            buffers.Indices.Add((short)(start + 3));
            buffers.Indices.Add((short)(start + 2));
        }
        
    }

    private static void SlopeN(ref BlockInfo block, Rotation blockRotation, Matrix translationMatrix, Buffers buffers, float slopeFrom, float slopeTo)
    {
        var translation = GetRotationMatrix(blockRotation) * translationMatrix;

        // based on slope up

        RemapFaces(ref block, blockRotation, out var top, out var bottom, out var left, out var right);

        if (block.Lid.TileGraphic != 0)
        {
            var start = buffers.Vertices.Length;

            buffers.Vertices.Add(GetVertex(new Vector3(0, 0, slopeTo), new Vector2(0, 0), ref block.Lid, translation, blockRotation));
            buffers.Vertices.Add(GetVertex(new Vector3(1, 0, slopeTo), new Vector2(1, 0), ref block.Lid, translation, blockRotation));
            buffers.Vertices.Add(GetVertex(new Vector3(0, 1, slopeFrom), new Vector2(0, 1), ref block.Lid, translation, blockRotation));
            buffers.Vertices.Add(GetVertex(new Vector3(1, 1, slopeFrom), new Vector2(1, 1), ref block.Lid, translation, blockRotation));

            buffers.Indices.Add((short)(start + 0));
            buffers.Indices.Add((short)(start + 1));
            buffers.Indices.Add((short)(start + 2));
            buffers.Indices.Add((short)(start + 1));
            buffers.Indices.Add((short)(start + 3));
            buffers.Indices.Add((short)(start + 2));
        }


        if (left.TileGraphic != 0)
        {
            var start = buffers.Vertices.Length;

            buffers.Vertices.Add(GetVertex(new Vector3(0, 0, slopeTo), new Vector2(0, 1 - slopeTo), ref left, translation));
            buffers.Vertices.Add(GetVertex(new Vector3(0, 1, slopeFrom), new Vector2(1, 1 - slopeFrom), ref left, translation));
            buffers.Vertices.Add(GetVertex(new Vector3(0, 0, 0), new Vector2(0, 1), ref left, translation));

            buffers.Indices.Add((short)(start + 0));
            buffers.Indices.Add((short)(start + 1));
            buffers.Indices.Add((short)(start + 2));

            if (slopeFrom != 0)
            {
                buffers.Vertices.Add(GetVertex(new Vector3(0, 1, 0), new Vector2(1, 1), ref left, translation));
                buffers.Indices.Add((short)(start + 1));
                buffers.Indices.Add((short)(start + 3));
                buffers.Indices.Add((short)(start + 2));
            }
        }

        if (right.TileGraphic != 0)
        {
            var start = buffers.Vertices.Length;

            if (slopeFrom != 0)
            {
                buffers.Vertices.Add(GetVertex(new Vector3(1, 1, slopeFrom), new Vector2(0, 1 - slopeFrom), ref right, translation));
            }

            buffers.Vertices.Add(GetVertex(new Vector3(1, 0, slopeTo), new Vector2(1, 1 - slopeTo), ref right, translation));
            buffers.Vertices.Add(GetVertex(new Vector3(1, 1, 0), new Vector2(0, 1), ref right, translation));
            buffers.Vertices.Add(GetVertex(new Vector3(1, 0, 0), new Vector2(1, 1), ref right, translation));


            if (slopeFrom != 0)
            {
                buffers.Indices.Add((short)(start + 0));
                buffers.Indices.Add((short)(start + 1));
                buffers.Indices.Add((short)(start + 2));
                buffers.Indices.Add((short)(start + 1));
                buffers.Indices.Add((short)(start + 3));
                buffers.Indices.Add((short)(start + 2));
            }
            else
            {

                buffers.Indices.Add((short)(start + 0));
                buffers.Indices.Add((short)(start + 2));
                buffers.Indices.Add((short)(start + 1));
            }
        }

        if (top.TileGraphic != 0)
        {
            var start = buffers.Vertices.Length;

            buffers.Vertices.Add(GetVertex(new Vector3(1, 0, slopeTo), new Vector2(0, 1 - slopeTo), ref top, translation));
            buffers.Vertices.Add(GetVertex(new Vector3(0, 0, slopeTo), new Vector2(1, 1 - slopeTo), ref top, translation));
            buffers.Vertices.Add(GetVertex(new Vector3(1, 0, 0), new Vector2(0, 1), ref top, translation));
            buffers.Vertices.Add(GetVertex(new Vector3(0, 0, 0), new Vector2(1, 1), ref top, translation));

            buffers.Indices.Add((short)(start + 0));
            buffers.Indices.Add((short)(start + 1));
            buffers.Indices.Add((short)(start + 2));
            buffers.Indices.Add((short)(start + 1));
            buffers.Indices.Add((short)(start + 3));
            buffers.Indices.Add((short)(start + 2));
        }

        if (bottom.TileGraphic != 0)
        {
            var start = buffers.Vertices.Length;

            buffers.Vertices.Add(GetVertex(new Vector3(0, 1, slopeFrom), new Vector2(0, 1 - slopeFrom), ref bottom, translation));
            buffers.Vertices.Add(GetVertex(new Vector3(1, 1, slopeFrom), new Vector2(1, 1 - slopeFrom), ref bottom, translation));
            buffers.Vertices.Add(GetVertex(new Vector3(0, 1, 0), new Vector2(0, 1), ref bottom, translation));
            buffers.Vertices.Add(GetVertex(new Vector3(1, 1, 0), new Vector2(1, 1), ref bottom, translation));

            buffers.Indices.Add((short)(start + 0));
            buffers.Indices.Add((short)(start + 1));
            buffers.Indices.Add((short)(start + 2));
            buffers.Indices.Add((short)(start + 1));
            buffers.Indices.Add((short)(start + 3));
            buffers.Indices.Add((short)(start + 2));
        }
    }

    public static void Push(ref BlockInfo block, Vector3 offset, BufferArray<VertexPositionTile> vertices, BufferArray<short> indices)
    {
        var translation = Matrix.CreateTranslation(offset);
        var buffers = new Buffers(vertices, indices);

        switch (block.SlopeType.SlopeType)
        {
            case SlopeType.Up45:
                SlopeN(ref block, Rotation.Rotate0, translation, buffers, 0, 1);
                break;
            case SlopeType.Right45:
                SlopeN(ref block, Rotation.Rotate90, translation, buffers, 0, 1);
                break;
            case SlopeType.Down45:
                SlopeN(ref block, Rotation.Rotate180, translation, buffers, 0, 1);
                break;
            case SlopeType.Left45:
                SlopeN(ref block, Rotation.Rotate270, translation, buffers, 0, 1);
                break;
            case SlopeType.DiagonalFacingUpRight:
                SlopeDiagonal(ref block, Rotation.Rotate0, translation, buffers);
                break;
            case SlopeType.DiagonalFacingDownRight:
                SlopeDiagonal(ref block, Rotation.Rotate90, translation, buffers);
                break;
            case SlopeType.DiagonalFacingDownLeft:
                SlopeDiagonal(ref block, Rotation.Rotate180, translation, buffers);
                break;
            case SlopeType.DiagonalFacingUpLeft:
                SlopeDiagonal(ref block, Rotation.Rotate270, translation, buffers);
                break;
            case SlopeType.Up26_1:
                SlopeN(ref block, Rotation.Rotate0, translation, buffers, 0F, 0.5f * 1);
                break;
            case SlopeType.Up26_2:
                SlopeN(ref block, Rotation.Rotate0, translation, buffers, 0.5F, 0.5f * 2);
                break;
            case SlopeType.Down26_1:
                SlopeN(ref block, Rotation.Rotate180, translation, buffers, 0F, 0.5f * 1);
                break;
            case SlopeType.Down26_2:
                SlopeN(ref block, Rotation.Rotate180, translation, buffers, 0.5F, 0.5f * 2);
                break;
            case SlopeType.Left26_1:
                SlopeN(ref block, Rotation.Rotate270, translation, buffers, 0F, 0.5f * 1);
                break;
            case SlopeType.Left26_2:
                SlopeN(ref block, Rotation.Rotate270, translation, buffers, 0.5F, 0.5f * 2);
                break;
            case SlopeType.Right26_1:
                SlopeN(ref block, Rotation.Rotate90, translation, buffers, 0F, 0.5f * 1);
                break;
            case SlopeType.Right26_2:
                SlopeN(ref block, Rotation.Rotate90, translation, buffers, 0.5F, 0.5f * 2);
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
                SlopeN(ref block, Rotation.Rotate0, translation, buffers, 0.125f * (num - 1), 0.125f * num);
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
                SlopeN(ref block, Rotation.Rotate180, translation, buffers, 0.125f * (num - 1), 0.125f * num);
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
                SlopeN(ref block, Rotation.Rotate270, translation, buffers, 0.125f * (num - 1), 0.125f * num);
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
                SlopeN(ref block, Rotation.Rotate90, translation, buffers, 0.125f * (num - 1), 0.125f * num);
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
                // TODO partial blocks
                // TODO: diagonal slopes
                SlopeNone(ref block, translation, buffers);
                break;
            case SlopeType.Reserved:
                break;
            default:
            case SlopeType.SlopeAbove:
            case SlopeType.None:
                SlopeNone(ref block, translation, buffers);
                break;
        }
    }
}