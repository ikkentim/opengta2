﻿using System;
using Microsoft.Xna.Framework;
using OpenGta2.Client.Effects;
using OpenGta2.Data.Map;

namespace OpenGta2.Client;

public static class SlopeGenerator
{
    private static readonly Matrix MatrixUp = Matrix.Identity;
    private static readonly Matrix MatrixRight = Matrix.CreateRotationZ(MathF.PI / 2) * Matrix.CreateTranslation(1, 0, 0);
    private static readonly Matrix MatrixDown = Matrix.CreateRotationZ(MathF.PI) * Matrix.CreateTranslation(1, 1, 0);
    private static readonly Matrix MatrixLeft = Matrix.CreateRotationZ(-MathF.PI / 2) * Matrix.CreateTranslation(0, 1, 0);
    
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

        var tmpRot = (int)rotation;
        tmpRot -= (int)rotationBlock;
        if (tmpRot < 0)
        {
            tmpRot += 4;
        }

        rotation = (Rotation)tmpRot;

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
    
    private static void RemapFaces(ref BlockInfo block, Rotation rotation, out FaceInfo top, out FaceInfo bottom, out FaceInfo left, out FaceInfo right)
    {
        top = GetFace(ref block, Face.Top, rotation);
        bottom = GetFace(ref block, Face.Bottom, rotation);
        left = GetFace(ref block, Face.Left, rotation);
        right = GetFace(ref block, Face.Right, rotation);
    }

