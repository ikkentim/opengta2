using System.Globalization;
using System.Text;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using OpenGta2.Client.Assets.Effects;
using OpenGta2.Client.Utilities;

namespace OpenGta2.Client.Diagnostics;

public class DebuggingDrawingComponent : DrawableGameComponent
{
    private readonly Camera _camera;
    private readonly Controls _controls;
    private readonly StringBuilder _stringBuilder = new();

    private readonly SpriteBatch _spriteBatch;
    private SpriteFont? _font;
    private DebugLineEffect? _debugLineEffect;
    private float _time;

    private static readonly VertexPositionColor[] _blockVertices =
    {
        new(new Vector3(0, 1, 0), Color.White),
        new(new Vector3(1, 1, 0), Color.White),
        new(new Vector3(0, 0, 0), Color.White),
        new(new Vector3(1, 0, 0), Color.White),
        new(new Vector3(0, 1, 1), Color.White),
        new(new Vector3(1, 1, 1), Color.White),
        new(new Vector3(0, 0, 1), Color.White),
        new(new Vector3(1, 0, 1), Color.White),
    };

    private static readonly short[] _blockIndices = { 0, 1, 1, 3, 3, 2, 2, 0, 0, 4, 1, 5, 3, 7, 2, 6, 4, 5, 5, 7, 7, 6, 6, 4 };
    
    public DebuggingDrawingComponent(Game game, Camera camera, Controls controls) : base(game)
    {
        _camera = camera;
        _controls = controls;
        _spriteBatch = new SpriteBatch(GraphicsDevice);
    }

    private new GtaGame Game => (GtaGame)base.Game;

    public override void Initialize()
    {
        DrawOrder = 1000;
        
        base.Initialize();
    }

    protected override void LoadContent()
    {
        _font = Game.AssetManager.GetDebugFont();
        _debugLineEffect = Game.AssetManager.CreateDebugLineEffect();
    }
    
    public override void Draw(GameTime gameTime) => Draw(gameTime.GetDelta());

    public void Draw(float deltaTime)
    {
        PerformanceCounters.Drawing.StartMeasurement("Debug");

        if (_controls.IsKeyPressed(Keys.F1))
        {
            DrawDiagnosticHighlights();
        }
        else
        {
            DiagnosticHighlight.Reset();
        }

        DrawDiagnosticText(deltaTime);

        PerformanceCounters.Drawing.StopMeasurement();
    }

    private void DrawDiagnosticText(float deltaTime)
    {
        // draw fps and performance counters
        _time += (deltaTime - _time) / 5;

        _stringBuilder.AppendLine(CultureInfo.InvariantCulture, $"FPS: {(1 / _time):N1}");
        PerformanceCounters.Drawing.AppendText(_stringBuilder);

        foreach (var kv in DiagnosticValues.Values)
        {
            _stringBuilder.AppendLine(CultureInfo.InvariantCulture, $"{kv.Key}: {kv.Value}");
        }

        var text = _stringBuilder.ToString();
        _stringBuilder.Clear();


        _spriteBatch.Begin();
        _spriteBatch.DrawString(_font, text, new Vector2(10, 10), Color.White, 0, Vector2.Zero, Vector2.One, SpriteEffects.None, 0);
        _spriteBatch.End();

        PerformanceCounters.Drawing.Reset();

        // reset DepthStencil state after drawing 2d
        GraphicsDevice.DepthStencilState = DepthStencilState.Default;
    }

    private void DrawDiagnosticHighlights()
    {
        var tmp = GraphicsDevice.DepthStencilState;
        GraphicsDevice.DepthStencilState = DepthStencilState.None;
        foreach (var value in DiagnosticHighlight.HighlightedBlocks)
        {
            for (var i = 0; i < 8; i++)
            {
                _blockVertices[i].Color = value.color;
            }

            _debugLineEffect!.TransformMatrix = Matrix.CreateScale(value.scale) * Matrix.CreateTranslation(value.block) * _camera.ViewMatrix * _camera.Projection;
            _debugLineEffect.CurrentTechnique.Passes[0].Apply();

            GraphicsDevice.DrawUserIndexedPrimitives(PrimitiveType.LineList, _blockVertices, 0, 8, _blockIndices, 0, 12);
        }

        GraphicsDevice.DepthStencilState = tmp;

        DiagnosticHighlight.Reset();
    }
}