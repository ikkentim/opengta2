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
    public PlayerControllerComponent(GtaGame game, Controls controls, PedManager pedManager, LevelProvider levelProvider, Camera camera) : base(game)
    {
        _controls = controls;
        _pedManager = pedManager;
        _levelProvider = levelProvider;
        _camera = camera;
    }

    public override void Initialize()
    {
        _player = new Ped(new Vector3(11.5f, 2.5f, _levelProvider.Map.GetGroundZ(11, 2) + 0.01f), 0, 25);
        _pedManager.Peds.Add(_player);
        _camera.Attach(_player);
    }

    public override void Update(GameTime gameTime)
    {
        if (_player == null) return;

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

        _player.Rotation += lr * MathHelper.TwoPi * gameTime.GetDelta();
            
        var heading = new Vector2(MathF.Sin(-_player.Rotation), MathF.Cos(-_player.Rotation));
        var delta = heading * fd * gameTime.GetDelta() * 2 * (_moveSlow ? 0.1f : 1);

        var radius = _player.Body.Radius;
        DiagnosticHighlight.Add(_player.Position - new Vector3(radius, radius, 0), new Vector3(radius * 2, radius * 2, _player.Body.Height), Color.Blue);

        var dt = gameTime.GetDelta();
        if (_mover == null)
        {
            _mover = new CharacterMover(_levelProvider.CollisionWorld);
            _mover.Rescued += (from, to) => DiagnosticValues.Set("LastRescue", $"{from.X:F2} {from.Y:F2} {from.Z:F2} -> {to.X:F2} {to.Y:F2} {to.Z:F2}");
        }

        _mover.Move(ref _player.Body, dt > 0 ? new System.Numerics.Vector2(delta.X, delta.Y) / dt : default, dt);

        UpdateDiagnostics(_player);

        _player.Animation = fd != 0 ? PedAnimation.Walking  : PedAnimation.Idle;
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