    private static void SlopeNone(ref BlockInfo block, Rotation rotation, Matrix translationMatrix, BufferArray<VertexPositionTile> vertices,
        BufferArray<short> indices)
    {
        var translation = GetTranslation(rotation) * translationMatrix;

        RemapFaces(ref block, rotation, out var top, out var bottom, out var left, out var right);

        if (block.Lid.TileGraphic != 0)
        {
            var start = vertices.Length;
            vertices.Add(new VertexPositionTile(Vector3.Transform(new Vector3(0, 0, 1), translation), MapUv(block.Lid.TileGraphic, block.Lid.Rotation, rotation, block.Lid.Flip, 0, 0), block.Lid.Flat));
            vertices.Add(new VertexPositionTile(Vector3.Transform(new Vector3(1, 0, 1), translation), MapUv(block.Lid.TileGraphic, block.Lid.Rotation, rotation, block.Lid.Flip, 1, 0), block.Lid.Flat));
            vertices.Add(new VertexPositionTile(Vector3.Transform(new Vector3(0, 1, 1), translation), MapUv(block.Lid.TileGraphic, block.Lid.Rotation, rotation, block.Lid.Flip, 0, 1), block.Lid.Flat));
            vertices.Add(new VertexPositionTile(Vector3.Transform(new Vector3(1, 1, 1), translation), MapUv(block.Lid.TileGraphic, block.Lid.Rotation, rotation, block.Lid.Flip, 1, 1), block.Lid.Flat));

            indices.Add((short)(start + 0));
            indices.Add((short)(start + 1));
            indices.Add((short)(start + 2));
            indices.Add((short)(start + 1));
            indices.Add((short)(start + 3));
            indices.Add((short)(start + 2));
        }

        if (left.TileGraphic != 0)
        {
            var start = vertices.Length;

            vertices.Add(new VertexPositionTile(Vector3.Transform(new Vector3(0, 0, 1), translation), MapUv(left.TileGraphic, left.Rotation, Rotation.Rotate0, left.Flip, 0, 0), left.Flat));
            vertices.Add(new VertexPositionTile(Vector3.Transform(new Vector3(0, 1, 1), translation), MapUv(left.TileGraphic, left.Rotation, Rotation.Rotate0, left.Flip, 1, 0), left.Flat));
            vertices.Add(new VertexPositionTile(Vector3.Transform(new Vector3(0, 0, 0), translation), MapUv(left.TileGraphic, left.Rotation, Rotation.Rotate0, left.Flip, 0, 1), left.Flat));
            vertices.Add(new VertexPositionTile(Vector3.Transform(new Vector3(0, 1, 0), translation), MapUv(left.TileGraphic, left.Rotation, Rotation.Rotate0, left.Flip, 1, 1), left.Flat));

            indices.Add((short)(start + 0));
            indices.Add((short)(start + 1));
            indices.Add((short)(start + 2));
            indices.Add((short)(start + 1));
            indices.Add((short)(start + 3));
            indices.Add((short)(start + 2));
        }

        if (right.TileGraphic != 0)
        {
            var start = vertices.Length;

            vertices.Add(new VertexPositionTile(Vector3.Transform(new Vector3(1, 1, 1), translation), MapUv(right.TileGraphic, right.Rotation, Rotation.Rotate0, right.Flip, 0, 0), right.Flat));
            vertices.Add(new VertexPositionTile(Vector3.Transform(new Vector3(1, 0, 1), translation), MapUv(right.TileGraphic, right.Rotation, Rotation.Rotate0, right.Flip, 1, 0), right.Flat));
            vertices.Add(new VertexPositionTile(Vector3.Transform(new Vector3(1, 1, 0), translation), MapUv(right.TileGraphic, right.Rotation, Rotation.Rotate0, right.Flip, 0, 1), right.Flat));
            vertices.Add(new VertexPositionTile(Vector3.Transform(new Vector3(1, 0, 0), translation), MapUv(right.TileGraphic, right.Rotation, Rotation.Rotate0, right.Flip, 1, 1), right.Flat));

            indices.Add((short)(start + 0));
            indices.Add((short)(start + 1));
            indices.Add((short)(start + 2));
            indices.Add((short)(start + 1));
            indices.Add((short)(start + 3));
            indices.Add((short)(start + 2));
        }

        if (top.TileGraphic != 0)
        {
            var start = vertices.Length;
            
            vertices.Add(new VertexPositionTile(Vector3.Transform(new Vector3(1, 0, 1), translation), MapUv(top.TileGraphic, top.Rotation, Rotation.Rotate0, top.Flip, 0, 0), top.Flat));
            vertices.Add(new VertexPositionTile(Vector3.Transform(new Vector3(0, 0, 1), translation), MapUv(top.TileGraphic, top.Rotation, Rotation.Rotate0, top.Flip, 1, 0), top.Flat));
            vertices.Add(new VertexPositionTile(Vector3.Transform(new Vector3(1, 0, 0), translation), MapUv(top.TileGraphic, top.Rotation, Rotation.Rotate0, top.Flip, 0, 1), top.Flat));
            vertices.Add(new VertexPositionTile(Vector3.Transform(new Vector3(0, 0, 0), translation), MapUv(top.TileGraphic, top.Rotation, Rotation.Rotate0, top.Flip, 1, 1), top.Flat));

            indices.Add((short)(start + 0));
            indices.Add((short)(start + 1));
            indices.Add((short)(start + 2));
            indices.Add((short)(start + 1));
            indices.Add((short)(start + 3));
            indices.Add((short)(start + 2));
        }

        if (bottom.TileGraphic != 0)
        {
            var start = vertices.Length;

            vertices.Add(new VertexPositionTile(Vector3.Transform(new Vector3(0, 1, 1), translation), MapUv(bottom.TileGraphic, bottom.Rotation, Rotation.Rotate0, bottom.Flip, 0, 0), bottom.Flat));
            vertices.Add(new VertexPositionTile(Vector3.Transform(new Vector3(1, 1, 1), translation), MapUv(bottom.TileGraphic, bottom.Rotation, Rotation.Rotate0, bottom.Flip, 1, 0), bottom.Flat));
            vertices.Add(new VertexPositionTile(Vector3.Transform(new Vector3(0, 1, 0), translation), MapUv(bottom.TileGraphic, bottom.Rotation, Rotation.Rotate0, bottom.Flip, 0, 1), bottom.Flat));
            vertices.Add(new VertexPositionTile(Vector3.Transform(new Vector3(1, 1, 0), translation), MapUv(bottom.TileGraphic, bottom.Rotation, Rotation.Rotate0, bottom.Flip, 1, 1), bottom.Flat));

            indices.Add((short)(start + 0));
            indices.Add((short)(start + 1));
            indices.Add((short)(start + 2));
            indices.Add((short)(start + 1));
            indices.Add((short)(start + 3));
            indices.Add((short)(start + 2));
        }
    }

