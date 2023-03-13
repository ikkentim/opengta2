using System.Globalization;
using System.Text;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework;
using OpenGta2.Client.Utilities;

namespace OpenGta2.Client.Diagnostics;

public class DebuggingDrawingComponent : DrawableGameComponent
{
    private readonly StringBuilder _stringBuilder = new();

    private readonly SpriteBatch _spriteBatch;
    private SpriteFont? _font;
    private float _time;

    public DebuggingDrawingComponent(Game game) : base(game)
    {
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
    }
    
    public override void Draw(GameTime gameTime) => Draw(gameTime.GetDelta());

    public void Draw(float deltaTime)
    {
        // draw fps and performance counters
        _time += (deltaTime - _time) / 5;

        _stringBuilder.AppendLine(CultureInfo.InvariantCulture,  $"FPS: {(1 / _time):N1}");
        PerformanceCounters.Drawing.AppendText(_stringBuilder);

        foreach (var kv in DiagnosticValues.Values)
        {
            _stringBuilder.AppendLine(CultureInfo.InvariantCulture,  $"{kv.Key}: {kv.Value}");
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
}