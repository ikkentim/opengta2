using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using OpenGta2.Client.Diagnostics;
using OpenGta2.Client.Levels;
using OpenGta2.Client.Peds;
using OpenGta2.Client.Utilities;
using OpenGta2.Physics;

namespace OpenGta2.Client.Components;

public class PlayerControllerComponent : BaseComponent
{
    private readonly Controls _controls;
    private readonly PedManager _pedManager;
    private readonly LevelProvider _levelProvider;
    private readonly Camera _camera;
    private Ped? _player;
    private bool _moveSlow;
    private CharacterMover? _mover;
    private Vector3 _spawn;

    public PlayerControllerComponent(GtaGame game, Controls controls, PedManager pedManager, LevelProvider levelProvider, Camera camera) : base(game)
    {
        _controls = controls;
        _pedManager = pedManager;
        _levelProvider = levelProvider;
        _camera = camera;
    }

    public override void Initialize()
    {
        _spawn = new Vector3(11.5f, 2.5f, _levelProvider.Map.GetGroundZ(11, 2) + 0.01f);
        _player = new Ped(_spawn, 0, 25);
        _pedManager.Peds.Add(_player);
        _camera.Attach(_player);

        _mover = new CharacterMover(_levelProvider.CollisionWorld);
        _mover.Rescued += (from, to) => DiagnosticValues.Set("LastRescue", $"{from.X:F2} {from.Y:F2} {from.Z:F2} -> {to.X:F2} {to.Y:F2} {to.Z:F2}");
        _mover.EnteredWater += position =>
        {
            DiagnosticValues.Set("LastDrowning", $"{position.X:F2} {position.Y:F2}");
            _player?.Drown();
        };
    }

    /// <summary>
    /// Puts the player back at the start after drowning.
    /// </summary>
    private void Respawn(Ped player)
    {
        player.Position = _spawn;
        player.Body.VelocityZ = 0;
        player.Body.Grounded = false;
        player.Body.InWater = false;
        player.Body.FallStartZ = _spawn.Z;
        player.Revive();
    }

    public override void Update(GameTime gameTime)
    {
        if (_player == null || _mover == null) return;

        if (_player.DrownedFinished)
        {
            Respawn(_player);
        }

        var fd = 0f;
        var lr = 0f;
        
        if (_controls.IsKeyPressed(Control.Right))
            lr++;

        if (_controls.IsKeyPressed(Control.Left))
            lr--;

        if (_controls.IsKeyPressed(Control.Forward))
            fd++;

        if (_controls.IsKeyPressed(Control.Backward))
            fd--;

        if (_controls.IsKeyDown(Keys.F2))
        {
            _moveSlow = !_moveSlow;
        }

        if (_player.IsDrowning)
        {
            // no control while sinking
            fd = 0;
            lr = 0;
        }
        else if (_controls.IsKeyDown(Keys.NumPad5))
        {
            TeleportToCameraCentre(_player);
        }

        _player.Rotation += lr * MathHelper.TwoPi * gameTime.GetDelta();
            
        var heading = new Vector2(MathF.Sin(-_player.Rotation), MathF.Cos(-_player.Rotation));
        var delta = heading * fd * gameTime.GetDelta() * 2 * (_moveSlow ? 0.1f : 1);

        var radius = _player.Body.Radius;
        DiagnosticHighlight.Add(_player.Position - new Vector3(radius, radius, 0), new Vector3(radius * 2, radius * 2, _player.Body.Height), Color.Blue);

        var dt = gameTime.GetDelta();
        _mover.Move(ref _player.Body, dt > 0 ? new System.Numerics.Vector2(delta.X, delta.Y) / dt : default, dt);

        UpdateDiagnostics(_player);

        if (!_player.IsDrowning)
        {
            _player.Animation = fd != 0 ? PedAnimation.Walking : PedAnimation.Idle;
        }
    }

    /// <summary>
    /// Debug helper: puts the player on the highest surface in the middle of the screen (where the camera is looking)
    /// and makes the camera follow the player again.
    /// </summary>
    private void TeleportToCameraCentre(Ped player)
    {
        var x = _camera.Position.X;
        var y = _camera.Position.Y;

        var surfaces = new List<OpenGta2.Physics.Surface>();
        _levelProvider.CollisionWorld.GetSurfaces(x, y, surfaces);

        // highest surface with enough room for the player to stand on
        var found = false;
        var height = 0f;
        foreach (var surface in surfaces)
        {
            if (surface.Headroom < player.Body.Height || (found && surface.Height <= height)) continue;

            height = surface.Height;
            found = true;
        }

        if (!found)
        {
            DiagnosticValues.Set("Teleport", $"no surface at {x:F2} {y:F2}");
            return;
        }

        player.Body.Position = new System.Numerics.Vector3(x, y, height + 0.001f);
        player.Body.VelocityZ = 0;
        player.Body.Grounded = true;
        player.Body.FallStartZ = height;

        DiagnosticValues.Set("Teleport", $"to {x:F2} {y:F2} {height:F2}");
        _camera.Unfree();
    }

    /// <summary>
    /// Shows where the player is and what the collision code sees there: the blocks in the column and the surfaces derived from them.
    /// </summary>
    private void UpdateDiagnostics(Ped player)
    {
        var position = player.Position;
        var cellX = (int)MathF.Floor(position.X);
        var cellY = (int)MathF.Floor(position.Y);

        DiagnosticValues.Set("PlayerPos", $"{position.X:F2} {position.Y:F2} {position.Z:F2} grounded={player.Body.Grounded} vz={player.Body.VelocityZ:F2}");

        var map = _levelProvider.Map;
        if (cellX < 0 || cellY < 0 || cellX >= map.Width || cellY >= map.Height) return;

        DiagnosticValues.Set("Cell", $"{cellX},{cellY} at {position.X - cellX:F2},{position.Y - cellY:F2}");
        DiagnosticHighlight.Add(new IntVector3(cellX, cellY, (int)MathF.Floor(position.Z)), Color.Yellow);

        // blocks in this column, bottom to top: layer:slope type, L if it has a lid tile, ground type
        var column = map.GetColumn(cellX, cellY);
        var blocks = new System.Text.StringBuilder();
        for (var z = column.Offset; z < column.Height; z++)
        {
            ref var block = ref map.CompressedMap.Blocks[column.Blocks[z - column.Offset]];
            blocks.Append(CultureInfo.InvariantCulture, $"[z{z} {block.SlopeType.SlopeType} {(block.Lid.TileGraphic != 0 ? "lid" : "nolid")} {block.SlopeType.GroundType}] ");
        }

        DiagnosticValues.Set("Blocks", blocks.ToString());

        // surfaces as seen by the physics code at the exact position
        var surfaces = new List<OpenGta2.Physics.Surface>();
        _levelProvider.CollisionWorld.GetSurfaces(position.X, position.Y, surfaces);
        DiagnosticValues.Set("Surfaces", string.Join(" ", surfaces.Select(s =>
            string.Create(CultureInfo.InvariantCulture, $"[h={s.Height:F2} head={(float.IsInfinity(s.Headroom) ? "inf" : s.Headroom.ToString("F2", CultureInfo.InvariantCulture))}{(s.Standable ? "" : " nostand")}]"))));
    }
}