    private static void SlopeDiagonal(ref BlockInfo block, Rotation rotation, Matrix translationMatrix, BufferArray<VertexPositionTile> vertices, BufferArray<short> indices)
    {
        var translation = GetTranslation(rotation) * translationMatrix;

        // based on facing top-right
        
        RemapFaces(ref block, rotation, out var top, out var bottom, out var left, out var right);

        if (block.Lid.TileGraphic != 0)
        {
            var start = vertices.Length;
            vertices.Add(new VertexPositionTile(Vector3.Transform(new Vector3(0, 0, 1), translation), MapUv(block.Lid.TileGraphic, block.Lid.Rotation, rotation, block.Lid.Flip, 0, 0), block.Lid.Flat));
            vertices.Add(new VertexPositionTile(Vector3.Transform(new Vector3(1, 1, 1), translation), MapUv(block.Lid.TileGraphic, block.Lid.Rotation, rotation, block.Lid.Flip, 0, 0), block.Lid.Flat));
            vertices.Add(new VertexPositionTile(Vector3.Transform(new Vector3(0, 1, 1), translation), MapUv(block.Lid.TileGraphic, block.Lid.Rotation, rotation, block.Lid.Flip, 0, 0), block.Lid.Flat));

            indices.Add((short)(start + 0));
            indices.Add((short)(start + 1));
            indices.Add((short)(start + 2));
        }

        if (left.TileGraphic != 0)
        {
            var start = vertices.Length;

            vertices.Add(new VertexPositionTile(Vector3.Transform(new Vector3(0, 0, 1), translation), MapUv(left.TileGraphic, left.Rotation, Rotation.Rotate0, left.Flip, 0, 0), left.Flat));
            vertices.Add(new VertexPositionTile(Vector3.Transform(new Vector3(0, 1, 1), translation), MapUv(left.TileGraphic, left.Rotation, Rotation.Rotate0, left.Flip, 1, 0), left.Flat));
            vertices.Add(new VertexPositionTile(Vector3.Transform(new Vector3(0, 0, 0), translation), MapUv(left.TileGraphic, left.Rotation, Rotation.Rotate0, left.Flip, 0, 1), left.Flat));
            vertices.Add(new VertexPositionTile(Vector3.Transform(new Vector3(0, 1, 0), translation), MapUv(left.TileGraphic, left.Rotation, Rotation.Rotate0, left.Flip, 1, 1), left.Flat));

            indices.Add((short)(start + 0));
            indices.Add((short)(start + 1));
            indices.Add((short)(start + 2));
            indices.Add((short)(start + 1));
            indices.Add((short)(start + 3));
            indices.Add((short)(start + 2));
        }

        
        if (right.TileGraphic != 0)
        {
            var start = vertices.Length;

            vertices.Add(new VertexPositionTile(Vector3.Transform(new Vector3(1, 1, 1), translation), MapUv(right.TileGraphic, right.Rotation, Rotation.Rotate0, right.Flip, 0, 0), right.Flat));
            vertices.Add(new VertexPositionTile(Vector3.Transform(new Vector3(0, 0, 1), translation), MapUv(right.TileGraphic, right.Rotation, Rotation.Rotate0, right.Flip, 1, 0), right.Flat));
            vertices.Add(new VertexPositionTile(Vector3.Transform(new Vector3(1, 1, 0), translation), MapUv(right.TileGraphic, right.Rotation, Rotation.Rotate0, right.Flip, 0, 1), right.Flat));
            vertices.Add(new VertexPositionTile(Vector3.Transform(new Vector3(0, 0, 0), translation), MapUv(right.TileGraphic, right.Rotation, Rotation.Rotate0, right.Flip, 1, 1), right.Flat));

            indices.Add((short)(start + 0));
            indices.Add((short)(start + 1));
            indices.Add((short)(start + 2));
            indices.Add((short)(start + 1));
            indices.Add((short)(start + 3));
            indices.Add((short)(start + 2));
        }

        if (top.TileGraphic != 0)
        {
            var start = vertices.Length;

            vertices.Add(new VertexPositionTile(Vector3.Transform(new Vector3(1, 1, 1), translation), MapUv(top.TileGraphic, top.Rotation, Rotation.Rotate0, top.Flip, 0, 0), top.Flat));
            vertices.Add(new VertexPositionTile(Vector3.Transform(new Vector3(0, 0, 1), translation), MapUv(top.TileGraphic, top.Rotation, Rotation.Rotate0, top.Flip, 1, 0), top.Flat));
            vertices.Add(new VertexPositionTile(Vector3.Transform(new Vector3(1, 1, 0), translation), MapUv(top.TileGraphic, top.Rotation, Rotation.Rotate0, top.Flip, 0, 1), top.Flat));
            vertices.Add(new VertexPositionTile(Vector3.Transform(new Vector3(0, 0, 0), translation), MapUv(top.TileGraphic, top.Rotation, Rotation.Rotate0, top.Flip, 1, 1), top.Flat));

            indices.Add((short)(start + 0));
            indices.Add((short)(start + 1));
            indices.Add((short)(start + 2));
            indices.Add((short)(start + 1));
            indices.Add((short)(start + 3));
            indices.Add((short)(start + 2));
        }

        if (bottom.TileGraphic != 0)
        {
            var start = vertices.Length;

            vertices.Add(new VertexPositionTile(Vector3.Transform(new Vector3(0, 1, 1), translation), MapUv(bottom.TileGraphic, bottom.Rotation, Rotation.Rotate0, bottom.Flip, 0, 0), bottom.Flat));
            vertices.Add(new VertexPositionTile(Vector3.Transform(new Vector3(1, 1, 1), translation), MapUv(bottom.TileGraphic, bottom.Rotation, Rotation.Rotate0, bottom.Flip, 1, 0), bottom.Flat));
            vertices.Add(new VertexPositionTile(Vector3.Transform(new Vector3(0, 1, 0), translation), MapUv(bottom.TileGraphic, bottom.Rotation, Rotation.Rotate0, bottom.Flip, 0, 1), bottom.Flat));
            vertices.Add(new VertexPositionTile(Vector3.Transform(new Vector3(1, 1, 0), translation), MapUv(bottom.TileGraphic, bottom.Rotation, Rotation.Rotate0, bottom.Flip, 1, 1), bottom.Flat));

            indices.Add((short)(start + 0));
            indices.Add((short)(start + 1));
            indices.Add((short)(start + 2));
            indices.Add((short)(start + 1));
            indices.Add((short)(start + 3));
            indices.Add((short)(start + 2));
        }
    }

