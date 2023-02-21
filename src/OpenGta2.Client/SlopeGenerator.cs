using System;
using System.Diagnostics;
using Microsoft.Xna.Framework;
using OpenGta2.Client.Effects;
using OpenGta2.Data.Map;
using SharpDX.DXGI;

namespace OpenGta2.Client;

public static class SlopeGenerator
{
    // rotation of cube
    private static readonly Matrix MatrixRotateUp = Matrix.Identity;
    private static readonly Matrix MatrixRotateRight = Matrix.CreateRotationZ(MathF.PI / 2) * Matrix.CreateTranslation(1, 0, 0);
    private static readonly Matrix MatrixRotateDown = Matrix.CreateRotationZ(MathF.PI) * Matrix.CreateTranslation(1, 1, 0);
    private static readonly Matrix MatrixRotateLeft = Matrix.CreateRotationZ(-MathF.PI / 2) * Matrix.CreateTranslation(0, 1, 0);
    
    // face to cube
    private static readonly Matrix MatrixFaceToCubeLeft = Matrix.CreateRotationX(-MathHelper.PiOver2) *
                                             Matrix.CreateRotationZ(MathHelper.PiOver2) *
                                             Matrix.CreateTranslation(new Vector3(0, 0, 1));
    
    private static readonly Matrix MatrixFaceToCubeRight = Matrix.CreateRotationX(-MathHelper.PiOver2) *
                                              Matrix.CreateRotationZ(-MathHelper.PiOver2) *
                                              Matrix.CreateTranslation(new Vector3(1, 1, 1));

    private static readonly Matrix MatrixFaceToCubeTop = Matrix.CreateRotationX(-MathHelper.PiOver2) *
                                            Matrix.CreateRotationZ(MathHelper.Pi) *
                                            Matrix.CreateTranslation(new Vector3(1, 0, 1));

    private static readonly Matrix MatrixFaceToCubeBottom = Matrix.CreateRotationX(-MathHelper.PiOver2) *
                                               Matrix.CreateTranslation(new Vector3(0, 1, 1));

    private static readonly Matrix MatrixFaceToCubeLid = Matrix.CreateTranslation(new Vector3(0, 0, 1));

    /// <summary>
    /// Returns the transformation matrix applying the specified <paramref name="rotation"/> to a 1x1x1 cube.
    /// </summary>
    private static Matrix GetRotationMatrix(Rotation rotation)
    {
        return rotation switch
        {
            Rotation.Rotate90 => MatrixRotateRight,
            Rotation.Rotate180 => MatrixRotateDown,
            Rotation.Rotate270 => MatrixRotateLeft,
            Rotation.Rotate0 => MatrixRotateUp,
            _ => MatrixRotateUp
        };
    }

    private static Face Rotate(Face face, Rotation rotation)
    {
        if (face is Face.None or Face.Lid || rotation == Rotation.Rotate0)
        {
            return face;
        }

        face = rotation switch
        {
            Rotation.Rotate90 => face switch
            {
                Face.Top => Face.Right,
                Face.Right => Face.Bottom,
                Face.Bottom => Face.Left,
                _ => Face.Top,
            },
            Rotation.Rotate180 => face switch
            {
                Face.Top => Face.Bottom,
                Face.Right => Face.Left,
                Face.Bottom => Face.Top,
                _ => Face.Right,
            },
            Rotation.Rotate270 => face switch
            {
                Face.Top => Face.Left,
                Face.Right => Face.Top,
                Face.Bottom => Face.Right,
                _ => Face.Bottom,
            },
            _ => face
        };

        return face;
    }

