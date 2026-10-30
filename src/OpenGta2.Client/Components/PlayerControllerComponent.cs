using System;
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
        _mover ??= new CharacterMover(_levelProvider.CollisionWorld);
        _mover.Move(ref _player.Body, dt > 0 ? new System.Numerics.Vector2(delta.X, delta.Y) / dt : default, dt);
        DiagnosticValues.Set("PlayerPos", $"{_player.Position.X:F2} {_player.Position.Y:F2} {_player.Position.Z:F2} grounded={_player.Body.Grounded}");

        _player.Animation = fd != 0 ? PedAnimation.Walking  : PedAnimation.Idle;
    }
}