    private static void SlopeN(ref BlockInfo block, Rotation rotation, Matrix translationMatrix, BufferArray<VertexPositionTile> vertices,
        BufferArray<short> indices, float slopeFrom, float slopeTo)
    {
        var translation = GetTranslation(rotation) * translationMatrix;

        // based on slope up
        
        RemapFaces(ref block, rotation, out var top, out var bottom, out var left, out var right);

        if (block.Lid.TileGraphic != 0)
        {
            var start = vertices.Length;
            vertices.Add(new VertexPositionTile(Vector3.Transform(new Vector3(0, 0, slopeTo), translation), MapUv(block.Lid.TileGraphic, block.Lid.Rotation, rotation, block.Lid.Flip, 0, 0), block.Lid.Flat));
            vertices.Add(new VertexPositionTile(Vector3.Transform(new Vector3(1, 0, slopeTo), translation), MapUv(block.Lid.TileGraphic, block.Lid.Rotation, rotation, block.Lid.Flip, 1, 0), block.Lid.Flat));
            vertices.Add(new VertexPositionTile(Vector3.Transform(new Vector3(0, 1, slopeFrom), translation), MapUv(block.Lid.TileGraphic, block.Lid.Rotation, rotation, block.Lid.Flip, 0, 1), block.Lid.Flat));
            vertices.Add(new VertexPositionTile(Vector3.Transform(new Vector3(1, 1, slopeFrom), translation), MapUv(block.Lid.TileGraphic, block.Lid.Rotation, rotation, block.Lid.Flip, 1, 1), block.Lid.Flat));

            indices.Add((short)(start + 0));
            indices.Add((short)(start + 1));
            indices.Add((short)(start + 2));
            indices.Add((short)(start + 1));
            indices.Add((short)(start + 3));
            indices.Add((short)(start + 2));
        }


        if (left.TileGraphic != 0)
        {
            var start = vertices.Length;

            vertices.Add(new VertexPositionTile(Vector3.Transform(new Vector3(0, 0, slopeTo), translation), MapUv(left.TileGraphic, left.Rotation, Rotation.Rotate0, left.Flip, 0, 1-slopeTo), left.Flat));
            vertices.Add(new VertexPositionTile(Vector3.Transform(new Vector3(0, 1, slopeFrom), translation), MapUv(left.TileGraphic, left.Rotation, Rotation.Rotate0, left.Flip, 1, 1-slopeFrom), left.Flat));
            vertices.Add(new VertexPositionTile(Vector3.Transform(new Vector3(0, 0, 0), translation), MapUv(left.TileGraphic, left.Rotation, Rotation.Rotate0, left.Flip, 0, 1), left.Flat));

            indices.Add((short)(start + 0));
            indices.Add((short)(start + 1));
            indices.Add((short)(start + 2));

            if (slopeFrom != 0)
            {
                vertices.Add(new VertexPositionTile(Vector3.Transform(new Vector3(0, 1, 0), translation), MapUv(left.TileGraphic, left.Rotation, Rotation.Rotate0, left.Flip, 1, 1), left.Flat));
                indices.Add((short)(start + 1));
                indices.Add((short)(start + 3));
                indices.Add((short)(start + 2));
            }
        }

        if (right.TileGraphic != 0)
        {
            var start = vertices.Length;

            if (slopeFrom != 0)
            {
                vertices.Add(new VertexPositionTile(Vector3.Transform(new Vector3(1, 1, slopeFrom), translation), MapUv(right.TileGraphic, right.Rotation, Rotation.Rotate0, right.Flip, 0, 1 - slopeFrom), right.Flat));
            }

            vertices.Add(new VertexPositionTile(Vector3.Transform(new Vector3(1, 0, slopeTo), translation), MapUv(right.TileGraphic, right.Rotation, Rotation.Rotate0, right.Flip, 1, 1-slopeTo), right.Flat));
            vertices.Add(new VertexPositionTile(Vector3.Transform(new Vector3(1, 1, 0), translation), MapUv(right.TileGraphic, right.Rotation, Rotation.Rotate0, right.Flip, 0, 1), right.Flat));
            vertices.Add(new VertexPositionTile(Vector3.Transform(new Vector3(1, 0, 0), translation), MapUv(right.TileGraphic, right.Rotation, Rotation.Rotate0, right.Flip, 1, 1), right.Flat));


            if (slopeFrom != 0)
            {
                indices.Add((short)(start + 0));
                indices.Add((short)(start + 1));
                indices.Add((short)(start + 2));
                indices.Add((short)(start + 1));
                indices.Add((short)(start + 3));
                indices.Add((short)(start + 2));
            }
            else
            {
                
                indices.Add((short)(start + 0));
                indices.Add((short)(start + 2));
                indices.Add((short)(start + 1));
            }
        }

        if (top.TileGraphic != 0)
        {
            var start = vertices.Length;

            vertices.Add(new VertexPositionTile(Vector3.Transform(new Vector3(1, 0, slopeTo), translation), MapUv(top.TileGraphic, top.Rotation, Rotation.Rotate0, top.Flip, 0, 1-slopeTo), top.Flat));
            vertices.Add(new VertexPositionTile(Vector3.Transform(new Vector3(0, 0, slopeTo), translation), MapUv(top.TileGraphic, top.Rotation, Rotation.Rotate0, top.Flip, 1, 1-slopeTo), top.Flat));
            vertices.Add(new VertexPositionTile(Vector3.Transform(new Vector3(1, 0, 0), translation), MapUv(top.TileGraphic, top.Rotation, Rotation.Rotate0, top.Flip, 0, 1), top.Flat));
            vertices.Add(new VertexPositionTile(Vector3.Transform(new Vector3(0, 0, 0), translation), MapUv(top.TileGraphic, top.Rotation, Rotation.Rotate0, top.Flip, 1, 1), top.Flat));

            indices.Add((short)(start + 0));
            indices.Add((short)(start + 1));
            indices.Add((short)(start + 2));
            indices.Add((short)(start + 1));
            indices.Add((short)(start + 3));
            indices.Add((short)(start + 2));
        }

        if (bottom.TileGraphic != 0)
        {
            var start = vertices.Length;

            vertices.Add(new VertexPositionTile(Vector3.Transform(new Vector3(0, 1, slopeFrom), translation), MapUv(bottom.TileGraphic, bottom.Rotation, Rotation.Rotate0, bottom.Flip, 0, 1-slopeFrom), bottom.Flat));
            vertices.Add(new VertexPositionTile(Vector3.Transform(new Vector3(1, 1, slopeFrom), translation), MapUv(bottom.TileGraphic, bottom.Rotation, Rotation.Rotate0, bottom.Flip, 1, 1-slopeFrom), bottom.Flat));
            vertices.Add(new VertexPositionTile(Vector3.Transform(new Vector3(0, 1, 0), translation), MapUv(bottom.TileGraphic, bottom.Rotation, Rotation.Rotate0, bottom.Flip, 0, 1), bottom.Flat));
            vertices.Add(new VertexPositionTile(Vector3.Transform(new Vector3(1, 1, 0), translation), MapUv(bottom.TileGraphic, bottom.Rotation, Rotation.Rotate0, bottom.Flip, 1, 1), bottom.Flat));

            indices.Add((short)(start + 0));
            indices.Add((short)(start + 1));
            indices.Add((short)(start + 2));
            indices.Add((short)(start + 1));
            indices.Add((short)(start + 3));
            indices.Add((short)(start + 2));
        }
    }

