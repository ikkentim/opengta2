using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using OpenGta2.Client.Diagnostics;
using OpenGta2.Client.Levels;
using OpenGta2.Client.Rendering;

namespace OpenGta2.Client.Components;

public class SpriteTestComponent : DrawableGameComponent
{
    private Rendering.Effects.SpriteEffect? _sb;
    private readonly LevelProvider _levelProvider;
    private int _sheet = -1;
    private KeyboardState _lastState;
    private FontRenderer? _fontRenderer;
    public SpriteTestComponent(GtaGame game) : base(game)
    {
        _levelProvider = game.Services.GetService<LevelProvider>();
    }

    private new GtaGame Game => (GtaGame)base.Game;
    protected override void LoadContent()
    {
        _sb = new Rendering.Effects.SpriteEffect(Game.Content.Load<Effect>("SpriteEffect"));

        _fontRenderer = new FontRenderer(Game.AssetManager, Game.Services.GetService<LevelProvider>());
    }

    public override void Update(GameTime gameTime)
    {
        var kb = Keyboard.GetState();

        if (kb.IsKeyDown(Keys.OemPlus) && !_lastState.IsKeyDown(Keys.OemPlus))
        {
            if (++_sheet == _levelProvider.Style.SpriteGraphics.Length)
                _sheet = -1;
        }
        if (kb.IsKeyDown(Keys.OemMinus) && !_lastState.IsKeyDown(Keys.OemMinus))
        {
            if (--_sheet < -1)
                _sheet = _levelProvider.Style.SpriteGraphics.Length - 1;
        }


        _lastState = kb;

        base.Update(gameTime);
    }

    public override void Draw(GameTime gameTime)
    {
        PerformanceCounters.Drawing.StartMeasurement("DrawSpriteTest");

        if (_sheet >= 0)
        {
            _sb!.Texture = _levelProvider.Textures.SpritesTexture;
            _sb.CurrentTechnique.Passes[0].Apply();

            QuadRenderer.Render(GraphicsDevice, _sheet, Vector2.Zero, Vector2.One * 256 * 2);
        }

        _fontRenderer!.Draw(GraphicsDevice, new Vector2(500, 500), 0, "HELLO WORLD");

        PerformanceCounters.Drawing.StopMeasurement();
    }
}