    /// <summary>
    /// Returns the faces of the specified <paramref name="block"/>, after applying the specified <paramref name="rotation"/>.
    /// </summary>
    /// <param name="block">The block.</param>
    /// <param name="rotation">The rotation that will be applied to the block using a transformation.</param>
    /// <param name="top">Up face.</param>
    /// <param name="bottom">Down face.</param>
    /// <param name="left">Left face.</param>
    /// <param name="right">Right face.</param>
    private static void GetBlockFaces(ref BlockInfo block, Rotation rotation, out FaceInfo top, out FaceInfo bottom, out FaceInfo left, out FaceInfo right)
    {
        top = GetFace(ref block, Face.Top, rotation);
        bottom = GetFace(ref block, Face.Bottom, rotation);
        left = GetFace(ref block, Face.Left, rotation);
        right = GetFace(ref block, Face.Right, rotation);
        
        static ref FaceInfo GetFace(ref BlockInfo block, Face face, Rotation rotation)
        {
            switch (Rotate(face, rotation))
            {
                case Face.Left: return ref block.Left;
                case Face.Right: return ref block.Right;
                case Face.Top: return ref block.Top;
                case Face.Bottom: return ref block.Bottom;
                default: return ref block.Lid;
            }
        }
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

    /// <summary>
    /// Maps the specified uv point on the specified <paramref name="face"/>, taking into account the flip, rotation and graphic parameters of the face.
    /// </summary>
    /// <param name="face">The face to map the uv point on.</param>
    /// <param name="uvRotation">Subtract the specified rotation from the face rotation. Could be used on the lid face when the entire face is rotated using a transformation.</param>
    /// <param name="u">u component.</param>
    /// <param name="v">v component.</param>
    /// <returns>Mapped uv point. The third component is the tile graphic.</returns>
    private static Vector3 MapUv(ref FaceInfo face, Rotation uvRotation, float u, float v)
    {
        var rotation = face.Rotation;
        var x1 = u;
        var y1 = v;
        if (face.Flip)
        {
            if (uvRotation is Rotation.Rotate90 or Rotation.Rotate270)
            {
                y1 = 1 - y1;
            }
            else
            {
                x1 = 1 - x1;
            }
        }

        rotation = Subtract(rotation, uvRotation);
        
        float tmp;
        switch (rotation)
        {
            case Rotation.Rotate90:
                tmp = 1 - x1;
                x1 = y1;
                y1 = tmp;
                break;
            case Rotation.Rotate180:
                x1 = 1 - x1;
                y1 = 1 - y1;
                break;
            case Rotation.Rotate270:
                tmp = 1 - y1;
                y1 = x1;
                x1 = tmp;
                break;
        }
        
        return new Vector3(x1, y1, face.TileGraphic);
    }
    
    private static void ApplyDefaultShading(ref float shading, ref FaceInfo face, Face direction, Rotation blockRotation)
    {
        if (shading < 0)
        {
            var actualFace = Rotate(direction, blockRotation);
            shading = actualFace switch
            {
                Face.Lid => GetShade(face.LightingLevel + 1),
                Face.Left => GetShade(2),
                Face.Bottom => GetShade(4),
                Face.Top => GetShade(6),
                _ => GetShade(8)
            };
        }
    }

    private static float GetShade(int num) => MathF.Log10(num);

    /// <summary>
    /// Gets a vertex for the specified <paramref name="point"/> on the specified <paramref name="face"/>.
    /// </summary>
    /// <param name="point">The point on the face to use as the position of the vertex. The z component represents the depth on the face.</param>
    /// <param name="uv">uv point on the face.</param>
    /// <param name="face">The face to create the vertex on.</param>
    /// <param name="direction">The direction of the specified <paramref name="face"/>.</param>
    /// <param name="translation">The translation to apply to the position.</param>
    /// <param name="blockRotation">The rotation of the block. Is used to rotate the graphics on the lid and to apply shading to the face.</param>
    /// <param name="shading">The shading of the face. Value should range between 0-1. If the value is negative, the value is computed based on teh direction of the face.</param>
    /// <returns>The created vertex.</returns>
    private static VertexPositionTile GetVertexOnFace(Vector3 point, Vector2 uv, ref FaceInfo face, Face direction, Matrix translation, Rotation blockRotation, float shading = -1)
    {
        var matrix = direction switch
        {
            Face.Left => MatrixFaceToCubeLeft,
            Face.Right => MatrixFaceToCubeRight,
            Face.Top => MatrixFaceToCubeTop,
            Face.Bottom => MatrixFaceToCubeBottom,
            Face.Lid => MatrixFaceToCubeLid,
            _ => throw new ArgumentOutOfRangeException(nameof(face), face, null)
        };

        matrix *= translation;
        
        return GetVertex(point, uv, ref face, direction, matrix, blockRotation, shading);
    }

    /// <summary>
    /// Gets a vertex at the specified <paramref name="position"/> for the specified <paramref name="face"/>.
    /// </summary>
    /// <param name="position">The position of the vertex.</param>
    /// <param name="uv">uv point of the vertex.</param>
    /// <param name="face">The face to map the uv point on.</param>
    /// <param name="direction">The direction of the face.</param>
    /// <param name="translation">The translation to apply to the <paramref name="position"/>.</param>
    /// <param name="blockRotation">The rotation of the block. Is used to rotate the graphics on the lid and to apply shading to the face.</param>
    /// <param name="shading">The shading of the face. Value should range between 0-1. If the value is negative, the value is computed based on teh direction of the face.</param>
    /// <returns>The created vertex.</returns>
    private static VertexPositionTile GetVertex(Vector3 position, Vector2 uv, ref FaceInfo face, Face direction, Matrix translation, Rotation blockRotation, float shading = -1)
    {
        ApplyDefaultShading(ref shading, ref face, direction, blockRotation);

        return new VertexPositionTile(Vector3.Transform(position, translation), MapUv(ref face, direction == Face.Lid ? blockRotation : Rotation.Rotate0, uv.X, uv.Y), shading);
    }

    /// <summary>
    /// Returns the diagonal face. For diagonals slope types, the diagonal face is always the left or right face. Due to block rotations this might be the top face from the perspective of the slope generator.
    /// This is because in this slope generator all slopes are based on a top-right slope to which a rotation is applied to match the desired slope. 
    /// </summary>
    /// <param name="blockRotation">Rotation of the block</param>
    /// <param name="top">The top face.</param>
    /// <param name="right">The right face.</param>
    /// <param name="diagonalFaceDirection">The diagonal face direction.</param>
    /// <returns>The diagonal face.</returns>
    private static ref FaceInfo GetDiagonalFace(Rotation blockRotation, ref FaceInfo top, ref FaceInfo right, out Face diagonalFaceDirection)
    {
        if (blockRotation is Rotation.Rotate90 or Rotation.Rotate270)
        {
            diagonalFaceDirection = Face.Top;
            return ref top;
        }

        diagonalFaceDirection = Face.Right;
        return ref right;
    }

    private static void AddIndex(Buffers buffers, ref FaceInfo face, float drawOrder, int index, bool oppositeFlat = false)
    {
        if (face.Flat || oppositeFlat)
            buffers.FlatIndices.Add((drawOrder, (short)index));
        else
            buffers.Indices.Add((short)index);
    }

    private static void AddSimpleFace(ref FaceInfo face, float drawOrder, Face side, Buffers buffers, Matrix translation, bool oppositeFlat = false) =>
        AddSimpleFace(ref face, drawOrder, side, buffers, translation, Vector2.Zero, Vector2.One, 0, Rotation.Rotate0, oppositeFlat, 1);

    private static void AddSimpleFace(ref FaceInfo face, float drawOrder, Face side, Buffers buffers, Matrix translation, Vector2 min, Vector2 max, float depth,
        Rotation uvRotation, bool oppositeFlat, float oppositeDepth)
    {
        // a square face on the specified side.
        if (face.TileGraphic == 0)
            return;

        var start = buffers.Vertices.Length;

        if (face.Flat && oppositeFlat)
        {
            // render on both sides
            buffers.Vertices.Add(GetVertexOnFace(new Vector3(min.X, min.Y, -depth), new Vector2(min.X, min.Y), ref face, side, translation, uvRotation));
            buffers.Vertices.Add(GetVertexOnFace(new Vector3(max.X, min.Y, -depth), new Vector2(max.X, min.Y), ref face, side, translation, uvRotation));
            buffers.Vertices.Add(GetVertexOnFace(new Vector3(min.X, max.Y, -depth), new Vector2(min.X, max.Y), ref face, side, translation, uvRotation));
            buffers.Vertices.Add(GetVertexOnFace(new Vector3(max.X, max.Y, -depth), new Vector2(max.X, max.Y), ref face, side, translation, uvRotation));

            AddIndex(buffers, ref face, drawOrder, start + 0);
            AddIndex(buffers, ref face, drawOrder, start + 1);
            AddIndex(buffers, ref face, drawOrder, start + 2);
            AddIndex(buffers, ref face, drawOrder, start + 1);
            AddIndex(buffers, ref face, drawOrder, start + 3);
            AddIndex(buffers, ref face, drawOrder, start + 2);

            start = buffers.Vertices.Length;
        }

        var oz = oppositeFlat ? -oppositeDepth + 0.01f : -depth;
        buffers.Vertices.Add(GetVertexOnFace(new Vector3(min.X, min.Y, oz), new Vector2(min.X, min.Y), ref face, side, translation, uvRotation));
        buffers.Vertices.Add(GetVertexOnFace(new Vector3(max.X, min.Y, oz), new Vector2(max.X, min.Y), ref face, side, translation, uvRotation));
        buffers.Vertices.Add(GetVertexOnFace(new Vector3(min.X, max.Y, oz), new Vector2(min.X, max.Y), ref face, side, translation, uvRotation));
        buffers.Vertices.Add(GetVertexOnFace(new Vector3(max.X, max.Y, oz), new Vector2(max.X, max.Y), ref face, side, translation, uvRotation));

        AddIndex(buffers, ref face, drawOrder, start + 0, oppositeFlat);
        AddIndex(buffers, ref face, drawOrder, start + 1, oppositeFlat);
        AddIndex(buffers, ref face, drawOrder, start + 2, oppositeFlat);
        AddIndex(buffers, ref face, drawOrder, start + 1, oppositeFlat);
        AddIndex(buffers, ref face, drawOrder, start + 3, oppositeFlat);
        AddIndex(buffers, ref face, drawOrder, start + 2, oppositeFlat);
    }

    private static void SlopeNone(ref BlockInfo block, float drawOrder, Matrix translationMatrix, Buffers buffers)
    {
        AddSimpleFace(ref block.Lid, drawOrder + 0.5f, Face.Lid, buffers, translationMatrix);
        AddSimpleFace(ref block.Left, drawOrder, Face.Left, buffers, translationMatrix, block.Right.Flat);
        AddSimpleFace(ref block.Right, drawOrder, Face.Right, buffers, translationMatrix, block.Left.Flat);
        AddSimpleFace(ref block.Top, drawOrder, Face.Top, buffers, translationMatrix, block.Bottom.Flat);
        AddSimpleFace(ref block.Bottom, drawOrder, Face.Bottom, buffers, translationMatrix, block.Top.Flat);
    }

    private static void SlopeDiagonal(ref BlockInfo block, float drawOrder, Rotation blockRotation, Matrix translationMatrix, Buffers buffers)
    {
        // based on facing up-right
        var translation = GetRotationMatrix(blockRotation) * translationMatrix;
        GetBlockFaces(ref block, blockRotation, out var top, out var bottom, out var left, out var right);
        
        AddSimpleFace(ref left, drawOrder, Face.Left, buffers, translation);
        AddSimpleFace(ref bottom, drawOrder, Face.Bottom, buffers, translation);
        
        if (block.Lid.TileGraphic != 0)
        {
            var start = buffers.Vertices.Length;
            buffers.Vertices.Add(GetVertex(new Vector3(0, 0, 1), new Vector2(0, 0), ref block.Lid, Face.Lid, translation, blockRotation));
            buffers.Vertices.Add(GetVertex(new Vector3(1, 1, 1), new Vector2(1, 1), ref block.Lid, Face.Lid, translation, blockRotation));
            buffers.Vertices.Add(GetVertex(new Vector3(0, 1, 1), new Vector2(0, 1), ref block.Lid, Face.Lid, translation, blockRotation));

            AddIndex(buffers, ref block.Lid, drawOrder + 0.5f, start + 0);
            AddIndex(buffers, ref block.Lid, drawOrder + 0.5f, start + 1);
            AddIndex(buffers, ref block.Lid, drawOrder + 0.5f, start + 2);
        }
        
        ref var diagonal = ref GetDiagonalFace(blockRotation, ref top, ref right, out var slopeDirection);

        if (diagonal.TileGraphic != 0)
        {
            var diagonalShade = GetShade(blockRotation switch
            {
                Rotation.Rotate0 => 7,
                Rotation.Rotate90 => 6,
                Rotation.Rotate180 => 2,
                _ => 3
            });

            var start = buffers.Vertices.Length;
            
            buffers.Vertices.Add(GetVertex(new Vector3(1, 1, 1), new Vector2(0, 0), ref diagonal, slopeDirection, translation, blockRotation, diagonalShade));
            buffers.Vertices.Add(GetVertex(new Vector3(0, 0, 1), new Vector2(1, 0), ref diagonal, slopeDirection, translation, blockRotation, diagonalShade));
            buffers.Vertices.Add(GetVertex(new Vector3(1, 1, 0), new Vector2(0, 1), ref diagonal, slopeDirection, translation, blockRotation, diagonalShade));
            buffers.Vertices.Add(GetVertex(new Vector3(0, 0, 0), new Vector2(1, 1), ref diagonal, slopeDirection, translation, blockRotation, diagonalShade));
            
            AddIndex(buffers, ref diagonal, drawOrder, start + 0);
            AddIndex(buffers, ref diagonal, drawOrder, start + 1);
            AddIndex(buffers, ref diagonal, drawOrder, start + 2);
            AddIndex(buffers, ref diagonal, drawOrder, start + 1);
            AddIndex(buffers, ref diagonal, drawOrder, start + 3);
            AddIndex(buffers, ref diagonal, drawOrder, start + 2);
        }
        
    }

    private static void SlopeNSlope(ref BlockInfo block, float drawOrder, Rotation blockRotation, Matrix translationMatrix, Buffers buffers, float slopeFrom, float slopeTo)
    {
        var translation = GetRotationMatrix(blockRotation) * translationMatrix;

        // based on slope up

        GetBlockFaces(ref block, blockRotation, out var top, out var bottom, out var left, out var right);

        if (block.Lid.TileGraphic != 0)
        {
            // prefer non-default shading from map data over computed value
            var slopeShade = GetShade(block.Lid.LightingLevel > 0
                ? block.Lid.LightingLevel + 1
                : blockRotation switch
                {
                    Rotation.Rotate0 => 3,
                    Rotation.Rotate90 => 2,
                    Rotation.Rotate180 => 5,
                    _ => 7
                });

            var start = buffers.Vertices.Length;

            buffers.Vertices.Add(GetVertex(new Vector3(0, 0, slopeTo), new Vector2(0, 0), ref block.Lid, Face.Lid, translation, blockRotation, slopeShade));
            buffers.Vertices.Add(GetVertex(new Vector3(1, 0, slopeTo), new Vector2(1, 0), ref block.Lid, Face.Lid, translation, blockRotation, slopeShade));
            buffers.Vertices.Add(GetVertex(new Vector3(0, 1, slopeFrom), new Vector2(0, 1), ref block.Lid, Face.Lid, translation, blockRotation, slopeShade));
            buffers.Vertices.Add(GetVertex(new Vector3(1, 1, slopeFrom), new Vector2(1, 1), ref block.Lid, Face.Lid, translation, blockRotation, slopeShade));

            AddIndex(buffers, ref block.Lid, drawOrder + 0.5f, start + 0);
            AddIndex(buffers, ref block.Lid, drawOrder + 0.5f, start + 1);
            AddIndex(buffers, ref block.Lid, drawOrder + 0.5f, start + 2);
            AddIndex(buffers, ref block.Lid, drawOrder + 0.5f, start + 1);
            AddIndex(buffers, ref block.Lid, drawOrder + 0.5f, start + 3);
            AddIndex(buffers, ref block.Lid, drawOrder + 0.5f, start + 2);
        }


        if (left.TileGraphic != 0)
        {
            var start = buffers.Vertices.Length;

            // todo: double render if left.flat and right.flat
            buffers.Vertices.Add(GetVertex(new Vector3(0, 0, slopeTo), new Vector2(0, 1 - slopeTo), ref left, Face.Left, translation, blockRotation));
            buffers.Vertices.Add(GetVertex(new Vector3(0, 1, slopeFrom), new Vector2(1, 1 - slopeFrom), ref left, Face.Left, translation, blockRotation));
            buffers.Vertices.Add(GetVertex(new Vector3(0, 0, 0), new Vector2(0, 1), ref left, Face.Left, translation, blockRotation));

            AddIndex(buffers, ref left, drawOrder, start + 0, right.Flat);
            AddIndex(buffers, ref left, drawOrder, start + 1, right.Flat);
            AddIndex(buffers, ref left, drawOrder, start + 2, right.Flat);

            if (slopeFrom != 0)
            {
                buffers.Vertices.Add(GetVertex(new Vector3(0, 1, 0), new Vector2(1, 1), ref left, Face.Left, translation, blockRotation));
                AddIndex(buffers, ref left, drawOrder, start + 1, right.Flat);
                AddIndex(buffers, ref left, drawOrder, start + 3, right.Flat);
                AddIndex(buffers, ref left, drawOrder, start + 2, right.Flat);
            }
        }

        if (right.TileGraphic != 0)
        {
            var start = buffers.Vertices.Length;
            
            // todo: double render if left.flat and right.flat
            if (slopeFrom != 0)
            {
                buffers.Vertices.Add(GetVertex(new Vector3(1, 1, slopeFrom), new Vector2(0, 1 - slopeFrom), ref right, Face.Right, translation, blockRotation));
            }

            buffers.Vertices.Add(GetVertex(new Vector3(1, 0, slopeTo), new Vector2(1, 1 - slopeTo), ref right, Face.Right, translation, blockRotation));
            buffers.Vertices.Add(GetVertex(new Vector3(1, 1, 0), new Vector2(0, 1), ref right, Face.Right, translation, blockRotation));
            buffers.Vertices.Add(GetVertex(new Vector3(1, 0, 0), new Vector2(1, 1), ref right, Face.Right, translation, blockRotation));


            if (slopeFrom != 0)
            {
                AddIndex(buffers, ref right, drawOrder, start + 0, left.Flat);
                AddIndex(buffers, ref right, drawOrder, start + 1, left.Flat);
                AddIndex(buffers, ref right, drawOrder, start + 2, left.Flat);
                AddIndex(buffers, ref right, drawOrder, start + 1, left.Flat);
                AddIndex(buffers, ref right, drawOrder, start + 3, left.Flat);
                AddIndex(buffers, ref right, drawOrder, start + 2, left.Flat);
            }
            else
            {

                AddIndex(buffers,  ref right, drawOrder, start + 0, left.Flat);
                AddIndex(buffers,  ref right, drawOrder, start + 2, left.Flat);
                AddIndex(buffers,  ref right, drawOrder, start + 1, left.Flat);
            }
        }

        if (top.TileGraphic != 0)
        {
            var start = buffers.Vertices.Length;

            buffers.Vertices.Add(GetVertex(new Vector3(1, 0, slopeTo), new Vector2(0, 1 - slopeTo), ref top, Face.Top, translation, blockRotation));
            buffers.Vertices.Add(GetVertex(new Vector3(0, 0, slopeTo), new Vector2(1, 1 - slopeTo), ref top, Face.Top, translation, blockRotation));
            buffers.Vertices.Add(GetVertex(new Vector3(1, 0, 0), new Vector2(0, 1), ref top, Face.Top, translation, blockRotation));
            buffers.Vertices.Add(GetVertex(new Vector3(0, 0, 0), new Vector2(1, 1), ref top, Face.Top, translation, blockRotation));

            AddIndex(buffers, ref top, drawOrder, start + 0);
            AddIndex(buffers, ref top, drawOrder, start + 1);
            AddIndex(buffers, ref top, drawOrder, start + 2);
            AddIndex(buffers, ref top, drawOrder, start + 1);
            AddIndex(buffers, ref top, drawOrder, start + 3);
            AddIndex(buffers, ref top, drawOrder, start + 2);
        }

        if (bottom.TileGraphic != 0)
        {
            var start = buffers.Vertices.Length;

            buffers.Vertices.Add(GetVertex(new Vector3(0, 1, slopeFrom), new Vector2(0, 1 - slopeFrom), ref bottom, Face.Bottom, translation, blockRotation));
            buffers.Vertices.Add(GetVertex(new Vector3(1, 1, slopeFrom), new Vector2(1, 1 - slopeFrom), ref bottom, Face.Bottom, translation, blockRotation));
            buffers.Vertices.Add(GetVertex(new Vector3(0, 1, 0), new Vector2(0, 1), ref bottom, Face.Bottom, translation, blockRotation));
            buffers.Vertices.Add(GetVertex(new Vector3(1, 1, 0), new Vector2(1, 1), ref bottom, Face.Bottom, translation, blockRotation));

            AddIndex(buffers, ref bottom, drawOrder, start + 0);
            AddIndex(buffers, ref bottom, drawOrder, start + 1);
            AddIndex(buffers, ref bottom, drawOrder, start + 2);
            AddIndex(buffers, ref bottom, drawOrder, start + 1);
            AddIndex(buffers, ref bottom, drawOrder, start + 3);
            AddIndex(buffers, ref bottom, drawOrder, start + 2);
        }
    }

    private static void SlopeDiagonalSlope(ref BlockInfo block, float drawOrder, Rotation blockRotation, Matrix translationMatrix, Buffers buffers)
    {
        var translation = GetRotationMatrix(blockRotation) * translationMatrix;
        GetBlockFaces(ref block, blockRotation, out var top, out var bottom, out var left, out var right);
        
        ref var diagonal = ref GetDiagonalFace(blockRotation, ref top, ref right, out var diagonalFaceDiretion);
        
        var diagonalShade = GetShade(blockRotation switch
        {
            Rotation.Rotate0 => 6,
            Rotation.Rotate90 => 5,
            Rotation.Rotate180 => 2,
            _ => 3
        });

        // based on facing up-right
        if (block.Lid.TileGraphic == 1023)
        {
            // 3-sided: a triangular base with a diagonals to an upper corner
            if (block.Left.TileGraphic != 0)
            {
                var start = buffers.Vertices.Length;

                buffers.Vertices.Add(GetVertex(new Vector3(0, 1, 1), new Vector2(1, 0), ref block.Left, Face.Left, translation, blockRotation));
                buffers.Vertices.Add(GetVertex(new Vector3(0, 1, 0), new Vector2(1, 1), ref block.Left, Face.Left, translation, blockRotation));
                buffers.Vertices.Add(GetVertex(new Vector3(0, 0, 0), new Vector2(0, 1), ref block.Left, Face.Left, translation, blockRotation));

                AddIndex(buffers, ref block.Left, drawOrder, start + 0);
                AddIndex(buffers, ref block.Left, drawOrder, start + 1);
                AddIndex(buffers, ref block.Left, drawOrder, start + 2);
            }
            
            if (block.Bottom.TileGraphic != 0)
            {
                var start = buffers.Vertices.Length;

                buffers.Vertices.Add(GetVertex(new Vector3(0, 1, 1), new Vector2(0, 0), ref block.Bottom, Face.Bottom, translation, blockRotation));
                buffers.Vertices.Add(GetVertex(new Vector3(1, 1, 0), new Vector2(1, 1), ref block.Bottom, Face.Bottom, translation, blockRotation));
                buffers.Vertices.Add(GetVertex(new Vector3(0, 1, 0), new Vector2(1, 0), ref block.Bottom, Face.Bottom, translation, blockRotation));

                AddIndex(buffers, ref block.Bottom, drawOrder, start + 0);
                AddIndex(buffers, ref block.Bottom, drawOrder, start + 1);
                AddIndex(buffers, ref block.Bottom, drawOrder, start + 2);
            }

            if (diagonal.TileGraphic != 0)
            {
                var start = buffers.Vertices.Length;

                buffers.Vertices.Add(GetVertex(new Vector3(0, 1, 1), new Vector2(0.5f, 0), ref diagonal, diagonalFaceDiretion, translation, blockRotation, diagonalShade));
                buffers.Vertices.Add(GetVertex(new Vector3(0, 0, 0), new Vector2(1, 1), ref diagonal, diagonalFaceDiretion, translation, blockRotation, diagonalShade));
                buffers.Vertices.Add(GetVertex(new Vector3(1, 1, 0), new Vector2(0, 1), ref diagonal, diagonalFaceDiretion, translation, blockRotation, diagonalShade));

                AddIndex(buffers, ref diagonal, drawOrder, start + 0);
                AddIndex(buffers, ref diagonal, drawOrder, start + 1);
                AddIndex(buffers, ref diagonal, drawOrder, start + 2);
            }
        }
        else
        {
            // 4-sided: a triangular lid with diagonals to the inverse lower corner
            AddSimpleFace(ref block.Left, drawOrder, Face.Left, buffers, translationMatrix);
            AddSimpleFace(ref block.Bottom, drawOrder, Face.Bottom, buffers, translationMatrix);
            
            if (block.Lid.TileGraphic != 0)
            {
                var start = buffers.Vertices.Length;
                buffers.Vertices.Add(GetVertex(new Vector3(0, 0, 1), new Vector2(0, 0), ref block.Lid, Face.Lid, translation, blockRotation));
                buffers.Vertices.Add(GetVertex(new Vector3(1, 1, 1), new Vector2(1, 1), ref block.Lid, Face.Lid, translation, blockRotation));
                buffers.Vertices.Add(GetVertex(new Vector3(0, 1, 1), new Vector2(0, 1), ref block.Lid, Face.Lid, translation, blockRotation));

                AddIndex(buffers, ref block.Lid, drawOrder + 0.5f, start + 0);
                AddIndex(buffers, ref block.Lid, drawOrder + 0.5f, start + 1);
                AddIndex(buffers, ref block.Lid, drawOrder + 0.5f, start + 2);
            }

            if (diagonal.TileGraphic != 0)
            {
                var start = buffers.Vertices.Length;

                buffers.Vertices.Add(GetVertex(new Vector3(1, 1, 1), new Vector2(0, 0), ref block.Left, Face.Left, translation, blockRotation, diagonalShade));
                buffers.Vertices.Add(GetVertex(new Vector3(0, 0, 1), new Vector2(1, 0), ref block.Left, Face.Left, translation, blockRotation, diagonalShade));
                buffers.Vertices.Add(GetVertex(new Vector3(1, 0, 0), new Vector2(0.5f, 1), ref block.Left, Face.Left, translation, blockRotation, diagonalShade));

                AddIndex(buffers, ref block.Left, drawOrder, start + 0);
                AddIndex(buffers, ref block.Left, drawOrder, start + 1);
                AddIndex(buffers, ref block.Left, drawOrder, start + 2);
            }
        }
    }
    
    private static void SlopeHalfBlock(ref BlockInfo block, float drawOrder, Rotation blockRotation, Matrix translationMatrix, Buffers buffers)
    {
        // based on left
        var translation = GetRotationMatrix(blockRotation) * translationMatrix;
        GetBlockFaces(ref block, blockRotation, out var top, out var bottom, out var left, out var right);

        const float width = 24 / 64f;// 24 of 64 pixels wide wall

        AddSimpleFace(ref block.Lid, drawOrder + 0.5f, Face.Lid, buffers, translation, Vector2.Zero, new Vector2(width, 1.0f), 0, blockRotation, false, 1);
        AddSimpleFace(ref left, drawOrder, Face.Left, buffers, translation, Vector2.Zero, Vector2.One, 0, Rotation.Rotate0, right.Flat, width);
        AddSimpleFace(ref right, drawOrder, Face.Right, buffers, translation, Vector2.Zero, Vector2.One, 1 - width, Rotation.Rotate0, left.Flat, 1);
        AddSimpleFace(ref top, drawOrder, Face.Top, buffers, translation, new Vector2(1-width, 0), Vector2.One, 0, Rotation.Rotate0, bottom.Flat, 1);
        AddSimpleFace(ref bottom, drawOrder, Face.Bottom, buffers, translation, Vector2.Zero, new Vector2(width, 1), 0, Rotation.Rotate0, top.Flat, 1);
    }

    private static void SlopeCornerBlock(ref BlockInfo block, float drawOrder, Rotation blockRotation, Matrix translationMatrix, Buffers buffers)
    {
        // based on top left
        var translation = GetRotationMatrix(blockRotation) * translationMatrix;
        GetBlockFaces(ref block, blockRotation, out var top, out var bottom, out var left, out var right);

        const float width = 24 / 64f;// 24 of 64 pixels wide wall

        AddSimpleFace(ref block.Lid, drawOrder + 0.5f, Face.Lid, buffers, translation, Vector2.Zero, new Vector2(width, width), 0, blockRotation, false, 1);
        AddSimpleFace(ref left, drawOrder, Face.Left, buffers, translation, Vector2.Zero, new Vector2(width, 1), 0, Rotation.Rotate0, right.Flat, width);
        AddSimpleFace(ref right, drawOrder, Face.Right, buffers, translation, new Vector2(1 - width, 0), Vector2.One, 1 - width, Rotation.Rotate0, left.Flat, 1);
        AddSimpleFace(ref top, drawOrder, Face.Top, buffers, translation, new Vector2(1 - width, 0), Vector2.One, 0, Rotation.Rotate0, bottom.Flat, width);
        AddSimpleFace(ref bottom, drawOrder, Face.Bottom, buffers, translation, Vector2.Zero, new Vector2(width, 1), 1 - width, Rotation.Rotate0, top.Flat, 1);
    }

    private static void SlopeCentreBlock(ref BlockInfo block, float drawOrder, Matrix translationMatrix, Buffers buffers)
    {
        const float width = 16 / 64f;// 16 of 64 pixels wide wall
        const float min = (1 - width) / 2;
        const float max = min + width;
        
        AddSimpleFace(ref block.Lid, drawOrder + 0.5f, Face.Lid, buffers, translationMatrix, new Vector2(min, min), new Vector2(max, max), 0, Rotation.Rotate0, false, 1);
        AddSimpleFace(ref block.Left, drawOrder, Face.Left, buffers, translationMatrix, new Vector2(min, 0), new Vector2(max, 1), min, Rotation.Rotate0, block.Right.Flat, max);
        AddSimpleFace(ref block.Right, drawOrder, Face.Right, buffers, translationMatrix, new Vector2(min, 0), new Vector2(max, 1), min, Rotation.Rotate0, block.Left.Flat, max);
        AddSimpleFace(ref block.Top, drawOrder, Face.Top, buffers, translationMatrix, new Vector2(min, 0), new Vector2(max, 1), min, Rotation.Rotate0, block.Bottom.Flat, max);
        AddSimpleFace(ref block.Bottom, drawOrder, Face.Bottom, buffers, translationMatrix, new Vector2(min, 0), new Vector2(max, 1), min, Rotation.Rotate0, block.Top.Flat, max);
    }

    public static void Push(ref BlockInfo block, Vector3 offset, BufferArray<VertexPositionTile> vertices, BufferArray<short> indices, BufferArray<(float drawOrder, short index)> flatIndices)
    {
        var translation = Matrix.CreateTranslation(offset);
        var buffers = new Buffers(vertices, indices, flatIndices);

        switch (block.SlopeType.SlopeType)
        {
            case SlopeType.Up45:
                SlopeNSlope(ref block, offset.Z, Rotation.Rotate0, translation, buffers, 0, 1);
                break;
            case SlopeType.Right45:
                SlopeNSlope(ref block, offset.Z, Rotation.Rotate90, translation, buffers, 0, 1);
                break;
            case SlopeType.Down45:
                SlopeNSlope(ref block, offset.Z, Rotation.Rotate180, translation, buffers, 0, 1);
                break;
            case SlopeType.Left45:
                SlopeNSlope(ref block, offset.Z, Rotation.Rotate270, translation, buffers, 0, 1);
                break;
            case SlopeType.DiagonalFacingUpRight:
                SlopeDiagonal(ref block, offset.Z, Rotation.Rotate0, translation, buffers);
                break;
            case SlopeType.DiagonalFacingDownRight:
                SlopeDiagonal(ref block, offset.Z, Rotation.Rotate90, translation, buffers);
                break;
            case SlopeType.DiagonalFacingDownLeft:
                SlopeDiagonal(ref block, offset.Z, Rotation.Rotate180, translation, buffers);
                break;
            case SlopeType.DiagonalFacingUpLeft:
                SlopeDiagonal(ref block, offset.Z, Rotation.Rotate270, translation, buffers);
                break;
            case SlopeType.Up26_1:
                SlopeNSlope(ref block, offset.Z, Rotation.Rotate0, translation, buffers, 0F, 0.5f * 1);
                break;
            case SlopeType.Up26_2:
                SlopeNSlope(ref block, offset.Z, Rotation.Rotate0, translation, buffers, 0.5F, 0.5f * 2);
                break;
            case SlopeType.Down26_1:
                SlopeNSlope(ref block, offset.Z, Rotation.Rotate180, translation, buffers, 0F, 0.5f * 1);
                break;
            case SlopeType.Down26_2:
                SlopeNSlope(ref block, offset.Z, Rotation.Rotate180, translation, buffers, 0.5F, 0.5f * 2);
                break;
            case SlopeType.Left26_1:
                SlopeNSlope(ref block, offset.Z, Rotation.Rotate270, translation, buffers, 0F, 0.5f * 1);
                break;
            case SlopeType.Left26_2:
                SlopeNSlope(ref block, offset.Z, Rotation.Rotate270, translation, buffers, 0.5F, 0.5f * 2);
                break;
            case SlopeType.Right26_1:
                SlopeNSlope(ref block, offset.Z, Rotation.Rotate90, translation, buffers, 0F, 0.5f * 1);
                break;
            case SlopeType.Right26_2:
                SlopeNSlope(ref block, offset.Z, Rotation.Rotate90, translation, buffers, 0.5F, 0.5f * 2);
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
                SlopeNSlope(ref block, offset.Z, Rotation.Rotate0, translation, buffers, 0.125f * (num - 1), 0.125f * num);
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
                SlopeNSlope(ref block, offset.Z, Rotation.Rotate180, translation, buffers, 0.125f * (num - 1), 0.125f * num);
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
                SlopeNSlope(ref block, offset.Z, Rotation.Rotate270, translation, buffers, 0.125f * (num - 1), 0.125f * num);
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
                SlopeNSlope(ref block, offset.Z, Rotation.Rotate90, translation, buffers, 0.125f * (num - 1), 0.125f * num);
                break;
            }
            case SlopeType.DiagonalSlopeFacingUpRight:
                SlopeDiagonalSlope(ref block, offset.Z, Rotation.Rotate0, translation, buffers);
                break;
            case SlopeType.DiagonalSlopeFacingDownRight:
                SlopeDiagonalSlope(ref block, offset.Z, Rotation.Rotate90, translation, buffers);
                break;
            case SlopeType.DiagonalSlopeFacingDownLeft:
                SlopeDiagonalSlope(ref block, offset.Z, Rotation.Rotate180, translation, buffers);
                break;
            case SlopeType.DiagonalSlopeFacingUpLeft:
                SlopeDiagonalSlope(ref block, offset.Z, Rotation.Rotate270, translation, buffers);
                break;
            case SlopeType.PartialBlockLeft:
                SlopeHalfBlock(ref block, offset.Z, Rotation.Rotate0, translation, buffers);
                break;
            case SlopeType.PartialBlockTop:
                SlopeHalfBlock(ref block, offset.Z, Rotation.Rotate90, translation, buffers);
                break;
            case SlopeType.PartialBlockRight:
                SlopeHalfBlock(ref block, offset.Z, Rotation.Rotate180, translation, buffers);
                break;
            case SlopeType.PartialBlockBottom:
                SlopeHalfBlock(ref block, offset.Z, Rotation.Rotate270, translation, buffers);
                break;
            case SlopeType.PartialBlockTopLeftCorner:
                SlopeCornerBlock(ref block, offset.Z, Rotation.Rotate0, translation, buffers);
                break;
            case SlopeType.PartialBlockTopRightCorner:
                SlopeCornerBlock(ref block, offset.Z, Rotation.Rotate90, translation, buffers);
                break;
            case SlopeType.PartialBlockBottomRightCorner:
                SlopeCornerBlock(ref block, offset.Z, Rotation.Rotate180, translation, buffers);
                break;
            case SlopeType.PartialBlockBottomLeftCorner:
                SlopeCornerBlock(ref block, offset.Z, Rotation.Rotate270, translation, buffers);
                break;
            case SlopeType.PartialBlockCentre:
                SlopeCentreBlock(ref block, offset.Z, translation, buffers);
                break;
            case SlopeType.Reserved:
                break;
            default:
            case SlopeType.SlopeAbove:
            case SlopeType.None:
                SlopeNone(ref block, offset.Z, translation, buffers);
                break;
        }
    }
    
    private record struct Buffers(BufferArray<VertexPositionTile> Vertices, BufferArray<short> Indices, BufferArray<(float drawOrder, short index)> FlatIndices);
}