    public static void Push(ref BlockInfo block, Vector3 offset, BufferArray<VertexPositionTile> vertices, BufferArray<short> indices)
    {
        var m = Matrix.CreateTranslation(offset);
        
        switch (block.SlopeType.SlopeType)
        {
            case SlopeType.Up45:
                SlopeN(ref block, Rotation.Rotate0, m, vertices, indices, 0, 1);
                break;
            case SlopeType.Right45:
                SlopeN(ref block, Rotation.Rotate90, m, vertices, indices, 0, 1);
                break;
            case SlopeType.Down45:
                SlopeN(ref block, Rotation.Rotate180, m, vertices, indices, 0, 1);
                break;
            case SlopeType.Left45:
                SlopeN(ref block, Rotation.Rotate270, m, vertices, indices, 0, 1);
                break;
            case SlopeType.DiagonalFacingUpRight:
                SlopeDiagonal(ref block, Rotation.Rotate0, m, vertices, indices);
                break;
            case SlopeType.DiagonalFacingDownRight:
                SlopeDiagonal(ref block, Rotation.Rotate90, m, vertices, indices);
                break;
            case SlopeType.DiagonalFacingDownLeft:
                SlopeDiagonal(ref block, Rotation.Rotate180, m, vertices, indices);
                break;
            case SlopeType.DiagonalFacingUpLeft:
                SlopeDiagonal(ref block, Rotation.Rotate270, m, vertices, indices);
                break;
            case SlopeType.Up26_1:
                SlopeN(ref block, Rotation.Rotate0, m, vertices, indices, 0.5f * (1 - 1), 0.5f * 1);
                break;
            case SlopeType.Up26_2:
                SlopeN(ref block, Rotation.Rotate0, m, vertices, indices, 0.5f * (2 - 1), 0.5f * 2);
                break;
            case SlopeType.Down26_1:
                SlopeN(ref block, Rotation.Rotate180, m, vertices, indices, 0.5f * (1 - 1), 0.5f * 1);
                break;
            case SlopeType.Down26_2:
                SlopeN(ref block, Rotation.Rotate180, m, vertices, indices, 0.5f * (2 - 1), 0.5f * 2);
                break;
            case SlopeType.Left26_1:
                SlopeN(ref block, Rotation.Rotate270, m, vertices, indices, 0.5f * (1 - 1), 0.5f * 1);
                break;
            case SlopeType.Left26_2:
                SlopeN(ref block, Rotation.Rotate270, m, vertices, indices, 0.5f * (2 - 1), 0.5f * 2);
                break;
            case SlopeType.Right26_1:
                SlopeN(ref block, Rotation.Rotate90, m, vertices, indices, 0.5f * (1 - 1), 0.5f * 1);
                break;
            case SlopeType.Right26_2:
                SlopeN(ref block, Rotation.Rotate90, m, vertices, indices, 0.5f * (2 - 1), 0.5f * 2);
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
                SlopeN(ref block, Rotation.Rotate0, m, vertices, indices, 0.125f * (num - 1), 0.125f * num);
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
                SlopeN(ref block, Rotation.Rotate180, m, vertices, indices, 0.125f * (num - 1), 0.125f * num);
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
                SlopeN(ref block, Rotation.Rotate270, m, vertices, indices, 0.125f * (num - 1), 0.125f * num);
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
                SlopeN(ref block, Rotation.Rotate90, m, vertices, indices, 0.125f * (num - 1), 0.125f * num);
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
                SlopeNone(ref block, Rotation.Rotate0, m, vertices, indices);
                break;
            case SlopeType.Reserved:
                break;
            default:
            case SlopeType.SlopeAbove:
            case SlopeType.None:
                SlopeNone(ref block, Rotation.Rotate0, m, vertices, indices);
                break;
        }
    }
}