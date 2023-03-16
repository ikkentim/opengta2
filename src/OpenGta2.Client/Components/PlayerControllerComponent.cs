using System;
using Microsoft.Xna.Framework;
using OpenGta2.Client.Diagnostics;
using OpenGta2.Client.Levels;
using OpenGta2.Client.Peds;
using OpenGta2.Client.Utilities;

namespace OpenGta2.Client.Components;

public class PlayerControllerComponent : BaseComponent
{
    private readonly Controls _controls;
    private readonly PedManager _pedManager;
    private readonly LevelProvider _levelProvider;
    private readonly Camera _camera;
    private Ped? _player;
    public PlayerControllerComponent(GtaGame game, Controls controls, PedManager pedManager, LevelProvider levelProvider, Camera camera) : base(game)
    {
        _controls = controls;
        _pedManager = pedManager;
        _levelProvider = levelProvider;
        _camera = camera;
    }

    public override void Initialize()
    {
        _player = new Ped(new Vector3(11.5f, 2.5f, _levelProvider.Map.GetGroundZ(12, 2)), 0, 25);
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

        _player.Rotation += lr * MathHelper.TwoPi * gameTime.GetDelta();
            
        var heading = new Vector2(MathF.Sin(-_player.Rotation), MathF.Cos(-_player.Rotation));
        var delta = heading * fd * gameTime.GetDelta() * 2;

        const float playerWidth = (18f / 64);

        DiagnosticHighlight.Add(_player.Position - new Vector3(playerWidth/2, playerWidth/2, 0), new Vector3(playerWidth, playerWidth, 1), Color.Blue);

        // _player.Position += new Vector3(delta, 0);
        _player.Position = _levelProvider.CollisionMap.CalculateMovement(_player.Position, playerWidth, delta);

        _player.Animation = fd != 0 ? PedAnimation.Walking  : PedAnimation.Idle;
    }
}