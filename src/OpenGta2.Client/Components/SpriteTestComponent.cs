using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using OpenGta2.Client.Levels;
using OpenGta2.Client.Rendering;

namespace OpenGta2.Client.Components;

public class SpriteTestComponent : DrawableGameComponent
{
    private Rendering.Effects.SpriteEffect? _sb;
    private readonly LevelProvider _levelProvider;

    public SpriteTestComponent(GtaGame game) : base(game)
    {
        _levelProvider = game.Services.GetService<LevelProvider>();
    }

    protected override void LoadContent()
    {
        _sb = new Rendering.Effects.SpriteEffect(Game.Content.Load<Effect>("SpriteEffect"));
    }

    private int _sheet = 0;

    private KeyboardState _lastState;

    public override void Update(GameTime gameTime)
    {
        var kb = Keyboard.GetState();

        if (kb.IsKeyDown(Keys.OemPlus) && !_lastState.IsKeyDown(Keys.OemPlus))
        {
            _sheet++;
            _sheet %= _levelProvider.Style.SpriteGraphics.Length;
        }
        if (kb.IsKeyDown(Keys.OemMinus) && !_lastState.IsKeyDown(Keys.OemMinus))
        {
            _sheet--;
            if (_sheet < 0)
                _sheet = _levelProvider.Style.SpriteGraphics.Length - 1;
        }


        _lastState = kb;

        base.Update(gameTime);
    }

    public override void Draw(GameTime gameTime)
    {
        _sb!.Texture = _levelProvider.Textures.SpritesTexture;

        _sb.CurrentTechnique.Passes[0].Apply();

        QuadRenderer.Render(GraphicsDevice, _sheet, Vector2.Zero, Vector2.One * 256 * 2);
    }
}